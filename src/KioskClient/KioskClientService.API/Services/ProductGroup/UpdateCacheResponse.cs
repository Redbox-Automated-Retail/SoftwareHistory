using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProductGroup;

public class UpdateCacheResponse : BaseResponse, IMessageScrub
{
	public UpdateCacheResponse()
		: base(200)
	{
		base.Success = true;
	}

	public override object Scrub()
	{
		return this.ToJsonIndented();
	}
}
