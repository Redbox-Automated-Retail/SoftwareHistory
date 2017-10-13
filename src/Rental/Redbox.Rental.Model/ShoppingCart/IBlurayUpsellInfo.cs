using System.Collections.Generic;

namespace Redbox.Rental.Model.ShoppingCart;

public interface IBlurayUpsellInfo
{
	bool DoNotShowAgain { get; set; }

	IList<IBlurayUpsellItem> Items { get; }

	bool DialogNeedsToBeShown { get; }

	void AddMissingBlurayUpsellItems();

	void RejectUndecidedUpsellItems();

	void RemoveUndecidedUpsellItems();

	void SetUpsellOffer(long dvdProductId, BlurayUpsellItemOfferResponse offerResponse);
}
