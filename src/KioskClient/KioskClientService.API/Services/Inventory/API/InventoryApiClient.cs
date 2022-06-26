using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.Inventory.Models;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Inventory.API;

public class InventoryApiClient : IInventoryApiClient
{
	private readonly IApplicationConfiguration _applicationConfiguration;

	private readonly IHttpService _httpService;

	private long _kioskId;

	private const string _getMerchandizingOrdersEndpoint = "api/merchandizingorder/{KioskId}/{OrderDate}";

	private const string _updateMerchandizingStatusEndpoint = "api/disc/updatemerchandizingstatus";

	private const string _orderTypeField = "orderType";

	private const string _getProductGroupOrderEndpoint = "api/productgroupsortorder/{productGroupSortOrderType}";

	private const string _discActionEndpoint = "api/disc/discaction";

	private const string _discFraudEndpoint = "api/disc/discfraud";

	private const string _syncEndpoint = "api/kiosk/sync";

	public InventoryApiClient(IStoreService store, IApplicationConfiguration applicationConfiguration, IHttpService httpService)
	{
		_applicationConfiguration = applicationConfiguration;
		_httpService = httpService;
		_httpService.Timeout = _applicationConfiguration.InventoryApi.Timeout ?? 60000;
		_kioskId = store?.KioskId ?? 0;
	}

	public async Task<APIResponse<MerchandizingOrdersResponse>> GetMerchandizingOrders(DateTime orderDate, MerchandizingOrderType? orderType)
	{
		string text = "api/merchandizingorder/{KioskId}/{OrderDate}".Replace("{KioskId}", _kioskId.ToString()).Replace("{OrderDate}", orderDate.ToString("yyyy-MM-dd"));
		if (orderType.HasValue)
		{
			text = text + "?orderType=" + (int)orderType.Value;
		}
		return await _httpService.SendRequest<MerchandizingOrdersResponse>(_applicationConfiguration.InventoryApi.Url, text, null, HttpMethod.Get, _applicationConfiguration.InventoryApi.GetKioskCloudHeaders(null, _kioskId), logResponseContent: false);
	}

	public async Task<APIResponse<MerchandStatusUpdateResponse>> UpdateMerchandizingStatuses(List<MerchandStatusUpdate> updates)
	{
		return await _httpService.SendRequest<MerchandStatusUpdateResponse>(_applicationConfiguration.InventoryApi.Url, "api/disc/updatemerchandizingstatus", updates, HttpMethod.Post, _applicationConfiguration.InventoryApi.GetKioskCloudHeaders(null, _kioskId));
	}

	public async Task<APIResponse<GetProductGroupSortOrder>> GetProductGroupOrder(int productGroupSortOrderType)
	{
		string endpoint = "api/productgroupsortorder/{productGroupSortOrderType}".Replace("{productGroupSortOrderType}", productGroupSortOrderType.ToString());
		return await _httpService.SendRequest<GetProductGroupSortOrder>(_applicationConfiguration.InventoryApi.Url, endpoint, null, HttpMethod.Get, _applicationConfiguration.InventoryApi.GetKioskCloudHeaders(null, _kioskId));
	}

	public async Task<InventoryResponse> Sync(InventorySnapshotRequest request)
	{
		APIResponse<InventoryResponse> response = await _httpService.SendGZipRequest<InventoryResponse>(_applicationConfiguration.InventoryApi.Url, "api/kiosk/sync", request, HttpMethod.Post, HeadersFromRequest(request));
		InventoryResponse inventoryResponse;
		if (response.HttpResponse == null)
		{
			inventoryResponse = new InventoryResponse
			{
				Success = false,
				StatusCode = 500
			};
			inventoryResponse.Errors.Add(new Error
			{
				Code = response.HttpResponse.StatusCode.ToString(),
				Message = "No response from api call"
			});
		}
		else
		{
			string text = await response.HttpResponse.Content.ReadAsStringAsync();
			if (response.HttpResponse.IsSuccessStatusCode)
			{
				inventoryResponse = JsonConvert.DeserializeObject<InventoryResponse>(text);
			}
			else
			{
				inventoryResponse = new InventoryResponse
				{
					Success = false,
					StatusCode = (int)response.HttpResponse.StatusCode
				};
				inventoryResponse.Errors.Add(new Error
				{
					Code = response.HttpResponse.StatusCode.ToString(),
					Message = text
				});
			}
		}
		return inventoryResponse;
	}

	public async Task<InventoryResponse> DiscAction(DiscActionRequest request)
	{
		APIResponse<InventoryResponse> response = await _httpService.SendRequest<InventoryResponse>(_applicationConfiguration.InventoryApi.Url, "api/disc/discaction", request, HttpMethod.Post, HeadersFromRequest(request));
		InventoryResponse inventoryResponse;
		if (response.HttpResponse == null)
		{
			inventoryResponse = new InventoryResponse
			{
				Success = false,
				StatusCode = 500
			};
			inventoryResponse.Errors.Add(new Error
			{
				Code = response.HttpResponse.StatusCode.ToString(),
				Message = "No response from api call"
			});
		}
		else
		{
			string text = await response.HttpResponse.Content.ReadAsStringAsync();
			if (response.HttpResponse.IsSuccessStatusCode)
			{
				inventoryResponse = JsonConvert.DeserializeObject<InventoryResponse>(text);
			}
			else
			{
				inventoryResponse = new InventoryResponse
				{
					Success = false,
					StatusCode = (int)response.HttpResponse.StatusCode
				};
				inventoryResponse.Errors.Add(new Error
				{
					Code = response.HttpResponse.StatusCode.ToString(),
					Message = text
				});
			}
		}
		return inventoryResponse;
	}

	public async Task<InventoryResponse> DiscFraud(DiscFraudRequest request)
	{
		APIResponse<InventoryResponse> response = await _httpService.SendRequest<InventoryResponse>(_applicationConfiguration.InventoryApi.Url, "api/disc/discfraud", request, HttpMethod.Post, HeadersFromRequest(request));
		InventoryResponse inventoryResponse;
		if (response.HttpResponse == null)
		{
			inventoryResponse = new InventoryResponse
			{
				Success = false,
				StatusCode = 500
			};
			inventoryResponse.Errors.Add(new Error
			{
				Code = response.HttpResponse.StatusCode.ToString(),
				Message = "No response from api call"
			});
		}
		else
		{
			string text = await response.HttpResponse.Content.ReadAsStringAsync();
			if (response.HttpResponse.IsSuccessStatusCode)
			{
				inventoryResponse = JsonConvert.DeserializeObject<InventoryResponse>(text);
			}
			else
			{
				inventoryResponse = new InventoryResponse
				{
					Success = false,
					StatusCode = (int)response.HttpResponse.StatusCode
				};
				inventoryResponse.Errors.Add(new Error
				{
					Code = response.HttpResponse.StatusCode.ToString(),
					Message = text
				});
			}
		}
		return inventoryResponse;
	}

	private List<Header> HeadersFromRequest(InventoryBaseRequest request)
	{
		List<Header> kioskCloudHeaders = _applicationConfiguration.InventoryApi.GetKioskCloudHeaders();
		kioskCloudHeaders.Remove(kioskCloudHeaders.SingleOrDefault((Header h) => h.Key == "x-redbox-activityid"));
		kioskCloudHeaders.Add(new Header("x-redbox-activityid", request.MessageId.ToString()));
		kioskCloudHeaders.Remove(kioskCloudHeaders.SingleOrDefault((Header h) => h.Key == "x-redbox-kioskid"));
		kioskCloudHeaders.Add(new Header("x-redbox-kioskid", request.KioskId.ToString()));
		return kioskCloudHeaders;
	}
}
