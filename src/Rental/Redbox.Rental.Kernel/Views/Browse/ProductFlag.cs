namespace Redbox.Rental.Kernel.Views.Browse;

public class ProductFlag
{
	public class ImageNameConstants
	{
		public const string GoldFlagImage = "image_flag_gold_small";

		public const string GrayFlagImage = "image_flag_gray_small";

		public const string RedFlagImage = "image_flag_red_small";

		public const string BlueFlagImage = "image_flag_blue_small";

		public const string OrangeFlagImage = "image_flag_orange_small";
	}

	public class FlagKeyNameConstants
	{
		public const string OutOfStockKey = "flag_out_of_stock";

		public const string ComingSoonKey = "flag_coming_soon";

		public const string NewKey = "flag_new";

		public const string LastChanceKey = "flag_last_chance";

		public const string BackAgainKey = "flag_back_again";

		public const string PriceTestKey = "flag_price_test";

		public const string AwardWinnerKey = "flag_award_winner";

		public const string ClassicsKey = "flag_classics";
	}

	public ProductFlags Flag { get; set; }

	public string ImageName { get; set; }

	public string Text { get; set; }
}
