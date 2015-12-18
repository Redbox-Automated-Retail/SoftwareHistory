namespace Redbox.Rental.Services;

public class RebalanceOut : IKioskFile
{
	public int KioskId { get; set; }

	public string Barcode { get; set; }
}
