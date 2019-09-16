using System.Collections.Generic;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.Promotion;

public interface IPromoCodeValidationInfo
{
	bool IsCartValid { get; set; }

	bool IsCartInvalidBecauseOfMultiNightPricing { get; set; }

	bool IsCartInvalidBecauseOfFlashDeal { get; set; }

	decimal Price { get; set; }

	decimal Percent { get; set; }

	string Title { get; set; }

	string Message { get; set; }

	IList<IPromotionItem> PromotionItems { get; set; }

	IList<IRentalShoppingCartItem> QualifyingItems { get; set; }
}
