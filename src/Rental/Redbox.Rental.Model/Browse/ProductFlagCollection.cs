using System.Collections.Generic;

namespace Redbox.Rental.Model.Browse;

public class ProductFlagCollection : List<ProductFlag>
{
	private static ProductFlagCollection _instance;

	public static ProductFlagCollection Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new ProductFlagCollection
				{
					new ProductFlag
					{
						Flag = ProductFlags.AwardWinner,
						TextKeyName = "flag_award_winner",
						FlagStyleName = "gold_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.BackAgain,
						TextKeyName = "flag_back_again",
						FlagStyleName = "red_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.Classics,
						TextKeyName = "flag_classics",
						FlagStyleName = "red_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.ComingSoon,
						TextKeyName = "flag_coming_soon",
						FlagStyleName = "blue_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.LastChance,
						TextKeyName = "flag_last_chance",
						FlagStyleName = "orange_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.New,
						TextKeyName = "flag_new",
						FlagStyleName = "red_flag_style"
					},
					new ProductFlag
					{
						Flag = ProductFlags.OutOfStock,
						TextKeyName = "flag_out_of_stock",
						FlagStyleName = "gray_flag_style"
					}
				};
			}
			return _instance;
		}
	}
}
