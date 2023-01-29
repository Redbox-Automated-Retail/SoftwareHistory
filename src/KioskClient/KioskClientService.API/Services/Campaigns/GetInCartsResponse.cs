using KioskClientService.API.App;

namespace KioskClientService.API.Services.Campaigns;

public class GetInCartsResponse : ApiBaseResponse
{
	public KioskInCartDetails InCart { get; set; }

	public override object Scrub()
	{
		return string.Empty;
	}
}
