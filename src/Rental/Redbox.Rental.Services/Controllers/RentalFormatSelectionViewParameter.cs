using Redbox.Rental.Services.KioskProduct;

namespace Redbox.Rental.Services.Controllers;

public class RentalFormatSelectionViewParameter
{
	public bool IsPurchase { get; set; }

	public TitleRollupProduct RollupProduct { get; set; }

	public RentalFormatSelectionViewParameter(TitleRollupProduct rollupProduct, bool isPurchase)
	{
		RollupProduct = rollupProduct;
		IsPurchase = isPurchase;
	}
}
