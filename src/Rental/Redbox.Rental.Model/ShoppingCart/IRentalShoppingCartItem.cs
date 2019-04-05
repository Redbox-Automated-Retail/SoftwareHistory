using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.Pricing;

namespace Redbox.Rental.Model.ShoppingCart;

public interface IRentalShoppingCartItem
{
	long ProductId { get; }

	TitleType TitleType { get; }

	Ratings Rating { get; }

	DateTime MerchandiseDate { get; }

	List<Genres> Genres { get; }

	TitleFamily TitleFamily { get; }

	RentalShoppingCartItemAction Action { get; set; }

	RentalShoppingCartItemSequence Sequence { get; set; }

	ItemSourceType SourceType { get; set; }

	bool MultiDisc { get; }

	int? DiscNumber { get; }

	string OfferCode { get; set; }

	decimal Price { get; set; }

	decimal ExtendedPrice { get; }

	decimal PurchasePrice { get; }

	decimal TaxRate { get; set; }

	string SortName { get; }

	LineItemStatus Status { get; set; }

	string Barcode { get; set; }

	decimal Quantity { get; set; }

	IPricingRecord PricingRecord { get; set; }

	ILoyaltyPointsRecord LoyaltyPointsRecord { get; set; }

	ITitleProduct TitleProduct { get; }

	new string ToString();
}
