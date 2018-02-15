using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface IMerchantServices
{
	void Reconcile(string sessionId, string CustomerProfileNumber, RemoteServiceCallback callback);

	void ValidatePickup(string sessionId, List<long> referenceNumbers, RemoteServiceCallback completeCallback);

	void PreAuth(string sessionId, string CustomerProfileNumber, RemoteServiceCallback completeCallback);

	void Authorize(string sessionId, string CustomerProfileNumber, RemoteServiceCallback completeCallback);

	void GCAuthorize(string sessionId, string CustomerProfileNumber, RemoteServiceCallback completeCallback);

	void ReservationAuthorize(string sessionId, string CustomerProfileNumber, long referenceNumber, RemoteServiceCallback completeCallback);

	void ModResvPickup(string storeNumber, string emailAddress, long referenceNumber, string sessionId, bool modifiedCart, RemoteServiceCallback callback);

	bool CanSwitch();
}
