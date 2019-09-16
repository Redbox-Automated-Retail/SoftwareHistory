using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface IMerchantServices
{
	void Reconcile(string sessionId, string CustomerProfileNumber, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback callback);

	void PickupReconcile(string sessionId, string customerProfileNumber, long referenceNumber, IDictionary<string, ReadOnlyCollection<IProductItem>> pickupResults, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback callback);

	void ValidatePickup(string sessionId, List<long> referenceNumbers, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback completeCallback);

	void PreAuth(Guid sessionId, string CustomerProfileNumber, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback completeCallback);

	void Authorize(Guid sessionId, AuthorizeType authorizeType, string CustomerProfileNumber, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback completeCallback);

	void ReservationAuthorize(string sessionId, string CustomerProfileNumber, long referenceNumber, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback completeCallback);

	void PickupAuthorize(string sessionId, string customerProfileNumber, long referenceNumber, bool cartWasModified, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback completeCallback);

	void ModResvPickup(string storeNumber, string emailAddress, long referenceNumber, string sessionId, bool modifiedCart, Redbox.KioskEngine.ComponentModel.KioskServices.RemoteServiceCallback callback);

	bool CanSwitch();
}
