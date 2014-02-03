using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

internal sealed class ShoppingCartTotal : IShoppingCartTotal
{
	public decimal TaxRate { get; internal set; }

	public decimal SubTotal { get; internal set; }

	public decimal TaxAmount { get; internal set; }

	public decimal GrandTotal { get; internal set; }

	public decimal DiscountedSubtotal { get; internal set; }
}
