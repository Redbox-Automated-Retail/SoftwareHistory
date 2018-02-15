using System;
using System.Security;

namespace Redbox.Rental.Model.Personalization;

public interface IPersonalizationService
{
	bool IsEarlyIdEnabled { get; }

	bool IsLoyaltyEnabled { get; }

	void LoginWithEmail(string emailAddress, SecureString password, Action<ILoginResult> loginResultCallback);

	void LoginWithPhoneNumber(string phoneNumber, SecureString pin, Action<ILoginResult> loginResultCallback);
}
