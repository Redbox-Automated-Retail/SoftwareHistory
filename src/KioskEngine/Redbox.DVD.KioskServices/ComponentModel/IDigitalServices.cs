namespace Redbox.KioskServices.Proxy.ComponentModel;

internal interface IDigitalServices
{
	void GetCustomerCredits(string sessionId, RemoteServiceCallback callback);

	void GetCustomer(string sessionId, string cardId, RemoteServiceCallback callback);

	void CreateRBIFreeTrial(string sessionId, string emailAddress, string status, RemoteServiceCallback callback);

	bool CanSwitch();
}
