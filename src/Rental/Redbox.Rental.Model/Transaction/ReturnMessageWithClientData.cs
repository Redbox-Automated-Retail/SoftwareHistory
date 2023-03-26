namespace Redbox.Rental.Model.Transaction;

public class ReturnMessageWithClientData
{
	public ReturnMessageClientData ClientData { get; set; }

	public ReturnMessage ReturnMessage { get; set; }
}
