using System;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.ShoppingCart;

public class SubTotalGroup : ISubTotalGroup
{
	private IRentalShoppingCart _rentalShoppingCart;

	public RentalShoppingCartItemAction Action { get; set; }

	public decimal Amount => Math.Max(_rentalShoppingCart?.Items?.Where((IRentalShoppingCartItem x) => x.Action == Action && (x.Status == LineItemStatus.Pending || x.Status == LineItemStatus.ProductSelected || x.Status == LineItemStatus.Fullfilled))?.Sum((IRentalShoppingCartItem x) => x.ExtendedPrice) ?? 0m, 0m);

	public decimal? DiscountedAmount
	{
		get
		{
			decimal? result = null;
			Promotion promotion = _rentalShoppingCart.Promotion;
			if (Action == RentalShoppingCartItemAction.rental && promotion != null)
			{
				decimal amount = Amount;
				if (promotion.Intent == PromotionIntentCode.PercentOffCartTotal)
				{
					decimal num = decimal.Round(amount * promotion.Percent, 2, MidpointRounding.AwayFromZero);
					result = amount - num;
					promotion.Price = num;
					(ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetSession(_rentalShoppingCart.ShoppingSessionId.ToString()))?.ShoppingCart?.SetDiscount(promotion.Name, promotion.Price);
				}
				else
				{
					result = amount - (decimal)promotion.Quantity * promotion.Price;
				}
			}
			if (result.HasValue)
			{
				result = Math.Max(decimal.Round(result.Value, 16, MidpointRounding.AwayFromZero), 0m);
			}
			return result;
		}
	}

	public decimal DiscountAmountUsed
	{
		get
		{
			decimal result = 0m;
			decimal? discountedAmount = DiscountedAmount;
			if (discountedAmount.HasValue)
			{
				return Math.Max(decimal.Round(Amount - discountedAmount.Value, 16, MidpointRounding.AwayFromZero), 0m);
			}
			return result;
		}
	}

	public decimal TaxAmount
	{
		get
		{
			if (HasDigitalCodes)
			{
				return decimal.Round(DigitalPurchaseAmount * DigitalTaxRate + NonDigitalPurchaseAmount * NonDigitalTaxRate, 16, MidpointRounding.AwayFromZero);
			}
			return decimal.Round(SubTotal * TaxRate, 16, MidpointRounding.AwayFromZero);
		}
	}

	public decimal TaxRate
	{
		get
		{
			if (HasDigitalCodes)
			{
				if (!HasNonDigitalCodes)
				{
					return DigitalTaxRate;
				}
				return NonDigitalTaxRate;
			}
			return _rentalShoppingCart?.Items?.FirstOrDefault((IRentalShoppingCartItem x) => x.Action == Action)?.TaxRate ?? 0m;
		}
	}

	private bool HasDigitalCodes => _rentalShoppingCart?.Items?.Any((IRentalShoppingCartItem x) => x.Action == Action && x.TitleType == TitleType.DigitalCode) ?? false;

	private bool HasNonDigitalCodes => _rentalShoppingCart?.Items?.Any((IRentalShoppingCartItem x) => x.Action == Action && x.TitleType != TitleType.DigitalCode) ?? false;

	public decimal DigitalTaxRate => _rentalShoppingCart?.Items?.FirstOrDefault((IRentalShoppingCartItem x) => x.Action == Action && x.TitleType == TitleType.DigitalCode)?.TaxRate ?? 0m;

	public decimal NonDigitalTaxRate => _rentalShoppingCart?.Items?.FirstOrDefault((IRentalShoppingCartItem x) => x.Action == Action && x.TitleType != TitleType.DigitalCode)?.TaxRate ?? 0m;

	private decimal DigitalPurchaseAmount => Math.Max(_rentalShoppingCart?.Items?.Where((IRentalShoppingCartItem x) => x.Action == Action && x.TitleType == TitleType.DigitalCode && (x.Status == LineItemStatus.Pending || x.Status == LineItemStatus.ProductSelected || x.Status == LineItemStatus.Fullfilled))?.Sum((IRentalShoppingCartItem x) => x.ExtendedPrice) ?? 0m, 0m);

	private decimal NonDigitalPurchaseAmount => Math.Max(_rentalShoppingCart?.Items?.Where((IRentalShoppingCartItem x) => x.Action == Action && x.TitleType != TitleType.DigitalCode && (x.Status == LineItemStatus.Pending || x.Status == LineItemStatus.ProductSelected || x.Status == LineItemStatus.Fullfilled))?.Sum((IRentalShoppingCartItem x) => x.ExtendedPrice) ?? 0m, 0m);

	public decimal SubTotal
	{
		get
		{
			if (!DiscountedAmount.HasValue)
			{
				return Amount;
			}
			return DiscountedAmount.Value;
		}
	}

	public decimal GrantTotal => SubTotal + TaxAmount;

	public SubTotalGroup(IRentalShoppingCart rentalShoppingCart)
	{
		_rentalShoppingCart = rentalShoppingCart;
	}
}
