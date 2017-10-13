using System.Collections.Generic;

namespace Redbox.KioskServices.Proxy.ComponentModel;

internal interface IMerchantServices
{
	void Reconcile(string sessionId, RemoteServiceCallback callback);

	void ValidatePickup(string sessionId, List<long> referenceNumbers, RemoteServiceCallback completeCallback);

	void Authorize(string sessionId, RemoteServiceCallback completeCallback);

	void GCAuthorize(string sessionId, RemoteServiceCallback completeCallback);

	void ReservationAuthorize(string sessionId, long referenceNumber, RemoteServiceCallback completeCallback);

	bool CanSwitch();
}
