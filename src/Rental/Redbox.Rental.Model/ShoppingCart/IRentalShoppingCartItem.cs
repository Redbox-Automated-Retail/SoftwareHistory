using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;

namespace Redbox.Rental.Model.ShoppingCart;

public interface IRentalShoppingCartItem
{
	long ProductId { get; set; }

	TitleType TitleType { get; set; }

	Ratings Rating { get; set; }

	DateTime MerchandiseDate { get; set; }

	List<Genres> Genres { get; }

	TitleFamily TitleFamily { get; set; }

	RentalShoppingCartItemAction Action { get; set; }

	RentalShoppingCartItemSequence Sequence { get; set; }

	ItemSourceType SourceType { get; set; }

	bool MultiDisc { get; set; }

	int? DiscNumber { get; set; }

	decimal Price { get; set; }

	decimal ExtendedPrice { get; }

	decimal PurchasePrice { get; set; }

	decimal TaxRate { get; set; }

	string SortName { get; set; }

	LineItemStatus Status { get; set; }

	string Barcode { get; set; }

	decimal Quantity { get; set; }

	IPricingRecord PricingRecord { get; set; }

	new string ToString();
}
