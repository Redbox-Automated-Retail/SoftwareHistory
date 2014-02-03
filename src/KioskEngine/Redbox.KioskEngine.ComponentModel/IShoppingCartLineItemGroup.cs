using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public interface IShoppingCartLineItemGroup
{
	IShoppingCartTotal Totals { get; }

	IList<IShoppingCartLineItem> Items { get; }

	IShoppingCartLineItem AddLineItem(string barcode, LineItemStatus status, decimal price, decimal quantity, string productFamily, string productType, ItemSourceType sourceType);
}
