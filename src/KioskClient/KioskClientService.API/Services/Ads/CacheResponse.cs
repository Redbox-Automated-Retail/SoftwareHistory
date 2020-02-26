using KioskClientService.API.App;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Ads;

public class CacheResponse : BaseResponse, IMessageScrub
{
	public int AssetCacheCount { get; set; }

	public CacheResponse()
		: base(200)
	{
		base.Success = true;
	}

	public object Scrub()
	{
		return JsonConvert.SerializeObject(this);
	}
}
