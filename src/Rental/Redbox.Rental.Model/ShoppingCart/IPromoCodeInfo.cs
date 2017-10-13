namespace Redbox.Rental.Model.ShoppingCart;

public interface IPromoCodeInfo
{
	bool IsCartValid { get; set; }

	string Title { get; set; }

	string Message { get; set; }
}
