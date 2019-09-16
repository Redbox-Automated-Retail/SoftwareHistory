using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.KioskProduct;

namespace Redbox.Rental.Services.ShoppingCart;

public class RentalShoppingCartItem : IRentalShoppingCartItem
{
	private ITitleProduct _titleProduct;

	private List<Genres> _genres = new List<Genres>();

	public long ProductId => _titleProduct.ProductId;

	public TitleType TitleType => _titleProduct.TitleType;

	public Ratings Rating => (Ratings)(_titleProduct.RatingId ?? 0);

	public DateTime MerchandiseDate => _titleProduct.MerchandiseDate;

	public List<Genres> Genres => _genres;

	public TitleFamily TitleFamily => _titleProduct.TitleFamily;

	public RentalShoppingCartItemAction Action { get; set; }

	public RentalShoppingCartItemSequence Sequence { get; set; }

	public ItemSourceType SourceType { get; set; }

	public bool MultiDisc
	{
		get
		{
			if (!(_titleProduct is MDVProduct))
			{
				return DiscNumber > 1;
			}
			return true;
		}
	}

	public int? DiscNumber => _titleProduct.DiscNumber;

	public string OfferCode { get; set; }

	public decimal Price { get; set; }

	public decimal ExtendedPrice => Price * Quantity;

	public decimal PurchasePrice => _titleProduct.PurchasePrice;

	public decimal TaxRate { get; set; }

	public string SortName => _titleProduct.SortName;

	public LineItemStatus Status { get; set; }

	public string Barcode { get; set; }

	public decimal Quantity { get; set; }

	public IPricingRecord PricingRecord { get; set; }

	public ILoyaltyPointsRecord LoyaltyPointsRecord { get; set; }

	public ITitleProduct TitleProduct => _titleProduct;

	public bool HasDeal
	{
		get
		{
			if (_titleProduct != null)
			{
				return _titleProduct.HasDeal;
			}
			return false;
		}
	}

	public RentalShoppingCartItem(ITitleProduct titleProduct)
	{
		_titleProduct = titleProduct;
	}

	public override string ToString()
	{
		return $"ProductId: {ProductId}, Title Type: {TitleType}, Rating: {Rating}, Merchandise Date: {MerchandiseDate}, Title Family: {TitleFamily}, Action: {Action}, Sequence: {Sequence}, Source Type: {SourceType}, MultiDisc: {MultiDisc}, Price: {Price}, Sort Name: {SortName}, Status: {Status}, Barcode: {Barcode}, Quantity: {Quantity}";
	}
}
