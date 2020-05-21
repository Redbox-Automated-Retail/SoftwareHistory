using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Inventory.API;

public class InventoryApiClient : IInventoryApiClient
{
	private readonly IApplicationConfiguration _applicationConfiguration;

	private readonly IHttpService _httpService;

	private readonly ILogger<IInventoryApiClient> _logger;

	private readonly List<Header> _requestHeaders;

	private long _kioskId;

	private const string _getMerchandizingOrdersEndpoint = "api/merchandizingorder/{KioskId}/{OrderDate}";

	private const string _updateMerchandizingStatusEndpoint = "api/disc/updatemerchandizingstatus";

	private const string _orderTypeField = "orderType";

	private const string _getProductGroupOrderEndpoint = "api/productgroupsortorder/{productGroupSortOrderType}";

	public InventoryApiClient(IStoreService store, IApplicationConfiguration applicationConfiguration, IHttpService httpService, ILogger<IInventoryApiClient> logger)
	{
		_applicationConfiguration = applicationConfiguration;
		_httpService = httpService;
		_logger = logger;
		_httpService.Timeout = _applicationConfiguration.InventoryApi.Timeout ?? 60000;
		_kioskId = store?.KioskId ?? 0;
		_requestHeaders = _applicationConfiguration.InventoryApi.GetKioskCloudHeaders(_kioskId);
	}

	public async Task<APIResponse<MerchandizingOrdersResponse>> GetMerchandizingOrders(DateTime orderDate, MerchandizingOrderType? orderType)
	{
		string text = "api/merchandizingorder/{KioskId}/{OrderDate}".Replace("{KioskId}", _kioskId.ToString()).Replace("{OrderDate}", orderDate.ToString("yyyy-MM-dd"));
		if (orderType.HasValue)
		{
			text = text + "?orderType=" + (int)orderType.Value;
		}
		return await _httpService.SendRequest<MerchandizingOrdersResponse>(_applicationConfiguration.InventoryApi.Url, text, null, HttpMethod.Get, _requestHeaders);
	}

	public async Task<APIResponse<MerchandStatusUpdateResponse>> UpdateMerchandizingStatuses(List<MerchandStatusUpdate> updates)
	{
		return await _httpService.SendRequest<MerchandStatusUpdateResponse>(_applicationConfiguration.InventoryApi.Url, "api/disc/updatemerchandizingstatus", updates, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<GetProductGroupSortOrder>> GetProductGroupOrder(int productGroupSortOrderType)
	{
		string endpoint = "api/productgroupsortorder/{productGroupSortOrderType}".Replace("{productGroupSortOrderType}", productGroupSortOrderType.ToString());
		return await _httpService.SendRequest<GetProductGroupSortOrder>(_applicationConfiguration.InventoryApi.Url, endpoint, null, HttpMethod.Get, _requestHeaders);
	}
}
