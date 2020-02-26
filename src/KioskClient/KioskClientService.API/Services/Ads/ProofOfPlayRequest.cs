using System;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Ads;

public class ProofOfPlayRequest : BaseRequest, IMessageScrub
{
	public DateTime DisplayedTime { get; set; }

	public int DisplayedMilliseconds { get; set; }

	public object Scrub()
	{
		return this.ToJsonIndented();
	}
}
