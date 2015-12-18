using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public interface IShoppingCartLineItem
{
	decimal Price { get; }

	string Barcode { get; }

	decimal Quantity { get; }

	LineItemStatus Status { get; }

	IDictionary<string, decimal> Prices { get; }

	string ProductFamily { get; }

	string ProductType { get; }

	ItemSourceType SourceType { get; }

	IDictionary<string, object> BlurayUpsell { get; }

	bool MultiDisc { get; }

	void SetPrice(string name, decimal value);

	void SetBlurayUpsell(string name, object value);

	void SetStatus(LineItemStatus status);

	void SetMultiDisc(bool multiDisc);
}
