using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services;
using Redbox.Rental.Services.Pricing;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Kernel;

public static class RentalShoppingCartFunctions
{
	[KernelFunction(Name = "kernel.create_rental_shopping_cart")]
	public static string CreateRentalShoppingCart(string id)
	{
		Guid id2 = ((id != null) ? new Guid(id) : Guid.NewGuid());
		ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CreateNew(id2);
		return id2.ToString();
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_add_item_with_prices")]
	public static void AddItemWithPrices(string sessionId, LuaTable catalogEntry, string actionCode, string sourceType, LuaTable priceData, string initialNightPrice)
	{
		if (catalogEntry == null || priceData == null)
		{
			return;
		}
		IPricingRecord pricingRecord = null;
		long result = 0L;
		long.TryParse(GetLuaTableStringValue(catalogEntry, "product_id"), out result);
		if (result != 0L)
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			IKioskProduct kioskProduct = null;
			service?.GetProduct(result, excludeTitleRollup: true, out kioskProduct);
			ITitleProduct titleProduct = kioskProduct as ITitleProduct;
			if (titleProduct != null)
			{
				pricingRecord = GetPricingRecordFromPriceSetData(priceData)?.PriceRecords.FirstOrDefault((IPricingRecord x) => (titleProduct.TitleFamily == TitleFamily.Movies && titleProduct.TitleType == TitleType.DVD && x.TitleType == TitleType.DVD && x.TitleFamily == TitleFamily.Movies) || (titleProduct.TitleFamily == TitleFamily.Movies && titleProduct.TitleType == TitleType.Bluray && x.TitleType == TitleType.Bluray && x.TitleFamily == TitleFamily.Movies) || (titleProduct.TitleFamily == TitleFamily.Games && x.TitleType == TitleType.All && x.TitleFamily == TitleFamily.Games));
				if (pricingRecord != null)
				{
					decimal result2 = 0m;
					if (decimal.TryParse(initialNightPrice, out result2))
					{
						pricingRecord.InitialNight = result2;
					}
				}
			}
		}
		AddItem(sessionId, catalogEntry, actionCode, sourceType, pricingRecord);
	}

	private static IPricingSet GetPricingRecordFromPriceSetData(LuaTable priceData)
	{
		IPricingSet pricingSet = null;
		LogHelper.Instance.Log("Getting reservation item price set values");
		if (priceData != null)
		{
			pricingSet = new PricingSet();
			if (priceData["Movie"] is LuaTable luaTable)
			{
				if (luaTable["default"] is LuaTable luaTable2)
				{
					IPricingRecord pricingRecord = new PricingRecord
					{
						TitleFamily = TitleFamily.Movies,
						TitleType = TitleType.DVD
					};
					pricingSet.PriceRecords.Add(pricingRecord);
					AssignPriceValuesFromLuaTable(pricingRecord, luaTable2);
				}
				if (luaTable["BluRay"] is LuaTable luaTable3)
				{
					IPricingRecord pricingRecord2 = new PricingRecord
					{
						TitleFamily = TitleFamily.Movies,
						TitleType = TitleType.Bluray
					};
					pricingSet.PriceRecords.Add(pricingRecord2);
					AssignPriceValuesFromLuaTable(pricingRecord2, luaTable3);
				}
			}
			if (priceData["Game"] is LuaTable luaTable4)
			{
				LuaTable luaTable5 = luaTable4["default"] as LuaTable;
				IPricingRecord pricingRecord3 = new PricingRecord
				{
					TitleFamily = TitleFamily.Games,
					TitleType = TitleType.All
				};
				pricingSet.PriceRecords.Add(pricingRecord3);
				AssignPriceValuesFromLuaTable(pricingRecord3, luaTable5);
			}
		}
		return pricingSet;
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

	[KernelFunction(Name = "kernel.rental_shopping_cart_add_item")]
	public static void AddItem(string sessionId, LuaTable catalogEntry, string actionCode, string sourceType)
	{
		AddItem(sessionId, catalogEntry, actionCode, sourceType, null);
	}

	public static void AddItem(string sessionId, LuaTable catalogEntry, string actionCode, string sourceType, IPricingRecord pricingRecord)
	{
		if (catalogEntry == null)
		{
			return;
		}
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		long result = 0L;
		long.TryParse(GetLuaTableStringValue(catalogEntry, "product_id"), out result);
		IRentalShoppingCartItem rentalShoppingCartItem = service.CreateRentalShoppingCartItemFromProductId(result);
		if (rentalShoppingCartItem != null)
		{
			ItemSourceType result2 = ItemSourceType.Browse;
			if (!string.IsNullOrEmpty(sourceType))
			{
				Enum.TryParse<ItemSourceType>(sourceType, out result2);
			}
			rentalShoppingCartItem.SourceType = result2;
			RentalShoppingCartItemAction result3 = RentalShoppingCartItemAction.rental;
			if (!string.IsNullOrEmpty(actionCode))
			{
				Enum.TryParse<RentalShoppingCartItemAction>(actionCode, out result3);
			}
			rentalShoppingCartItem.Action = result3;
			if (rentalShoppingCartItem.SourceType == ItemSourceType.Reservation)
			{
				rentalShoppingCartItem.Barcode = GetLuaTableStringValue(catalogEntry, "barcode");
			}
			rentalShoppingCartItem.Quantity = 1m;
			rentalShoppingCartItem.Status = LineItemStatus.Pending;
			Guid id = new Guid(sessionId);
			(service?.GetRentalShoppingCart(id))?.AddItem(rentalShoppingCartItem, pricingRecord);
		}
		else
		{
			LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.AddItem. Unable to find product with id {0}", result);
		}
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_remove_item")]
	public static void RemoveItem(string sessionId, string indexValue, string userRemovedValue)
	{
		bool result = true;
		if (!bool.TryParse(userRemovedValue, out result))
		{
			result = true;
		}
		int result2 = 0;
		if (int.TryParse(indexValue, out result2))
		{
			result2--;
			Guid id = new Guid(sessionId);
			ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.RemoveItemFromRentalShoppingCart(id, result2, result);
		}
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

	[KernelFunction(Name = "kernel.current_rental_shopping_cart_bluray_upsell_dialog_needs_to_be_shown")]
	public static bool DialogNeedsToBeShown()
	{
		return (ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart)?.BlurayUpsellInfo.DialogNeedsToBeShown ?? false;
	}

	[KernelFunction(Name = "kernel.current_rental_shopping_cart_bluray_upsell_set_do_not_show_again")]
	public static void SetDoNotShowAgain(bool doNotShowAgain)
	{
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null)
		{
			rentalShoppingCart.BlurayUpsellInfo.DoNotShowAgain = doNotShowAgain;
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

	[KernelFunction(Name = "kernel.rental_shopping_cart_set_pickup_recommendations_shown")]
	public static void SetPickupRecommendationsShown(string sessionId, bool pickupRecommendationsShown)
	{
		ProcessRentalShoppingCartAction(sessionId, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			rentalShoppingCart.PickupRecommendationsShown = pickupRecommendationsShown;
		});
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

	[KernelFunction(Name = "kernel.current_rental_shopping_cart_bluray_upsell_add_missing_upsell_items")]
	public static void AddMissingUpsellItems()
	{
		(ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart)?.BlurayUpsellInfo.AddMissingBlurayUpsellItems();
	}

	[KernelFunction(Name = "kernel.current_rental_shopping_cart_reject_undecided_bluray_upsell_items")]
	public static void RejectUndecidedBlurayUpsellItems()
	{
		(ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart)?.BlurayUpsellInfo.RejectUndecidedUpsellItems();
	}

	[KernelFunction(Name = "kernel.current_rental_shopping_cart_remove_undecided_bluray_upsell_items")]
	public static void RemoveUndecidedBlurayUpsellItems()
	{
		(ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart)?.BlurayUpsellInfo.RemoveUndecidedUpsellItems();
	}

	[KernelFunction(Name = "kernel.current_rental_shopping_cart_bluray_upsell_set_offer_response")]
	public static void SetOfferResponse(long dvdProductId, string offerResponseValue)
	{
		BlurayUpsellItemOfferResponse result = BlurayUpsellItemOfferResponse.none;
		if (Enum.TryParse<BlurayUpsellItemOfferResponse>(offerResponseValue, out result))
		{
			(ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart)?.BlurayUpsellInfo.SetUpsellOffer(dvdProductId, result);
			return;
		}
		LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.SetOfferResponse. Unable to parse offer response value {0}", offerResponseValue);
	}

	[KernelFunction(Name = "kernel.current_rental_shopping_cart_get_bluray_upsell_item_count")]
	public static int GetBlurayUpsellItemCount(bool includeOnlyUndecied)
	{
		int result = 0;
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null)
		{
			result = rentalShoppingCart.BlurayUpsellInfo.Items.Count((IBlurayUpsellItem x) => !includeOnlyUndecied || x.OfferResponse == BlurayUpsellItemOfferResponse.none);
		}
		return result;
	}

	[KernelFunction(Name = "kernel.current_rental_shopping_cart_get_bluray_upsell_item_by_index")]
	public static LuaTable GetBlurayUpsellItemByIndex(int index)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null && rentalShoppingCart.BlurayUpsellInfo.Items.Count >= index)
		{
			IBlurayUpsellItem blurayUpsellItem = rentalShoppingCart.BlurayUpsellInfo.Items[index - 1];
			dictionary["dvd_product_id"] = blurayUpsellItem.DVDProductId;
			dictionary["blu_ray_product_id"] = blurayUpsellItem.BlurayProductId;
			dictionary["offer"] = ((blurayUpsellItem.OfferResponse == BlurayUpsellItemOfferResponse.none) ? null : blurayUpsellItem.OfferResponse.ToString());
		}
		if (dictionary.Count <= 0)
		{
			return null;
		}
		return dictionary.ToLuaTable();
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

	[KernelFunction(Name = "kernel.rental_shopping_cart_remove_multi_disc_discs")]
	public static void RemoveMultiDiscDiscs(string sessionId)
	{
		if (sessionId != null)
		{
			Guid shoppingCartSessionId = new Guid(sessionId);
			ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.RemoveMultiDiscDiscsFromRentalShoppingCart(shoppingCartSessionId);
		}
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_remove_item_product_id")]
	public static void RemoveItemByProduct(string sessionId, LuaTable product, string userRemovedValue)
	{
		bool result = true;
		if (!bool.TryParse(userRemovedValue, out result))
		{
			result = true;
		}
		long result2 = 0L;
		if (product != null && product["product_id"] != null)
		{
			long.TryParse(product["product_id"].ToString(), out result2);
			Guid shoppingCartSessionId = new Guid(sessionId);
			ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.RemoveItemFromRentalShoppingCartByProductId(shoppingCartSessionId, result2, result);
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

	[KernelFunction(Name = "kernel.rental_shopping_cart_has_item")]
	public static bool HasItem(string sessionId, string productId)
	{
		bool result = false;
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(new Guid(sessionId));
		if (rentalShoppingCart != null)
		{
			long result2 = 0L;
			if (long.TryParse(productId, out result2))
			{
				result = rentalShoppingCart.HasItem(result2);
			}
			else
			{
				LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.HasItem.  Unable to parse product id  {0}", productId);
			}
		}
		else
		{
			LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.HasItem.  Unable to find RentalShoppingCart with id {0}", sessionId);
		}
		return result;
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_has_mult_disc_item")]
	public static bool HasMultiDiscItem(string sessionId)
	{
		bool result = false;
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(new Guid(sessionId));
		if (rentalShoppingCart != null)
		{
			result = rentalShoppingCart.HasMultiDiscItem();
		}
		else
		{
			LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.HasMultiDiscItem.  Unable to find RentalShoppingCart with id {0}", sessionId);
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

	[KernelFunction(Name = "kernel.rental_shopping_cart_item_count")]
	public static int ItemCount(string sessionId)
	{
		int result = 0;
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		if (service != null && sessionId != null)
		{
			result = (service?.GetRentalShoppingCart(new Guid(sessionId)))?.Count ?? 0;
		}
		return result;
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_item_count")]
	public static int ItemCount(string sessionId, string productFamily, string actionCode)
	{
		int num = 0;
		TitleFamily? titleFamily = null;
		if (productFamily != null && Enum.TryParse<TitleFamily>(productFamily, out var result))
		{
			titleFamily = result;
		}
		RentalShoppingCartItemAction? rentalShoppingCartItemAction = null;
		if (actionCode != null && Enum.TryParse<RentalShoppingCartItemAction>(actionCode, out var result2))
		{
			rentalShoppingCartItemAction = result2;
		}
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		if (service != null && sessionId != null)
		{
			IList<IRentalShoppingCartItem> items = (service?.GetRentalShoppingCart(new Guid(sessionId))).Items;
			if (items != null)
			{
				IDataService dataService = ServiceLocator.Instance.GetService(typeof(IDataService)) as IDataService;
				foreach (IRentalShoppingCartItem item in items)
				{
					if ((!titleFamily.HasValue || item.TitleFamily == titleFamily) && (!rentalShoppingCartItemAction.HasValue || item.Action == rentalShoppingCartItemAction))
					{
						bool flag = true;
						dataService.GetProduct(item.ProductId, excludeTitleRollup: true, out var kioskProduct);
						if (kioskProduct != null && kioskProduct is ITitleProduct { DiscNumber: var discNumber } titleProduct)
						{
							flag = !discNumber.HasValue || titleProduct.DiscNumber == 1;
						}
						if (flag)
						{
							num++;
						}
					}
				}
			}
		}
		return num;
	}

	[KernelFunction(Name = "kernel.current_rental_shopping_cart_product_count")]
	public static int ProductCount(string productId)
	{
		int result = 0;
		long result2 = 0L;
		if (long.TryParse(productId, out result2))
		{
			IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			if (service != null && service.CurrentRentalShoppingCart != null)
			{
				result = service.CurrentRentalShoppingCart.GetCount(result2);
			}
		}
		else
		{
			LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.ProductCount. Unable to parse product id {0}", productId);
		}
		return result;
	}

	[KernelFunction(Name = "kernel.current_rental_shopping_cart_has_item_with_action")]
	public static bool HasItemWithAction(string productId, string actionCode)
	{
		bool result = false;
		if (Enum.TryParse<RentalShoppingCartItemAction>(actionCode, out var result2))
		{
			long result3 = 0L;
			if (long.TryParse(productId, out result3))
			{
				IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
				if (service != null && service.CurrentRentalShoppingCart != null)
				{
					result = service.CurrentRentalShoppingCart.HasItemWithAction(result3, result2);
				}
			}
			else
			{
				LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.HasItemWithAction. Unable to parse product id {0}", productId);
			}
		}
		else
		{
			LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.HasItemWithAction. Unable to parse action code {0}", actionCode);
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

	[KernelFunction(Name = "kernel.get_rental_shopping_use_credits")]
	public static string GetUseCredits(string id)
	{
		CreditElection result = CreditElection.NotSpecified;
		ProcessRentalShoppingCartAction(id, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			result = rentalShoppingCart.UseCredits;
		});
		return result.ToString();
	}

	[KernelFunction(Name = "kernel.set_rental_shopping_use_credits")]
	public static void SetUseCredits(string id, string value)
	{
		CreditElection newCreditElectionValue = CreditElection.NotSpecified;
		if (Enum.TryParse<CreditElection>(value, ignoreCase: true, out newCreditElectionValue))
		{
			ProcessRentalShoppingCartAction(id, delegate(IRentalShoppingCart rentalShoppingCart)
			{
				rentalShoppingCart.UseCredits = newCreditElectionValue;
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

	[KernelFunction(Name = "kernel.rental_shopping_cart_set_session_cart_groups")]
	public static void SetSessionCartGroups(string sessionId)
	{
		ProcessRentalShoppingCartAction(sessionId, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			rentalShoppingCart.SetShoppingCartGroups();
		});
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_clear_promo_code")]
	public static void ClearPromoCode(string sessionId)
	{
		ProcessRentalShoppingCartAction(sessionId, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			rentalShoppingCart.ClearPromoCode();
		});
	}

	[KernelFunction(Name = "kernel.rental_shopping_cart_get_sub_total")]
	public static LuaTable GetSubTotals(string sessionId)
	{
		LuaTable result = null;
		ITotals totals = null;
		ProcessRentalShoppingCartAction(sessionId, delegate(IRentalShoppingCart rentalShoppingCart)
		{
			totals = rentalShoppingCart.Totals;
		});
		if (totals != null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			foreach (ISubTotalGroup subTotalGroup in totals.SubTotalGroups)
			{
				Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
				dictionary2.Add("amount", subTotalGroup.Amount);
				dictionary2.Add("tax_rate", subTotalGroup.TaxRate);
				dictionary2.Add("discounted_amount", subTotalGroup.DiscountedAmount);
				dictionary2.Add("tax_amount", subTotalGroup.TaxAmount);
				dictionary2.Add("sub_total", subTotalGroup.SubTotal);
				dictionary2.Add("grand_total", subTotalGroup.GrantTotal);
				dictionary.Add(subTotalGroup.Action.ToString(), dictionary2.ToLuaTable());
			}
			result = dictionary.ToLuaTable();
		}
		return result;
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
		OrchestrationService.ViewShow("keyboard_email_receipt_view", new KeyboardInfo
		{
			MaxLength = 64,
			KeyboardText = (string.IsNullOrEmpty(emailAddress) ? "" : emailAddress),
			IsValidEntry = ValidationServices.IsValidEmailAddress,
			CancelAction = delegate
			{
				if (LuaCancelCallback != null)
				{
					LuaCancelCallback();
				}
			},
			ContinueAction = delegate(string keyboardText)
			{
				if (LuaContinueCallback != null)
				{
					LuaContinueCallback(keyboardText);
				}
			}
		});
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
			IBlurayUpsellItem blurayUpsellItem = rentalShoppingCart?.BlurayUpsellInfo?.Items.FirstOrDefault((IBlurayUpsellItem x) => x.DVDProductId == rentalShoppingCartItem.ProductId || x.BlurayProductId == rentalShoppingCartItem.ProductId);
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			if (blurayUpsellItem != null)
			{
				dictionary2.Add("product_group_id", blurayUpsellItem.ProductGroupId);
				dictionary2.Add("offer", blurayUpsellItem.OfferResponse.ToString());
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
