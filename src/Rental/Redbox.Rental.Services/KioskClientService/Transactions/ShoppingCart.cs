using System.Collections.Generic;

namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class ShoppingCart
{
	public List<LineItemGroup> Groups { get; set; }

	public List<Discount> Discounts { get; set; }

	public ShoppingCart()
	{
		Groups = new List<LineItemGroup>();
		Discounts = new List<Discount>();
	}
}
