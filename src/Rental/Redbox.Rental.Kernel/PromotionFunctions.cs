using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services;

namespace Redbox.Rental.Kernel;

public static class PromotionFunctions
{
	[KernelFunction(Name = "kernel.rental_promotion_get_promotion")]
	internal static LuaTable GetPromotion()
	{
		Promotion promotion = PromotionService.Instance.GetPromotion();
		if (promotion == null)
		{
			return null;
		}
		return GetPromotionAsLuaFormat(promotion).ToLuaTable();
	}

	[KernelFunction(Name = "kernel.rental_promotion_set_promotion")]
	internal static void SetPromotion(LuaTable promotion)
	{
		Promotion luaTableAsPromotion = GetLuaTableAsPromotion(promotion);
		if (luaTableAsPromotion != null)
		{
			PromotionService.Instance.SetPromotion(luaTableAsPromotion);
		}
	}

	[KernelFunction(Name = "kernel.rental_promotion_free_in_cart_template_is_running")]
	internal static bool IsFreeInCartOfferRunning()
	{
		return PromotionService.Instance.IsFreeInCartOfferRunning;
	}

	[KernelFunction(Name = "kernel.rental_promotion_is_free_in_cart_valid")]
	internal static bool IsFreeInCartOfferValid()
	{
		return PromotionService.Instance.IsFreeInCartOfferValid;
	}

	[KernelFunction(Name = "kernel.rental_promotion_rent_two_in_cart_template_is_running")]
	internal static bool IsRentTwoInCartOfferRunning()
	{
		return PromotionService.Instance.IsRentTwoInCartOfferRunning;
	}

	[KernelFunction(Name = "kernel.rental_promotion_rent_two_in_cart_item_count")]
	internal static int GetRentTwoInCartOfferItemCount()
	{
		return PromotionService.Instance.GetRentTwoInCartOfferItemCount();
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

	[KernelFunction(Name = "kernel.rental_promotion_cart_satisfies_in_cart_offer")]
	internal static bool CartSatisfiesInCartOffer(string id)
	{
		Guid id2 = new Guid(id);
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(id2);
		return PromotionService.Instance.CartSatisfiesInCartOffer(rentalShoppingCart);
	}

	private static Dictionary<string, object> GetPromotionAsLuaFormat(Promotion promotion)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["name"] = promotion.Name;
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
		return dictionary;
	}

	private static Promotion GetLuaTableAsPromotion(LuaTable promotion)
	{
		try
		{
			Promotion promotion2 = new Promotion();
			promotion2.Name = (string)promotion["name"];
			promotion2.OriginalPrice = decimal.Parse(promotion["original_price"].ToString());
			promotion2.Price = decimal.Parse(promotion["price"].ToString());
			promotion2.Quantity = int.Parse(promotion["quantity"].ToString());
			promotion2.Intent = Enum<PromotionIntentCode>.Parse((string)promotion["intent"]);
			promotion2.OriginalPercent = decimal.Parse(promotion["original_percent"].ToString());
			promotion2.Percent = decimal.Parse(promotion["percent"].ToString());
			promotion2.RentQuantity = byte.Parse(promotion["rent_qty"].ToString());
			promotion2.RentFormat = null;
			if (promotion["rent_format"] != null)
			{
				promotion2.RentFormat = Enum<PromotionRentFormat>.Parse((string)promotion["rent_format"]);
			}
			promotion2.GetQuantity = byte.Parse(promotion["get_qty"].ToString());
			promotion2.GetFormat = null;
			if (promotion["get_format"] != null)
			{
				promotion2.GetFormat = Enum<PromotionGetFormat>.Parse((string)promotion["get_format"]);
			}
			promotion2.CampaignTitles = null;
			if (promotion["campaign_titles"] != null)
			{
				LuaTable luaTable = (LuaTable)promotion["campaign_titles"];
				LuaTable obj = (LuaTable)luaTable["titles"];
				List<long> list = new List<long>();
				foreach (object value in obj.Values)
				{
					list.Add(long.Parse(value.ToString()));
				}
				promotion2.CampaignTitles = new CampaignTitles();
				promotion2.CampaignTitles.Include = (bool)luaTable["include"];
				promotion2.CampaignTitles.Titles = list;
			}
			return promotion2;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("PromotionFunctions.GetLuaTableAsPromotion - an unhandled exception occurred.", e);
			return null;
		}
	}
}
