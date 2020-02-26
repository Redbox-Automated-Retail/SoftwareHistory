namespace Redbox.Rental.UI.Models;

public interface ITitleRollupFormatButtonTextValues
{
	string Unavailable { get; }

	string InCartText { get; }

	string OutOfStockFormat { get; }

	string OutOfStockText { get; }

	string Add { get; }

	string CurrencySymbol { get; }

	string InCartFormat { get; }
}
