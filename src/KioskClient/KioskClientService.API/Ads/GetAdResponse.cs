using KioskClientService.API.App;
using KioskClientService.API.Services.Ads;

namespace KioskClientService.API.Ads;

public class GetAdResponse : BaseResponse, IMessageScrub
{
	public string AdvertisementId { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public string MimeType { get; set; }

	public string AssetPath { get; set; }

	public AdLocation AdLocation { get; set; }

	public GetAdResponse()
		: base(200)
	{
		base.Success = true;
	}

	public object Scrub()
	{
		return this.ToJsonIndented();
	}

	public GetAdResponse CopyFrom(Advertisement ad)
	{
		AdvertisementId = ad.Id;
		Width = ad.Width;
		Height = ad.Height;
		MimeType = ad.MimeType;
		return this;
	}

	public GetAdResponse CopyFrom(CachedAdvertisement ca)
	{
		AdLocation = ca.AdLocation;
		AssetPath = ca.AssetPath;
		CopyFrom(ca.Advertisement);
		return this;
	}
}
