using System.Collections.Generic;

namespace Redbox.KioskServices.Proxy.ComponentModel;

internal interface IPromotionServices
{
	bool LiveCall { get; set; }

	string[] PromoCodes { get; set; }

	bool SimulateServerBusyError { get; set; }

	bool SimulateCommunicationsFailure { get; set; }

	void Reset();

	void ValidatePromotionCode(string storeNumber, string promotionCode, RemoteServiceCallback completeCallback);

	bool CanSwitch();

	void KioskOptIn(string storeNumber, List<IOptInItem> optInItems, bool sendCardHash, RemoteServiceCallback callback);

	IOptInItem NewOptInItem();
}
