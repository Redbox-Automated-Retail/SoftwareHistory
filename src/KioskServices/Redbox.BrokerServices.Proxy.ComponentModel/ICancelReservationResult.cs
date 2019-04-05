namespace Redbox.BrokerServices.Proxy.ComponentModel;

public interface ICancelReservationResult
{
	bool Success { get; set; }

	string ErrorMessage { get; set; }
}
