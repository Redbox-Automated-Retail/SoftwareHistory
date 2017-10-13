using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.ShoppingCart;

public class RentalShoppingCartItem : IRentalShoppingCartItem
{
	private List<Genres> _genres = new List<Genres>();

	public long ProductId { get; set; }

	public TitleType TitleType { get; set; }

	public Ratings Rating { get; set; }

	public DateTime MerchandiseDate { get; set; }

	public List<Genres> Genres => _genres;

	public TitleFamily TitleFamily { get; set; }

	public RentalShoppingCartItemAction Action { get; set; }

	public RentalShoppingCartItemSequence Sequence { get; set; }

	public ItemSourceType SourceType { get; set; }

	public bool MultiDisc { get; set; }

	public int? DiscNumber { get; set; }

	public decimal Price { get; set; }

	public decimal ExtendedPrice => Price * Quantity;

	public decimal PurchasePrice { get; set; }

	public decimal TaxRate { get; set; }

	public string SortName { get; set; }

	public LineItemStatus Status { get; set; }

	public string Barcode { get; set; }

	public decimal Quantity { get; set; }

	public IPricingRecord PricingRecord { get; set; }

	public override string ToString()
	{
		return $"ProductId: {ProductId}, Title Type: {TitleType}, Rating: {Rating}, Merchandise Date: {MerchandiseDate}, Title Family: {TitleFamily}, Action: {Action}, Sequence: {Sequence}, Source Type: {SourceType}, MultiDisc: {MultiDisc}, Price: {Price}, Sort Name: {SortName}, Status: {Status}, Barcode: {Barcode}, Quantity: {Quantity}";
	}
}
