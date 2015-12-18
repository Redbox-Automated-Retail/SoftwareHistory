namespace Redbox.Rental.Kernel.Views.Browse;

public class Banner
{
	public class ImageNameConstants
	{
		public const string Bluray = "image_banner_blu_ray";

		public const string ComingSoon = "image_banner_coming_soon_grey";

		public const string NintendoDS = "image_banner_nintendo_ds";

		public const string PS2 = "image_banner_ps2";

		public const string PS3 = "image_banner_ps3";

		public const string Wii = "image_banner_wii";

		public const string XBox = "image_banner_xbox";

		public const string Purchase = "image_banner_purchase";

		public const string MultipleDisc = "image_banner_purchase";
	}

	public class TextConstants
	{
		public const string Bluray = "Blu-ray";

		public const string ComingSoon = "Coming Soon";

		public const string NintendoDS = "Nintendo DS";

		public const string PS2 = "PS2";

		public const string PS3 = "PS3";

		public const string Wii = "Wii";

		public const string XBox = "XBox 360";

		public const string Purchase = "PURCHASE";
	}

	private BannerStyle _bannerStyle;

	public BannerType BannerType { get; set; }

	public BannerStyle BannerStyle
	{
		get
		{
			return _bannerStyle;
		}
		set
		{
			_bannerStyle = value;
		}
	}

	public string ImageName { get; set; }

	public string Text { get; set; }
}
