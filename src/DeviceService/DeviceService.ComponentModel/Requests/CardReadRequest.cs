namespace DeviceService.ComponentModel.Requests;

public class CardReadRequest : ICardReadRequest
{
	public decimal? Amount { get; set; }

	public DeviceInputType InputType { get; set; }

	public int Timeout { get; set; }
}
