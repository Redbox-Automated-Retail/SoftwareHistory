using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Campaigns;

internal class CampaignService : ICampaignService
{
	private readonly ILogger<CampaignService> _logger;

	private readonly IStoreService _storeService;

	private readonly IOptionsSnapshotKioskConfiguration _configuration;

	private readonly IProxyApi _proxyApi;

	private readonly IPersistentDataCacheService _persistentDataCacheService;

	private static DateTime? _nextRefresh;

	private readonly string _campaignPath;

	private const string CampaignFolder = "Campaigns";

	private const string CampaignDataFileName = "Campaigns";

	public CampaignService(ILogger<CampaignService> logger, IStoreService storeService, IOptionsSnapshotKioskConfiguration configuration, IProxyApi proxyApi, IPersistentDataCacheService persistentDataCacheService)
	{
		_logger = logger;
		_storeService = storeService;
		_configuration = configuration;
		_proxyApi = proxyApi;
		_persistentDataCacheService = persistentDataCacheService;
		if (!_nextRefresh.HasValue)
		{
			int num = new Random().Next(60, Math.Max(2, _configuration.Campaigns.RefreshInterval) * 60);
			_nextRefresh = DateTime.Now.AddSeconds(num);
			_logger.LogInfoWithSource($"Next refresh at: {_nextRefresh}", ".ctor");
		}
		_campaignPath = Path.Combine(_storeService.DataPath, "Campaigns");
		if (!Directory.Exists(_campaignPath))
		{
			Directory.CreateDirectory(_campaignPath);
		}
	}

	public async Task<GetStartScreenResponse> GetStartScreen()
	{
		return await GetCampaign<GetStartScreenResponse, StartScreenData>(CampaignType.StartScreen);
	}

	private async Task<R> GetCampaign<R, D>(CampaignType campaignType) where R : ConcreteBaseResponse<D>, new() where D : ICampaignData
	{
		R result = new R();
		try
		{
			string campaignPath = GetCampaignPath(campaignType);
			if (File.Exists(campaignPath))
			{
				CampaignList<D> campaignList = JsonConvert.DeserializeObject<CampaignList<D>>(await File.ReadAllTextAsync(campaignPath));
				if (campaignList == null)
				{
					_logger.LogErrorWithSource("Campaign file: " + campaignPath + " is not valid json.", "GetCampaign");
					result.StatusCode = HttpStatusCode.UnprocessableEntity;
					result.Error = "Campaign: '" + campaignPath + "' was not valid campaign json.";
					result.Campaign = null;
				}
				else
				{
					Campaign<D> activeCampaign = campaignList.GetActiveCampaign();
					if (activeCampaign == null)
					{
						_logger.LogErrorWithSource("Active campaign not found in " + campaignPath + ".", "GetCampaign");
						result.StatusCode = HttpStatusCode.NoContent;
						result.Error = "Campaign: '" + campaignPath + "' valid but no active campaign found.";
						result.Campaign = null;
					}
					else
					{
						_logger.LogInfoWithSource($"Campaign lookup successful, returning id: {activeCampaign.CampaignId}.", "GetCampaign");
						result.StatusCode = HttpStatusCode.OK;
						result.Error = null;
						result.Campaign = activeCampaign;
					}
				}
			}
			else
			{
				_logger.LogErrorWithSource("Campaign file: " + campaignPath + " not found.", "GetCampaign");
				result.StatusCode = HttpStatusCode.NotFound;
				result.Error = "Campaign: '" + campaignPath + "' was not found.";
				result.Campaign = null;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occurred", "GetCampaign");
			result.StatusCode = HttpStatusCode.InternalServerError;
			result.Error = ex.Message;
			result.Campaign = null;
		}
		return result;
	}

	internal string GetCampaignPath(CampaignType campaignType)
	{
		return Path.Combine(_campaignPath, $"{campaignType}.json");
	}

	public async Task<ApiBaseResponse> RefreshIfNeeded()
	{
		if (_nextRefresh < DateTime.Now)
		{
			_nextRefresh = DateTime.Now.AddMinutes(_configuration.Campaigns.RefreshInterval);
			_logger.LogInfoWithSource($"Running campaign refresh if needed. Next refresh at: {_nextRefresh}", "RefreshIfNeeded");
			return await Refresh();
		}
		return new ApiBaseResponse
		{
			StatusCode = HttpStatusCode.OK
		};
	}

	public async Task<ApiBaseResponse> Refresh()
	{
		_logger?.LogInfoWithSource("Started", "Refresh");
		try
		{
			if (_storeService.KioskId == 0L)
			{
				_logger?.LogErrorWithSource("KioskId must be > 0", "Refresh");
				return new ApiBaseResponse
				{
					StatusCode = HttpStatusCode.InternalServerError,
					Error = "KioskId is 0"
				};
			}
			CampaignData data = await ReadCampaign();
			PostKioskCampaignsRequest request = new PostKioskCampaignsRequest
			{
				LastSyncTime = data.LastSyncTime
			};
			APIResponse<PostKioskCampaignsResponse> aPIResponse = await _proxyApi.PostKioskCampaigns(_storeService.KioskId, request);
			if (!aPIResponse.IsSuccessStatusCode)
			{
				return new ApiBaseResponse
				{
					StatusCode = HttpStatusCode.InternalServerError,
					Error = "Error retrieving kiosk campaigns"
				};
			}
			if (aPIResponse.StatusCode == HttpStatusCode.AlreadyReported)
			{
				return new ApiBaseResponse
				{
					StatusCode = HttpStatusCode.AlreadyReported
				};
			}
			ProcessResponse(aPIResponse.Response, data);
			return new ApiBaseResponse
			{
				StatusCode = HttpStatusCode.OK
			};
		}
		catch (Exception ex)
		{
			string text = "Exception occured retrieving kiosk capaigns";
			_logger.LogErrorWithSource(ex, text, "Refresh");
			return new ApiBaseResponse
			{
				StatusCode = HttpStatusCode.InternalServerError,
				Error = text
			};
		}
	}

	private async void ProcessResponse(PostKioskCampaignsResponse response, CampaignData data)
	{
		data.LastSyncTime = response.LastSyncTime;
		if (response.InCarts != null)
		{
			data.InCarts.Clear();
			data.InCarts.AddRange(response.InCarts);
			_logger.LogInfoWithSource("Processed updates to Incarts", "ProcessResponse");
		}
		await WriteCampaign(data);
	}

	private async Task<CampaignData> ReadCampaign()
	{
		PersistentDataWrapper<CampaignData> persistentDataWrapper = await _persistentDataCacheService.Read<CampaignData>("Campaigns", useCache: true);
		if (persistentDataWrapper == null || persistentDataWrapper.Data == null)
		{
			_logger?.LogInfoWithSource("Cache is not set", "ReadCampaign");
			return new CampaignData();
		}
		return persistentDataWrapper.Data;
	}

	private async Task<bool> WriteCampaign(CampaignData campaignData)
	{
		bool flag = await _persistentDataCacheService.Write(campaignData, "Campaigns");
		if (!flag)
		{
			_logger.LogInfoWithSource("Campaign data could not be written", "WriteCampaign");
		}
		_logger.LogInfoWithSource("Campaign data persisted", "WriteCampaign");
		return flag;
	}

	public bool DeleteCampaignData()
	{
		return _persistentDataCacheService.Delete("Campaigns");
	}

	public async Task<GetInCartsResponse> GetInCart()
	{
		try
		{
			if (!_configuration.Campaigns.InCartsActive)
			{
				return new GetInCartsResponse
				{
					StatusCode = HttpStatusCode.NoContent,
					Error = "InCartsActive is false"
				};
			}
			KioskInCartDetails kioskInCartDetails = await GetActiveInCart();
			return new GetInCartsResponse
			{
				InCart = kioskInCartDetails,
				StatusCode = ((kioskInCartDetails == null) ? HttpStatusCode.NoContent : HttpStatusCode.OK)
			};
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occurred", "GetInCart");
			return new GetInCartsResponse
			{
				StatusCode = HttpStatusCode.InternalServerError,
				Error = "Exception occurred"
			};
		}
	}

	private async Task<KioskInCartDetails> GetActiveInCart()
	{
		InCartDetails inCartDetails = (from c in await GetAllInCarts()
			where c.StartDate <= DateTime.Now && (!c.EndDate.HasValue || c.EndDate > DateTime.Now)
			orderby c.Rank descending, c.StartDate descending
			select c).FirstOrDefault();
		if (inCartDetails == null)
		{
			return null;
		}
		return KioskInCartDetails.FromInCartDetails(inCartDetails);
	}

	private async Task<List<InCartDetails>> GetAllInCarts()
	{
		CampaignData campaignData = await ReadCampaign();
		if (campaignData.InCarts != null)
		{
			return campaignData.InCarts;
		}
		return new List<InCartDetails>();
	}
}
