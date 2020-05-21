using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel;

public class ReservationFunctions
{
	public class Constants
	{
		public const string IsMultiNightPrice = "is_mnp";

		public const string TaxRate = "tax_rate";

		public const string AppliedHiveOnlinePromo = "applied_hive_online_promo";

		public const string SubTotal = "sub_total";

		public const string TransactionDate = "transaction_date";

		public const string PromoCode = "promo_code";

		public const string IsMultiDiscVend = "is_mdv";

		public const string AuthorizeAtPickup = "authorize_at_pickup";

		public const string ActualServiceFee = "actual_service_Fee";

		public const string ZipCode = "zip_code";

		public const string IsGiftCard = "is_gc";

		public const string DefaultServiceFee = "default_service_fee";

		public const string GrandTotal = "grand_total";

		public const string Tax = "tax";

		public const string IsLoyaltyRedemption = "is_loyalty_redemption";

		public const string EmailAddress = "email_address";

		public const string ReservationVersion = "reservation_version";

		public const string DiscountedSubtotal = "discounted_sub_total";

		public const string AppliedTitleMarketing = "applied_title_marketing";

		public const string HiveOnlinePromoDiscount = "hive_online_promo_discount";

		public const string CustomerNumber = "customer_number";

		public const string HistoryTitleIds = "history_title_ids";

		public const string CreditCardIds = "cc_ids";

		public const string GiftCardIds = "gc_ids";

		public const string ReservationTitles = "reservation_titles";

		public const string TitleId = "title_id";

		public const string DiscountedPrice = "discounted_price";

		public const string DiscountType = "discount_type";

		public const string ItemId = "item_id";

		public const string Discount = "discount";

		public const string ReservationType = "reservation_type";

		public const string Price = "price";

		public const string LoyaltyPoints = "loyalty_points";

		public const string Barcodes = "barcodes";

		public const string Movie = "Movie";

		public const string Game = "Game";

		public const string Default = "default";

		public const string Bluray = "BluRay";

		public const string _4kuhd = "_4kuhd";

		public const string InitialNight = "initial_night";

		public const string ExtraNight = "extra_night";

		public const string ExpirationPrice = "expiration_price";

		public const string NonReturn = "non_return";

		public const string NonReturnDays = "non_return_days";

		public const string PurchasePrice = "purchase_price";

		public const string PriceSetId = "pricesetid";

		public const string Prices = "prices";
	}

	private static Action<LuaTable, string> _updateBarcodeStatusCallback;

	[KernelFunction(Name = "kernel.register_get_reservation_info_callback")]
	internal static void RegisterGetReservationInfoCallback(Func<string, LuaTable> getReservationInfoCallback)
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterGetReservationInfoCallback(getReservationInfoCallback);
	}

	[KernelFunction(Name = "kernel.register_vend_only_reserved_titles_callback")]
	internal static void RegisterVendOnlyReservedTitlesCallback(Action<string, string> vendOnlyReservedTitlesCallback)
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterVendOnlyReservedTitlesCallback(vendOnlyReservedTitlesCallback);
	}

	[KernelFunction(Name = "kernel.register_reservation_authorize_callback")]
	internal static void RegisterReservationAuthorizeCallback(Action<string, string> reservationAuthorizeCallback)
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterReservationAuthorizeCallback(reservationAuthorizeCallback);
	}

	[KernelFunction(Name = "kernel.register_authorize_on_pickup_callback")]
	internal static void RegisterAuthorizeOnPickupCallback(Action<string, bool> callback)
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterAuthorizeOnPickupCallback(callback);
	}

	[KernelFunction(Name = "kernel.is_qualified_for_recommendation_on_pickup")]
	internal static bool IsQualifiedForRecommendationOnPickup()
	{
		return Redbox.Rental.Services.Controllers.ReservationFunctions.IsQualifiedForRecommendationOnPickup();
	}

	[KernelFunction(Name = "kernel.add_recommended_products_to_analytics")]
	internal static void AddRecommendedProductsToAnalytics()
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.AddRecommendedProductsToAnalytics();
	}

	[KernelFunction(Name = "kernel.check_for_duplicate_reservations")]
	internal static LuaTable CheckForDuplicateReservations()
	{
		List<string> list = Redbox.Rental.Services.Controllers.ReservationFunctions.CheckForDuplicateReservations();
		List<long> list2 = new List<long>();
		foreach (string item in list)
		{
			if (long.TryParse(item, out var result))
			{
				list2.Add(result);
			}
		}
		return list2.ToLuaTable();
	}

	[KernelFunction(Name = "kernel.get_account_reservations")]
	internal static void GetAccountReservations()
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.GetAccountReservations();
	}

	[KernelFunction(Name = "kernel.populate_shopping_cart_with_reservation_items")]
	internal static void PopulateShoppingCartWithReservationItems(string sessionId, string referenceNumber)
	{
		Guid id = new Guid(sessionId);
		Redbox.Rental.Services.Controllers.ReservationFunctions.PopulateShoppingCartWithReservationItems(ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(id), referenceNumber);
	}

	[KernelFunction(Name = "kernel.sync_reservations")]
	internal static void SyncReservations()
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.SyncReservations();
	}

	[KernelFunction(Name = "kernel.get_reserved_barcodes")]
	internal static LuaTable GetReservedBarcodes()
	{
		LuaTable result = null;
		List<string> reservedBarcodes = Redbox.Rental.Services.Controllers.ReservationFunctions.GetReservedBarcodes();
		if (reservedBarcodes != null && reservedBarcodes.Count > 0)
		{
			result = reservedBarcodes.ToLuaTable();
		}
		return result;
	}

	[KernelFunction(Name = "kernel.get_reserved_barcodes_count")]
	internal static int GetReservedBarcodesCount()
	{
		return Redbox.Rental.Services.Controllers.ReservationFunctions.GetReservedBarcodesCount();
	}

	[KernelFunction(Name = "kernel.get_account_reservations_count")]
	internal static int GetAccountReservationsCount(bool checkExpire = false)
	{
		return Redbox.Rental.Services.Controllers.ReservationFunctions.GetAccountReservations(checkExpire).Count;
	}

	[KernelFunction(Name = "kernel.get_account_reservations_info")]
	internal static LuaTable GetAccountReservationsInfo()
	{
		List<IReservation> accountReservations = Redbox.Rental.Services.Controllers.ReservationFunctions.GetAccountReservations();
		return new Dictionary<string, object>
		{
			["number_of_reservations"] = accountReservations.Count,
			["reference_numbers"] = accountReservations.Select((IReservation y) => y.ReferenceNumber).ToList(),
			["non_rap_reference_numbers"] = (from y in accountReservations
				where !y.ReservationDetails.AuthorizeAtPickup
				select y.ReferenceNumber).ToList(),
			["rap_reference_numbers"] = (from y in accountReservations
				where y.ReservationDetails.AuthorizeAtPickup
				select y.ReferenceNumber).ToList()
		}.ToLuaTable();
	}

	[KernelFunction(Name = "kernel.is_barcode_reserved")]
	internal static bool IsBarcodeReserved(string barcode)
	{
		return Redbox.Rental.Services.Controllers.ReservationFunctions.IsBarcodeReserved(barcode);
	}

	[KernelFunction(Name = "kernel.register_update_barcode_status_callback")]
	internal static void RegisterUpdateBarcodeStatusCallback(Action<LuaTable, string> updateBarcodeStatusCallback)
	{
		_updateBarcodeStatusCallback = updateBarcodeStatusCallback;
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterUpdateBarcodeStatusCallback(UpdateBarcodeStatus);
	}

	internal static void UpdateBarcodeStatus(List<string> barcodes, string status)
	{
		LuaTable luaTable = barcodes.ToLuaTable();
		if (luaTable != null)
		{
			_updateBarcodeStatusCallback?.Invoke(luaTable, status);
		}
	}

	[KernelFunction(Name = "kernel.register_get_store_return_time_callback")]
	internal static void RegisterGetStoreReturnTimeCallback(Func<string> getStoreReturnTimeCallback)
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterGetStoreReturnTimeCallback(getStoreReturnTimeCallback);
	}

	[KernelFunction(Name = "kernel.cancel_reservation")]
	internal static LuaTable CancelReservation(string referenceNumber)
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.CancelReservation(referenceNumber);
		return new Dictionary<string, object> { { "success", true } }.ToLuaTable();
	}

	[KernelFunction(Name = "kernel.is_reservation_authorize_at_pickup")]
	internal static bool IsReservationAuthorizeAtPickup(string referenceNumber)
	{
		return Redbox.Rental.Services.Controllers.ReservationFunctions.IsAuthorizeAtPickup(referenceNumber);
	}

	[KernelFunction(Name = "kernel.is_reservation_only_authorize_at_pickup")]
	internal static bool IsReservationOnlyAuthorizeAtPickup()
	{
		return Redbox.Rental.Services.Controllers.ReservationFunctions.IsOnlyAuthorizeAtPickup();
	}

	[KernelFunction(Name = "kernel.initialize_expire_reservation_timer")]
	internal static void CreateExpireReservationTimer()
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.InitializeExpireReservationTimer();
	}

	[KernelFunction(Name = "kernel.get_reserved_barcodes_to_vend")]
	internal static LuaTable GetReservedBarcodesToVend()
	{
		return Redbox.Rental.Services.Controllers.ReservationFunctions.GetReservedBarcodesToVend().ToLuaTable();
	}

	[KernelFunction(Name = "kernel.preposition_vend_complete")]
	internal static void ReservationPrepositionVendComplete(LuaTable vendResults)
	{
		if (vendResults != null)
		{
			List<VendResult> list = new List<VendResult>();
			foreach (object value in vendResults.Values)
			{
				if (value is LuaTable)
				{
					LuaTable luaTable = value as LuaTable;
					VendResult item = new VendResult
					{
						barcode = luaTable["barcode"]?.ToString(),
						status = luaTable["status"]?.ToString()
					};
					list.Add(item);
				}
			}
			Redbox.Rental.Services.Controllers.ReservationFunctions.ReservationPrepositionVendComplete(list);
		}
		else
		{
			LogHelper.Instance.Log("ReservationPrepositionVendComplete: Vend result must not be null.");
		}
	}

	[KernelFunction(Name = "kernel.recommendation_on_pickup_preposition_vend_complete")]
	internal static void RecommendationOnPickupPrepositionVendComplete(LuaTable vendResults, string referenceNumber, string modifiedCart)
	{
		if (vendResults != null)
		{
			List<VendResult> list = new List<VendResult>();
			foreach (object value in vendResults.Values)
			{
				if (value is LuaTable)
				{
					LuaTable luaTable = value as LuaTable;
					VendResult item = new VendResult
					{
						barcode = luaTable["barcode"]?.ToString(),
						status = luaTable["status"]?.ToString()
					};
					list.Add(item);
				}
			}
			bool result = false;
			bool.TryParse(modifiedCart, out result);
			Redbox.Rental.Services.Controllers.ReservationFunctions.RecommendationOnPickupPrepositionVendComplete(list, referenceNumber, result);
		}
		else
		{
			LogHelper.Instance.Log("ReservationPrepositionVend: Vend result must not be null.");
		}
	}

	[KernelFunction(Name = "kernel.on_pickup_validation_failure")]
	internal static void OnPickupValidationFailure()
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.OnPickupValidationFailure();
	}

	[KernelFunction(Name = "kernel.register_with_broker_service")]
	internal static void RegisterWithBrokerService()
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterWithBrokerService();
	}

	[KernelFunction(Name = "kernel.set_validated_reservations")]
	internal static void SetValidatedReservations(LuaTable validatedReservations)
	{
		List<string> list = new List<string>();
		if (validatedReservations == null)
		{
			return;
		}
		foreach (object value in validatedReservations.Values)
		{
			list.Add(value.ToString());
		}
		Redbox.Rental.Services.Controllers.ReservationFunctions.SetValidatedReservations(list);
	}

	[KernelFunction(Name = "kernel.register_zero_touch_pickup_callbacks")]
	internal static void RegisterZeroTouchPickupCallbacks(Action continueReservation)
	{
		Redbox.Rental.Services.Controllers.ReservationFunctions.RegisterZeroTouchPickupCallbacks(continueReservation);
	}

	public static void ConvertLuaReservation(LuaTable reservationLuaTable, IReservation reservation)
	{
		try
		{
			if (reservationLuaTable == null || reservationLuaTable.Keys.Count <= 0)
			{
				return;
			}
			foreach (object key in reservationLuaTable.Keys)
			{
				try
				{
					LogHelper.Instance.Log($"Converting reservation with reference number {key}");
					ReservationDetails reservationDetails = (ReservationDetails)(reservation.ReservationDetails = new ReservationDetails
					{
						ReferenceNumber = key?.ToString()
					});
					if (reservationLuaTable[key] is LuaTable reservationDataInfoLuaTable)
					{
						AssignReservationProperties(reservationDetails, reservationDataInfoLuaTable);
						AddPrices(reservationDetails, reservationDataInfoLuaTable);
						AddReservationTitles(reservationDetails, reservationDataInfoLuaTable);
						AddBarcodes(reservationDetails, reservationDataInfoLuaTable);
						AddHistoryTitles(reservationDetails, reservationDataInfoLuaTable);
						AddCreditCardIds(reservationDetails, reservationDataInfoLuaTable);
						AddGiftCardIds(reservationDetails, reservationDataInfoLuaTable);
					}
				}
				catch (Exception arg)
				{
					LogHelper.Instance.Log($"Error in ConvertLuaReservation while converting a reservation. {arg}");
				}
			}
		}
		catch (Exception arg2)
		{
			LogHelper.Instance.Log($"Error in ConvertLuaReservation. {arg2}");
		}
	}

	public static LuaTable ConvertReservationToLuaTable(IReservation reservation)
	{
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable luaTable = service.NewTable() as LuaTable;
		long.TryParse(reservation.ReferenceNumber, out var result);
		LuaTable luaTable2 = (LuaTable)(luaTable[result] = service.NewTable() as LuaTable);
		luaTable2["is_mnp"] = reservation.ReservationDetails.IsMultiNightPrice;
		luaTable2["tax_rate"] = reservation.ReservationDetails.TaxRate;
		luaTable2["applied_hive_online_promo"] = reservation.ReservationDetails.AppliedHiveOnlinePromo;
		luaTable2["sub_total"] = reservation.ReservationDetails.SubTotal;
		luaTable2["transaction_date"] = reservation.ReservationDetails.TransactionDate.ToString("yyyyMMddHHmmss");
		luaTable2["promo_code"] = reservation.ReservationDetails.PromoCode;
		luaTable2["is_mdv"] = reservation.ReservationDetails.IsMultiDiscVend;
		luaTable2["authorize_at_pickup"] = reservation.ReservationDetails.AuthorizeAtPickup;
		luaTable2["actual_service_Fee"] = reservation.ReservationDetails.ActualServiceFee;
		luaTable2["zip_code"] = reservation.ReservationDetails.ZipCode;
		luaTable2["is_gc"] = reservation.ReservationDetails.IsGiftCard;
		luaTable2["default_service_fee"] = reservation.ReservationDetails.DefaultServiceFee;
		luaTable2["grand_total"] = reservation.ReservationDetails.GrandTotal;
		luaTable2["tax"] = reservation.ReservationDetails.Tax;
		luaTable2["is_loyalty_redemption"] = reservation.ReservationDetails.IsLoyaltyRedemption;
		luaTable2["email_address"] = reservation.ReservationDetails.EmailAddress;
		luaTable2["reservation_version"] = reservation.ReservationDetails.ReservationVersion;
		luaTable2["discounted_sub_total"] = reservation.ReservationDetails.DiscountedSubTotal;
		luaTable2["applied_title_marketing"] = reservation.ReservationDetails.AppliedTitleMarketing;
		luaTable2["hive_online_promo_discount"] = reservation.ReservationDetails.HiveOnlinePromoDiscount;
		luaTable2["customer_number"] = reservation.ReservationDetails.CustomerNumber;
		luaTable2["history_title_ids"] = reservation.ReservationDetails.HistoryTitleIds.ToLuaTable();
		luaTable2["gc_ids"] = reservation.ReservationDetails.GiftCardIds.ToLuaTable();
		luaTable2["cc_ids"] = reservation.ReservationDetails.CreditCardIds.ToLuaTable();
		ConvertReservedTitles(reservation.ReservationDetails, luaTable2);
		ConvertPrices(reservation.ReservationDetails, luaTable2);
		return luaTable;
	}

	private static void ConvertPrices(IReservationDetails eachReservationDetail, LuaTable reservationDetailLuaTable)
	{
		if (eachReservationDetail.PricingSet == null || eachReservationDetail.PricingSet.PriceRecords.Count <= 0)
		{
			return;
		}
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable luaTable = (LuaTable)(reservationDetailLuaTable["prices"] = service.NewTable() as LuaTable);
		IEnumerable<IPricingRecord> enumerable = eachReservationDetail.PricingSet.PriceRecords.Where((IPricingRecord x) => x.TitleFamily == TitleFamily.Movies);
		if (enumerable != null && enumerable.Count() > 0)
		{
			LuaTable titleFamilyPricesLuaTable = (LuaTable)(luaTable["Movie"] = service.NewTable() as LuaTable);
			foreach (IPricingRecord item in enumerable)
			{
				string text = null;
				PopulatePricingRecordLuaTable(service, titleFamilyPricesLuaTable, item, item.TitleType switch
				{
					TitleType.Bluray => "BluRay", 
					TitleType._4KUHD => "_4kuhd", 
					_ => "default", 
				}, eachReservationDetail.PricingSet.PriceSetId);
			}
		}
		IEnumerable<IPricingRecord> enumerable2 = eachReservationDetail.PricingSet.PriceRecords.Where((IPricingRecord x) => x.TitleFamily == TitleFamily.Games);
		if (enumerable2 == null || enumerable2.Count() <= 0)
		{
			return;
		}
		LuaTable titleFamilyPricesLuaTable2 = (LuaTable)(luaTable["Game"] = service.NewTable() as LuaTable);
		foreach (IPricingRecord item2 in enumerable2)
		{
			string titleTypeName = "default";
			PopulatePricingRecordLuaTable(service, titleFamilyPricesLuaTable2, item2, titleTypeName, eachReservationDetail.PricingSet.PriceSetId);
		}
	}

	private static void PopulatePricingRecordLuaTable(IKernelService kernelService, LuaTable titleFamilyPricesLuaTable, IPricingRecord titleFormatPricingRecord, string titleTypeName, string priceSetId)
	{
		LuaTable luaTable = (LuaTable)(titleFamilyPricesLuaTable[titleTypeName] = kernelService.NewTable() as LuaTable);
		luaTable["initial_night"] = titleFormatPricingRecord.InitialNight;
		luaTable["extra_night"] = titleFormatPricingRecord.ExtraNight;
		luaTable["expiration_price"] = titleFormatPricingRecord.ExpirationPrice;
		luaTable["non_return"] = titleFormatPricingRecord.NonReturn;
		luaTable["non_return_days"] = titleFormatPricingRecord.NonReturnDays;
		luaTable["purchase_price"] = titleFormatPricingRecord.Purchase;
		int result = 1;
		int.TryParse(priceSetId, out result);
		luaTable["pricesetid"] = result;
	}

	private static void ConvertReservedTitles(IReservationDetails reservationDetail, LuaTable reservationDetailLuaTable)
	{
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		if (reservationDetail.ReservedTitles == null)
		{
			return;
		}
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable luaTable = service.NewTable() as LuaTable;
		LuaTable luaTable2 = service.NewTable() as LuaTable;
		LuaTable luaTable3 = service.NewTable() as LuaTable;
		LuaTable luaTable4 = service.NewTable() as LuaTable;
		int num = 0;
		foreach (IReservedTitle reservedTitle in reservationDetail.ReservedTitles)
		{
			num++;
			LuaTable luaTable5 = (LuaTable)(luaTable4[num] = service.NewTable() as LuaTable);
			luaTable5["title_id"] = reservedTitle.TitleId;
			luaTable5["item_id"] = num - 1;
			luaTable5["discount"] = reservedTitle.Discount;
			luaTable5["discounted_price"] = reservedTitle.DiscountedPrice;
			luaTable5["discount_type"] = ((object)reservedTitle.DiscountType/*cast due to constrained. prefix*/).ToString();
			luaTable5["reservation_type"] = Capitalize(reservedTitle.Action.ToString());
			luaTable5["price"] = reservedTitle.Price;
			luaTable5["loyalty_points"] = reservedTitle.LoyaltyPoints;
			if (reservedTitle.Action == RentalShoppingCartItemAction.rental)
			{
				int num2 = luaTable2.Values.Count + 1;
				luaTable2[num2] = reservedTitle.Barcode;
			}
			else if (reservedTitle.Action == RentalShoppingCartItemAction.purchase)
			{
				int num3 = luaTable3.Values.Count + 1;
				luaTable3[num3] = reservedTitle.Barcode;
			}
		}
		reservationDetailLuaTable["reservation_titles"] = luaTable4;
		if (luaTable2.Values.Count > 0)
		{
			luaTable[RentalShoppingCartItemAction.rental.ToString()] = luaTable2;
		}
		if (luaTable3.Values.Count > 0)
		{
			luaTable[RentalShoppingCartItemAction.purchase.ToString()] = luaTable3;
		}
		reservationDetailLuaTable["barcodes"] = luaTable;
	}

	private static string Capitalize(string value)
	{
		string result = null;
		if (!string.IsNullOrWhiteSpace(value))
		{
			result = char.ToUpper(value.First()) + value.Substring(1).ToLower();
		}
		return result;
	}

	private static void AddPrices(ReservationDetails reservationDetails, LuaTable reservationDataInfoLuaTable)
	{
		LuaTable priceData = reservationDataInfoLuaTable["prices"] as LuaTable;
		reservationDetails.PricingSet = GetPricingRecordFromPriceSetData(priceData);
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
					AssignPriceSetIdFromLuaTable(pricingSet, luaTable2);
				}
				else
				{
					LogHelper.Instance.Log("ReservationFunctions.GetPricingRecordFromPriceSetData default (dvd) priceData is null.", LogEntryType.Info);
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
					AssignPriceSetIdFromLuaTable(pricingSet, luaTable3);
				}
				else
				{
					LogHelper.Instance.Log("ReservationFunctions.GetPricingRecordFromPriceSetData BluRay priceData is null.", LogEntryType.Info);
				}
				if (luaTable["_4kuhd"] is LuaTable luaTable4)
				{
					IPricingRecord pricingRecord3 = new PricingRecord
					{
						TitleFamily = TitleFamily.Movies,
						TitleType = TitleType._4KUHD
					};
					pricingSet.PriceRecords.Add(pricingRecord3);
					AssignPriceValuesFromLuaTable(pricingRecord3, luaTable4);
					AssignPriceSetIdFromLuaTable(pricingSet, luaTable4);
				}
				else
				{
					LogHelper.Instance.Log("ReservationFunctions.GetPricingRecordFromPriceSetData _4kuhd priceData is null.", LogEntryType.Info);
				}
			}
			else
			{
				LogHelper.Instance.Log("ReservationFunctions.GetPricingRecordFromPriceSetData priceData['Movie'] is null.", LogEntryType.Info);
			}
			if (priceData["Game"] is LuaTable luaTable5)
			{
				LuaTable luaTable6 = luaTable5["default"] as LuaTable;
				IPricingRecord pricingRecord4 = new PricingRecord
				{
					TitleFamily = TitleFamily.Games,
					TitleType = TitleType.All
				};
				pricingSet.PriceRecords.Add(pricingRecord4);
				AssignPriceValuesFromLuaTable(pricingRecord4, luaTable6);
				AssignPriceSetIdFromLuaTable(pricingSet, luaTable6);
			}
			else
			{
				LogHelper.Instance.Log("ReservationFunctions.GetPricingRecordFromPriceSetData priceData['Game'] is null.", LogEntryType.Info);
			}
		}
		else
		{
			LogHelper.Instance.Log("ReservationFunctions.GetPricingRecordFromPriceSetData priceData is null.", LogEntryType.Info);
		}
		return pricingSet;
	}

	private static void AssignPriceSetIdFromLuaTable(IPricingSet pricingSet, LuaTable luaTable)
	{
		string value = null;
		if (GetStringValue(luaTable, "pricesetid", out value))
		{
			pricingSet.PriceSetId = value;
		}
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
		if (GetDecimalValue(luaTable, "purchase_price", out value))
		{
			pricingRecord.Purchase = value;
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

	private static bool GetStringValue(LuaTable luaTable, string propertyName, out string value)
	{
		bool result = false;
		value = null;
		if (luaTable != null)
		{
			object obj = luaTable[propertyName];
			if (obj != null)
			{
				value = obj?.ToString();
				result = true;
			}
		}
		return result;
	}

	private static void AddGiftCardIds(ReservationDetails reservationDetails, LuaTable reservationDataInfoLuaTable)
	{
		if (!(reservationDataInfoLuaTable["gc_ids"] is LuaTable luaTable))
		{
			return;
		}
		foreach (object value in luaTable.Values)
		{
			reservationDetails.GiftCardIds.Add(value?.ToString());
		}
	}

	private static void AddCreditCardIds(ReservationDetails reservationDetails, LuaTable reservationDataInfoLuaTable)
	{
		if (!(reservationDataInfoLuaTable["cc_ids"] is LuaTable luaTable))
		{
			return;
		}
		foreach (object value in luaTable.Values)
		{
			reservationDetails.CreditCardIds.Add(value?.ToString());
		}
	}

	private static void AddHistoryTitles(ReservationDetails reservationDetails, LuaTable reservationDataInfoLuaTable)
	{
		try
		{
			if (!(reservationDataInfoLuaTable["history_title_ids"] is LuaTable luaTable))
			{
				return;
			}
			foreach (object value in luaTable.Values)
			{
				int result = 0;
				if (int.TryParse(value?.ToString(), out result))
				{
					reservationDetails.HistoryTitleIds.Add(result);
				}
			}
		}
		catch (Exception arg)
		{
			LogHelper.Instance.Log($"Error in AddHistoryTitles. {arg}");
		}
	}

	private static void AddBarcodes(ReservationDetails reservationDetails, LuaTable reservationDataInfoLuaTable)
	{
		if (reservationDataInfoLuaTable["barcodes"] is LuaTable barcodesLuaTable)
		{
			IInventoryData service = ServiceLocator.Instance.GetService<IInventoryData>();
			AssignBarcodes(reservationDetails, barcodesLuaTable, service, RentalShoppingCartItemAction.rental);
			AssignBarcodes(reservationDetails, barcodesLuaTable, service, RentalShoppingCartItemAction.purchase);
		}
	}

	private static void AssignReservationProperties(ReservationDetails reservationDetails, LuaTable reservationDataInfoLuaTable)
	{
		reservationDetails.EmailAddress = reservationDataInfoLuaTable["email_address"] as string;
		bool result = false;
		if (bool.TryParse(reservationDataInfoLuaTable["is_mnp"]?.ToString(), out result))
		{
			reservationDetails.IsMultiNightPrice = result;
		}
		decimal result2 = 0m;
		if (decimal.TryParse(reservationDataInfoLuaTable["tax_rate"]?.ToString(), out result2))
		{
			reservationDetails.TaxRate = result2;
		}
		bool result3 = false;
		if (bool.TryParse(reservationDataInfoLuaTable["applied_hive_online_promo"]?.ToString(), out result3))
		{
			reservationDetails.AppliedHiveOnlinePromo = result3;
		}
		decimal result4 = 0m;
		if (decimal.TryParse(reservationDataInfoLuaTable["sub_total"]?.ToString(), out result4))
		{
			reservationDetails.SubTotal = result4;
		}
		string s = reservationDataInfoLuaTable["transaction_date"] as string;
		try
		{
			DateTime transactionDate = DateTime.ParseExact(s, "yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo);
			reservationDetails.TransactionDate = transactionDate;
		}
		catch
		{
		}
		reservationDetails.PromoCode = reservationDataInfoLuaTable["promo_code"] as string;
		bool result5 = false;
		if (bool.TryParse(reservationDataInfoLuaTable["is_mdv"]?.ToString(), out result5))
		{
			reservationDetails.IsMultiDiscVend = result5;
		}
		reservationDetails.ZipCode = reservationDataInfoLuaTable["zip_code"] as string;
		bool result6 = false;
		if (bool.TryParse(reservationDataInfoLuaTable["is_gc"]?.ToString(), out result6))
		{
			reservationDetails.IsGiftCard = result6;
		}
		bool result7 = false;
		if (bool.TryParse(reservationDataInfoLuaTable["authorize_at_pickup"]?.ToString(), out result7))
		{
			reservationDetails.AuthorizeAtPickup = result7;
		}
		reservationDetails.CustomerNumber = reservationDataInfoLuaTable["customer_number"] as string;
		decimal result8 = 0m;
		if (decimal.TryParse(reservationDataInfoLuaTable["grand_total"]?.ToString(), out result8))
		{
			reservationDetails.GrandTotal = result8;
		}
		decimal result9 = 0m;
		if (decimal.TryParse(reservationDataInfoLuaTable["tax"]?.ToString(), out result9))
		{
			reservationDetails.Tax = result9;
		}
		decimal result10 = 0m;
		if (decimal.TryParse(reservationDataInfoLuaTable["hive_online_promo_discount"]?.ToString(), out result10))
		{
			reservationDetails.HiveOnlinePromoDiscount = result10;
		}
		int result11 = 0;
		if (int.TryParse(reservationDataInfoLuaTable["reservation_version"]?.ToString(), out result11))
		{
			reservationDetails.ReservationVersion = result11;
		}
		decimal result12 = 0m;
		if (decimal.TryParse(reservationDataInfoLuaTable["discounted_sub_total"]?.ToString(), out result12))
		{
			reservationDetails.DiscountedSubTotal = result12;
		}
		bool result13 = false;
		if (bool.TryParse(reservationDataInfoLuaTable["is_loyalty_redemption"]?.ToString(), out result13))
		{
			reservationDetails.IsLoyaltyRedemption = result13;
		}
		bool result14 = false;
		if (bool.TryParse(reservationDataInfoLuaTable["applied_title_marketing"]?.ToString(), out result14))
		{
			reservationDetails.AppliedTitleMarketing = result14;
		}
		decimal result15 = 0m;
		if (decimal.TryParse(reservationDataInfoLuaTable["default_service_fee"]?.ToString(), out result15))
		{
			reservationDetails.DefaultServiceFee = result15;
		}
		decimal result16 = 0m;
		if (decimal.TryParse(reservationDataInfoLuaTable["actual_service_Fee"]?.ToString(), out result16))
		{
			reservationDetails.ActualServiceFee = result16;
		}
	}

	private static void AddReservationTitles(ReservationDetails reservationDetails, LuaTable reservationDataInfoLuaTable)
	{
		if (!(reservationDataInfoLuaTable["reservation_titles"] is LuaTable luaTable))
		{
			return;
		}
		foreach (object key in luaTable.Keys)
		{
			LuaTable obj = luaTable[key] as LuaTable;
			ReservedTitle reservedTitle = new ReservedTitle();
			int result = 0;
			if (int.TryParse(obj["title_id"]?.ToString(), out result))
			{
				reservedTitle.TitleId = result;
			}
			decimal result2 = 0m;
			if (decimal.TryParse(obj["discounted_price"]?.ToString(), out result2))
			{
				reservedTitle.DiscountedPrice = result2;
			}
			decimal result3 = 0m;
			if (decimal.TryParse(obj["discount"]?.ToString(), out result3))
			{
				reservedTitle.Discount = result3;
			}
			RentalShoppingCartItemAction result4 = RentalShoppingCartItemAction.rental;
			if (Enum.TryParse<RentalShoppingCartItemAction>(obj["reservation_type"]?.ToString(), ignoreCase: true, out result4))
			{
				reservedTitle.Action = result4;
			}
			decimal result5 = 0m;
			if (decimal.TryParse(obj["price"]?.ToString(), out result5))
			{
				reservedTitle.Price = result5;
			}
			reservationDetails.ReservedTitles.Add(reservedTitle);
		}
	}

	private static void AssignBarcodes(ReservationDetails reservationDetails, LuaTable barcodesLuaTable, IInventoryData inventoryData, RentalShoppingCartItemAction action)
	{
		string key = action.ToString().ToLower();
		if (!(barcodesLuaTable[key] is LuaTable luaTable))
		{
			return;
		}
		foreach (object value in luaTable.Values)
		{
			string barcode = value?.ToString();
			Inventory inventory = inventoryData?.GetInventory(barcode);
			IReservedTitle reservedTitle = reservationDetails.ReservedTitles.FirstOrDefault((IReservedTitle x) => x.Action == action && x.TitleId == (inventory?.TitleId ?? 0));
			if (reservedTitle != null)
			{
				reservedTitle.Barcode = barcode;
			}
		}
	}
}
