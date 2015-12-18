using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

internal sealed class ShoppingCartLineItem : IShoppingCartLineItem
{
	private readonly IDictionary<string, decimal> m_prices = new Dictionary<string, decimal>();

	private readonly IDictionary<string, object> m_blurayUpsell = new Dictionary<string, object>();

	public decimal Price { get; internal set; }

	public string Barcode { get; internal set; }

	public decimal Quantity { get; internal set; }

	public LineItemStatus Status { get; internal set; }

	public IDictionary<string, decimal> Prices => m_prices;

	public string ProductFamily { get; internal set; }

	public string ProductType { get; internal set; }

	public ItemSourceType SourceType { get; internal set; }

	public IDictionary<string, object> BlurayUpsell => m_blurayUpsell;

	public bool MultiDisc { get; internal set; }

	public void SetPrice(string name, decimal value)
	{
		Prices[name] = value;
	}

	public void SetBlurayUpsell(string name, object value)
	{
		BlurayUpsell[name] = value;
	}

	public void SetStatus(LineItemStatus status)
	{
		Status = status;
	}

	public void SetMultiDisc(bool multiDisc)
	{
		MultiDisc = multiDisc;
	}
}
