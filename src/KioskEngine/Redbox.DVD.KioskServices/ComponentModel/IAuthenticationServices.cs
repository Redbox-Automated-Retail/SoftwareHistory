namespace Redbox.KioskServices.Proxy.ComponentModel;

internal interface IAuthenticationServices
{
	void Reset();

	void Authenticate(string storeNumber, string userName, string password, bool useLdapAuthentication, RemoteServiceCallback completeCallback);

	bool CanSwitch();
}
