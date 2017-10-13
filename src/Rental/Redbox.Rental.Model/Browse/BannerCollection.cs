using System.Collections.Generic;

namespace Redbox.Rental.Model.Browse;

public class BannerCollection : List<Banner>
{
	private static BannerCollection _bannerCollection;

	public static BannerCollection Instance
	{
		get
		{
			if (_bannerCollection == null)
			{
				_bannerCollection = new BannerCollection
				{
					new Banner
					{
						BannerType = BannerType.Bluray,
						BannerStyleName = "banner_bluray_background",
						TextKey = "blu_ray"
					},
					new Banner
					{
						BannerType = BannerType.ComingSoon,
						BannerStyleName = "banner_coming_soon_background",
						TextKey = "coming_label",
						BannerSize = BannerSize.Large
					},
					new Banner
					{
						BannerType = BannerType.Purchase,
						BannerStyleName = "banner_purchase_background",
						TextKey = "mini_cart_purchase_text",
						BannerSize = BannerSize.Mini
					},
					new Banner
					{
						BannerType = BannerType.MultipleDisc,
						BannerStyleName = "banner_multiple_disc_background",
						BannerSize = BannerSize.Mini
					},
					new Banner
					{
						BannerType = BannerType.DigitalCode,
						BannerStyleName = "banner_digital_code_background",
						TextKey = "digital_code_banner"
					}
				};
			}
			return _bannerCollection;
		}
	}
}
