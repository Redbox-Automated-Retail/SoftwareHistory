using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.Controllers;

public class ReservationTitle
{
	public long ProductId { get; set; }

	public decimal Price { get; set; }

	public decimal Discount { get; set; }

	public RentalShoppingCartItemAction Action { get; set; }

	public int NumberOfCredits { get; set; }
}
