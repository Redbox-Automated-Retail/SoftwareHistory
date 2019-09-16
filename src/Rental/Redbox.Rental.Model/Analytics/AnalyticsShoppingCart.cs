using System.Collections.Generic;
using System.Linq;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.Analytics;

public class AnalyticsShoppingCart
{
	public decimal SubTotal { get; set; }

	public decimal TaxAmount { get; set; }

	public decimal GrandTotal { get; set; }

	public List<AnalyticsShoppingCartItem> Items { get; } = new List<AnalyticsShoppingCartItem>();

	public List<AnalyticsDiscount> Discounts { get; } = new List<AnalyticsDiscount>();

	public static AnalyticsShoppingCart ToShoppingCart(IRentalShoppingCart sc)
	{
		AnalyticsShoppingCart cart = new AnalyticsShoppingCart
		{
			SubTotal = ((sc.Totals == null) ? 0m : sc.Totals.SubTotal),
			TaxAmount = ((sc.Totals == null) ? 0m : sc.Totals.TaxAmount),
			GrandTotal = ((sc.Totals == null) ? 0m : sc.Totals.GrandTotal)
		};
		(sc?.Items?.Where((IRentalShoppingCartItem x) => x?.LoyaltyPointsRecord?.ToBeRedeemed ?? false))?.ToList().ForEach(delegate(IRentalShoppingCartItem x)
		{
			cart.Discounts.Add(new AnalyticsDiscount
			{
				ProductId = x.ProductId,
				DiscountType = DiscountType.Loyalty,
				RedemptionPoints = x.LoyaltyPointsRecord.PointsToRedeem,
				Amount = (x.PricingRecord?.InitialNight ?? 0m)
			});
		});
		if (sc.Promotion != null)
		{
			sc.Promotion.PromotionItems?.ToList().ForEach(delegate(IPromotionItem x)
			{
				cart.Discounts.Add(new AnalyticsDiscount
				{
					Amount = x.PromotionAmount,
					DiscountType = DiscountType.PromotionCode,
					PromotionCode = sc.Promotion.Name,
					PromotionCodeValue = sc.Promotion.OriginalPrice,
					PromotionIntentCode = sc.Promotion.Intent,
					ProductId = x.RentalShoppingCartItem.ProductId,
					ApplyOnlyToProduct = (sc.Promotion.IsValidatedPostVend ? new bool?(sc.Promotion.IsProductSpecific) : ((bool?)null))
				});
			});
		}
		sc.Items.ToList().ForEach(delegate(IRentalShoppingCartItem item)
		{
			cart.Items.Add(AnalyticsShoppingCartItem.ToShoppingCartItem(item));
		});
		return cart;
	}
}
