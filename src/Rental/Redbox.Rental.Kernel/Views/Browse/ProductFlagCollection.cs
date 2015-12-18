using System.Collections.Generic;

namespace Redbox.Rental.Kernel.Views.Browse;

public class ProductFlagCollection : List<ProductFlag>
{
	private static ProductFlagCollection _instance;

	public static ProductFlagCollection Instance
	{
		get
		{
			if (_instance == null || _instance.Count == 0)
			{
				Dictionary<string, object> dictionary = BundleFunctions.GetDictionary("strings");
				_instance = new ProductFlagCollection();
				AddProductFlag(dictionary, ProductFlags.AwardWinner, "image_flag_gold_small", "flag_award_winner");
				AddProductFlag(dictionary, ProductFlags.BackAgain, "image_flag_red_small", "flag_back_again");
				AddProductFlag(dictionary, ProductFlags.Classics, "image_flag_red_small", "flag_classics");
				AddProductFlag(dictionary, ProductFlags.ComingSoon, "image_flag_blue_small", "flag_coming_soon");
				AddProductFlag(dictionary, ProductFlags.LastChance, "image_flag_orange_small", "flag_last_chance");
				AddProductFlag(dictionary, ProductFlags.New, "image_flag_red_small", "flag_new");
				AddProductFlag(dictionary, ProductFlags.OutOfStock, "image_flag_gray_small", "flag_out_of_stock");
				AddProductFlag(dictionary, ProductFlags.PriceTest, "image_flag_red_small", "flag_price_test");
			}
			return _instance;
		}
	}

	private static void AddProductFlag(Dictionary<string, object> strings, ProductFlags flag, string imageName, string flagKeyName)
	{
		object value = null;
		if (strings.TryGetValue(flagKeyName, out value))
		{
			string text = value.ToString();
			if (!string.IsNullOrEmpty(text))
			{
				_instance.Add(new ProductFlag
				{
					Flag = flag,
					ImageName = imageName,
					Text = text
				});
			}
		}
	}
}
