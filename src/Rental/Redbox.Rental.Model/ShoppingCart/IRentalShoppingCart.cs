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

	IUpsellInfo UpsellInfo { get; }

	IComingSoonOptinInfo ComingSoonOptInInfo { get; }

	bool InCartPromoOfferMade { get; set; }

	bool InCartPromoOfferCancel { get; set; }

	bool InCartDoNotShow { get; set; }

	bool InCartTitleMarketing { get; set; }

	ITotals Totals { get; }

	IPromotion Promotion { get; }

	bool PickupRecommendationsShown { get; set; }

	bool PickupRecommendationsSelected { get; }

	OptInConfirmationStatus MarketingOptInConfirmationStatus { get; set; }

	bool NeedLookupOfLoyaltyPointsToBeEarned { get; }

	bool HasItemsToBeRedeemed { get; }

	IServiceFee ServiceFee { get; set; }

	bool ServiceFeeEnabled { get; }

	decimal DefaultServiceFee { get; }

	event RentalShoppingCartChanged OnChange;

	int GetCount(long productId);

	void Abandon(string reason);

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

	void ReValidatePromoCodeAfterVend();

	void SetShoppingCartGroups();

	void ClearLoyaltyPointsToBeEarned();

	void ClearAllLoyaltyPointsRecords();

	List<IRentalShoppingCartItem> GetItemsByCriteria(IRentalShoppingCartFilterCriteria filterCriteria);

	List<IRentalShoppingCartItem> GetItemsByCriteria(RentalShoppingCartItemAction? action, RentalShoppingCartItemSequence? sequence, TitleType titleType, TitleFamily titleFamily, List<long> includeProducts, List<long> excludeProducts, List<TitleType> productTypes, int? productTypeId, List<int> formatIds, bool includeMultiNightPricedItems, bool includeLoyaltyRedeemedItems);

	void UpdateServiceFee();

	void SetDefaultServiceFee(decimal defaultServiceFee);

	void AddCartItemToAnalytics(IRentalShoppingCartItem rentalShoppingCartItem, bool userRemoved, ISessionAnalyticsService analytics);
}
