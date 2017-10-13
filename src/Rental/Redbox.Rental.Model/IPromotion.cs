using System.Collections.Generic;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model;

public interface IPromotion
{
	bool IsFreeInCartOfferRunning { get; }

	bool IsRentTwoInCartOfferRunning { get; }

	bool IsFreeInCartOfferValid { get; }

	Promotion GetPromotion();

	void SetPromotion(Promotion promotion);

	void ClearPromotion();

	void LoadInCartData();

	int GetRentTwoInCartOfferItemCount();

	void UpdateInCartPromoValue(IRentalShoppingCart rentalShoppingCart);

	bool CartSatisfiesInCartOffer(IRentalShoppingCart rentalShoppingCart);

	List<long> GetInCartIncludeTitleList();

	List<long> GetInCartExcludeTitleList();
}
