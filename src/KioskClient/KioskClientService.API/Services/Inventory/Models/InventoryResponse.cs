using System;
using KioskClientService.API.App;
using KioskClientService.API.Services.HttpQueue;

namespace KioskClientService.API.Services.Inventory.Models;

public class InventoryResponse : BaseResponse, IReplayableApiCall
{
	public Guid MessageId { get; set; }

	public bool Replayable => true;

	public InventoryResponse()
		: base(200)
	{
	}

	public override object Scrub()
	{
		return new { base.StatusCode, base.Errors }.ToJson();
	}
}
