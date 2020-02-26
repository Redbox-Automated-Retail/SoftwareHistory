using KioskClientService.API.App;

namespace KioskClientService.API.Services.Ads;

public class ExpireResponse : BaseResponse, IMessageScrub
{
	public ExpireResponse()
		: base(200)
	{
		base.Success = true;
	}

	public object Scrub()
	{
		return this.ToJsonIndented();
	}
}
