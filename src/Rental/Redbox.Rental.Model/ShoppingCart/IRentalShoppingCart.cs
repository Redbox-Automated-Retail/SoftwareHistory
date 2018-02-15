using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Promotion;

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

	bool InCartDoNotShow { get; set; }

	bool InCartTitleMarketing { get; set; }

	ITotals Totals { get; }

	IPromotion Promotion { get; }

	bool PickupRecommendationsShown { get; set; }

	CreditElection UseCredits { get; set; }

	bool NeedLookupOfLoyaltyPointsToBeEarned { get; }

	bool HasItemsToBeRedeemed { get; }

	event RentalShoppingCartChanged OnChange;

	int GetCount(long productId);

	void AddItem(IRentalShoppingCartItem rentalShoppingCartItem);

	void AddItem(IRentalShoppingCartItem rentalShoppingCartItem, IPricingRecord pricingRecord);

	void RemoveItem(IRentalShoppingCartItem rentalShoppingCartItem, bool userRemoved);

	void Clear();

	IRentalShoppingCartItem GetItem(int index);

	bool HasItem(long productId);

	bool HasItemWithAction(long productId, RentalShoppingCartItemAction action);

	bool HasMultiDiscItem();

	void ClearPromoCode();

	IPromoCodeValidationInfo ValidateAndUpdatePromoCode();

	void SetShoppingCartGroups();

	void ClearLoyaltyPointsToBeEarned();

	void ClearAllLoyaltyPointsRecords();

	List<IRentalShoppingCartItem> GetItemsByCriteria(IRentalShoppingCartFilterCriteria filterCriteria);

	List<IRentalShoppingCartItem> GetItemsByCriteria(RentalShoppingCartItemAction? action, RentalShoppingCartItemSequence? sequence, TitleType titleType, TitleFamily titleFamily, List<long> includeProducts, List<long> excludeProducts, List<TitleType> productTypes, bool includeMultiNightPricedItems, bool includeLoyaltyRedeemedItems);
}
