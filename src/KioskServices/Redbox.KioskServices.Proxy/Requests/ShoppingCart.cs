using System.Collections.Generic;

namespace Redbox.KioskServices.Proxy.Requests;

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
