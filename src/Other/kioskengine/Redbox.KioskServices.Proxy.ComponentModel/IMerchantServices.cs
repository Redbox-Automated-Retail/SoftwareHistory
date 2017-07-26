using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface IMerchantServices
{
	void Reconcile(string sessionId, RemoteServiceCallback callback);

	void ValidatePickup(string sessionId, List<long> referenceNumbers, RemoteServiceCallback completeCallback);

	void PreAuth(string sessionId, RemoteServiceCallback completeCallback);

	void Authorize(string sessionId, RemoteServiceCallback completeCallback);

	void GCAuthorize(string sessionId, RemoteServiceCallback completeCallback);

	void ReservationAuthorize(string sessionId, long referenceNumber, RemoteServiceCallback completeCallback);

	bool CanSwitch();
}
