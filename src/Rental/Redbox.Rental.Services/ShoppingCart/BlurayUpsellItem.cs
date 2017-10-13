using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.ShoppingCart;

public class BlurayUpsellItem : IBlurayUpsellItem
{
	public long DVDProductId { get; set; }

	public long BlurayProductId { get; set; }

	public BlurayUpsellItemOfferResponse OfferResponse { get; set; }

	public long ProductGroupId { get; set; }
}
