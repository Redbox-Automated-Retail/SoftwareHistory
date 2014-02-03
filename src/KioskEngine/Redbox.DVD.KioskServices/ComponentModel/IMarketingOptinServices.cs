namespace Redbox.KioskServices.Proxy.ComponentModel;

internal interface IMarketingOptinServices
{
	bool LiveCall { get; set; }

	bool SimulateCommunicationsFailure { get; set; }

	void SendEmailConfirm(string storeNumber, string emailAddress, RemoteServiceCallback completeCallback);

	bool CanSwitch();

	void Reset();
}
