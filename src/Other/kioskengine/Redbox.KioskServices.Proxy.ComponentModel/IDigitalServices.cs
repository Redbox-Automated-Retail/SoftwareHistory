using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface IDigitalServices
{
	void GetCustomerCredits(string sessionId, RemoteServiceCallback callback);

	void GetCustomer(string sessionId, string cardId, RemoteServiceCallback callback);

	bool CanSwitch();
}
