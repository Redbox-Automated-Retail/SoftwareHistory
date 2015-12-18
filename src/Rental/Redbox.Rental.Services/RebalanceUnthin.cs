namespace Redbox.Rental.Services;

public class RebalanceUnthin : IKioskFile
{
	public int KioskId { get; set; }

	public string Barcode { get; set; }
}
