namespace KioskClientService.API.App;

public class ApplicationResponse : BaseResponse, IMessageScrub
{
	public ApplicationResponse()
		: base(200)
	{
		base.Success = true;
	}

	public object Scrub()
	{
		return this.ToJsonIndented();
	}
}
