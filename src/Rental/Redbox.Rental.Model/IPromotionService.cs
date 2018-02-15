using System;
using System.Collections.Generic;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model;

public interface IPromotionService
{
	bool IsFreeInCartOfferRunning { get; }

	bool NeedToShowFreeInCartMessage { get; }

	bool IsRentTwoInCartOfferRunning { get; }

	bool NeedToShowRentTwoInCartMessage { get; }

	IPromotion GetPromotion();

	void SetPromotion(IPromotion promotion);

	void ClearPromotion();

	void LoadInCartData();

	IPromoCodeValidationInfo ValidateFreeInCartOffer();

	IPromoCodeValidationInfo ValidateRentTwoInCartPromo();

	IPromoCodeValidationInfo ValidateRegularInCartPromo(IRentalShoppingCart rentalShoppingCart);

	List<long> GetInCartIncludeTitleList();

	List<long> GetInCartExcludeTitleList();

	bool CartQualifiesForInCartOffer(IRentalShoppingCart rentalShoppingCart);

	InCartOfferMessageText GetInCartOfferMessageText();

	void SetFreeInCartPromo(IRentalShoppingCart rentalShoppingCart);

	void SetRentTwoInCartPromo(IRentalShoppingCart rentalShoppingCart);

	void SetPromoToFixedPriceInCart(IRentalShoppingCart rentalShoppingCart, decimal promoValue);

	IPromotionRule GetInCartPromoActionRule();

	void ValidatePromoCodeOnServer(string promoCode, Action<IPromoCodeValidationResult> promoCodeValidationResultCallback);

	string GetInCartPromoCode();

	void SetShoppingCartDiscount(string shoppingSessionId, IPromotion newPromotion);
}
