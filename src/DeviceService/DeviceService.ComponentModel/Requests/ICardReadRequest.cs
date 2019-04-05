namespace DeviceService.ComponentModel.Requests;

public interface ICardReadRequest
{
	decimal? Amount { get; }

	DeviceInputType InputType { get; }

	int Timeout { get; }
}
