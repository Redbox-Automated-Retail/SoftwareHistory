using Redbox.BrokerServices.Proxy.ComponentModel;

namespace Redbox.BrokerServices.Proxy;

public class LocalCancelReservationResult : ICancelReservationResult
{
	public bool Success { get; set; }

	public string ErrorMessage { get; set; }
}
