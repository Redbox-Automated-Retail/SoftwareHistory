namespace Redbox.Rental.Services;

public class RebalanceIn : IKioskFile
{
	public int KioskId { get; set; }

	public int TitleId { get; set; }

	public int Quantity { get; set; }
}
