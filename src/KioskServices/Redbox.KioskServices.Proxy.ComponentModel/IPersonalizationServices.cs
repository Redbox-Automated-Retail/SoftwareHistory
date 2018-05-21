using System.Security;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface IPersonalizationServices
{
	string CustomerProfileNumber { get; set; }

	void Login(int kioskId, string emailAddress, SecureString password, string phoneNumber, SecureString pin, RemoteServiceCallback completeCallback);

	void UpdateAccount(int kioskId, string customerProfileNumber, string emailAddress, string phoneNumber, SecureString pin, bool punchcardOptIn, bool textClubOptIn, RemoteServiceCallback completeCallback);
}
