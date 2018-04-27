using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

public interface IReservationServices
{
	bool CanSwitch();

	void Reset();

	void Pickup(string storeNumber, string emailAddress, long referenceNumber, IDictionary<string, ReadOnlyCollection<IProductItem>> pickupResults, bool isBlocklisted, RemoteServiceCallback callback);

	void Expire(string storeNumber, long referenceNumber, RemoteServiceCallback callback);

	void Register(string storeNumber, IDictionary<string, IDictionary<string, IDictionary<string, decimal>>> prices, ReservationRequestCallback reservationRequestCallback);

	void Register(string storeNumber, IDictionary<string, IDictionary<string, IDictionary<string, decimal>>> prices, ReservationRequestCallback2 reservationRequestCallback);

	void Register(string storeNumber, IDictionary<string, IDictionary<string, IDictionary<string, decimal>>> prices, ReservationRequestCallback3 reservationRequestCallback);

	void GetReservations(string storeNumber, RemoteServiceCallback completeCallback);

	IProductItem NewProductItem();

	IReservedItem NewReservedItem();

	IReservationResult NewReservationResult();

	IReservationResult SimulateCreateReservation(string cardId, string email, long referenceNumber, DateTime reserveDate, IDictionary<string, List<int>> productsToReserve, bool appliedCredit, bool appliedHiveOnlinePromotion, bool appliedTitleMarketing, string customerNumber, decimal discountedSubTotal, decimal grandTotal, List<int> historyTitleIDs, decimal hiveOnlinePromotionDiscount, bool isMultiNightPricing, int numberOfCreditsRemaining, string promoCode, decimal subTotal, decimal tax, List<ReservationTitle3> reservationTitles, bool isGc, List<string> gcIds, List<string> ccIds, decimal taxRate, string zip, bool isMdv, bool isLoyaltyRedemption, decimal defaultServiceFee, decimal actualServiceFee);
}
