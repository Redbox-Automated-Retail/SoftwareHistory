using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

internal sealed class ShoppingCartLineItemGroup : IShoppingCartLineItemGroup
{
	private readonly IShoppingCartTotal m_totals = new ShoppingCartTotal();

	private readonly List<IShoppingCartLineItem> m_items = new List<IShoppingCartLineItem>();

	public IShoppingCartTotal Totals => m_totals;

	public IList<IShoppingCartLineItem> Items => m_items;

	public IShoppingCartLineItem AddLineItem(string barcode, LineItemStatus status, decimal price, decimal quantity, string productFamily, string productType, ItemSourceType sourceType)
	{
		LogHelper.Instance.Log("Add line item: barcode={0}, status={1}, price={2}, quantity={3}, product_family={4}, product_type={5}, source_type={6}", barcode, status, price, quantity, productFamily, productType, sourceType);
		if (string.IsNullOrEmpty(barcode))
		{
			LogHelper.Instance.Log("...barcode is null or empty returning null.");
			return null;
		}
		ShoppingCartLineItem shoppingCartLineItem = (ShoppingCartLineItem)m_items.Find((IShoppingCartLineItem each) => string.Compare(each.Barcode, barcode, ignoreCase: true) == 0);
		if (shoppingCartLineItem == null)
		{
			LogHelper.Instance.Log("...A line item for this barcode does not exist, create it.");
			shoppingCartLineItem = new ShoppingCartLineItem();
			m_items.Add(shoppingCartLineItem);
		}
		shoppingCartLineItem.Price = price;
		shoppingCartLineItem.Status = status;
		shoppingCartLineItem.Barcode = barcode;
		shoppingCartLineItem.Quantity = quantity;
		shoppingCartLineItem.ProductFamily = productFamily;
		shoppingCartLineItem.ProductType = productType;
		shoppingCartLineItem.SourceType = sourceType;
		LogHelper.Instance.Log("...Line item added/updated.");
		return shoppingCartLineItem;
	}
}
