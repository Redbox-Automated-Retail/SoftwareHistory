namespace Redbox.KioskEngine.ComponentModel;

public interface IShoppingCartTotal
{
	decimal TaxRate { get; }

	decimal SubTotal { get; }

	decimal TaxAmount { get; }

	decimal GrandTotal { get; }

	decimal DiscountedSubtotal { get; }
}
