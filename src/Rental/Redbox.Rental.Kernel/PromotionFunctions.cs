using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
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

	[KernelFunction(Name = "kernel.rental_promotion_clear_promotion")]
	internal static void ClearPromotion()
	{
		PromotionService.Instance.ClearPromotion();
	}

	[KernelFunction(Name = "kernel.rental_promotion_rxgx_validate_cart")]
	internal static LuaTable RxGxValidateCart(LuaTable cartItems, decimal dvdPrice, decimal bluRayPrice, decimal gamePrice)
	{
		Promotion promotion = PromotionService.Instance.GetPromotion();
		if (promotion == null)
		{
			return null;
		}
		if (promotion.Intent != PromotionIntentCode.RxGx)
		{
			return null;
		}
		if (promotion.RentQuantity == 0 || !promotion.RentFormat.HasValue || promotion.GetQuantity == 0 || !promotion.GetFormat.HasValue)
		{
			return null;
		}
		try
		{
			int num = RxGxGetRequiredCount("Dvd", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value) - RxGxGetCartItemCount(cartItems, "Dvd", null, null);
			num = ((num > 0) ? num : 0);
			int num2 = RxGxGetRequiredCount("BluRay", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value) - RxGxGetCartItemCount(cartItems, "BluRay", null, null);
			num2 = ((num2 > 0) ? num2 : 0);
			int num3 = RxGxGetRequiredCount("Game", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value) - RxGxGetCartItemCount(cartItems, "Game", null, null);
			num3 = ((num3 > 0) ? num3 : 0);
			int num4 = RxGxGetRequiredCount("Any", promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value) - RxGxGetCartItemCount(cartItems, "Any", promotion.RentQuantity, promotion.RentFormat.ToString());
			num4 = ((num4 > 0) ? num4 : 0);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["is_cart_valid"] = num == 0 && num2 == 0 && num3 == 0 && num4 == 0;
			dictionary["dvds_remaining"] = num;
			dictionary["blu_rays_remaining"] = num2;
			dictionary["games_remaining"] = num3;
			dictionary["any_remaining"] = num4;
			dictionary["promo_code_value"] = RxGxGetPromoCodeValue((bool)dictionary["is_cart_valid"], promotion.GetQuantity, promotion.GetFormat.Value, cartItems, dvdPrice, bluRayPrice, gamePrice);
			Dictionary<string, string> dictionary2 = RxGxGetTitle(promotion.RentQuantity, promotion.RentFormat.Value, promotion.GetQuantity, promotion.GetFormat.Value);
			dictionary["title_english"] = dictionary2["english"];
			dictionary["title_spanish"] = dictionary2["spanish"];
			Dictionary<string, string> dictionary3 = RxGxGetMessage(num, num2, num3, num4);
			dictionary["message_english"] = dictionary3["english"];
			dictionary["message_spanish"] = dictionary3["spanish"];
			return dictionary.ToLuaTable();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("PromotionFunctions.RxGxValidateCart - an unhandled exception occurred.", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.rental_promotion_tsp_validate_cart")]
	internal static bool TspValidateCart(LuaTable cartItems)
	{
		Promotion promotion = PromotionService.Instance.GetPromotion();
		if (promotion == null)
		{
			return false;
		}
		if (promotion.CampaignTitles == null)
		{
			return false;
		}
		try
		{
			List<long> list = new List<long>();
			foreach (LuaTable value in cartItems.Values)
			{
				if (!(((string)value["action"]).ToLower() != "rental"))
				{
					list.Add(long.Parse(value["product_id"].ToString()));
				}
			}
			int num = list.Intersect(promotion.CampaignTitles.Titles).ToList().Count();
			if (promotion.CampaignTitles.Include && num > 0)
			{
				return true;
			}
			if (!promotion.CampaignTitles.Include && num != list.Count())
			{
				return true;
			}
			return false;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("PromotionFunctions.TspValidateCart - an unhandled exception occurred.", e);
			return false;
		}
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

	private static byte RxGxGetRequiredCount(string format, byte rentQuantity, PromotionRentFormat rentFormat, byte getQuantity, PromotionGetFormat getFormat)
	{
		byte b = 0;
		if (format == rentFormat.ToString())
		{
			b += rentQuantity;
		}
		if (format == getFormat.ToString())
		{
			b += getQuantity;
		}
		return b;
	}

	private static byte RxGxGetCartItemCount(LuaTable cartItems, string format, byte? excludeQuantity, string excludeFormat)
	{
		byte b = 0;
		byte b2 = 0;
		foreach (LuaTable value in cartItems.Values)
		{
			if (((string)value["action"]).ToLower() != "rental")
			{
				continue;
			}
			string text = (string)value["product_family"];
			text = ((text.ToLower() == "games") ? "Game" : text);
			string text2 = (string)value["product_type"];
			text2 = ((text2.ToLower() == "blu-ray") ? "BluRay" : text2);
			if (excludeQuantity.HasValue && excludeFormat != null)
			{
				byte num = b2;
				int? num2 = excludeQuantity;
				if (num < num2.GetValueOrDefault() && num2.HasValue && (excludeFormat.ToLower() == text.ToLower() || excludeFormat.ToLower() == text2.ToLower()))
				{
					b2++;
					continue;
				}
			}
			if (format.ToLower() == "any")
			{
				b++;
			}
			else if (format.ToLower() == text.ToLower() || format.ToLower() == text2.ToLower())
			{
				b++;
			}
		}
		return b;
	}

	private static decimal RxGxGetPromoCodeValue(bool isCartValid, byte getQuantity, PromotionGetFormat getFormat, LuaTable cartItems, decimal dvdPrice, decimal bluRayPrice, decimal gamePrice)
	{
		if (!isCartValid)
		{
			return 0m;
		}
		switch (getFormat)
		{
		case PromotionGetFormat.Dvd:
			return dvdPrice * (decimal)getQuantity;
		case PromotionGetFormat.BluRay:
			return bluRayPrice * (decimal)getQuantity;
		case PromotionGetFormat.Game:
			return gamePrice * (decimal)getQuantity;
		case PromotionGetFormat.Any:
		{
			List<decimal> list = new List<decimal>();
			foreach (LuaTable value in cartItems.Values)
			{
				if (!(((string)value["action"]).ToLower() != "rental"))
				{
					list.Add(decimal.Parse(value["price"].ToString()));
				}
			}
			list = list.OrderBy((decimal price) => price).ToList();
			decimal result = 0m;
			for (int num = 0; num < getQuantity; num++)
			{
				result += list[num];
			}
			return result;
		}
		default:
			return 0m;
		}
	}

	private static Dictionary<string, string> RxGxGetTitle(byte rentQuantity, PromotionRentFormat rentFormat, byte getQuantity, PromotionGetFormat getFormat)
	{
		string text = rentFormat.ToString();
		string text2 = rentFormat.ToString();
		string text3 = getFormat.ToString();
		string text4 = getFormat.ToString();
		if (rentFormat == PromotionRentFormat.Dvd)
		{
			text = ((rentQuantity > 1) ? "DVDs" : "DVD");
			text2 = ((rentQuantity > 1) ? "DVDs" : "DVD");
		}
		if (rentFormat == PromotionRentFormat.BluRay)
		{
			text = ((rentQuantity > 1) ? "Blu-rays" : "Blu-ray");
			text2 = ((rentQuantity > 1) ? "Blu-rays" : "Blu-ray");
		}
		if (rentFormat == PromotionRentFormat.Game)
		{
			text = ((rentQuantity > 1) ? "Games" : "Game");
			text2 = ((rentQuantity > 1) ? "Juegos" : "Juego");
		}
		if (getFormat == PromotionGetFormat.Dvd)
		{
			text3 = ((getQuantity > 1) ? "DVDs" : "DVD");
			text4 = ((getQuantity > 1) ? "DVDs" : "DVD");
		}
		if (getFormat == PromotionGetFormat.BluRay)
		{
			text3 = ((getQuantity > 1) ? "Blu-rays" : "Blu-ray");
			text4 = ((getQuantity > 1) ? "Blu-rays" : "Blu-ray");
		}
		if (getFormat == PromotionGetFormat.Game)
		{
			text3 = ((getQuantity > 1) ? "Games" : "Game");
			text4 = ((getQuantity > 1) ? "Juegos" : "Juego");
		}
		if (getFormat == PromotionGetFormat.Any)
		{
			text3 = "Any";
			text4 = "Alguna";
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["english"] = $"You have entered a promo code for Rent {rentQuantity} {text} Get {getQuantity} {text3}.";
		dictionary["spanish"] = $"Ha ingresado un código promocional para Rentar {rentQuantity} {text2} Recibe {getQuantity} {text4}.";
		return dictionary;
	}

	private static Dictionary<string, string> RxGxGetMessage(int dvdsRemaining, int bluRaysRemaining, int gamesRemaining, int anyRemaining)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["english"] = "";
		dictionary["spanish"] = "";
		if (dvdsRemaining > 0 || bluRaysRemaining > 0 || gamesRemaining > 0 || anyRemaining > 0)
		{
			string text = "";
			string text2 = "";
			int num = 0;
			if (dvdsRemaining > 0)
			{
				text += string.Format("{0} DVD{1}", dvdsRemaining, (dvdsRemaining > 1) ? "s" : "");
				text2 += string.Format("{0} DVD{1}", dvdsRemaining, (dvdsRemaining > 1) ? "s" : "");
				num++;
			}
			if (bluRaysRemaining > 0)
			{
				text += string.Format("{0}{1} Blu-ray{2}", (num > 0) ? " and " : "", bluRaysRemaining, (bluRaysRemaining > 1) ? "s" : "");
				text2 += string.Format("{0}{1} Blu-ray{2}", (num > 0) ? " y " : "", bluRaysRemaining, (bluRaysRemaining > 1) ? "s" : "");
				num++;
			}
			if (gamesRemaining > 0)
			{
				text += string.Format("{0}{1} Game{2}", (num > 0) ? " and " : "", gamesRemaining, (gamesRemaining > 1) ? "s" : "");
				text2 += string.Format("{0}{1} Juego{2}", (num > 0) ? " y " : "", gamesRemaining, (gamesRemaining > 1) ? "s" : "");
				num++;
			}
			if (anyRemaining > 0)
			{
				text += string.Format("{0}{1} other item{2}", (num > 0) ? " and " : "", anyRemaining, (anyRemaining > 1) ? "s" : "");
				text2 += string.Format("{0}{1} otro artículo{2}", (num > 0) ? " y " : "", anyRemaining, (anyRemaining > 1) ? "s" : "");
				num++;
			}
			dictionary["english"] = $"In addition to your cart, please add at least {text} to use this promo code.";
			dictionary["spanish"] = $"Además de su compra, por favor ingrese al menos {text2} para utilizar el código promocional.";
		}
		return dictionary;
	}
}
