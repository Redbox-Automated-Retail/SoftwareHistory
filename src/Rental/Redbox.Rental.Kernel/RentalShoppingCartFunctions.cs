using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Kernel;

public static class RentalShoppingCartFunctions
{
	[KernelFunction(Name = "kernel.abandon_rental_shopping_cart")]
	public static void AbandonRentalShoppingCart(string id, string reason)
	{
		Guid id2 = ((id != null) ? new Guid(id) : Guid.NewGuid());
		(ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(id2))?.Abandon(reason);
	}

	[KernelFunction(Name = "kernel.create_rental_shopping_cart")]
	public static string CreateRentalShoppingCart(string id)
	{
		Guid id2 = ((id != null) ? new Guid(id) : Guid.NewGuid());
		ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CreateNew(id2);
		return id2.ToString();
	}

	private static void AssignPriceValuesFromLuaTable(IPricingRecord pricingRecord, LuaTable luaTable)
	{
		if (GetDecimalValue(luaTable, "initial_night", out var value))
		{
			pricingRecord.InitialNight = value;
		}
		if (GetDecimalValue(luaTable, "extra_night", out value))
		{
			pricingRecord.ExtraNight = value;
		}
		if (GetDecimalValue(luaTable, "non_return", out value))
		{
			pricingRecord.NonReturn = value;
		}
		if (GetDecimalValue(luaTable, "non_return_days", out value))
		{
			pricingRecord.NonReturnDays = value;
		}
		if (GetDecimalValue(luaTable, "expiration_price", out value))
		{
			pricingRecord.ExpirationPrice = value;
		}
	}

	private static bool GetDecimalValue(LuaTable luaTable, string propertyName, out decimal value)
	{
		bool result = false;
		value = 0m;
		if (luaTable != null)
		{
			object obj = luaTable[propertyName];
			if (obj != null && decimal.TryParse(obj.ToString(), out var result2))
			{
				value = result2;
				result = true;
			}
		}
		return result;
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_clear")]
	public static void Clear(string sessionId)
	{
		ProcessRentalShoppingCartAction(sessionId, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			rentalShoppingCart?.Clear();
		});
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_update_item_barcode")]
	public static void UpdateItemBarcode(string sessionId, string indexValue, string barcode)
	{
		ProcessRentalShoppingCartItemAction(sessionId, indexValue, delegate(IRentalShoppingCartItem rentalShoppingCartItem)
		{
			rentalShoppingCartItem.Barcode = barcode;
		});
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_update_item_status")]
	public static void UpdateItemStatus(string sessionId, string indexValue, string statusValue)
	{
		ProcessRentalShoppingCartItemAction(sessionId, indexValue, delegate(IRentalShoppingCartItem rentalShoppingCartItem)
		{
			if (Enum.TryParse<LineItemStatus>(statusValue, out var result))
			{
				rentalShoppingCartItem.Status = result;
			}
			else
			{
				LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.UpdateItemStatus.  Unable to parse status value {0}", statusValue);
			}
		});
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_assign_barcodes_to_vend")]
	public static LuaTable AssignBarcodesToVend(string sessionId)
	{
		List<string> barcodeList = null;
		ProcessRentalShoppingCartAction(sessionId, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			barcodeList = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.AssignBarcodesToVend(rentalShoppingCart);
		});
		return barcodeList?.ToLuaTable();
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_update_in_cart_offer_made")]
	public static void UpdateInCartPromoOfferMade(string sessionId, string value)
	{
		ProcessRentalShoppingCartAction(sessionId, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			bool result = false;
			bool.TryParse(value, out result);
			rentalShoppingCart.InCartPromoOfferMade = result;
		});
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_update_in_cart_do_not_show")]
	public static void UpdateInCartDoNowShow(string sessionId, string value)
	{
		ProcessRentalShoppingCartAction(sessionId, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			bool result = false;
			bool.TryParse(value, out result);
			rentalShoppingCart.InCartDoNotShow = result;
		});
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_update_in_cart_title_marketing")]
	public static void UpdateInCartTitleMarketing(string sessionId, string value)
	{
		ProcessRentalShoppingCartAction(sessionId, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			bool result = false;
			bool.TryParse(value, out result);
			rentalShoppingCart.InCartTitleMarketing = result;
		});
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_get_in_cart_title_marketing")]
	public static bool GetInCartTitleMarketing(string sessionId)
	{
		return GetValueFromRentalShoppingCartFunction(sessionId, (IRentalShoppingCart rentalShoppingCart) => rentalShoppingCart.InCartTitleMarketing);
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_update_in_cart_offer_cancel")]
	public static void UpdateInCartPromoOfferCancel(string sessionId, string value)
	{
		ProcessRentalShoppingCartAction(sessionId, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			bool result = false;
			bool.TryParse(value, out result);
			rentalShoppingCart.InCartPromoOfferCancel = result;
		});
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_get_in_cart_offer_cancel")]
	public static bool GetInCartPromoOfferCancel(string sessionId)
	{
		return GetValueFromRentalShoppingCartFunction(sessionId, (IRentalShoppingCart rentalShoppingCart) => rentalShoppingCart.InCartPromoOfferCancel);
	}

	private static void ProcessRentalShoppingCartAction(string sessionId, Action<IRentalShoppingCart> rentalShoppingCartAction)
	{
		Guid id = new Guid(sessionId);
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(id);
		if (rentalShoppingCart != null)
		{
			if (rentalShoppingCartAction != null)
			{
				rentalShoppingCartAction(rentalShoppingCart);
				return;
			}
			LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.ProcessRentalShoppingCartAction.  Unable to find rental shopping cart with id {0}", sessionId);
		}
	}

	private static T GetValueFromRentalShoppingCartFunction<T>(string sessionId, Func<IRentalShoppingCart, T> rentalShoppingCartFunction)
	{
		Guid id = new Guid(sessionId);
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(id);
		if (rentalShoppingCart != null)
		{
			if (rentalShoppingCartFunction != null)
			{
				return rentalShoppingCartFunction(rentalShoppingCart);
			}
			LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.ProcessRentalShoppingCartAction.  Unable to find rental shopping cart with id {0}", sessionId);
		}
		return default(T);
	}

	private static void ProcessRentalShoppingCartItemAction(string sessionId, string indexValue, Action<IRentalShoppingCartItem> updateItemAction)
	{
		int result = 0;
		if (int.TryParse(indexValue, out result))
		{
			result--;
			Guid id = new Guid(sessionId);
			IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(id);
			if (rentalShoppingCart != null)
			{
				IRentalShoppingCartItem item = rentalShoppingCart.GetItem(result);
				if (item != null)
				{
					updateItemAction?.Invoke(item);
					return;
				}
				LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.UpdateItem.  Unable to get item with index value {0}", result);
			}
		}
		else
		{
			LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.UpdateItem.  Unable to parse index value {0}", indexValue);
		}
	}

	[KernelFunction(Name = "kernel.current_rental_shopping_cart_coming_soon_optin_set_email_address")]
	public static void SetEmailAddress(string emailAddress)
	{
		IComingSoonOptinInfo comingSoonOptinInfo = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart?.ComingSoonOptInInfo;
		if (comingSoonOptinInfo != null)
		{
			comingSoonOptinInfo.EmailAddress = emailAddress;
		}
	}

	[KernelFunction(Name = "kernel.current_rental_shopping_cart_coming_soon_optin_add_product_reminder")]
	public static void AddProductReminder(string productId)
	{
		IComingSoonOptinInfo comingSoonOptinInfo = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart?.ComingSoonOptInInfo;
		if (comingSoonOptinInfo != null)
		{
			long result = 0L;
			if (long.TryParse(productId, out result))
			{
				comingSoonOptinInfo.AddProduct(result);
			}
		}
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_get_pickup_recommendations_shown")]
	public static bool GetPickupRecommendationsShown(string id)
	{
		bool result = false;
		ProcessRentalShoppingCartAction(id, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			result = rentalShoppingCart.PickupRecommendationsShown;
		});
		return result;
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_add_multi_disc_discs")]
	public static void AddMultiDiscDiscs(string sessionId)
	{
		if (sessionId != null)
		{
			Guid shoppingCartSessionId = new Guid(sessionId);
			ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.AddMultiDiscDiscsToRentalShoppingCart(shoppingCartSessionId);
		}
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_remove_non_reserved_products")]
	public static void RemoveNonReservedProductsFromRentalShoppingCart(string sessionId)
	{
		if (sessionId != null)
		{
			Guid shoppingCartSessionId = new Guid(sessionId);
			ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.RemoveNonReservedProductsFromRentalShoppingCart(shoppingCartSessionId);
		}
	}

	[KernelFunction(Name = "kernel.current_rental_shopping_cart_has_item")]
	public static bool HasItem(string productId)
	{
		bool result = false;
		long result2 = 0L;
		if (long.TryParse(productId, out result2))
		{
			IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			if (service != null && service.CurrentRentalShoppingCart != null)
			{
				result = service.CurrentRentalShoppingCart.HasItem(result2);
			}
		}
		return result;
	}

	[KernelFunction(Name = "kernel.current_rental_shopping_cart_is_full")]
	public static bool IsFull()
	{
		bool result = false;
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		if (service != null)
		{
			result = service.CurrentRentalShoppingCartIsFull;
		}
		return result;
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_is_full")]
	public static bool IsFull(string sessionId)
	{
		bool result = false;
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		if (service != null && sessionId != null)
		{
			result = (service?.GetRentalShoppingCart(new Guid(sessionId)))?.IsFull ?? false;
		}
		return result;
	}

	[KernelFunction(Name = "kernel.current_rental_shopping_cart_item_count")]
	public static int ItemCount()
	{
		int result = 0;
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		if (service != null)
		{
			result = service.CurrentRentalShoppingCart.Count;
		}
		return result;
	}

	[KernelFunction(Name = "kernel.get_rental_shopping_cart_item")]
	public static LuaTable GetRentalShoppingCartItem(string id, int index)
	{
		index--;
		LuaTable result = null;
		Guid id2 = new Guid(id);
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(id2);
		if (rentalShoppingCart != null && rentalShoppingCart.Count > index)
		{
			IRentalShoppingCartItem item = rentalShoppingCart.GetItem(index);
			result = CreateLuaTableFromRentalShoppingCartItem(rentalShoppingCart, item).ToLuaTable();
		}
		return result;
	}

	[KernelFunction(Name = "kernel.get_rental_shopping_cart_item_index_by_barcode")]
	public static int GetRentalShoppingCartItemIndexByBarcode(string id, string barcode)
	{
		int result = 0;
		ProcessRentalShoppingCartAction(id, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
			{
				if (item.Barcode == barcode)
				{
					result = rentalShoppingCart.Items.IndexOf(item) + 1;
				}
			}
		});
		return result;
	}

	[KernelFunction(Name = "kernel.get_rental_shopping_marketing_opt_in_confirmation_status")]
	public static string GetMarketingOptInConfirmationStatus(string id)
	{
		OptInConfirmationStatus result = OptInConfirmationStatus.DoNotShow;
		ProcessRentalShoppingCartAction(id, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			result = rentalShoppingCart.MarketingOptInConfirmationStatus;
		});
		return result.ToString();
	}

	[KernelFunction(Name = "kernel.set_rental_shopping_marketing_opt_in_confirmation_status")]
	public static void SetMarketingOptInConfirmationStatus(string id, string value)
	{
		OptInConfirmationStatus newMarketingOptInConfirmationStatus = OptInConfirmationStatus.DoNotShow;
		if (Enum.TryParse<OptInConfirmationStatus>(value, ignoreCase: true, out newMarketingOptInConfirmationStatus))
		{
			ProcessRentalShoppingCartAction(id, delegate(IRentalShoppingCart rentalShoppingCart)
			{
				rentalShoppingCart.MarketingOptInConfirmationStatus = newMarketingOptInConfirmationStatus;
			});
		}
	}

	[KernelFunction(Name = "kernel.get_rental_shopping_cart_items")]
	public static LuaTable GetRentalShoppingCartItems(string id)
	{
		LuaTable result = null;
		Guid id2 = new Guid(id);
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(id2);
		if (rentalShoppingCart != null)
		{
			List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
			foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
			{
				list.Add(CreateLuaTableFromRentalShoppingCartItem(rentalShoppingCart, item));
			}
			result = list.ToLuaTable();
		}
		return result;
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_clear_promo_code")]
	public static void ClearPromoCode(string sessionId)
	{
		ProcessRentalShoppingCartAction(sessionId, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			rentalShoppingCart.ClearPromoCode();
		});
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_send_cart_to_analytics")]
	public static void SendRentalShoppingCartToAnalytics(string sessionId)
	{
		IRentalShoppingCartService rentalShoppingCartService = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		ProcessRentalShoppingCartAction(sessionId, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			rentalShoppingCartService?.SendRentalShoppingCartToAnalytics(rentalShoppingCart);
		});
	}

	[KernelFunction(Name = "kernel.show_check_out_keyboard_view")]
	public static void showCheckOutKeyboardView(string emailAddress, Action<string> LuaContinueCallback, Action LuaCancelCallback)
	{
		object lockObject = new object();
		bool hasActionExecuted = false;
		OrchestrationService.ViewShow("keyboard_email_receipt_view", new KeyboardInfo
		{
			MaxLength = 64,
			KeyboardText = (string.IsNullOrEmpty(emailAddress) ? "" : emailAddress),
			IsValidEntry = ValidationServices.IsValidEmailAddress,
			CancelAction = delegate
			{
				LockHelper.TryLockObject(lockObject, delegate
				{
					if (!hasActionExecuted)
					{
						hasActionExecuted = true;
						if (LuaCancelCallback != null)
						{
							LuaCancelCallback();
						}
					}
				}, "showCheckOutKeyboardView cancel action");
			},
			ContinueAction = delegate(string keyboardText)
			{
				LockHelper.TryLockObject(lockObject, delegate
				{
					if (!hasActionExecuted)
					{
						hasActionExecuted = true;
						if (LuaContinueCallback != null)
						{
							LuaContinueCallback(keyboardText);
						}
					}
				}, "showCheckOutKeyboardView continue action");
			}
		});
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_service_fee_enabled")]
	public static bool ServiceFeeEnabled(string sessionId)
	{
		bool result = false;
		ProcessRentalShoppingCartAction(sessionId, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			result = rentalShoppingCart.ServiceFeeEnabled;
		});
		return result;
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_default_service_fee_cents")]
	public static int DefaultServiceFeeCents(string sessionId)
	{
		int result = 0;
		ProcessRentalShoppingCartAction(sessionId, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			result = (int)(rentalShoppingCart.DefaultServiceFee * 100m);
		});
		return result;
	}

	private static Dictionary<string, object> CreateLuaTableFromRentalShoppingCartItem(IRentalShoppingCart rentalShoppingCart, IRentalShoppingCartItem rentalShoppingCartItem)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (rentalShoppingCartItem != null)
		{
			dictionary["product_id"] = rentalShoppingCartItem.ProductId;
			dictionary["action"] = rentalShoppingCartItem.Action.ToString();
			dictionary["barcode"] = rentalShoppingCartItem.Barcode;
			dictionary["price"] = rentalShoppingCartItem.Price;
			dictionary["product_type"] = ((rentalShoppingCartItem.TitleType == TitleType.Bluray) ? "Blu-ray" : rentalShoppingCartItem.TitleType.ToString());
			dictionary["product_type_id"] = (int)rentalShoppingCartItem.TitleType;
			dictionary["product_family"] = rentalShoppingCartItem.TitleFamily.ToString();
			dictionary["rating_id"] = (int)rentalShoppingCartItem.Rating;
			dictionary["merchandise_date"] = rentalShoppingCartItem.MerchandiseDate.ToString("yyyyMMddHHmmss");
			dictionary["sort_name"] = rentalShoppingCartItem.SortName;
			dictionary["add_sequence"] = rentalShoppingCartItem.Sequence.ToString();
			dictionary["status"] = rentalShoppingCartItem.Status.ToString();
			dictionary["quantity"] = rentalShoppingCartItem.Quantity;
			dictionary["source_type"] = rentalShoppingCartItem.SourceType.ToString();
			IUpsellItem upsellItem = rentalShoppingCart?.UpsellInfo?.Items.FirstOrDefault((IUpsellItem x) => x.OriginalProductId == rentalShoppingCartItem.ProductId || x.UpsellProductId == rentalShoppingCartItem.ProductId);
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			if (upsellItem != null)
			{
				dictionary2.Add("product_group_id", upsellItem.ProductGroupId);
				dictionary2.Add("offer", upsellItem.OfferResponse.ToString());
				dictionary2.Add("title_id", rentalShoppingCartItem.ProductId);
			}
			dictionary["blu_ray_upsell"] = dictionary2.ToLuaTable();
			dictionary["multi_disc"] = rentalShoppingCartItem.MultiDisc.ToString();
			dictionary["pricing_policy"] = ((rentalShoppingCartItem.PricingRecord != null) ? rentalShoppingCartItem.PricingRecord.ToDictionary().ToLuaTable() : new Dictionary<string, object>().ToLuaTable());
		}
		return dictionary;
	}

	private static string GetLuaTableStringValue(LuaTable luaTable, string propertyName)
	{
		string result = null;
		if (luaTable != null && luaTable[propertyName] != null)
		{
			result = luaTable[propertyName].ToString();
		}
		return result;
	}
}
