using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Services;

namespace Redbox.Rental.Kernel;

public static class PromotionFunctions
{
	[KernelFunction(Name = "kernel.rental_promotion_get_promotion")]
	internal static LuaTable GetPromotion()
	{
		IPromotion promotion = PromotionService.Instance.GetPromotion();
		if (promotion == null)
		{
			return null;
		}
		return GetPromotionAsLuaFormat(promotion).ToLuaTable();
	}

	[KernelFunction(Name = "kernel.get_in_cart_promo_code")]
	internal static string GetInCartPromoCode()
	{
		return PromotionService.Instance.GetInCartPromoCode();
	}

	[KernelFunction(Name = "kernel.rental_promotion_free_in_cart_template_is_running")]
	internal static bool IsFreeInCartOfferRunning()
	{
		return PromotionService.Instance.IsFreeInCartOfferRunning;
	}

	[KernelFunction(Name = "kernel.rental_promotion_rent_two_in_cart_template_is_running")]
	internal static bool IsRentTwoInCartOfferRunning()
	{
		return PromotionService.Instance.IsRentTwoInCartOfferRunning;
	}

	[KernelFunction(Name = "kernel.rental_promotion_load_incart_data")]
	internal static void LoadInCartData()
	{
		PromotionService.Instance.LoadInCartData();
	}

	[KernelFunction(Name = "kernel.rental_promotion_clear_promotion")]
	internal static void ClearPromotion()
	{
		PromotionService.Instance.ClearPromotion();
	}

	private static Dictionary<string, object> GetPromotionAsLuaFormat(IPromotion promotion)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["name"] = promotion.Name;
		dictionary["discount_type"] = promotion.DiscountType.ToString();
		dictionary["original_price"] = promotion.OriginalPrice;
		dictionary["price"] = promotion.Price;
		dictionary["quantity"] = promotion.Quantity;
		dictionary["intent"] = promotion.Intent.ToString();
		dictionary["original_percent"] = promotion.OriginalPercent;
		dictionary["percent"] = promotion.Percent;
		dictionary["rent_qty"] = promotion.RentQuantity;
		dictionary["rent_format"] = (promotion.RentFormat.HasValue ? promotion.RentFormat.ToString() : null);
		dictionary["get_qty"] = promotion.GetQuantity;
		dictionary["get_format"] = (promotion.GetFormat.HasValue ? promotion.GetFormat.ToString() : null);
		dictionary["campaign_titles"] = null;
		if (promotion.CampaignTitles != null)
		{
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			dictionary2["include"] = promotion.CampaignTitles.Include;
			dictionary2["titles"] = promotion.CampaignTitles.Titles;
			dictionary["campaign_titles"] = dictionary2;
		}
		dictionary["product_type_id"] = promotion.ProductTypeId;
		dictionary["format_ids"] = promotion.FormatIds;
		return dictionary;
	}
}
