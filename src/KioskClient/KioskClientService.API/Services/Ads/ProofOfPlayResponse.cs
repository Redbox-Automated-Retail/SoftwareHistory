using KioskClientService.API.App;

namespace KioskClientService.API.Services.Ads;

public class ProofOfPlayResponse : BaseResponse, IMessageScrub
{
	public ProofOfPlayResponse()
		: base(200)
	{
		base.Success = true;
	}

	public object Scrub()
	{
		return this.ToJsonIndented();
	}
}
