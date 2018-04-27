using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Pricing;

namespace Redbox.Rental.Services.Controllers;

public class ReservationFunctions
{
	private static Func<LuaTable> _getReservationInfoCallback;

	private static Action<string, string> _vendOnlyReservedTitlesCallback;

	private static Action<string, string> _reservationAuthorizeCallback;

	public static int GetRecommendedProductCount()
	{
		return GetRecommendedProducts().Count();
	}

	public static void AddRecommendedProductsToAnalytics()
	{
		List<IKioskProduct> recommendedProducts = GetRecommendedProducts();
		string elementValue = recommendedProducts.Select((IKioskProduct x) => (x as ITitleProduct)?.ProductId.ToString()).Join(",");
		ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null && service != null && service.SessionExists(rentalShoppingCart.ShoppingSessionId.ToString()))
		{
			service.AddElement("session", "recommendationsFlow", "True", rentalShoppingCart.ShoppingSessionId.ToString());
			service.AddElement("session", "recommendedTitles", elementValue, rentalShoppingCart.ShoppingSessionId.ToString());
			service.AddElement("session", "recommendedTitlesShown", recommendedProducts.Count.ToString(), rentalShoppingCart.ShoppingSessionId.ToString());
		}
	}

	public static List<IKioskProduct> GetRecommendedProducts()
	{
		LogHelper.Instance.Log("ReservationFunctions.GetRecommendedProducts", LogEntryType.Info);
		List<IKioskProduct> result = new List<IKioskProduct>();
		ReservationInfo reservationInfo = InvokeGetReservationInfoCallback();
		if (reservationInfo != null)
		{
			List<long> list = new List<long>();
			list.AddRange(reservationInfo.CustomerHistory);
			bool flag = false;
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			if (reservationInfo.Barcodes != null)
			{
				foreach (string barcode in reservationInfo.Barcodes)
				{
					IKioskProduct kioskProduct = null;
					service.GetProductByBarcode(barcode, out kioskProduct);
					if (kioskProduct is ITitleProduct titleProduct)
					{
						list.Add(titleProduct.ProductId);
						if (titleProduct.TitleType == TitleType.Bluray)
						{
							flag = true;
						}
					}
					else
					{
						LogHelper.Instance.Log($"ReservationFunctions.GetRecommendedProducts unable to find product using barcode: {barcode}", LogEntryType.Info);
					}
				}
			}
			else
			{
				LogHelper.Instance.Log("ReservationFunctions.GetRecommendedProducts reservationInfo.Barcodes is null.", LogEntryType.Info);
			}
			LogHelper.Instance.Log(string.Format("ReservationFunctions.GetRecommendedProducts GenerateRecommendations({0}, {1})", flag, string.Join(",", list)), LogEntryType.Info);
			result = RecommendationOnPickupViewController.GenerateRecommendations(flag, list);
		}
		else
		{
			LogHelper.Instance.Log("ReservationFunctions.GetRecommendedProducts reservationInfo is null.", LogEntryType.Info);
		}
		return result;
	}

	public static void RegisterGetReservationInfoCallback(Func<LuaTable> getReservationInfoCallback)
	{
		_getReservationInfoCallback = getReservationInfoCallback;
	}

	public static ReservationInfo InvokeGetReservationInfoCallback()
	{
		ReservationInfo reservationInfo = new ReservationInfo();
		if (_getReservationInfoCallback != null)
		{
			LuaTable luaTable = _getReservationInfoCallback();
			if (luaTable != null)
			{
				if (luaTable["barcodes"] is LuaTable luaTable2 && luaTable2.Keys.Count > 0)
				{
					for (int i = 0; i < luaTable2.Keys.Count; i++)
					{
						reservationInfo.Barcodes.Add(luaTable2[i + 1]?.ToString());
					}
				}
				else
				{
					LogHelper.Instance.Log("ReservationFunctions.InvokeGetReservationInfoCallback luaTable[barcodes] is null or empty.", LogEntryType.Info);
				}
				reservationInfo.EmailAddress = luaTable["email_address"] as string;
				reservationInfo.ReferenceNumber = luaTable["reference_number"]?.ToString();
				if (luaTable["reservation"] is LuaTable luaTable3)
				{
					decimal result = 0m;
					if (decimal.TryParse(luaTable3["default_service_fee"]?.ToString(), out result))
					{
						reservationInfo.DefaultServiceFee = result;
					}
					if (luaTable3["history_title_ids"] is LuaTable luaTable4)
					{
						for (int j = 0; j < luaTable4.Keys.Count; j++)
						{
							long result2 = 0L;
							if (long.TryParse(luaTable4[j + 1].ToString(), out result2))
							{
								reservationInfo.CustomerHistory.Add(result2);
							}
						}
					}
					reservationInfo.PricingSet = GetPricingRecordFromPriceSetData(luaTable3["prices"] as LuaTable);
					reservationInfo.ReservationData.PromoCode = luaTable3["promo_code"]?.ToString();
					bool result3 = false;
					if (bool.TryParse(luaTable3["applied_hive_online_promo"]?.ToString(), out result3))
					{
						reservationInfo.ReservationData.AppliedHiveOnlinePromo = result3;
					}
					decimal result4 = 0m;
					if (decimal.TryParse(luaTable3["hive_online_promo_discount"]?.ToString(), out result4))
					{
						reservationInfo.ReservationData.HiveOnlinePromoDiscount = Math.Abs(result4);
					}
					bool result5 = false;
					if (bool.TryParse(luaTable3["applied_credit"]?.ToString(), out result5))
					{
						reservationInfo.ReservationData.AppliedCredit = result5;
					}
					bool result6 = false;
					if (bool.TryParse(luaTable3["applied_title_marketing"].ToString(), out result6))
					{
						reservationInfo.ReservationData.AppliedTitleMarketing = result6;
					}
					if (luaTable3["reservation_titles"] is LuaTable luaTable5 && luaTable5.Keys.Count > 0)
					{
						for (int k = 0; k < luaTable5.Keys.Count; k++)
						{
							if (luaTable5[k + 1] is LuaTable luaTable6)
							{
								long result7 = 0L;
								if (long.TryParse(luaTable6["title_id"]?.ToString(), out result7))
								{
									ReservationTitle reservationTitle = new ReservationTitle
									{
										ProductId = result7
									};
									decimal result8 = 0m;
									if (decimal.TryParse(luaTable6["price"]?.ToString(), out result8))
									{
										reservationTitle.Price = result8;
									}
									decimal result9 = 0m;
									if (decimal.TryParse(luaTable6["discount"]?.ToString(), out result9))
									{
										reservationTitle.Discount = Math.Abs(result9);
									}
									int result10 = 0;
									if (int.TryParse(luaTable6["number_of_credits"]?.ToString(), out result10))
									{
										reservationTitle.NumberOfCredits = result10;
									}
									RentalShoppingCartItemAction result11 = RentalShoppingCartItemAction.rental;
									Enum.TryParse<RentalShoppingCartItemAction>(luaTable6["reservation_type"]?.ToString(), ignoreCase: true, out result11);
									reservationTitle.Action = result11;
									reservationInfo.ReservationData.Titles.Add(reservationTitle);
								}
								else
								{
									LogHelper.Instance.Log("ReservationFunctions.InvokeGetReservationInfoCallback reservationTitleLuaTable failed to retrieve productId.", LogEntryType.Info);
								}
							}
							else
							{
								LogHelper.Instance.Log("ReservationFunctions.InvokeGetReservationInfoCallback reservationTitleLuaTable is null or empty.", LogEntryType.Info);
							}
						}
					}
					else
					{
						LogHelper.Instance.Log("ReservationFunctions.InvokeGetReservationInfoCallback reservationTitlesLuaTable is null or empty.", LogEntryType.Info);
					}
					_ = luaTable3["prices"];
				}
				else
				{
					LogHelper.Instance.Log("ReservationFunctions.InvokeGetReservationInfoCallback reservationLuaTable is null or empty.", LogEntryType.Info);
				}
			}
			else
			{
				LogHelper.Instance.Log("ReservationFunctions.InvokeGetReservationInfoCallback luaTable is null.", LogEntryType.Info);
			}
		}
		else
		{
			LogHelper.Instance.Log("ReservationFunctions.InvokeGetReservationInfoCallback _getReservationInfoCallback is null.", LogEntryType.Info);
		}
		return reservationInfo;
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

	public static void RegisterVendOnlyReservedTitlesCallback(Action<string, string> vendOnlyReservedTitlesCallback)
	{
		_vendOnlyReservedTitlesCallback = vendOnlyReservedTitlesCallback;
	}

	public static void InvokeVendOnlyReservedTitlesCallback(string emailAddress, string referenceNumber)
	{
		_vendOnlyReservedTitlesCallback?.Invoke(emailAddress, referenceNumber);
	}

	public static void RegisterReservationAuthorizeCallback(Action<string, string> reservationAuthorizeCallback)
	{
		_reservationAuthorizeCallback = reservationAuthorizeCallback;
	}

	public static void InvokeReservationAuthorizeCallback(string emailAddress, string referenceNumber)
	{
		_reservationAuthorizeCallback?.Invoke(emailAddress, referenceNumber);
	}
}
