using System;
using System.Security;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface IPersonalizationServices
{
	bool LiveCall { get; set; }

	bool SimulateSuccess { get; set; }

	KioskLoginResponseErrors ResponseCode { get; set; }

	string CustomerProfileNumber { get; set; }

	string FirstName { get; set; }

	string LoginEmail { get; set; }

	string PhoneNumber { get; set; }

	string PostalCode { get; set; }

	DateTime? BirthDate { get; set; }

	int LoyaltyPointBalance { get; set; }

	string LoyaltyTier { get; set; }

	int LoyaltyTierCounter { get; set; }

	int PointsExpiring { get; set; }

	DateTime? PointsExpirationDate { get; set; }

	void Login(int kioskId, string emailAddress, SecureString password, string phoneNumber, SecureString pin, RemoteServiceCallback completeCallback);
}
