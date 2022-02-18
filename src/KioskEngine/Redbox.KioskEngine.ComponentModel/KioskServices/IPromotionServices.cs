namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public interface IPromotionServices
{
	bool LiveCall { get; set; }

	string[] PromoCodes { get; set; }

	bool SimulateServerBusyError { get; set; }

	bool SimulateCommunicationsFailure { get; set; }

	void Reset();

	void ValidatePromotionCode(string storeNumber, string promotionCode, RemoteServiceCallback completeCallback);

	bool CanSwitch();
}
