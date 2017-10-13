using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface IAuthenticationServices
{
	void Reset();

	void Authenticate(string storeNumber, string userName, string password, bool useLdapAuthentication, RemoteServiceCallback completeCallback);

	bool CanSwitch();
}
