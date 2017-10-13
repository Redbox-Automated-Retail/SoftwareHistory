using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Model.ShoppingCart;

public interface IRentalShoppingCart
{
	Guid ShoppingSessionId { get; }

	int Count { get; }

	bool IsFull { get; }

	IList<IRentalShoppingCartItem> Items { get; }

	int MaximumCapacity { get; }

	IBlurayUpsellInfo BlurayUpsellInfo { get; }

	IComingSoonOptinInfo ComingSoonOptInInfo { get; }

	bool InCartPromoOfferMade { get; set; }

	bool InCartPromoOfferCancel { get; set; }

	bool InCartTitleMarketing { get; set; }

	ITotals Totals { get; }

	Promotion Promotion { get; }

	bool PickupRecommendationsShown { get; set; }

	CreditElection UseCredits { get; set; }

	event RentalShoppingCartChanged OnChange;

	int GetCount(long productId);

	void AddItem(IRentalShoppingCartItem rentalShoppingCartItem);

	void RemoveItem(IRentalShoppingCartItem rentalShoppingCartItem, bool userRemoved);

	void Clear();

	IRentalShoppingCartItem GetItem(int index);

	bool HasItem(long productId);

	bool HasItemWithAction(long productId, RentalShoppingCartItemAction action);

	bool HasMultiDiscItem();

	void ClearPromoCode();

	IPromoCodeInfo ValidateAndUpdatePromoCode();

	void SetShoppingCartGroups();
}
