namespace KioskClientService.API.App;

public class ViewShowResponse : BaseResponse, IMessageScrub
{
	public ViewShowResponse()
		: base(200)
	{
		base.Success = true;
	}

	public object Scrub()
	{
		return this.ToJson();
	}
}
