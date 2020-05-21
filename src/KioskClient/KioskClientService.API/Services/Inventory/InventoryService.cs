using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Inventory.API;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Inventory;

public class InventoryService : IInventoryService
{
	private IInventoryApiClient _inventoryClient;

	private ILogger<InventoryService> _logger;

	public InventoryService(IInventoryApiClient inventoryApiClient, ILogger<InventoryService> logger)
	{
		_inventoryClient = inventoryApiClient;
		_logger = logger;
	}

	public async Task<GetMerchandizingOrdersResponse> GetAllCurrentMerchandizingOrders()
	{
		GetMerchandizingOrdersResponse result = new GetMerchandizingOrdersResponse();
		APIResponse<MerchandizingOrdersResponse> aPIResponse = await _inventoryClient.GetMerchandizingOrders(DateTime.UtcNow, null);
		aPIResponse.CopyTo(result);
		if (aPIResponse != null && aPIResponse.IsSuccessStatusCode)
		{
			result.MerchandizingOrders = aPIResponse.Response;
		}
		else
		{
			_logger.LogErrorWithSource(aPIResponse.Exception, "GetMerchandizingOrders was unsuccessful.", "GetAllCurrentMerchandizingOrders");
			LogResponseErrors(aPIResponse);
		}
		return result;
	}

	private void LogResponseErrors<T>(APIResponse<T> response)
	{
		if (response.Errors == null || response.Errors.Count <= 0)
		{
			return;
		}
		foreach (Error error in response.Errors)
		{
			_logger.LogErrorWithSource("Error code " + error.Code + " message " + error.Message, "LogResponseErrors");
		}
	}

	public async Task<UpdateMerchandizingStatusResponse> UpdateMerchandizingStatus(List<MerchandStatusUpdate> updates)
	{
		UpdateMerchandizingStatusResponse result = new UpdateMerchandizingStatusResponse();
		APIResponse<MerchandStatusUpdateResponse> aPIResponse = await _inventoryClient.UpdateMerchandizingStatuses(updates);
		aPIResponse.CopyTo(result);
		if (aPIResponse != null && aPIResponse.Response != null)
		{
			result.FailedBarcodes = aPIResponse.Response.Data;
			result.Error = aPIResponse.Response.error;
		}
		if (aPIResponse == null || !aPIResponse.IsSuccessStatusCode)
		{
			_logger.LogErrorWithSource(aPIResponse.Exception, "UpdateMerchandizingStatus was unsuccessful.", "UpdateMerchandizingStatus");
			LogResponseErrors(aPIResponse);
		}
		return result;
	}
}
