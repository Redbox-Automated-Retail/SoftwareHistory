using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface IMarketingServices
{
	ConfirmationStatus ConfirmationStatusCode { get; set; }

	string CustomerProfileNumber { get; set; }

	bool? EmailVerified { get; set; }

	bool LiveCall { get; set; }

	string MobilePhoneNumber { get; set; }

	bool PromptForEarlyId { get; set; }

	bool PromptForPerks { get; set; }

	bool SimulateCommunicationsFailure { get; set; }

	bool TextClubEnabled { get; set; }

	bool CanSwitch();

	void SendEmailConfirm(long kioskid, string emailAddress, string accountNumber, SendEmailConfirmCallback completeCallback);

	void Reset();

	void KioskOptIn(long kioskId, List<IOptInItem> optInItems, RemoteServiceCallback callback);

	IOptInItem NewOptInItem();
}
