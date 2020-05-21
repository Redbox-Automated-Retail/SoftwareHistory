using System.Collections.Generic;
using KioskClientService.API.App;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Inventory;

public class UpdateMerchandizingStatusResponse : BaseResponse, IMessageScrub
{
	public List<MerchandStatusUpdate> FailedBarcodes { get; set; }

	public string Error { get; set; }

	public UpdateMerchandizingStatusResponse()
		: base(200)
	{
	}

	public UpdateMerchandizingStatusResponse(int statusCode)
		: base(statusCode)
	{
	}

	public object Scrub()
	{
		return JsonConvert.SerializeObject(this);
	}
}
