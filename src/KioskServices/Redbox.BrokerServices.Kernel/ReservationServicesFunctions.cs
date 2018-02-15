using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

namespace Redbox.BrokerServices.Kernel;

internal static class ReservationServicesFunctions
{
	[KernelFunction(Name = "kernel.resvsvcs_expire")]
	internal static void Expire(string storeNumber, long referenceNumber, LuaFunction callback)
	{
		IReservationServices service = ServiceLocator.Instance.GetService<IReservationServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService == null)
		{
			return;
		}
		RemoteServiceCallback callback2 = null;
		if (callback != null)
		{
			callback2 = delegate(IRemoteServiceResult r)
			{
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = callback,
					Parameters = new LuaTable[1] { r.AsTable() }
				});
			};
		}
		service.Expire(storeNumber, referenceNumber, callback2);
	}

	[KernelFunction(Name = "kernel.resvsvcs_pickup")]
	internal static void Pickup(string storeNumber, string emailAddress, long referenceNumber, LuaTable pickupResults, bool isBlocklisted, LuaFunction callback)
	{
		IReservationServices service = ServiceLocator.Instance.GetService<IReservationServices>();
		if (service == null)
		{
			return;
		}
		Dictionary<string, ReadOnlyCollection<IProductItem>> dictionary = new Dictionary<string, ReadOnlyCollection<IProductItem>>();
		foreach (object key in pickupResults.Keys)
		{
			if (!(pickupResults[key] is LuaTable luaTable))
			{
				continue;
			}
			List<IProductItem> list = new List<IProductItem>();
			foreach (object key2 in luaTable.Keys)
			{
				if (luaTable[key2] is LuaTable luaTable2)
				{
					IProductItem productItem = service.NewProductItem();
					productItem.Barcode = (string)luaTable2["barcode"];
					productItem.MultiDisc = (bool)luaTable2["multi_disc"];
					productItem.Status = Enum<LineItemStatus>.ParseIgnoringCase((string)luaTable2["status"], LineItemStatus.InventoryError);
					list.Add(productItem);
				}
			}
			dictionary[key.ToString()] = list.AsReadOnly();
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService == null)
		{
			return;
		}
		RemoteServiceCallback callback2 = null;
		if (callback != null)
		{
			callback2 = delegate(IRemoteServiceResult r)
			{
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = callback,
					Parameters = new LuaTable[1] { r.AsTable() }
				});
			};
		}
		service.Pickup(storeNumber, emailAddress, referenceNumber, dictionary, isBlocklisted, callback2);
		pickupResults.Dispose();
	}

	[KernelFunction(Name = "kernel.resvsvcs_reset")]
	internal static void Reset()
	{
		IReservationServices service = ServiceLocator.Instance.GetService<IReservationServices>();
		if (service != null)
		{
			LogHelper.Instance.Log("> Broker Connection Reset/Close Requested");
			service.Reset();
		}
	}

	[KernelFunction(Name = "kernel.resvsvcs_register")]
	internal static void Register(string storeNumber, LuaTable productFamilyPrices, LuaFunction requestCallback)
	{
		LogHelper.Instance.Log("> Broker Registry BEGIN");
		IReservationServices reservationServices = ServiceLocator.Instance.GetService<IReservationServices>();
		if (reservationServices == null)
		{
			return;
		}
		Dictionary<string, IDictionary<string, IDictionary<string, decimal>>> dictionary = new Dictionary<string, IDictionary<string, IDictionary<string, decimal>>>();
		if (productFamilyPrices != null)
		{
			foreach (object key in productFamilyPrices.Keys)
			{
				if (!(productFamilyPrices[key] is LuaTable luaTable))
				{
					continue;
				}
				dictionary[key.ToString()] = new Dictionary<string, IDictionary<string, decimal>>();
				foreach (object key2 in luaTable.Keys)
				{
					if (!(luaTable[key2] is LuaTable luaTable2))
					{
						continue;
					}
					Dictionary<string, decimal> dictionary2 = new Dictionary<string, decimal>();
					foreach (object key3 in luaTable2.Keys)
					{
						dictionary2[key3.ToString()] = Convert.ToDecimal(luaTable2[key3]);
					}
					dictionary[key.ToString()][key2.ToString()] = dictionary2;
				}
			}
		}
		ReservationRequestCallback reservationRequestCallback = delegate(string cardId, string email, long referenceNumber, DateTime reserveDate, IDictionary<string, List<int>> productsToReserve)
		{
			IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
			IReservationResult reservationResult = reservationServices.NewReservationResult();
			if (requestCallback == null)
			{
				reservationResult.ErrorMessage = "No script callback specified.";
				return reservationResult;
			}
			LuaTable luaTable3 = (LuaTable)service.NewTable();
			foreach (KeyValuePair<string, List<int>> item in productsToReserve)
			{
				LuaTable luaTable4 = (LuaTable)service.NewTable();
				int num = 1;
				foreach (int item2 in item.Value)
				{
					LuaTable luaTable5 = (LuaTable)service.NewTable();
					luaTable5["product_id"] = item2;
					luaTable4[num++] = luaTable5;
				}
				luaTable3[item.Key] = luaTable4;
			}
			ReadOnlyCollection<object> readOnlyCollection = requestCallback.Call(cardId, email, referenceNumber, reserveDate.ToString("yyyyMMddHHmmss"), luaTable3);
			LuaTable luaTable6 = ((readOnlyCollection != null && readOnlyCollection.Count > 0) ? ((LuaTable)readOnlyCollection[0]) : null);
			if (luaTable6 != null)
			{
				reservationResult.Success = (bool)luaTable6["success"];
				if (!reservationResult.Success)
				{
					reservationResult.ErrorMessage = (string)luaTable6["error_message"];
				}
				List<IReservedItem> list = new List<IReservedItem>();
				if (luaTable6["items"] is LuaTable luaTable7)
				{
					foreach (object key4 in luaTable7.Keys)
					{
						if (luaTable7[key4] is LuaTable luaTable8)
						{
							IReservedItem reservedItem = reservationServices.NewReservedItem();
							reservedItem.ProductId = Convert.ToInt32(luaTable8["product_id"]);
							reservedItem.Barcode = (string)luaTable8["barcode"];
							reservedItem.Status = Enum<LineItemStatus>.ParseIgnoringCase((string)luaTable8["status"], LineItemStatus.Fullfilled);
							list.Add(reservedItem);
						}
					}
				}
				reservationResult.Items = list;
			}
			return reservationResult;
		};
		reservationServices.Register(storeNumber, dictionary, reservationRequestCallback);
		LogHelper.Instance.Log("> Broker Registry COMPLETE");
	}

	[KernelFunction(Name = "kernel.resvsvcs_register2")]
	internal static void Register2(string storeNumber, LuaTable productFamilyPrices, LuaFunction requestCallback)
	{
		LogHelper.Instance.Log("> Broker Registry BEGIN");
		IReservationServices reservationServices = ServiceLocator.Instance.GetService<IReservationServices>();
		if (reservationServices == null)
		{
			return;
		}
		Dictionary<string, IDictionary<string, IDictionary<string, decimal>>> dictionary = new Dictionary<string, IDictionary<string, IDictionary<string, decimal>>>();
		if (productFamilyPrices != null)
		{
			foreach (object key in productFamilyPrices.Keys)
			{
				if (!(productFamilyPrices[key] is LuaTable luaTable))
				{
					continue;
				}
				dictionary[key.ToString()] = new Dictionary<string, IDictionary<string, decimal>>();
				foreach (object key2 in luaTable.Keys)
				{
					if (!(luaTable[key2] is LuaTable luaTable2))
					{
						continue;
					}
					Dictionary<string, decimal> dictionary2 = new Dictionary<string, decimal>();
					foreach (object key3 in luaTable2.Keys)
					{
						dictionary2[key3.ToString()] = Convert.ToDecimal(luaTable2[key3]);
					}
					dictionary[key.ToString()][key2.ToString()] = dictionary2;
				}
			}
		}
		ReservationRequestCallback2 reservationRequestCallback = delegate(string cardId, string email, long referenceNumber, DateTime reserveDate, IDictionary<string, List<int>> productsToReserve, bool appliedCredit, bool appliedHiveOnlinePromotion, bool appliedTitleMarketing, string customerNumber, decimal discountedSubTotal, decimal grandTotal, List<int> historyTitleIds, decimal hiveOnlinePromotionDiscount, int numberOfCreditsRemaining, string promoCode, decimal subTotal, decimal tax, List<ReservationTitle2> reservationTitles)
		{
			IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
			IReservationResult reservationResult = reservationServices.NewReservationResult();
			if (requestCallback == null)
			{
				reservationResult.ErrorMessage = "No script callback specified.";
				return reservationResult;
			}
			LuaTable luaTable3 = (LuaTable)service.NewTable();
			foreach (KeyValuePair<string, List<int>> item in productsToReserve)
			{
				LuaTable luaTable4 = (LuaTable)service.NewTable();
				int num = 1;
				foreach (int item2 in item.Value)
				{
					LuaTable luaTable5 = (LuaTable)service.NewTable();
					luaTable5["product_id"] = item2;
					luaTable4[num++] = luaTable5;
				}
				luaTable3[item.Key] = luaTable4;
			}
			LuaTable luaTable6 = (LuaTable)service.NewTable();
			int num2 = 1;
			foreach (int historyTitleId in historyTitleIds)
			{
				luaTable6[num2++] = historyTitleId;
			}
			LuaTable luaTable7 = (LuaTable)service.NewTable();
			int num3 = 1;
			foreach (ReservationTitle2 reservationTitle in reservationTitles)
			{
				LuaTable luaTable8 = (LuaTable)service.NewTable();
				luaTable8["credit_id"] = (reservationTitle.CreditID.HasValue ? reservationTitle.CreditID : new long?(0L));
				luaTable8["disc_format"] = reservationTitle.DiscFormat.ToString();
				luaTable8["disc_format_id"] = reservationTitle.DiscFormatID;
				luaTable8["discount"] = reservationTitle.Discount;
				luaTable8["discounted_price"] = reservationTitle.DiscountedPrice;
				luaTable8["item_id"] = reservationTitle.ItemID;
				luaTable8["number_of_credits"] = reservationTitle.NumberOfCredits;
				luaTable8["price"] = reservationTitle.Price;
				luaTable8["reservation_type"] = reservationTitle.ReservationType.ToString();
				luaTable8["reservation_type_id"] = reservationTitle.ReservationTypeID;
				luaTable8["title_id"] = reservationTitle.TitleID;
				luaTable7[num3++] = luaTable8;
			}
			ReadOnlyCollection<object> readOnlyCollection = requestCallback.Call(cardId, email, referenceNumber, reserveDate.ToString("yyyyMMddHHmmss"), luaTable3, appliedCredit, appliedHiveOnlinePromotion, appliedTitleMarketing, customerNumber, discountedSubTotal, grandTotal, luaTable6, hiveOnlinePromotionDiscount, numberOfCreditsRemaining, promoCode, subTotal, tax, luaTable7);
			LuaTable luaTable9 = ((readOnlyCollection != null && readOnlyCollection.Count > 0) ? ((LuaTable)readOnlyCollection[0]) : null);
			if (luaTable9 != null)
			{
				reservationResult.Success = (bool)luaTable9["success"];
				if (!reservationResult.Success)
				{
					reservationResult.ErrorMessage = (string)luaTable9["error_message"];
				}
				List<IReservedItem> list = new List<IReservedItem>();
				if (luaTable9["items"] is LuaTable luaTable10)
				{
					foreach (object key4 in luaTable10.Keys)
					{
						if (luaTable10[key4] is LuaTable luaTable11)
						{
							IReservedItem reservedItem = reservationServices.NewReservedItem();
							reservedItem.ProductId = Convert.ToInt32(luaTable11["product_id"]);
							reservedItem.Barcode = (string)luaTable11["barcode"];
							reservedItem.Status = Enum<LineItemStatus>.ParseIgnoringCase((string)luaTable11["status"], LineItemStatus.Fullfilled);
							list.Add(reservedItem);
						}
					}
				}
				reservationResult.Items = list;
			}
			return reservationResult;
		};
		reservationServices.Register(storeNumber, dictionary, reservationRequestCallback);
		LogHelper.Instance.Log("> Broker Registry COMPLETE");
	}

	[KernelFunction(Name = "kernel.resvsvcs_register3")]
	internal static void Register3(string storeNumber, LuaTable productFamilyPrices, LuaFunction requestCallback)
	{
		LogHelper.Instance.Log("> Broker Registry BEGIN");
		IReservationServices reservationServices = ServiceLocator.Instance.GetService<IReservationServices>();
		if (reservationServices == null)
		{
			return;
		}
		Dictionary<string, IDictionary<string, IDictionary<string, decimal>>> dictionary = new Dictionary<string, IDictionary<string, IDictionary<string, decimal>>>();
		if (productFamilyPrices != null)
		{
			foreach (object key in productFamilyPrices.Keys)
			{
				if (!(productFamilyPrices[key] is LuaTable luaTable))
				{
					continue;
				}
				dictionary[key.ToString()] = new Dictionary<string, IDictionary<string, decimal>>();
				foreach (object key2 in luaTable.Keys)
				{
					if (!(luaTable[key2] is LuaTable luaTable2))
					{
						continue;
					}
					Dictionary<string, decimal> dictionary2 = new Dictionary<string, decimal>();
					foreach (object key3 in luaTable2.Keys)
					{
						dictionary2[key3.ToString()] = Convert.ToDecimal(luaTable2[key3]);
					}
					dictionary[key.ToString()][key2.ToString()] = dictionary2;
				}
			}
		}
		ReservationRequestCallback3 reservationRequestCallback = delegate(string cardId, string email, long referenceNumber, DateTime reserveDate, IDictionary<string, List<int>> productsToReserve, bool appliedCredit, bool appliedHiveOnlinePromotion, bool appliedTitleMarketing, string customerNumber, decimal discountedSubTotal, decimal grandTotal, List<int> historyTitleIds, decimal hiveOnlinePromotionDiscount, bool isMultiNightPricing, int numberOfCreditsRemaining, string promoCode, decimal subTotal, decimal tax, List<ReservationTitle3> reservationTitles, bool isGc, List<string> gcIds, List<string> ccIds, decimal taxRate, string zip, bool isMdv, bool isLoyaltyRedemption)
		{
			IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
			IReservationResult reservationResult = reservationServices.NewReservationResult();
			if (requestCallback == null)
			{
				reservationResult.ErrorMessage = "No script callback specified.";
				return reservationResult;
			}
			LuaTable luaTable3 = (LuaTable)service.NewTable();
			foreach (KeyValuePair<string, List<int>> item in productsToReserve)
			{
				LuaTable luaTable4 = (LuaTable)service.NewTable();
				int num = 1;
				foreach (int item2 in item.Value)
				{
					LuaTable luaTable5 = (LuaTable)service.NewTable();
					luaTable5["product_id"] = item2;
					luaTable4[num++] = luaTable5;
				}
				luaTable3[item.Key] = luaTable4;
			}
			LuaTable luaTable6 = (LuaTable)service.NewTable();
			int num2 = 1;
			foreach (int historyTitleId in historyTitleIds)
			{
				luaTable6[num2++] = historyTitleId;
			}
			LuaTable luaTable7 = (LuaTable)service.NewTable();
			int num3 = 1;
			foreach (ReservationTitle3 reservationTitle in reservationTitles)
			{
				LuaTable luaTable8 = (LuaTable)service.NewTable();
				luaTable8["discount"] = reservationTitle.Discount;
				luaTable8["discounted_price"] = reservationTitle.DiscountedPrice;
				luaTable8["item_id"] = reservationTitle.ItemID;
				luaTable8["number_of_credits"] = reservationTitle.NumberOfCredits;
				luaTable8["price"] = reservationTitle.Price;
				luaTable8["reservation_type"] = reservationTitle.ReservationType.ToString();
				luaTable8["title_id"] = reservationTitle.TitleID;
				luaTable7[num3++] = luaTable8;
			}
			LuaTable luaTable9 = (LuaTable)service.NewTable();
			int num4 = 1;
			foreach (string gcId in gcIds)
			{
				luaTable9[num4++] = gcId;
			}
			LuaTable luaTable10 = (LuaTable)service.NewTable();
			int num5 = 1;
			foreach (string ccId in ccIds)
			{
				luaTable10[num5++] = ccId;
			}
			ReadOnlyCollection<object> readOnlyCollection = requestCallback.Call(cardId, email, referenceNumber, reserveDate.ToString("yyyyMMddHHmmss"), luaTable3, appliedCredit, appliedHiveOnlinePromotion, appliedTitleMarketing, customerNumber, discountedSubTotal, grandTotal, luaTable6, hiveOnlinePromotionDiscount, isMultiNightPricing, numberOfCreditsRemaining, promoCode, subTotal, tax, luaTable7, isGc, luaTable9, luaTable10, taxRate, zip, isMdv, isLoyaltyRedemption);
			LuaTable luaTable11 = ((readOnlyCollection != null && readOnlyCollection.Count > 0) ? ((LuaTable)readOnlyCollection[0]) : null);
			if (luaTable11 != null)
			{
				reservationResult.Success = (bool)luaTable11["success"];
				if (!reservationResult.Success)
				{
					reservationResult.ErrorMessage = (string)luaTable11["error_message"];
				}
				List<IReservedItem> list = new List<IReservedItem>();
				if (luaTable11["items"] is LuaTable luaTable12)
				{
					foreach (object key4 in luaTable12.Keys)
					{
						if (luaTable12[key4] is LuaTable luaTable13)
						{
							IReservedItem reservedItem = reservationServices.NewReservedItem();
							reservedItem.ProductId = Convert.ToInt32(luaTable13["product_id"]);
							reservedItem.Barcode = (string)luaTable13["barcode"];
							reservedItem.Status = Enum<LineItemStatus>.ParseIgnoringCase((string)luaTable13["status"], LineItemStatus.Fullfilled);
							list.Add(reservedItem);
						}
					}
				}
				reservationResult.Items = list;
			}
			return reservationResult;
		};
		reservationServices.Register(storeNumber, dictionary, reservationRequestCallback);
		LogHelper.Instance.Log("> Broker Registry COMPLETE");
	}

	[KernelFunction(Name = "kernel.resvsvcs_getreservations")]
	internal static void GetReservations(string storeNumber, LuaFunction completeCallback)
	{
		ServiceLocator.Instance.GetService<IReservationServices>()?.GetReservations(storeNumber, delegate(IRemoteServiceResult r)
		{
			IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
			LuaTable luaTable = r.AsTable();
			int num = 1;
			LuaTable luaTable2 = (LuaTable)service.NewTable();
			foreach (IReservation item in (List<IReservation>)r.Properties["reservations"])
			{
				LuaTable luaTable3 = (LuaTable)service.NewTable();
				luaTable3["card_id"] = item.CardId;
				luaTable3["transaction_date"] = item.TransactionDate.ToString("yyyyMMddHHmmss");
				luaTable3["reference_number"] = item.ReferenceNumber;
				LuaTable luaTable4 = (LuaTable)service.NewTable();
				foreach (KeyValuePair<string, ICollection<string>> barcode in item.Barcodes)
				{
					int num2 = 1;
					LuaTable luaTable5 = (LuaTable)service.NewTable();
					foreach (string item2 in barcode.Value)
					{
						luaTable5[num2++] = item2;
					}
					luaTable4[barcode.Key.ToLower()] = luaTable5;
				}
				luaTable3["barcodes"] = luaTable4;
				luaTable2[num++] = luaTable3;
			}
			((LuaTable)luaTable["properties"])["reservations"] = luaTable2;
			ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new ResultCallbackEntry
			{
				Function = completeCallback,
				Parameters = new LuaTable[1] { luaTable }
			});
		});
	}
}
