using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

public interface IReservationServices
{
	bool CanSwitch();

	void Reset();

	void Expire(string storeNumber, long referenceNumber, RemoteServiceCallback callback);

	void Cancel(string storeNumber, long referenceNumber, RemoteServiceCallback callback);

	void Register(string storeNumber, IDictionary<string, IDictionary<string, IDictionary<string, decimal>>> prices, ReservationRequestCallback3 reservationRequestCallback, CancelReservationCallback cancelRequestCallback);

	void GetReservations(string storeNumber, RemoteServiceCallback completeCallback);

	void ValidateReservation(string storeNumber, long referenceNumber, RemoteServiceCallback callback);

	IProductItem NewProductItem();

	IReservedItem NewReservedItem();

	IReservationResult NewReservationResult();

	ILocalCancelReservationResult NewCancelReservationResult();

	IReservationResult SimulateCreateReservation(string cardId, string email, long referenceNumber, DateTime reserveDate, IDictionary<string, List<int>> productsToReserve, bool appliedCredit, bool appliedHiveOnlinePromotion, bool appliedTitleMarketing, string customerNumber, decimal discountedSubTotal, decimal grandTotal, List<int> historyTitleIDs, decimal hiveOnlinePromotionDiscount, bool isMultiNightPricing, int numberOfCreditsRemaining, string promoCode, decimal subTotal, decimal tax, List<ReservationTitle3> reservationTitles, bool isGc, List<string> gcIds, List<string> ccIds, decimal taxRate, string zip, bool isMdv, bool isLoyaltyRedemption, decimal defaultServiceFee, decimal actualServiceFee, bool authorizeAtPickup, ReservationCardHash cardHashData, List<ReservationCardHash> alternateCardHashes);

	ILocalCancelReservationResult CancelReservation(long referenceNumber);

	void ProcessValidationResponse(IMessage message, IDictionary<string, object> result, object clientData);
}
