namespace Redbox.Rental.Model.ShoppingCart;

public interface IBlurayUpsellItem
{
	long DVDProductId { get; set; }

	long BlurayProductId { get; set; }

	BlurayUpsellItemOfferResponse OfferResponse { get; set; }

	long ProductGroupId { get; set; }
}
