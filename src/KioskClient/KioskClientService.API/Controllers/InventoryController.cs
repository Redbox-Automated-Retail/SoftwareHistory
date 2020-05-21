using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.Services.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/inventory")]
[ApiController]
public class InventoryController : ControllerBase
{
	private readonly ILogger<InventoryController> _logger;

	private readonly IInventoryService _inventoryService;

	public InventoryController(ILogger<InventoryController> logger, IInventoryService inventoryService)
	{
		_logger = logger;
		_inventoryService = inventoryService;
	}

	[Route("MerchandizingOrders")]
	[HttpGet]
	public async Task<ActionResult<GetMerchandizingOrdersResponse>> GetCurrentMerchandizingOrders()
	{
		_logger?.LogInfoWithSource("GetCurrentMerchandizingOrders called.", "GetCurrentMerchandizingOrders");
		return new OkObjectResult(await _inventoryService.GetAllCurrentMerchandizingOrders());
	}

	[Route("UpdateMerchandizingStatus")]
	[HttpPost]
	public async Task<ActionResult<UpdateMerchandizingStatusResponse>> UpdateMerchandizingStatus([FromBody] List<MerchandStatusUpdate> updates)
	{
		_logger?.LogInfoWithSource($"UpdateMerchandizingStatus called for {updates?.Count ?? 0} barcodes.", "UpdateMerchandizingStatus");
		return new OkObjectResult(await _inventoryService.UpdateMerchandizingStatus(updates));
	}
}
