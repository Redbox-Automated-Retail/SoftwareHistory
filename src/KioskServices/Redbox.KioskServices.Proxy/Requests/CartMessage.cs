namespace Redbox.KioskServices.Proxy.Requests;

public class CartMessage : MessageBase
{
	public string CustomerProfileNumber { get; set; }

	public ShoppingCart ShoppingCart { get; set; }

	public CartMessage()
	{
		ShoppingCart = new ShoppingCart();
	}
}
