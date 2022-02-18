using System;
using System.Linq;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.ShoppingCart;

public class SubTotalGroup : ISubTotalGroup
{
	private IRentalShoppingCart _rentalShoppingCart;

	public RentalShoppingCartItemAction Action { get; set; }

	public decimal Amount => Math.Max((_rentalShoppingCart?.TitleItems?.Where((IRentalShoppingCartTitleItem x) => x.Action == Action && (x.Status == LineItemStatus.Pending || x.Status == LineItemStatus.ProductSelected || x.Status == LineItemStatus.Fullfilled))?.Sum((IRentalShoppingCartTitleItem x) => x.ExtendedPrice) ?? 0m) + (_rentalShoppingCart?.SubscriptionItems?.Where((IRentalShoppingCartSubscriptionItem x) => x.Action == Action)?.Sum((IRentalShoppingCartSubscriptionItem x) => x.ExtendedPrice) ?? 0m), 0m);

	public decimal? DiscountedAmount
	{
		get
		{
			decimal? result = null;
			IPromotion promotion = _rentalShoppingCart.Promotion;
			decimal num = ((promotion?.CartItemAction != Action) ? 0m : (promotion?.Price ?? 0m));
			decimal num2 = _rentalShoppingCart?.TitleItems?.Where((IRentalShoppingCartTitleItem x) => x.Action == Action && (x.Status == LineItemStatus.Pending || x.Status == LineItemStatus.ProductSelected || x.Status == LineItemStatus.Fullfilled) && (x.LoyaltyPointsRecord?.ToBeRedeemed ?? false))?.Sum((IRentalShoppingCartTitleItem x) => x.ExtendedPrice) ?? 0m;
			if (num2 > 0m || num > 0m)
			{
				result = Amount - num - num2;
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
			decimal d = 0m;
			if (Action != RentalShoppingCartItemAction.subscription)
			{
				d = ((!HasDigitalCodes) ? (SubTotal * TaxRate) : (DigitalPurchaseAmount * DigitalTaxRate + NonDigitalPurchaseAmount * NonDigitalTaxRate));
			}
			else
			{
				foreach (IRentalShoppingCartSubscriptionItem subscriptionItem in _rentalShoppingCart.SubscriptionItems)
				{
					d += subscriptionItem.Price * subscriptionItem.TaxRate;
				}
			}
			return decimal.Round(d, 16, MidpointRounding.AwayFromZero);
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
			if (Action == RentalShoppingCartItemAction.subscription)
			{
				return _rentalShoppingCart?.SubscriptionItems?.FirstOrDefault().TaxRate ?? 0m;
			}
			return _rentalShoppingCart?.TitleItems?.FirstOrDefault((IRentalShoppingCartTitleItem x) => x.Action == Action)?.TaxRate ?? 0m;
		}
	}

	private bool HasDigitalCodes => _rentalShoppingCart?.TitleItems?.Any((IRentalShoppingCartTitleItem x) => x.Action == Action && x.TitleType == TitleType.DigitalCode) ?? false;

	private bool HasNonDigitalCodes => _rentalShoppingCart?.TitleItems?.Any((IRentalShoppingCartTitleItem x) => x.Action == Action && x.TitleType != TitleType.DigitalCode) ?? false;

	public decimal DigitalTaxRate => _rentalShoppingCart?.TitleItems?.FirstOrDefault((IRentalShoppingCartTitleItem x) => x.Action == Action && x.TitleType == TitleType.DigitalCode)?.TaxRate ?? 0m;

	public decimal NonDigitalTaxRate => _rentalShoppingCart?.TitleItems?.FirstOrDefault((IRentalShoppingCartTitleItem x) => x.Action == Action && x.TitleType != TitleType.DigitalCode)?.TaxRate ?? 0m;

	private decimal DigitalPurchaseAmount => Math.Max(_rentalShoppingCart?.TitleItems?.Where((IRentalShoppingCartTitleItem x) => x.Action == Action && x.TitleType == TitleType.DigitalCode && (x.Status == LineItemStatus.Pending || x.Status == LineItemStatus.ProductSelected || x.Status == LineItemStatus.Fullfilled))?.Sum((IRentalShoppingCartTitleItem x) => x.ExtendedPrice) ?? 0m, 0m);

	private decimal NonDigitalPurchaseAmount => Math.Max(_rentalShoppingCart?.TitleItems?.Where((IRentalShoppingCartTitleItem x) => x.Action == Action && x.TitleType != TitleType.DigitalCode && (x.Status == LineItemStatus.Pending || x.Status == LineItemStatus.ProductSelected || x.Status == LineItemStatus.Fullfilled))?.Sum((IRentalShoppingCartTitleItem x) => x.ExtendedPrice) ?? 0m, 0m);

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
