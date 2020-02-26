using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Ads;

public class GetAdsResponse : BaseResponse, IMessageScrub
{
	public List<GetAdResponse> GetAdResponses { get; set; }

	public GetAdsResponse()
		: base(200)
	{
		base.Success = true;
		GetAdResponses = new List<GetAdResponse>();
	}

	public object Scrub()
	{
		return this.ToJsonIndented();
	}
}
