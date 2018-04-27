using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel;

public class CheckoutFunctions
{
	[KernelFunction(Name = "kernel.view_shopping_cart")]
	internal static void ViewShoppingCart()
	{
		ShoppingCartViewFunctions.ViewShoppingCart();
	}

	[KernelFunction(Name = "kernel.show_email_receipt_view")]
	internal static void ShowEmailReceiptView(string emailAddress)
	{
		PersonalizationFunctions.ShowEmailReceiptAddress(emailAddress);
	}
}
