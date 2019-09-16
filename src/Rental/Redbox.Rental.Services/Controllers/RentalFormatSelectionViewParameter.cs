using System;
using Redbox.Rental.Services.KioskProduct;

namespace Redbox.Rental.Services.Controllers;

public class RentalFormatSelectionViewParameter
{
	public bool IsPurchase { get; set; }

	public TitleRollupProduct RollupProduct { get; set; }

	public Action CancelAction { get; set; }

	public bool ShowMnp { get; set; }

	public Func<decimal, decimal> DiscountedPriceCalc { get; set; }

	public RentalFormatSelectionViewParameter(TitleRollupProduct rollupProduct, bool isPurchase, Action cancelAction = null, bool showMnp = true, Func<decimal, decimal> discountedPriceCalc = null)
	{
		RollupProduct = rollupProduct;
		IsPurchase = isPurchase;
		CancelAction = cancelAction;
		ShowMnp = showMnp;
		DiscountedPriceCalc = discountedPriceCalc;
	}
}
