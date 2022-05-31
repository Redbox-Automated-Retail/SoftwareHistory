using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.Rental.Model.KioskClientService.Transactions;

public interface IKioskClientServiceTransaction
{
	void Authorize(Guid sessionId, AuthorizeType authorizeType, string customerProfileNumber, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback completeCallback);

	Task<IAuthorizeResult> AuthorizeAsync(Guid sessionId, AuthorizeType authorizeType, string customerProfileNumber);

	void Return(string storeNumber, ReturnType returnType, string barcode, string returnDate, int deck, int slot, bool failedSecurityRead, string fraudFileName, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback completeCallback);

	Task<IReturnResult> ReturnAsync(string storeNumber, ReturnType returnType, string barcode, string returnDate, int deck, int slot, bool failedSecurityRead, string fraudFileName);

	void PickupAuthorize(string sessionId, string customerProfileNumber, long referenceNumber, bool cartWasModified, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback completeCallback);

	Task<IAuthorizeResult> PickupAuthorizeAsync(string sessionIdString, string customerProfileNumber, long referenceNumber, bool cartWasModified);

	void ReservationAuthorize(string sessionId, string customerProfileNumber, long referenceNumber, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback completeCallback);

	Task<IAuthorizeResult> ReservationAuthorizeAsync(string sessionIdString, string customerProfileNumber, long referenceNumber);

	void CancelReservation(string storeNumber, long referenceNumber, string reasonCode, Redbox.BrokerServices.Proxy.ComponentModel.RemoteServiceCallback callback);

	Task<Redbox.KioskEngine.ComponentModel.KioskServices.ICancelReservationResult> CancelReservationAsync(string storeNumber, long referenceNumber, string reasonCode);

	void ExpireReservation(string storeNumber, long referenceNumber, Redbox.BrokerServices.Proxy.ComponentModel.RemoteServiceCallback callback);

	Task<IExpireReservationResult> ExpireReservationAsync(string storeNumber, long referenceNumber);

	void Reconcile(string sessionId, string customerProfileNumber, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback callback);

	Task<IReconcileResult> ReconcileAsync(string sessionId, string customerProfileNumber);

	void ReservationPickup(string sessionId, string customerProfileNumber, string emailAddress, long referenceNumber, IDictionary<string, ReadOnlyCollection<IProductItem>> pickupResults, bool isBlocklisted, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback callback);

	Task<IReconcileResult> ReservationPickupAsync(string sessionId, string customerProfileNumber, string emailAddress, long referenceNumber, IDictionary<string, ReadOnlyCollection<IProductItem>> pickupResults, bool isBlocklisted);

	Task<IGetCustomerOffersResult> GetCustomerOffersAsync(string kioskId, Dictionary<string, object> card, Dictionary<string, object> cart, string utcOffset, string lang, string storefrontEventType);

	void ValidatePickup(string sessionId, List<long> referenceNumbers, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback callback);

	Task<IPickupValidationResult> ValidatePickupAsync(string sessionId, List<long> referenceNumbers);

	void ReturnPromoCheck(string barcode, Action<ReturnVisitPromotionInfo> completeCallback);

	Task<ReturnVisitPromotionInfo> ReturnPromoCheckAsync(string barcode);
}
