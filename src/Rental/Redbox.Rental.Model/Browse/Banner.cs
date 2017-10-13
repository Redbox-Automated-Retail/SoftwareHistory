namespace Redbox.Rental.Model.Browse;

public class Banner
{
	public class BannerStyleNameConstants
	{
		public const string Bluray = "banner_bluray_background";

		public const string ComingSoon = "banner_coming_soon_background";

		public const string Purchase = "banner_purchase_background";

		public const string MultipleDisc = "banner_multiple_disc_background";

		public const string DigitalCode = "banner_digital_code_background";
	}

	public class TextKeyConstants
	{
		public const string Bluray = "blu_ray";

		public const string ComingSoon = "coming_label";

		public const string Purchase = "mini_cart_purchase_text";

		public const string DigitalCode = "digital_code_banner";
	}

	public BannerType BannerType { get; set; }

	public BannerSize BannerSize { get; set; }

	public string BannerStyleName { get; set; }

	public string TextKey { get; set; }
}
