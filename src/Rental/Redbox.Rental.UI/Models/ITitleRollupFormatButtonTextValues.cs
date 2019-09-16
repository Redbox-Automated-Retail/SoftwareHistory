namespace Redbox.Rental.UI.Models;

public interface ITitleRollupFormatButtonTextValues
{
	string Unavailable { get; }

	string InCartText { get; }

	string OutOfStockFormat { get; }

	string OutOfStockText { get; }

	string Buy { get; }

	string Add { get; }

	string CurrencySymbol { get; }

	string RentalPriceFormat { get; }

	string RentalPriceFormatNoCurrency { get; }

	string InCartFormat { get; }
}
