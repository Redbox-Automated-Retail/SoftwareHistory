using System.Collections.Generic;

namespace Redbox.Rental.Kernel.Views.Browse;

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
						ImageName = "image_banner_blu_ray",
						Text = "Blu-ray"
					},
					new Banner
					{
						BannerType = BannerType.NintendoDS,
						ImageName = "image_banner_nintendo_ds",
						Text = "Nintendo DS"
					},
					new Banner
					{
						BannerType = BannerType.PS2,
						ImageName = "image_banner_ps2",
						Text = "PS2"
					},
					new Banner
					{
						BannerType = BannerType.PS3,
						ImageName = "image_banner_ps3",
						Text = "PS3"
					},
					new Banner
					{
						BannerType = BannerType.Wii,
						ImageName = "image_banner_wii",
						Text = "Wii"
					},
					new Banner
					{
						BannerType = BannerType.Xbox360,
						ImageName = "image_banner_xbox",
						Text = "XBox 360"
					},
					new Banner
					{
						BannerType = BannerType.ComingSoon,
						ImageName = "image_banner_coming_soon_grey",
						Text = "Coming Soon"
					},
					new Banner
					{
						BannerType = BannerType.Purchase,
						ImageName = "image_banner_purchase",
						Text = "PURCHASE",
						BannerStyle = BannerStyle.Mini
					},
					new Banner
					{
						BannerType = BannerType.MultipleDisc,
						ImageName = "image_banner_purchase",
						BannerStyle = BannerStyle.Mini
					}
				};
			}
			return _bannerCollection;
		}
	}
}
