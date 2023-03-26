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

namespace KioskClientService.API.Services.Campaigns;

internal class CampaignService : ICampaignService
{
	private const string CampaignDataFileName = "Campaigns";

	private const string StartAssetImagesFolder = "StartAssetImages";

	private readonly string _startAssetControlFolderPath;

	private readonly ILogger<CampaignService> _logger;

	private readonly IStoreService _storeService;

	private readonly IOptionsSnapshotKioskConfiguration _kioskConfiguration;

	private readonly IProxyApi _proxyApi;

	private readonly IPersistentDataCacheService _persistentDataCacheService;

	private readonly IHttpService _httpService;

	private static DateTime? _nextRefresh;

	public CampaignService(ILogger<CampaignService> logger, IStoreService storeService, IOptionsSnapshotKioskConfiguration kioskConfiguration, IProxyApi proxyApi, IPersistentDataCacheService persistentDataCacheService, IHttpService httpService)
	{
		_logger = logger;
		_storeService = storeService;
		_kioskConfiguration = kioskConfiguration;
		_proxyApi = proxyApi;
		_persistentDataCacheService = persistentDataCacheService;
		_httpService = httpService;
		_startAssetControlFolderPath = Path.Combine(_storeService.DataPath, "StartAssetImages");
		if (!Directory.Exists(_startAssetControlFolderPath))
		{
			_logger.LogInfoWithSource("Creating Start Asset Image directory: " + _startAssetControlFolderPath, ".ctor");
			Directory.CreateDirectory(_startAssetControlFolderPath);
		}
		if (!_nextRefresh.HasValue)
		{
			int num = new Random().Next(60, Math.Max(2, _kioskConfiguration.Campaigns.RefreshInterval) * 60);
			_nextRefresh = DateTime.Now.AddSeconds(num);
			_logger.LogInfoWithSource($"Next refresh at: {_nextRefresh}", ".ctor");
		}
	}

	public async Task<ApiBaseResponse> RefreshIfNeeded()
	{
		if (_nextRefresh < DateTime.Now)
		{
			_nextRefresh = DateTime.Now.AddMinutes(_kioskConfiguration.Campaigns.RefreshInterval);
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
		if (response.InCarts != null)
		{
			data.InCarts.Clear();
			data.InCarts.AddRange(response.InCarts);
			_logger.LogInfoWithSource("Processed updates to Incarts", "ProcessResponse");
		}
		if (response.StartScreens != null)
		{
			List<StartScreenDetails> processedStartScreens = new List<StartScreenDetails>();
			List<string> existingFiles = (from f in new DirectoryInfo(_startAssetControlFolderPath).GetFiles()
				select f.Name).ToList();
			foreach (StartScreenDetails startScreen in response.StartScreens)
			{
				var (flag, item) = await ProcessStartScreen(startScreen, existingFiles);
				if (!flag)
				{
					_logger.LogInfoWithSource($"Processing StartScreen: {startScreen.CampaignId} failed, exiting without updating campaigns cache", "ProcessResponse");
					return;
				}
				processedStartScreens.Add(item);
			}
			data.StartScreens.Clear();
			data.StartScreens.AddRange(processedStartScreens);
			_logger.LogInfoWithSource($"expired assets to delete: {existingFiles.Count}", "ProcessResponse");
			existingFiles.ForEach(DeleteStartAssetImage);
			_logger.LogInfoWithSource("Processed updates to StartScreens", "ProcessResponse");
		}
		data.LastSyncTime = response.LastSyncTime;
		await WriteCampaign(data);
	}

	private async Task<(bool, StartScreenDetails)> ProcessStartScreen(StartScreenDetails source, List<string> existingFiles)
	{
		IEnumerable<IControl> enumerable = source.Controls.Where((IControl c) => c.ControlType == ControlType.StartAsset);
		foreach (IControl item in enumerable)
		{
			StartAssetControl startAsset = item as StartAssetControl;
			Uri uri = new Uri(startAsset.Asset.PresignedUrl);
			string text = await DownloadAsset(uri, existingFiles);
			if (text == null)
			{
				_logger.LogErrorWithSource($"Failed to download asset for asset id: {startAsset.Asset.Id}", "ProcessStartScreen");
				return (false, null);
			}
			startAsset.Asset.FullPath = text;
			Asset asset = startAsset.Asset;
			string url = (startAsset.Asset.PresignedUrl = new Uri(text).AbsoluteUri);
			asset.Url = url;
		}
		return (true, source);
	}

	private async Task<string> DownloadAsset(Uri uri, List<string> existingFiles)
	{
		string result = null;
		try
		{
			string imageFilename = Path.GetFileName(uri.LocalPath);
			_logger.LogInfoWithSource("Downloading Asset: " + imageFilename, "DownloadAsset");
			APIResponse<byte[]> response = await _httpService.DownloadFile(uri.AbsoluteUri);
			if (response.IsSuccessStatusCode)
			{
				string destination = Path.Combine(_startAssetControlFolderPath, imageFilename);
				await File.WriteAllBytesAsync(destination, response.Response);
				_logger.LogInfoWithSource($"saved asset file: {imageFilename}, size: {response.Response.Length}", "DownloadAsset");
				existingFiles.Remove(imageFilename);
				result = destination;
			}
			else
			{
				_logger.LogErrorWithSource($"Received status {response.StatusCode} downloading asset file: {imageFilename} from {uri.AbsoluteUri}", "DownloadAsset");
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Error downloading asset: " + uri.AbsoluteUri, "DownloadAsset");
		}
		return result;
	}

	private void DeleteStartAssetImage(string file)
	{
		try
		{
			File.Delete(Path.Combine(_startAssetControlFolderPath, file));
			_logger.LogInfoWithSource("deleted start asset image: " + file, "DeleteStartAssetImage");
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Unable to delete start asset image file: " + file);
		}
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

	public async Task<GetInCartResponse> GetInCart()
	{
		try
		{
			if (!_kioskConfiguration.Campaigns.InCartsActive)
			{
				return new GetInCartResponse
				{
					StatusCode = HttpStatusCode.NoContent,
					Error = "InCartsActive is false"
				};
			}
			KioskInCartDetails kioskInCartDetails = await GetActiveInCart();
			return new GetInCartResponse
			{
				InCart = kioskInCartDetails,
				StatusCode = ((kioskInCartDetails == null) ? HttpStatusCode.NotFound : HttpStatusCode.OK)
			};
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occurred", "GetInCart");
			return new GetInCartResponse
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

	public async Task<GetStartScreenResponse> GetStartScreen()
	{
		try
		{
			if (!_kioskConfiguration.Campaigns.StartScreensActive)
			{
				return new GetStartScreenResponse
				{
					StatusCode = HttpStatusCode.NoContent,
					Error = "StartScreensActive is false"
				};
			}
			KioskStartScreenDetails kioskStartScreenDetails = await GetActiveStartScreen();
			return new GetStartScreenResponse
			{
				StartScreen = kioskStartScreenDetails,
				StatusCode = ((kioskStartScreenDetails == null) ? HttpStatusCode.NotFound : HttpStatusCode.OK)
			};
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occurred", "GetStartScreen");
			return new GetStartScreenResponse
			{
				StatusCode = HttpStatusCode.InternalServerError,
				Error = ex.Message
			};
		}
	}

	private async Task<KioskStartScreenDetails> GetActiveStartScreen()
	{
		StartScreenDetails startScreenData = (from c in await GetAllStartScreens()
			where c.StartDate <= DateTime.Now && (!c.EndDate.HasValue || c.EndDate > DateTime.Now)
			orderby c.StartDate descending
			orderby c.Rank descending, c.StartDate descending
			select c).FirstOrDefault();
		return KioskStartScreenDetails.FromStartScreenDetails(startScreenData);
	}

	private async Task<List<StartScreenDetails>> GetAllStartScreens()
	{
		return (await ReadCampaign())?.StartScreens ?? new List<StartScreenDetails>();
	}
}
