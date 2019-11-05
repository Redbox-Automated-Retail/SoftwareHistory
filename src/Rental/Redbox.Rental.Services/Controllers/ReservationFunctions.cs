using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskEngine.Environment.CardDataReader;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Loyalty;
using Redbox.Rental.Services.Promotion;
using Redbox.Rental.Services.Session;
using Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

namespace Redbox.Rental.Services.Controllers;

public class ReservationFunctions
{
	public static class Constants
	{
		public const string ReservationsDownloadedFlag = "deddie";

		public const string Reservations = "reservations";

		public const string OnlinePickup = "onlinePickup";

		public const string CartItem = "cartItem";

		public const string Barcode = "barcode";

		public const string MultiDisc = "multi_disc";

		public const string Status = "status";

		public const string Known = "Known";

		public const string Empty = "empty";

		public const string Reserved = "Reserved";
	}

	private class ProductItem : IProductItem
	{
		public string Barcode { get; set; }

		public bool MultiDisc { get; set; }

		public LineItemStatus Status { get; set; }
	}

	private static Func<string> _getStoreReturnTimeCallback;

	private static Action<List<string>, string> _updateBarcodeStatusCallback;

	private static Func<string, LuaTable> _getReservationInfoCallback;

	private static Action<string, string> _vendOnlyReservedTitlesCallback;

	private static Action<string, string> _reservationAuthorizeCallback;

	private static Action<string, bool> _authorizeOnPickupCallback;

	public static int GetRecommendedProductCount(List<Redbox.Rental.Model.Reservation.IReservation> reservations)
	{
		return GetRecommendedProducts(reservations).Count();
	}

	public static List<string> GetReservedBarcodes()
	{
		List<string> list = new List<string>();
		List<Redbox.Rental.Model.Reservation.IReservation> allReservations = GetAllReservations();
		if (allReservations != null)
		{
			foreach (Redbox.Rental.Model.Reservation.IReservation item in allReservations)
			{
				foreach (IReservedTitle reservedTitle in item.ReservationDetails.ReservedTitles)
				{
					if (!string.IsNullOrEmpty(reservedTitle.Barcode))
					{
						list.Add(reservedTitle.Barcode);
					}
				}
			}
		}
		return list;
	}

	public static int GetReservedBarcodesCount()
	{
		int num = 0;
		List<Redbox.Rental.Model.Reservation.IReservation> allReservations = GetAllReservations();
		if (allReservations != null)
		{
			foreach (Redbox.Rental.Model.Reservation.IReservation item in allReservations)
			{
				foreach (IReservedTitle reservedTitle in item.ReservationDetails.ReservedTitles)
				{
					if (!string.IsNullOrEmpty(reservedTitle.Barcode))
					{
						num++;
					}
				}
			}
		}
		return num;
	}

	public static bool IsBarcodeReserved(string barcode)
	{
		bool result = false;
		List<Redbox.Rental.Model.Reservation.IReservation> allReservations = GetAllReservations();
		if (allReservations != null)
		{
			foreach (Redbox.Rental.Model.Reservation.IReservation item in allReservations)
			{
				foreach (IReservedTitle reservedTitle in item.ReservationDetails.ReservedTitles)
				{
					if (reservedTitle.Barcode == barcode)
					{
						result = true;
						break;
					}
				}
			}
		}
		return result;
	}

	public static List<Redbox.Rental.Model.Reservation.IReservation> GetAllReservations()
	{
		return ServiceLocator.Instance.GetService<IDataService>()?.GetAllReservations() ?? new List<Redbox.Rental.Model.Reservation.IReservation>();
	}

	private static string ObfuscateHashedCardId(string hashedCardId)
	{
		return hashedCardId.Substring(0, Math.Min(5, hashedCardId.Length));
	}

	public static void ExpireReservations(List<Redbox.Rental.Model.Reservation.IReservation> reservations = null)
	{
		DateTime dateTime = DateTime.Parse(GetStoreReturnTime(), new CultureInfo("en-US"));
		LogHelper.Instance.Log($"Reservation expire running for time {dateTime}");
		if (DateTime.Now < dateTime)
		{
			dateTime = dateTime.AddDays(-1.0);
			LogHelper.Instance.Log("Setting expire time back a day since current time is before expire time");
		}
		dateTime = dateTime.Date;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (reservations == null)
		{
			reservations = service?.GetAllReservations();
		}
		List<IReservationDetails> list = new List<IReservationDetails>();
		foreach (Redbox.Rental.Model.Reservation.IReservation reservation in reservations)
		{
			if (!(reservation.HashedCardId != "deddie"))
			{
				continue;
			}
			if (reservation.ReservationDetails.TransactionDate < dateTime)
			{
				LogHelper.Instance.Log($"Expiring Reference Number {reservation.ReservationDetails?.ReferenceNumber}");
				IReservationServices service2 = ServiceLocator.Instance.GetService<IReservationServices>();
				if (long.TryParse(reservation.ReservationDetails?.ReferenceNumber, out var result))
				{
					service2?.Expire(service?.GetStore()?.Key, result, null);
				}
				list.Add(reservation.ReservationDetails);
			}
			else
			{
				LogHelper.Instance.Log($"Reservation has not expired. Reference Number: {reservation.ReservationDetails?.ReferenceNumber}");
			}
		}
		DeleteReservationDetails(list, reservations);
		foreach (IReservationDetails resDetail in list)
		{
			reservations.Remove(reservations.Where((Redbox.Rental.Model.Reservation.IReservation r) => r.ReferenceNumber == resDetail.ReferenceNumber).FirstOrDefault());
		}
	}

	public static bool IsAuthorizeAtPickup(string referenceNumber, List<Redbox.Rental.Model.Reservation.IReservation> reservations = null)
	{
		return PerformIsAuthorizeAtPickup(referenceNumber, reservations);
	}

	public static bool IsOnlyAuthorizeAtPickup(List<Redbox.Rental.Model.Reservation.IReservation> reservations = null)
	{
		return PerformIsAuthorizeAtPickup(null, reservations, evaluateAll: true);
	}

	private static bool PerformIsAuthorizeAtPickup(string referenceNumber, List<Redbox.Rental.Model.Reservation.IReservation> reservations = null, bool evaluateAll = false)
	{
		bool result = false;
		if (reservations == null)
		{
			reservations = GetAccountReservations();
		}
		foreach (Redbox.Rental.Model.Reservation.IReservation reservation in reservations)
		{
			if ((referenceNumber == null || reservation.ReferenceNumber == referenceNumber) && reservation.ReservationDetails.AuthorizeAtPickup)
			{
				result = true;
			}
			else if (evaluateAll)
			{
				return false;
			}
		}
		return result;
	}

	private static void DeleteReservationDetails(List<IReservationDetails> reservationDetailsToDelete, List<Redbox.Rental.Model.Reservation.IReservation> reservations = null)
	{
		if ((reservationDetailsToDelete?.Count ?? 0) <= 0)
		{
			return;
		}
		List<string> barcodeList = new List<string>();
		if (reservations == null)
		{
			reservations = ServiceLocator.Instance.GetService<IDataService>()?.GetAllReservations();
		}
		foreach (IReservationDetails eachReservationDetailToDelete in reservationDetailsToDelete)
		{
			Redbox.Rental.Model.Reservation.IReservation reservation = reservations.FirstOrDefault((Redbox.Rental.Model.Reservation.IReservation x) => x.ReferenceNumber == eachReservationDetailToDelete.ReferenceNumber);
			IReservationDetails reservationDetails = reservation?.ReservationDetails;
			if (reservation != null && reservationDetails != null)
			{
				reservationDetails.ReservedTitles.ForEach(delegate(IReservedTitle x)
				{
					barcodeList.Add(x.Barcode);
				});
				DeleteReservation(reservation.ReferenceNumber);
			}
		}
		UpdateBarcodeStatus(barcodeList, "Known");
	}

	public static void SyncReservations()
	{
		IDataService dataService = ServiceLocator.Instance.GetService<IDataService>();
		if (dataService != null && dataService.GetReservations("deddie") == null)
		{
			LogHelper.Instance.Log("ReservationFunctions.SyncReservations: downloading reservations from server.");
			IInventory inventoryService = ServiceLocator.Instance.GetService<IInventory>();
			ServiceLocator.Instance.GetService<IReservationServices>().GetReservations(dataService.GetStore()?.StoreId.ToString(), delegate(IRemoteServiceResult remoteServiceResult)
			{
				if (remoteServiceResult?.Properties != null && remoteServiceResult.Properties.ContainsKey("reservations"))
				{
					List<string> list = new List<string>();
					foreach (Redbox.BrokerServices.Proxy.ComponentModel.IReservation item2 in (List<Redbox.BrokerServices.Proxy.ComponentModel.IReservation>)remoteServiceResult.Properties["reservations"])
					{
						List<Redbox.Rental.Model.Reservation.IReservation> reservations = dataService.GetReservations(item2.CardId);
						Redbox.Rental.Model.Reservation.IReservation reservation = null;
						if (reservations == null)
						{
							reservation = new Reservation
							{
								HashedCardId = item2.CardId,
								Id = Guid.NewGuid()
							};
						}
						IReservationDetails reservationDetails = reservation.ReservationDetails;
						if (reservationDetails == null)
						{
							reservationDetails = (reservation.ReservationDetails = new ReservationDetails
							{
								ReferenceNumber = item2.ReferenceNumber.ToString(),
								TransactionDate = item2.TransactionDate
							});
							LogHelper.Instance.Log($"ReservationFunctions.SyncReservations - adding reservation with reference number {reservationDetails.ReferenceNumber}");
							if (item2.Barcodes != null)
							{
								foreach (KeyValuePair<string, ICollection<string>> barcode in item2.Barcodes)
								{
									if (barcode.Value != null)
									{
										foreach (string item3 in barcode.Value)
										{
											RentalShoppingCartItemAction result = RentalShoppingCartItemAction.rental;
											Enum.TryParse<RentalShoppingCartItemAction>(barcode.Key, ignoreCase: true, out result);
											Inventory inventory = inventoryService?.GetInventory(item3);
											IReservedTitle item = new ReservedTitle
											{
												Action = result,
												Barcode = item3,
												TitleId = (inventory?.TitleId ?? 0)
											};
											reservationDetails.ReservedTitles.Add(item);
											list.Add(item3);
										}
									}
								}
							}
							UpdateReservation(reservation);
						}
					}
					if (list.Count > 0)
					{
						UpdateBarcodeStatus(list, "Reserved");
					}
				}
			});
			UpdateReservation(new Reservation
			{
				HashedCardId = "deddie",
				Id = Guid.NewGuid()
			});
		}
		else
		{
			LogHelper.Instance.Log("ReservationFunctions.SyncReservations: reservations already downloaded from server.");
		}
	}

	public static void SetValidatedReservations(List<string> validatedRefNumbers)
	{
		List<Redbox.Rental.Model.Reservation.IReservation> list = new List<Redbox.Rental.Model.Reservation.IReservation>();
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		foreach (string validatedRefNumber in validatedRefNumbers)
		{
			Redbox.Rental.Model.Reservation.IReservation reservation = service.GetReservation(validatedRefNumber);
			if (reservation != null)
			{
				list.Add(reservation);
			}
		}
		Redbox.Rental.Services.Session.Session.Instance.ValidatedReservations = list;
	}

	public static void InitializeExpireReservationTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		ITimer timer = service.GetTimer("reservation-expire-timer");
		if (timer == null)
		{
			timer = service.CreateTimer("reservation-expire-timer", 30000, 7200000, delegate
			{
				ExpireReservations();
			});
		}
		timer.Start();
	}

	public static string GetStoreReturnTime()
	{
		return _getStoreReturnTimeCallback?.Invoke();
	}

	public static void RegisterGetStoreReturnTimeCallback(Func<string> getStoreReturnTimeCallback)
	{
		_getStoreReturnTimeCallback = getStoreReturnTimeCallback;
	}

	public static void UpdateBarcodeStatus(List<string> barcodes, string status)
	{
		if (barcodes != null && barcodes.Count > 0)
		{
			_updateBarcodeStatusCallback?.Invoke(barcodes, status);
		}
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
		IAnalyticsService service2 = ServiceLocator.Instance.GetService<IAnalyticsService>();
		if (rentalShoppingCart == null || service2 == null)
		{
			return;
		}
		List<ProductData> products = new List<ProductData>();
		recommendedProducts.ForEach(delegate(IKioskProduct p)
		{
			ITitleProduct titleProduct = p as ITitleProduct;
			if (p != null)
			{
				products.Add(ProductData.ToProductData(titleProduct));
			}
		});
		service2?.CurrentSession?.AddData(new ROPRecommendedProducts
		{
			RecommendedProducts = products,
			RecommendationsFlow = true
		});
	}

	public static List<IKioskProduct> GetRecommendedProducts(List<Redbox.Rental.Model.Reservation.IReservation> reservations = null)
	{
		LogHelper.Instance.Log("ReservationFunctions.GetRecommendedProducts", LogEntryType.Info);
		if (reservations == null)
		{
			reservations = GetAccountReservations();
		}
		new List<IKioskProduct>();
		List<long> excludeProducts = new List<long>();
		reservations.ForEach(delegate(Redbox.Rental.Model.Reservation.IReservation x)
		{
			x.ReservationDetails.HistoryTitleIds.ForEach(delegate(int t)
			{
				excludeProducts.Add(t);
			});
		});
		bool flag = false;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		foreach (string item in GetReservedBarcodesToVend(reservations))
		{
			service.GetProductByBarcode(item, out var kioskProduct);
			if (kioskProduct is ITitleProduct titleProduct)
			{
				excludeProducts.Add(titleProduct.ProductId);
				if (titleProduct.TitleType == TitleType.Bluray)
				{
					flag = true;
				}
			}
			else
			{
				LogHelper.Instance.Log($"ReservationFunctions.GetRecommendedProducts unable to find product using barcode: {item}", LogEntryType.Info);
			}
		}
		LogHelper.Instance.Log(string.Format("ReservationFunctions.GetRecommendedProducts GenerateRecommendations({0}, {1})", flag, string.Join(",", excludeProducts)), LogEntryType.Info);
		return RecommendationOnPickupViewController.GenerateRecommendations(flag, excludeProducts);
	}

	private static IReservationDetails FindReservationDetailsWithDuplicateTitles(IReservationDetails searchForReservationDetails, List<Redbox.Rental.Model.Reservation.IReservation> reservations)
	{
		IReservationDetails result = null;
		foreach (Redbox.Rental.Model.Reservation.IReservation reservation in reservations)
		{
			if (searchForReservationDetails.ReferenceNumber != reservation.ReservationDetails.ReferenceNumber && reservation.ReservationDetails.ReservedTitles.Count == searchForReservationDetails.ReservedTitles.Count && reservation.ReservationDetails.ReservedTitles.All((IReservedTitle x) => searchForReservationDetails.ReservedTitles.Any((IReservedTitle y) => x.TitleId == y.TitleId)))
			{
				result = reservation.ReservationDetails;
			}
		}
		return result;
	}

	public static List<string> CheckForDuplicateReservations()
	{
		List<string> list = new List<string>();
		LogHelper.Instance.Log("ReservationFunctions.CheckForDuplicateReservations called.", LogEntryType.Info);
		List<Redbox.Rental.Model.Reservation.IReservation> accountReservations = GetAccountReservations();
		foreach (Redbox.Rental.Model.Reservation.IReservation item in accountReservations)
		{
			if (list.Contains(item.ReferenceNumber))
			{
				continue;
			}
			IReservationDetails reservationDetails = FindReservationDetailsWithDuplicateTitles(item.ReservationDetails, accountReservations);
			if (reservationDetails == null)
			{
				continue;
			}
			IReservationDetails reservationDetails2 = ((reservationDetails.TransactionDate < item.ReservationDetails.TransactionDate) ? reservationDetails : item.ReservationDetails);
			if (!list.Contains(reservationDetails2.ReferenceNumber))
			{
				string text = reservationDetails?.ReservedTitles.Select((IReservedTitle x) => x.TitleId).ToList().Join(", ");
				LogHelper.Instance.Log($"ReservationFunctions.CheckForDuplicateReservations found duplicate reservations {item?.ReferenceNumber} and {reservationDetails?.ReferenceNumber} with titles {text}.  Removing duplicate reservation {reservationDetails2?.ReferenceNumber}.", LogEntryType.Info);
				list.Add(reservationDetails2.ReferenceNumber);
			}
		}
		return list;
	}

	public static void DeleteReservation(string referenceNumber)
	{
		ServiceLocator.Instance.GetService<IDataService>()?.DeleteReservation(referenceNumber);
		List<Redbox.Rental.Model.Reservation.IReservation> validatedReservations = Redbox.Rental.Services.Session.Session.Instance.ValidatedReservations;
		if (validatedReservations != null)
		{
			Redbox.Rental.Model.Reservation.IReservation reservation = validatedReservations.FirstOrDefault((Redbox.Rental.Model.Reservation.IReservation r) => r.ReferenceNumber == referenceNumber);
			if (reservation != null)
			{
				Redbox.Rental.Services.Session.Session.Instance.ValidatedReservations.Remove(reservation);
			}
		}
	}

	public static void UpdateReservation(Redbox.Rental.Model.Reservation.IReservation reservation)
	{
		LogHelper.Instance.Log($"ReservationFunctions.UpdateReservation {ObfuscateHashedCardId(reservation?.HashedCardId)}");
		ServiceLocator.Instance.GetService<IDataService>()?.SaveReservation(reservation);
	}

	public static void RegisterUpdateBarcodeStatusCallback(Action<List<string>, string> updateBarcodeStatusCallback)
	{
		_updateBarcodeStatusCallback = updateBarcodeStatusCallback;
	}

	public static void RegisterGetReservationInfoCallback(Func<string, LuaTable> getReservationInfoCallback)
	{
		_getReservationInfoCallback = getReservationInfoCallback;
	}

	public static IReservationDetails PopulateShoppingCartWithReservationItems(IRentalShoppingCart rentalShoppingCart, string referenceNumber = null, bool clearCart = true)
	{
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Invalid comparison between Unknown and I4
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Invalid comparison between Unknown and I4
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_045e: Invalid comparison between Unknown and I4
		IReservationDetails reservationDetails = null;
		try
		{
			IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			GetAccountReservations().ForEach(delegate(Redbox.Rental.Model.Reservation.IReservation y)
			{
				if (referenceNumber == null || y.ReferenceNumber == referenceNumber)
				{
					reservationDetails = y.ReservationDetails;
				}
			});
			if (reservationDetails != null)
			{
				if (clearCart)
				{
					rentalShoppingCart.Clear();
					PromotionService.Instance.ClearPromotion();
					rentalShoppingCart.PickupRecommendationsShown = false;
				}
				rentalShoppingCart?.SetDefaultServiceFee(reservationDetails.DefaultServiceFee);
				LogHelper.Instance.Log($"Setting cart default service fee to {reservationDetails.DefaultServiceFee} from reservation.");
				decimal num = 0m;
				decimal value = 0m;
				List<IPromotionItem> list = new List<IPromotionItem>();
				IDataService service2 = ServiceLocator.Instance.GetService<IDataService>();
				foreach (IReservedTitle reservedTitle2 in reservationDetails.ReservedTitles)
				{
					string barcode = reservedTitle2.Barcode;
					ErrorList product = service2.GetProduct(reservedTitle2.TitleId, excludeTitleRollup: false, out var kioskProduct);
					if (product == null || product.Count == 0)
					{
						if (kioskProduct == null)
						{
							LogHelper.Instance.Log($"Unable to find kiosk product using dataService.GetProductByBarcode( {barcode} )");
							Inventory inventory = ServiceLocator.Instance.GetService<IInventory>()?.GetInventory(barcode);
							if (inventory != null)
							{
								LogHelper.Instance.Log($"Found product id {inventory.TitleId} for barcode {barcode}");
								service2.GetProduct(inventory.TitleId, excludeTitleRollup: true, out kioskProduct);
								LogHelper.Instance.Log(string.Format("kiosk product for product id {0} {1}found", inventory.TitleId, (kioskProduct == null) ? "not " : ""));
							}
							else
							{
								LogHelper.Instance.Log("Unable to find product id from Inventory Service.");
							}
						}
						ITitleProduct titleProduct = kioskProduct as ITitleProduct;
						if (titleProduct != null)
						{
							LogHelper.Instance.Log($"Found titleProduct for barcode {barcode}");
							IReservedTitle reservedTitle = reservationDetails.ReservedTitles.FirstOrDefault((IReservedTitle x) => x.TitleId == titleProduct.ProductId);
							if (reservedTitle != null)
							{
								LogHelper.Instance.Log($"Found reserved title for barcode {barcode}");
								IRentalShoppingCartItem rentalShoppingCartItem = service?.CreateRentalShoppingCartItemFromTitleProduct(titleProduct);
								rentalShoppingCartItem.SourceType = ItemSourceType.Reservation;
								rentalShoppingCartItem.Barcode = barcode;
								rentalShoppingCartItem.Action = reservedTitle.Action;
								rentalShoppingCartItem.Quantity = 1m;
								rentalShoppingCartItem.Status = LineItemStatus.Pending;
								rentalShoppingCartItem.OfferCode = PersonalizationFunctions.GetLoyaltyOfferCode(rentalShoppingCartItem.TitleProduct);
								rentalShoppingCartItem.TivoQueryId = PersonalizationFunctions.GetTivoQueryId(rentalShoppingCartItem.TitleProduct);
								rentalShoppingCartItem.Tags = PersonalizationFunctions.GetTags(rentalShoppingCartItem.TitleProduct);
								LogHelper.Instance.Log($"Checking discounts for reserved title Id {titleProduct.ProductId}");
								if ((int)reservedTitle.DiscountType == 2)
								{
									rentalShoppingCartItem.LoyaltyPointsRecord = new LoyaltyPointsRecord(rentalShoppingCartItem)
									{
										ToBeRedeemed = true,
										PointsToRedeem = (int)reservedTitle.LoyaltyPoints
									};
								}
								else if ((int)reservedTitle.DiscountType == 1 && reservedTitle.Discount != 0m)
								{
									list.Add(new PromotionItem
									{
										PromotionAmount = Math.Abs(reservedTitle.Discount),
										RentalShoppingCartItem = rentalShoppingCartItem
									});
									value += reservedTitle.Discount;
								}
								LogHelper.Instance.Log($"Getting pricing record for reserved title Id {titleProduct.ProductId}");
								IPricingRecord pricingRecord = reservationDetails.PricingSet?.PriceRecords.FirstOrDefault((IPricingRecord x) => (titleProduct.TitleFamily == TitleFamily.Movies && titleProduct.TitleType == TitleType.DVD && x.TitleType == TitleType.DVD && x.TitleFamily == TitleFamily.Movies) || (titleProduct.TitleFamily == TitleFamily.Movies && titleProduct.TitleType == TitleType.Bluray && x.TitleType == TitleType.Bluray && x.TitleFamily == TitleFamily.Movies) || (titleProduct.TitleFamily == TitleFamily.Movies && titleProduct.TitleType == TitleType._4KUHD && x.TitleType == TitleType._4KUHD && x.TitleFamily == TitleFamily.Movies) || (titleProduct.TitleFamily == TitleFamily.Games && x.TitleType == TitleType.All && x.TitleFamily == TitleFamily.Games));
								if (pricingRecord != null)
								{
									LogHelper.Instance.Log($"Found pricing record for reserved title Id {titleProduct.ProductId}");
									pricingRecord = pricingRecord.Clone();
									if (reservedTitle.Action == RentalShoppingCartItemAction.rental)
									{
										pricingRecord.InitialNight = reservedTitle.Price;
									}
									else
									{
										pricingRecord.Purchase = reservedTitle.Price;
									}
								}
								else
								{
									LogHelper.Instance.Log($"ReservationFunctions.PopulateShoppingCartWithReservationItems - barcode {barcode} couldn't find a pricing record.", LogEntryType.Info);
								}
								if (reservationDetails.AppliedTitleMarketing && (int)reservedTitle.DiscountType == 1)
								{
									num += reservedTitle.Discount;
								}
								if (!rentalShoppingCart.HasItem(titleProduct.ProductId))
								{
									LogHelper.Instance.Log($"Adding reserved product {rentalShoppingCartItem.ProductId} to recommendation on pickup cart");
									rentalShoppingCart.AddItem(rentalShoppingCartItem, pricingRecord);
								}
							}
							else
							{
								LogHelper.Instance.Log($"ReservationFunctions.PopulateShoppingCartWithReservationItems - reservation title barcode {barcode} couldn't find a product.", LogEntryType.Info);
							}
						}
						else
						{
							LogHelper.Instance.Log($"ReservationFunctions.PopulateShoppingCartWithReservationItems - barcode {barcode} couldn't find a product.", LogEntryType.Info);
						}
					}
					else
					{
						LogHelper.Instance.Log($"ReservationFunctions.PopulateShoppingCartWithReservationItems - dataService.GetProductByBarcode returned errors: {product}", LogEntryType.Info);
					}
				}
				decimal num2 = 0m;
				if (reservationDetails.AppliedHiveOnlinePromo)
				{
					num2 = reservationDetails.HiveOnlinePromoDiscount;
					LogHelper.Instance.Log($"ReservationFunctions:- applied hive online promo, reserved discount amount: {num2}");
				}
				else if (reservationDetails.AppliedTitleMarketing)
				{
					num2 = num;
					LogHelper.Instance.Log($"ReservationFunctions:- applied title marketing promo, reserved discount amount: {num2}");
				}
				num2 = Math.Abs(num2);
				value = Math.Abs(value);
				if (num2 > 0m)
				{
					IPromotion promotion = new Redbox.Rental.Services.Promotion.Promotion
					{
						Name = reservationDetails.PromoCode,
						OriginalPrice = num2,
						Price = num2,
						Quantity = 1,
						Intent = PromotionIntentCode.FixedValue,
						OriginalPercent = 0m,
						Percent = 0m,
						RentQuantity = 0,
						RentFormat = null,
						GetQuantity = 0,
						GetFormat = null,
						CampaignTitles = null,
						ProductTypeId = null,
						FormatIds = null,
						PromotionItems = list
					};
					PromotionService.Instance.SetPromotion(promotion);
					if (value != num2)
					{
						service.CurrentRentalShoppingCart.ValidateAndUpdatePromoCode();
					}
					service.CurrentRentalShoppingCart.UpdateServiceFee();
				}
			}
			else
			{
				LogHelper.Instance.Log("ReservationFunctions.PopulateShoppingCartWithReservationItems reservationDetails is null.", LogEntryType.Info);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception building cart from reservation", e);
		}
		return reservationDetails;
	}

	public static bool IsQualifiedForRecommendationOnPickup()
	{
		bool flag = false;
		List<Redbox.Rental.Model.Reservation.IReservation> accountReservations = GetAccountReservations();
		int count = accountReservations.Count;
		bool flag2 = accountReservations.Any((Redbox.Rental.Model.Reservation.IReservation x) => x.ReservationDetails.IsGiftCard);
		bool flag3 = accountReservations.Any((Redbox.Rental.Model.Reservation.IReservation x) => x.ReservationDetails.IsMultiNightPrice);
		bool flag4 = accountReservations.Any((Redbox.Rental.Model.Reservation.IReservation x) => x.ReservationDetails.IsMultiDiscVend);
		bool flag5 = accountReservations.Any((Redbox.Rental.Model.Reservation.IReservation x) => x.ReservationDetails.IsLoyaltyRedemption);
		bool flag6 = accountReservations.Any((Redbox.Rental.Model.Reservation.IReservation x) => x.ReservationDetails.ReservedTitles.Any((IReservedTitle w) => w.Action == RentalShoppingCartItemAction.purchase));
		bool flag7 = OrchestrationService.IsGiftCard();
		List<string> reservedBarcodesToVend = GetReservedBarcodesToVend(accountReservations);
		LogHelper.Instance.Log($"IsQualifiedForRecommendationOnPickup() - num_reservations= {reservedBarcodesToVend.Count} num_reserved_titles= {count}");
		LogHelper.Instance.Log($"IsQualifiedForRecommendationOnPickup() part 2 - is_gift_card={flag7}   is_gc: {flag2}   is_mnp: {flag3}   is_mdv= {flag4}");
		LogHelper.Instance.Log($"IsQualifiedForRecommendationOnPickup() part 3 - is_loyalty_redemption= {flag5}  has_purchase= {flag6}");
		if (count == 1 && reservedBarcodesToVend.Count < 5 && !flag7 && !flag2 && !flag3 && !flag4 && !flag5 && !flag6 && GetRecommendedProductCount(accountReservations) > 2)
		{
			flag = true;
		}
		LogHelper.Instance.Log($"IsQualifiedForRecommendationOnPickup() qualifies: {flag}");
		return flag;
	}

	public static List<string> GetReservedBarcodesToVend(List<Redbox.Rental.Model.Reservation.IReservation> reservations = null)
	{
		List<string> result = new List<string>();
		if (reservations == null)
		{
			reservations = GetAccountReservations();
		}
		LogHelper.Instance.Log($"ReservationFunctions.GetBarcodesToVend - reservation count: {reservations?.Count ?? 0}");
		if (reservations != null)
		{
			foreach (Redbox.Rental.Model.Reservation.IReservation reservation in reservations)
			{
				reservation.ReservationDetails.ReservedTitles.ForEach(delegate(IReservedTitle x)
				{
					result.Add(x.Barcode);
					LogHelper.Instance.Log($"ReservationFunctions.GetBarcodesToVend - Barcode: {x.Barcode}");
				});
			}
		}
		LogHelper.Instance.Log($"ReservationFunctions.GetBarcodesToVend - barcode count: {result.Count}");
		return result;
	}

	public static List<Redbox.Rental.Model.Reservation.IReservation> GetAccountReservations(bool runExpire = false)
	{
		List<Redbox.Rental.Model.Reservation.IReservation> list = new List<Redbox.Rental.Model.Reservation.IReservation>();
		if (Redbox.Rental.Services.Session.Session.Instance.ValidatedReservations == null)
		{
			ITrackData trackData = Redbox.Rental.Services.Session.Session.Instance?.TrackData;
			if (trackData != null && trackData.HasValidData())
			{
				List<Redbox.Rental.Model.Reservation.IReservation> allReservations = ServiceLocator.Instance.GetService<IDataService>()?.GetAllReservations();
				string cardHashId = trackData.CardHashId;
				string reservationHashId = trackData.ReservationHashId;
				list = GetReservationsForHashedCardId(cardHashId, reservationHashId, allReservations);
				if (runExpire)
				{
					ExpireReservations(list);
				}
				LogHelper.Instance.Log($"ReservationFunctions:GetAccountReservations - found: {list?.Count}");
			}
			else
			{
				LogHelper.Instance.Log("ReservationFunctions.GetAccountReservations - track data is not valid");
			}
		}
		else
		{
			list.AddRange(Redbox.Rental.Services.Session.Session.Instance.ValidatedReservations);
			LogHelper.Instance.Log($"ReservationFunctions.GetAccountReservation - returned {list.Count} validated reservations. ");
		}
		return list;
	}

	public static void CancelReservation(string referenceNumber)
	{
		LogHelper.Instance.Log($"ReservationFunctions:CancelReservation called for Reference Number: {referenceNumber}");
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		List<IReservationDetails> list = new List<IReservationDetails>();
		Redbox.Rental.Model.Reservation.IReservation reservation = service.GetReservation(referenceNumber);
		if (reservation != null)
		{
			LogHelper.Instance.Log($"ReservationFunctions.CancelReservation removed reservation for Reference Number {referenceNumber}:");
			list.Add(reservation.ReservationDetails);
		}
		if (list.Count == 0)
		{
			LogHelper.Instance.Log($"ReservationFunctions.CancelReservation - reservation not found for Reference Number: {referenceNumber}");
		}
		else
		{
			DeleteReservationDetails(list, service.GetAllReservations());
		}
	}

	public static void OnPickupValidationFailure()
	{
		ProcessReservation("ReservationFunctions.OnPickupValidationFailure", LineItemStatus.CustomerDisabled, null, isFromReservationPrepositionVendComplete: false, commitShoppingCart: true, "Pickup Validation Failure.");
	}

	public static void ReservationPrepositionVendComplete(List<VendResult> vendResultList)
	{
		ProcessReservation("ReservationFunctions.ReservationPrepositionVendComplete", LineItemStatus.InventoryError, vendResultList, isFromReservationPrepositionVendComplete: true, commitShoppingCart: false, null);
	}

	private static void ProcessReservation(string logHeaderText, LineItemStatus defaultLineItemStatus, List<VendResult> vendResultList, bool isFromReservationPrepositionVendComplete, bool commitShoppingCart, string commitReason)
	{
		LogHelper.Instance.Log($"{logHeaderText} Begin");
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		IInventory service2 = ServiceLocator.Instance.GetService<IInventory>();
		Dictionary<string, List<IProductItem>> dictionary = new Dictionary<string, List<IProductItem>>();
		List<string> list = new List<string>();
		if (commitShoppingCart)
		{
			OrchestrationService.ShoppingCartCommit(rentalShoppingCart?.ShoppingSessionId.ToString(), commitReason, shouldEndAndSave: false);
		}
		foreach (Redbox.Rental.Model.Reservation.IReservation accountReservation in GetAccountReservations())
		{
			dictionary.Clear();
			foreach (IReservedTitle eachReservedTitle in accountReservation.ReservationDetails.ReservedTitles)
			{
				Inventory inventory = service2.GetInventory(eachReservedTitle.Barcode);
				IKioskProduct kioskProduct = null;
				service?.GetProduct(inventory?.TitleId ?? 0, excludeTitleRollup: false, out kioskProduct);
				MDVProduct mDVProduct = kioskProduct as MDVProduct;
				LineItemStatus result = defaultLineItemStatus;
				if (vendResultList != null)
				{
					Enum.TryParse<LineItemStatus>(vendResultList?.FirstOrDefault((VendResult x) => x.barcode == eachReservedTitle.Barcode)?.status, ignoreCase: true, out result);
				}
				IProductItem productItem = new ProductItem
				{
					Barcode = eachReservedTitle.Barcode,
					MultiDisc = (mDVProduct != null),
					Status = result
				};
				if (productItem.Status != LineItemStatus.Fullfilled)
				{
					LogHelper.Instance.Log($"Adding {productItem.Barcode} to reset barcode list");
					list.Add(productItem.Barcode);
				}
				if (rentalShoppingCart != null)
				{
					OrchestrationService.AnalyticsAddNode("onlinePickup", "cartItem", rentalShoppingCart.ShoppingSessionId);
					Dictionary<string, string> attributes = new Dictionary<string, string>
					{
						{ "barcode", productItem.Barcode },
						{
							"multi_disc",
							productItem.MultiDisc.ToString()
						},
						{
							"status",
							productItem.Status.ToString()
						}
					};
					OrchestrationService.AnalyticsAddAttribute("cartItem", attributes, rentalShoppingCart.ShoppingSessionId);
				}
				string key = eachReservedTitle.Action.ToString();
				if (!dictionary.TryGetValue(key, out var value))
				{
					value = (dictionary[key] = new List<IProductItem>());
				}
				value.Add(productItem);
			}
			LogHelper.Instance.Log($"{logHeaderText} results for reservation {accountReservation?.ReferenceNumber}");
			foreach (string key2 in dictionary.Keys)
			{
				LogHelper.Instance.Log($"{logHeaderText} results for group {key2}");
				foreach (IProductItem item in dictionary[key2])
				{
					LogHelper.Instance.Log($"{logHeaderText} group_result  barcode: {item?.Barcode}  multi_disc: {item?.MultiDisc}  status: {item?.Status}");
				}
			}
			if (dictionary.Count == 0)
			{
				LogHelper.Instance.Log($"Reservation {accountReservation?.ReferenceNumber} had 0 barcodes, skipping Pickup()/PickupReconcile() call");
				continue;
			}
			long.TryParse(accountReservation?.ReferenceNumber, out var result2);
			IDictionary<string, ReadOnlyCollection<IProductItem>> dictionary2 = new Dictionary<string, ReadOnlyCollection<IProductItem>>();
			foreach (string key3 in dictionary.Keys)
			{
				dictionary2[key3] = new ReadOnlyCollection<IProductItem>(dictionary[key3]);
			}
			if (isFromReservationPrepositionVendComplete && accountReservation.ReservationDetails.AuthorizeAtPickup)
			{
				if (rentalShoppingCart != null && !rentalShoppingCart.PickupRecommendationsShown)
				{
					LogHelper.Instance.Log($"{logHeaderText}: Calling PopulateShoppingCartWithReservationItems() for ref# {accountReservation?.ReferenceNumber}");
					PopulateShoppingCartWithReservationItems(rentalShoppingCart, accountReservation?.ReferenceNumber);
				}
				LogHelper.Instance.Log($"{logHeaderText}: Calling MerchantServices.PickupReconcile()");
				ServiceLocator.Instance.GetService<IMerchantServices>()?.PickupReconcile(rentalShoppingCart?.ShoppingSessionId.ToString(), Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.CustomerProfileNumber, result2, dictionary2, null);
			}
			else
			{
				LogHelper.Instance.Log($"{logHeaderText}: Calling ReservationServices.Pickup()");
				IReservationServices service3 = ServiceLocator.Instance.GetService<IReservationServices>();
				bool? obj = ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(rentalShoppingCart?.ShoppingSessionId.ToString())?.ShoppingCart.Blocklisted;
				service3?.Pickup(isBlocklisted: obj ?? false, storeNumber: (service?.GetStore()?.StoreId ?? (-1)).ToString(), emailAddress: accountReservation.ReservationDetails?.EmailAddress, referenceNumber: result2, pickupResults: dictionary2, callback: null, v: rentalShoppingCart?.ShoppingSessionId.ToString());
			}
			LogHelper.Instance.Log($"{logHeaderText}: Removing reservation for reference nuber... {accountReservation.ReferenceNumber}");
			DeleteReservation(accountReservation.ReferenceNumber);
		}
		if (list.Count > 0)
		{
			LogHelper.Instance.Log($"{logHeaderText}: resetting status to Known for {list.Count} discs");
			UpdateBarcodeStatus(list, "Known");
		}
		if (rentalShoppingCart != null)
		{
			OrchestrationService.CloseAnalytics(rentalShoppingCart.ShoppingSessionId, isSessionAbandoned: false);
		}
		LogHelper.Instance.Log($"{logHeaderText} COMPLETE");
	}

	public static void RecommendationOnPickupPrepositionVendComplete(List<VendResult> vendResultList, string referenceNumber, bool modifiedCart)
	{
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null)
		{
			IShoppingSession shoppingSession = ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(rentalShoppingCart.ShoppingSessionId.ToString());
			if (shoppingSession != null)
			{
				UpdateRentalShoppingCartItemStatuses(vendResultList, rentalShoppingCart);
				List<string> list = new List<string>();
				ProcessRentalShoppingCartItems(rentalShoppingCart, list);
				List<Redbox.Rental.Model.Reservation.IReservation> accountReservations = GetAccountReservations();
				CallMerchantService(referenceNumber, modifiedCart, rentalShoppingCart, shoppingSession, accountReservations);
				LogHelper.Instance.Log($"ReservationFunctions.ReservationPrepositionVend: Removing reservations for reference number...{referenceNumber}");
				DeleteReservation(referenceNumber);
				if (list.Count > 0)
				{
					LogHelper.Instance.Log($"ReservationFunctions.ReservationPrepositionVend: resetting status to Known for {list.Count} discs");
					UpdateBarcodeStatus(list, "Known");
				}
				OrchestrationService.CloseShoppingCartAnalytics(rentalShoppingCart.ShoppingSessionId, isSessionAbandoned: false);
				shoppingSession.End();
			}
			else
			{
				LogHelper.Instance.Log($"Unable find shopping cart session for {rentalShoppingCart.ShoppingSessionId.ToString()}");
			}
		}
		else
		{
			LogHelper.Instance.Log("Unable to update shopping cart item statuses.  Unable to get current rental shopping cart.");
		}
	}

	public static void RegisterWithBrokerService()
	{
		LogHelper.Instance.Log("ReservationFunctions.RegisterWithBrokerService");
		IReservationServices service = ServiceLocator.Instance.GetService<IReservationServices>();
		string storeNumber = ServiceLocator.Instance.GetService<IDataService>()?.GetStore()?.StoreId.ToString();
		service?.Register(storeNumber, GetpricesForBrokerService(isForRecommendationOnPickup: false), CreateReservationCallback, CancelReservationCallback);
	}

	private static IReservationResult CreateReservationCallback(string cardId, string email, long referenceNumber, DateTime reserveDate, IDictionary<string, List<int>> productsToReserve, bool appliedCredit, bool appliedHiveOnlinePromotion, bool appliedTitleMarketing, string customerNumber, decimal discountedSubTotal, decimal grandTotal, List<int> historyTitleIDs, decimal hiveOnlinePromotionDiscount, bool isMultiNightPricing, int numberOfCreditsRemaining, string promoCode, decimal subTotal, decimal tax, List<ReservationTitle3> reservationTitles, bool isGc, List<string> gcIds, List<string> ccIds, decimal taxRate, string zip, bool isMdv, bool isLoyaltyRedemption, decimal defaultServiceFee, decimal actualServiceFee, bool authorizeAtPickup, ReservationCardHash cardHashData, List<ReservationCardHash> alternateCardHashes)
	{
		LogHelper.Instance.Log("ReservationFunctions.CreateReservationCallback: Begin");
		IReservationServices service = ServiceLocator.Instance.GetService<IReservationServices>();
		IReservationResult reservationResult = service.NewReservationResult();
		List<string> barcodesAlreadyInPlay = GetBarcodesAlreadyInPlay();
		List<IBarcodeForProductResult> barcodesForNewReservation = GetBarcodesForNewReservation(reservationTitles, barcodesAlreadyInPlay);
		foreach (IBarcodeForProductResult item in barcodesForNewReservation)
		{
			IReservedItem reservedItem = service.NewReservedItem();
			reservedItem.ProductId = (int)item.ProductId;
			reservedItem.Barcode = (item.BarcodeAssigned ? item.Barcode : "empty");
			reservedItem.Status = (item.Status.HasValue ? item.Status.Value : LineItemStatus.InventoryError);
			reservationResult.Items.Add(reservedItem);
			if (item.BarcodeAssigned)
			{
				LogHelper.Instance.Log($"ReservationFunctions.CreateReservationCallback: found barcode {reservedItem?.Barcode} for product id: {reservedItem?.ProductId}");
			}
		}
		if (barcodesForNewReservation.Count > 0 && barcodesForNewReservation.All((IBarcodeForProductResult x) => x.BarcodeAssigned))
		{
			LogHelper.Instance.Log("ReservationFunctions.CreateReservationCallback: Creating reservation...");
			UpdateBarcodeStatus(barcodesForNewReservation.Select((IBarcodeForProductResult x) => x.Barcode).ToList(), "Reserved");
			CreateAndSaveNewReservation(cardId, email, referenceNumber, reserveDate, appliedHiveOnlinePromotion, appliedTitleMarketing, customerNumber, discountedSubTotal, grandTotal, historyTitleIDs, hiveOnlinePromotionDiscount, isMultiNightPricing, promoCode, subTotal, tax, reservationTitles, isGc, gcIds, ccIds, taxRate, zip, isMdv, isLoyaltyRedemption, defaultServiceFee, actualServiceFee, authorizeAtPickup, barcodesForNewReservation, cardHashData, alternateCardHashes);
			reservationResult.Success = true;
			if (OrchestrationService.IsRentalSessionActive() && ApplicationState.Instance.NeedToReloadCarousel)
			{
				StartViewController.ReloadCarousel(force: true);
			}
		}
		else
		{
			LogHelper.Instance.Log("ReservationFunctions.CreateReservationCallback: Could not satisfy reservation request; unable to reserve requested inventory.");
			reservationResult.ErrorMessage = null;
			reservationResult.Success = false;
		}
		return reservationResult;
	}

	private static List<IBarcodeForProductResult> GetBarcodesForNewReservation(List<ReservationTitle3> reservationTitles, List<string> barcodesAlreadyInPlay)
	{
		List<ISearchBarcodeForProduct> list = new List<ISearchBarcodeForProduct>();
		foreach (ReservationTitle3 reservationTitle in reservationTitles)
		{
			RentalShoppingCartItemAction result = RentalShoppingCartItemAction.rental;
			Enum.TryParse<RentalShoppingCartItemAction>(reservationTitle.ReservationType.ToString(), ignoreCase: true, out result);
			list.Add(new SearchBarcodeForProduct
			{
				ProductId = reservationTitle.TitleID,
				Action = result
			});
		}
		return ServiceLocator.Instance.GetService<IDataService>()?.GetBarcodesForProducts(list, barcodesAlreadyInPlay);
	}

	private static List<string> GetBarcodesAlreadyInPlay()
	{
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		IRentalShoppingCart rentalShoppingCart = service?.CurrentRentalShoppingCart;
		List<string> list = service?.AssignBarcodesToVend(rentalShoppingCart);
		AddBarcodesForMDVProducts(rentalShoppingCart, list);
		List<long> userVisibleProducts = new List<long>();
		switch (ServiceLocator.Instance.GetService<IViewService>()?.Peek())
		{
		case "start_view":
			if (OrchestrationService.IsRentalSessionActive())
			{
				AddTitleProductToUserVisibleProducts(userVisibleProducts, Redbox.Rental.Services.Session.Session.Instance?.BrowseState?.CenterCarouselProduct);
			}
			break;
		case "browse_view":
		case "recommendation_on_pickup_view":
		case "upsell_view":
			if (Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseProductsShown == null)
			{
				break;
			}
			foreach (IKioskProduct item in Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseProductsShown)
			{
				AddTitleProductToUserVisibleProducts(userVisibleProducts, item);
			}
			break;
		case "product_detail_view":
			AddTitleProductToUserVisibleProducts(userVisibleProducts, Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct);
			break;
		}
		List<string> userVisibleBarcodes = GetUserVisibleBarcodes(userVisibleProducts, list);
		List<string> list2 = new List<string>();
		list2.AddRange(list);
		list2.AddRange(userVisibleBarcodes);
		return list2;
	}

	private static void CreateAndSaveNewReservation(string cardId, string email, long referenceNumber, DateTime reserveDate, bool appliedHiveOnlinePromotion, bool appliedTitleMarketing, string customerNumber, decimal discountedSubTotal, decimal grandTotal, List<int> historyTitleIDs, decimal hiveOnlinePromotionDiscount, bool isMultiNightPricing, string promoCode, decimal subTotal, decimal tax, List<ReservationTitle3> reservationTitles, bool isGc, List<string> gcIds, List<string> ccIds, decimal taxRate, string zip, bool isMdv, bool isLoyaltyRedemption, decimal defaultServiceFee, decimal actualServiceFee, bool authorizeAtPickup, List<IBarcodeForProductResult> barcodeForProductResults, ReservationCardHash cardHashData, List<ReservationCardHash> alternateCardHashes)
	{
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		List<Redbox.Rental.Model.Reservation.IReservation> allReservations = GetAllReservations();
		Redbox.Rental.Model.Reservation.IReservation reservation = null;
		string hashedCardId = cardId;
		bool flag = cardHashData != null && cardHashData.CardBinHash != null && cardHashData.Last4 != null;
		if (flag)
		{
			hashedCardId = CreateCardHash(cardHashData);
		}
		reservation = allReservations.FirstOrDefault((Redbox.Rental.Model.Reservation.IReservation x) => x.ReferenceNumber == referenceNumber.ToString());
		bool flag2 = reservation == null;
		if (flag2)
		{
			reservation = new Reservation
			{
				HashedCardId = hashedCardId,
				Id = Guid.NewGuid(),
				ReferenceNumber = referenceNumber.ToString()
			};
		}
		IReservationDetails reservationDetails = new ReservationDetails
		{
			ReferenceNumber = referenceNumber.ToString(),
			ActualServiceFee = actualServiceFee,
			AppliedHiveOnlinePromo = appliedHiveOnlinePromotion,
			AppliedTitleMarketing = appliedTitleMarketing,
			AuthorizeAtPickup = authorizeAtPickup,
			CustomerNumber = customerNumber,
			DefaultServiceFee = defaultServiceFee,
			DiscountedSubTotal = discountedSubTotal,
			EmailAddress = email,
			GrandTotal = grandTotal,
			HiveOnlinePromoDiscount = hiveOnlinePromotionDiscount,
			IsGiftCard = isGc,
			IsLoyaltyRedemption = isLoyaltyRedemption,
			IsMultiDiscVend = isMdv,
			IsMultiNightPrice = isMultiNightPricing,
			PromoCode = promoCode,
			ReservationVersion = 3,
			SubTotal = subTotal,
			Tax = tax,
			TaxRate = taxRate,
			TransactionDate = reserveDate,
			ZipCode = zip,
			PricingSet = GetPriceSetForBrokerService(isForRecommendationOnPickup: true)
		};
		if (flag && alternateCardHashes != null)
		{
			foreach (ReservationCardHash alternateCardHash in alternateCardHashes)
			{
				ccIds.Add(CreateCardHash(alternateCardHash));
			}
		}
		if (ccIds != null)
		{
			reservationDetails.CreditCardIds.AddRange(ccIds);
		}
		if (gcIds != null)
		{
			reservationDetails.GiftCardIds.AddRange(gcIds);
		}
		if (historyTitleIDs != null)
		{
			reservationDetails.HistoryTitleIds.AddRange(historyTitleIDs);
		}
		foreach (ReservationTitle3 eachReservationTitle in reservationTitles)
		{
			RentalShoppingCartItemAction action = RentalShoppingCartItemAction.rental;
			Enum.TryParse<RentalShoppingCartItemAction>(eachReservationTitle.ReservationType.ToString(), ignoreCase: true, out action);
			IBarcodeForProductResult barcodeForProductResult = barcodeForProductResults.FirstOrDefault((IBarcodeForProductResult x) => x.ProductId == eachReservationTitle.TitleID && x.Action == action);
			if (barcodeForProductResult != null)
			{
				IReservedTitle item = new ReservedTitle
				{
					TitleId = eachReservationTitle.TitleID,
					Action = action,
					Barcode = barcodeForProductResult.Barcode,
					Discount = eachReservationTitle.Discount,
					DiscountedPrice = eachReservationTitle.DiscountedPrice,
					DiscountType = eachReservationTitle.DiscountType,
					LoyaltyPoints = eachReservationTitle.LoyaltyPoints,
					Price = eachReservationTitle.Price
				};
				reservationDetails.ReservedTitles.Add(item);
			}
		}
		reservation.ReservationDetails = reservationDetails;
		LogTheNewlyCreatedReservation(reservation, flag2, reservationDetails);
		UpdateReservation(reservation);
	}

	private static string CreateCardHash(ReservationCardHash cardHashData)
	{
		string text = CardHelper.GenerateHash(cardHashData.Last4);
		return cardHashData.CardBinHash + text;
	}

	private static void LogTheNewlyCreatedReservation(Redbox.Rental.Model.Reservation.IReservation reservation, bool isNewReservation, IReservationDetails newReservationDetails)
	{
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		string text = (isNewReservation ? "Adding" : "Updating");
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"{text} reservation: id: {reservation.Id},  card id: ...{ObfuscateHashedCardId(reservation.HashedCardId)},  Reference Number: {newReservationDetails.ReferenceNumber}, ");
		stringBuilder.Append($"Transation date: {newReservationDetails.TransactionDate},  Actual Service Fee: {newReservationDetails.ActualServiceFee},  ");
		stringBuilder.Append($"Applied Hive Online Promo: {newReservationDetails.AppliedHiveOnlinePromo},  Applied Title Marketing: {newReservationDetails.AppliedTitleMarketing},  ");
		stringBuilder.Append($"Authorize At Pickup: {newReservationDetails.AuthorizeAtPickup},  Customer Number: {newReservationDetails.CustomerNumber},  ");
		stringBuilder.Append($"Default Service Fee: {newReservationDetails.DefaultServiceFee},  Hive Online Promo Discount: {newReservationDetails.HiveOnlinePromoDiscount},  ");
		stringBuilder.Append($"Is Gift Card: {newReservationDetails.IsGiftCard},  Is Loyalty Redemption: {newReservationDetails.IsLoyaltyRedemption},  ");
		stringBuilder.Append($"Is Multi Disc Vend: {newReservationDetails.IsMultiDiscVend},  Is Multi Night Price: {newReservationDetails.IsMultiNightPrice},  ");
		stringBuilder.Append($"Reservation Version: {newReservationDetails.ReservationVersion},  Promo Code: {newReservationDetails.PromoCode},  ");
		stringBuilder.Append($"Tax Rate: {newReservationDetails.TaxRate},  Tax: {newReservationDetails.Tax},  ");
		stringBuilder.Append($"Sub Total: {newReservationDetails.SubTotal},  Discounted Sub Total: {newReservationDetails.DiscountedSubTotal},  ");
		stringBuilder.Append($"Grand Total: {newReservationDetails.GrandTotal}");
		LogHelper.Instance.Log(stringBuilder.ToString());
		foreach (IReservedTitle reservedTitle in newReservationDetails.ReservedTitles)
		{
			LogHelper.Instance.Log($"Reserved title {newReservationDetails.ReservedTitles.IndexOf(reservedTitle) + 1}: Barcode: {reservedTitle.Barcode}," + $" Product Id: {reservedTitle.TitleId}, Action: {reservedTitle.Action}, Price: {reservedTitle.Price}, Discount: {reservedTitle.Discount}," + $" Discount Type: {reservedTitle.DiscountType}, Discounted Price: {reservedTitle.DiscountedPrice}, Loyalty Points: {reservedTitle.LoyaltyPoints}");
		}
	}

	private static List<string> GetUserVisibleBarcodes(List<long> userVisibleProducts, List<string> excludeBarcodes)
	{
		List<string> list = new List<string>();
		if (userVisibleProducts != null)
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			List<ISearchBarcodeForProduct> list2 = new List<ISearchBarcodeForProduct>();
			foreach (long userVisibleProduct in userVisibleProducts)
			{
				list2.Add(new SearchBarcodeForProduct
				{
					ProductId = userVisibleProduct,
					Action = RentalShoppingCartItemAction.rental
				});
			}
			foreach (IBarcodeForProductResult item in service?.GetBarcodesForProducts(list2, excludeBarcodes))
			{
				if (item.BarcodeAssigned)
				{
					list.Add(item.Barcode);
				}
			}
		}
		return list;
	}

	private static void AddTitleProductToUserVisibleProducts(List<long> userVisibleProducts, IKioskProduct kioskProduct)
	{
		if (kioskProduct is ITitleRollupProduct { KioskProducts: not null } titleRollupProduct)
		{
			{
				foreach (ITitleProduct kioskProduct2 in titleRollupProduct.KioskProducts)
				{
					userVisibleProducts.Add(kioskProduct2.ProductId);
				}
				return;
			}
		}
		if (kioskProduct is MDVProduct mDVProduct)
		{
			{
				foreach (ITitleProduct titleProduct2 in mDVProduct.TitleProducts)
				{
					userVisibleProducts.Add(titleProduct2.ProductId);
				}
				return;
			}
		}
		if (kioskProduct is ITitleProduct titleProduct)
		{
			userVisibleProducts.Add(titleProduct.ProductId);
		}
	}

	private static void AddBarcodesForMDVProducts(IRentalShoppingCart rentalShoppingCart, List<string> rentalShoppingCartBarcodes)
	{
		List<long> list = new List<long>();
		foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
		{
			if (!item.MultiDisc || !(item.TitleProduct is MDVProduct { TitleProducts: not null } mDVProduct))
			{
				continue;
			}
			foreach (ITitleProduct titleProduct in mDVProduct.TitleProducts)
			{
				if (!rentalShoppingCart.HasItem(titleProduct.ProductId))
				{
					list.Add(titleProduct.ProductId);
				}
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		List<ISearchBarcodeForProduct> list2 = new List<ISearchBarcodeForProduct>();
		foreach (long item2 in list)
		{
			list2.Add(new SearchBarcodeForProduct
			{
				ProductId = item2,
				Action = RentalShoppingCartItemAction.rental
			});
		}
		List<IBarcodeForProductResult> list3 = ServiceLocator.Instance.GetService<IDataService>()?.GetBarcodesForProducts(list2, rentalShoppingCartBarcodes);
		if (list3 == null)
		{
			return;
		}
		foreach (IBarcodeForProductResult item3 in list3.Where((IBarcodeForProductResult x) => x.BarcodeAssigned))
		{
			rentalShoppingCartBarcodes.Add(item3.Barcode);
		}
	}

	private static ICancelReservationResult CancelReservationCallback(long referenceNumber, bool cancelOnServer = true)
	{
		IReservationServices service = ServiceLocator.Instance.GetService<IReservationServices>();
		ICancelReservationResult cancelReservationResult = service.NewCancelReservationResult();
		if (cancelOnServer)
		{
			string storeNumber = ServiceLocator.Instance.GetService<IDataService>()?.GetStore()?.StoreId.ToString();
			service.Cancel(storeNumber, referenceNumber, null);
		}
		CancelReservation(referenceNumber.ToString());
		cancelReservationResult.Success = true;
		return cancelReservationResult;
	}

	private static IDictionary<string, IDictionary<string, IDictionary<string, decimal>>> GetpricesForBrokerService(bool isForRecommendationOnPickup)
	{
		Dictionary<string, IDictionary<string, IDictionary<string, decimal>>> obj = new Dictionary<string, IDictionary<string, IDictionary<string, decimal>>>
		{
			{
				"Movie",
				new Dictionary<string, IDictionary<string, decimal>>()
			},
			{
				"Game",
				new Dictionary<string, IDictionary<string, decimal>>()
			}
		};
		IPricingSet priceSetForBrokerService = GetPriceSetForBrokerService(isForRecommendationOnPickup);
		IDictionary<string, decimal> value = CreatePricingRecordDictionary(priceSetForBrokerService.PriceRecords.FirstOrDefault((IPricingRecord x) => x.TitleFamily == TitleFamily.Movies && x.TitleType == TitleType.DVD), includePurchasePrice: true);
		IDictionary<string, decimal> value2 = CreatePricingRecordDictionary(priceSetForBrokerService.PriceRecords.FirstOrDefault((IPricingRecord x) => x.TitleFamily == TitleFamily.Movies && x.TitleType == TitleType.Bluray));
		IDictionary<string, decimal> value3 = CreatePricingRecordDictionary(priceSetForBrokerService.PriceRecords.FirstOrDefault((IPricingRecord x) => x.TitleFamily == TitleFamily.Movies && x.TitleType == TitleType._4KUHD));
		IDictionary<string, decimal> value4 = CreatePricingRecordDictionary(priceSetForBrokerService.PriceRecords.FirstOrDefault((IPricingRecord x) => x.TitleFamily == TitleFamily.Games && x.TitleType == TitleType.All));
		((IDictionary<string, IDictionary<string, IDictionary<string, decimal>>>)obj)["Movie"]["default"] = value;
		((IDictionary<string, IDictionary<string, IDictionary<string, decimal>>>)obj)["Movie"]["BluRay"] = value2;
		((IDictionary<string, IDictionary<string, IDictionary<string, decimal>>>)obj)["Movie"]["_4kuhd"] = value3;
		((IDictionary<string, IDictionary<string, IDictionary<string, decimal>>>)obj)["Game"]["default"] = value4;
		return obj;
	}

	private static IPricingSet GetPriceSetForBrokerService(bool isForRecommendationOnPickup)
	{
		IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
		return new PricingSet
		{
			PriceSetId = "1",
			PriceRecords = 
			{
				service.GetProductTypePricingRecord(TitleFamily.Movies, TitleType.DVD, isForRecommendationOnPickup),
				service.GetProductTypePricingRecord(TitleFamily.Movies, TitleType.Bluray, isForRecommendationOnPickup),
				service.GetProductTypePricingRecord(TitleFamily.Movies, TitleType._4KUHD, isForRecommendationOnPickup),
				service.GetProductTypePricingRecord(TitleFamily.Games, TitleType.All, isForRecommendationOnPickup)
			}
		};
	}

	private static IDictionary<string, decimal> CreatePricingRecordDictionary(IPricingRecord pricingRecord, bool includePurchasePrice = false)
	{
		IDictionary<string, decimal> dictionary = new Dictionary<string, decimal>
		{
			{ "initial_night", pricingRecord.InitialNight },
			{ "extra_night", pricingRecord.ExtraNight },
			{ "non_return", pricingRecord.NonReturn },
			{ "non_return_days", pricingRecord.NonReturnDays },
			{ "expiration_price", pricingRecord.ExpirationPrice },
			{
				"pricesetid",
				pricingRecord.PriceSetId ?? 1
			}
		};
		if (includePurchasePrice)
		{
			dictionary["purchase_price"] = pricingRecord.Purchase;
		}
		return dictionary;
	}

	private static void CallMerchantService(string referenceNumber, bool modifiedCart, IRentalShoppingCart rentalShoppingCart, IShoppingSession session, List<Redbox.Rental.Model.Reservation.IReservation> reservations)
	{
		if (session.Status != ShoppingSessionStatus.Committed)
		{
			return;
		}
		long.TryParse(referenceNumber, out var result);
		if (IsAuthorizeAtPickup(referenceNumber, reservations))
		{
			LogHelper.Instance.Log("ReservationFunctions.ReservationPrepositionVend: Calling MerchantServices.PickupReconcile()");
			ServiceLocator.Instance.GetService<IMerchantServices>()?.PickupReconcile(rentalShoppingCart?.ShoppingSessionId.ToString(), Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.CustomerProfileNumber, result, new Dictionary<string, ReadOnlyCollection<IProductItem>>(), null);
			return;
		}
		LogHelper.Instance.Log("ReservationFunctions.ReservationPrepositionVend: Calling MerchantServices.ModResvPickup()");
		if (Redbox.Rental.Services.Session.Session.Instance?.UserInfo?.Email != null)
		{
			session.ShoppingCart.SetEmail(Redbox.Rental.Services.Session.Session.Instance?.UserInfo?.Email);
		}
		IMerchantServices service = ServiceLocator.Instance.GetService<IMerchantServices>();
		IDataService service2 = ServiceLocator.Instance.GetService<IDataService>();
		service?.ModResvPickup((service2?.GetStore()?.StoreId ?? (-1)).ToString(), Redbox.Rental.Services.Session.Session.Instance?.UserInfo?.Email, result, rentalShoppingCart?.ShoppingSessionId.ToString(), modifiedCart, null);
		OrchestrationService.ShoppingCartEndSession(rentalShoppingCart.ShoppingSessionId.ToString());
		LogHelper.Instance.Log("ReservationFunctions.ReservationPrepositionVend: Calling MerchantServices.ModResvPickup() complete");
	}

	private static void ProcessRentalShoppingCartItems(IRentalShoppingCart rentalShoppingCart, List<string> barcodesToReset)
	{
		foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
		{
			LogHelper.Instance.Log($"> local cart item: {item.Barcode}");
			if (item.Status == LineItemStatus.Pending || item.Status == LineItemStatus.ProductSelected)
			{
				LogHelper.Instance.Log($"Item not fullfilled: {item.Barcode}");
				item.Status = LineItemStatus.NotFullfilled;
			}
			if (item.Status != LineItemStatus.Fullfilled)
			{
				LogHelper.Instance.Log($"> Adding {item.Barcode} to reset barcode list");
				barcodesToReset.Add(item.Barcode);
			}
			LogHelper.Instance.Log($"Session update Line Item: {item.Barcode} for {item.Action} Price:" + $" {item.Price} {item.TitleFamily} - {item.TitleType} source type: {item.SourceType}");
			OrchestrationService.AnalyticsAddNode("onlinePickup", "cartItem", rentalShoppingCart.ShoppingSessionId);
			Dictionary<string, string> attributes = new Dictionary<string, string>
			{
				{ "barcode", item.Barcode },
				{
					"status",
					item.Status.ToString()
				}
			};
			OrchestrationService.AnalyticsAddAttribute("cartItem", attributes, rentalShoppingCart.ShoppingSessionId);
		}
	}

	private static void UpdateRentalShoppingCartItemStatuses(List<VendResult> vendResultList, IRentalShoppingCart rentalShoppingCart)
	{
		if (vendResultList != null)
		{
			foreach (VendResult eachVendResult in vendResultList)
			{
				LogHelper.Instance.Log($"> vend_results barcode: {eachVendResult?.barcode}, status: {eachVendResult?.status}");
				IRentalShoppingCartItem rentalShoppingCartItem = rentalShoppingCart.Items.FirstOrDefault((IRentalShoppingCartItem x) => x.Barcode == eachVendResult.barcode);
				if (rentalShoppingCartItem != null)
				{
					if (Enum.TryParse<LineItemStatus>(eachVendResult.status, out var result))
					{
						rentalShoppingCartItem.Status = result;
					}
					else
					{
						LogHelper.Instance.Log($"Error in RentalShoppingCartFunctions.UpdateItemStatus.  Unable to parse status value {eachVendResult.status} for barcode {rentalShoppingCartItem.Barcode}");
					}
				}
				else
				{
					LogHelper.Instance.Log($"Unable to find rental shopping cart item with barcode: {eachVendResult?.barcode}");
				}
			}
			return;
		}
		LogHelper.Instance.Log("ReservationFunctions.UpdateRentalShoppingCartItemStatuses - error vendResultList is null");
	}

	private static List<Redbox.Rental.Model.Reservation.IReservation> GetReservationsForHashedCardId(string swipedhashedCardId, string newHashedCardId, List<Redbox.Rental.Model.Reservation.IReservation> allReservations)
	{
		List<Redbox.Rental.Model.Reservation.IReservation> list = new List<Redbox.Rental.Model.Reservation.IReservation>();
		if (!string.IsNullOrEmpty(swipedhashedCardId))
		{
			list.AddRange(allReservations.Where((Redbox.Rental.Model.Reservation.IReservation r) => r.HashedCardId == swipedhashedCardId));
		}
		if (!string.IsNullOrEmpty(newHashedCardId) && newHashedCardId != swipedhashedCardId)
		{
			list.AddRange(allReservations.Where((Redbox.Rental.Model.Reservation.IReservation r) => r.HashedCardId == newHashedCardId));
		}
		if (list.Count == 0)
		{
			foreach (Redbox.Rental.Model.Reservation.IReservation allReservation in allReservations)
			{
				if (!string.IsNullOrEmpty(swipedhashedCardId))
				{
					if (allReservation.ReservationDetails.CreditCardIds.Contains(swipedhashedCardId))
					{
						LogHelper.Instance.Log($"swiped card ...{ObfuscateHashedCardId(swipedhashedCardId)} matches associated credit card");
						list.Add(allReservation);
						break;
					}
					if (allReservation.ReservationDetails.GiftCardIds.Contains(swipedhashedCardId))
					{
						LogHelper.Instance.Log($"swiped card ...{ObfuscateHashedCardId(swipedhashedCardId)} matches associated gift card");
						list.Add(allReservation);
						break;
					}
				}
				if (!string.IsNullOrEmpty(newHashedCardId) && newHashedCardId != swipedhashedCardId && allReservation.ReservationDetails.CreditCardIds.Contains(newHashedCardId))
				{
					LogHelper.Instance.Log($"swiped card ...{ObfuscateHashedCardId(newHashedCardId)} matches associated credit card");
					list.Add(allReservation);
					break;
				}
			}
		}
		return list;
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

	public static void RegisterAuthorizeOnPickupCallback(Action<string, bool> callback)
	{
		_authorizeOnPickupCallback = callback;
	}

	public static void InvokeAuthorizeOnPickupCallback(string referenceNumber, bool cartWasModified)
	{
		_authorizeOnPickupCallback?.Invoke(referenceNumber, cartWasModified);
	}
}
