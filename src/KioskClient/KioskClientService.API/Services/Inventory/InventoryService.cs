using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Inventory.API;
using KioskClientService.API.Services.Inventory.Models;
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

	public async Task<InventoryResponse> Sync(InventorySnapshotRequest request)
	{
		InventoryResponse result = new InventoryResponse
		{
			MessageId = request.MessageId
		};
		_logger.LogInfoWithSource("Making Sync call to Inventory services api", "Sync");
		InventoryResponse inventoryResponse = await _inventoryClient.Sync(request);
		if (inventoryResponse == null)
		{
			string message = "Null response returned from Inventory Services Sync api!";
			_logger.LogErrorWithSource(message, "Sync");
			result.Success = false;
			result.Errors.Add(new Error
			{
				Code = "(V000)",
				Message = message
			});
		}
		else
		{
			result.Success = inventoryResponse.StatusCode == 200;
			result.StatusCode = inventoryResponse.StatusCode;
			result.Errors.AddRange(inventoryResponse.Errors);
			if (result.Success)
			{
				_logger.LogInfoWithSource("Sync was successful", "Sync");
			}
			else
			{
				_logger.LogErrorWithSource("Sync was unsuccessful, errors: " + result.Errors.ToJson(), "Sync");
			}
		}
		return result;
	}

	public async Task<InventoryResponse> DiscAction(DiscActionRequest request)
	{
		InventoryResponse result = new InventoryResponse
		{
			MessageId = request.MessageId
		};
		_logger.LogInfoWithSource("Making DiscAction call to Inventory services api", "DiscAction");
		InventoryResponse inventoryResponse = await _inventoryClient.DiscAction(request);
		if (inventoryResponse == null)
		{
			string message = "Null response returned from Inventory Services DiscAction api!";
			_logger.LogErrorWithSource(message, "DiscAction");
			result.Success = false;
			result.Errors.Add(new Error
			{
				Code = "(V000)",
				Message = message
			});
		}
		else
		{
			result.Success = inventoryResponse.StatusCode == 200;
			result.StatusCode = inventoryResponse.StatusCode;
			result.Errors.AddRange(inventoryResponse.Errors);
			if (result.Success)
			{
				_logger.LogInfoWithSource("DiscAction was successful", "DiscAction");
			}
			else
			{
				_logger.LogErrorWithSource("DiscAction was unsuccessful, errors: " + result.Errors.ToJson(), "DiscAction");
			}
		}
		return result;
	}

	public async Task<InventoryResponse> DiscFraud(DiscFraudRequest request)
	{
		InventoryResponse result = new InventoryResponse
		{
			MessageId = request.MessageId
		};
		_logger.LogInfoWithSource("Making DiscFraud call to Inventory services api", "DiscFraud");
		InventoryResponse inventoryResponse = await _inventoryClient.DiscFraud(request);
		if (inventoryResponse == null)
		{
			string message = "Null response returned from Inventory Services DiscFraud api!";
			_logger.LogErrorWithSource(message, "DiscFraud");
			result.Success = false;
			result.Errors.Add(new Error
			{
				Code = "(V000)",
				Message = message
			});
		}
		else
		{
			result.Success = inventoryResponse.StatusCode == 200;
			result.StatusCode = inventoryResponse.StatusCode;
			result.Errors.AddRange(inventoryResponse.Errors);
			if (result.Success)
			{
				_logger.LogInfoWithSource("DiscFraud was successful", "DiscFraud");
			}
			else
			{
				_logger.LogErrorWithSource("DiscFraud was unsuccessful, errors: " + result.Errors.ToJson(), "DiscFraud");
			}
		}
		return result;
	}
}
