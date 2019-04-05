using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
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

	[KernelFunction(Name = "kernel.begin_after_swipe_loyalty_signup_flow")]
	internal static void BeginAfterSwipeLoyaltySignUpFlow(Action continueCheckoutCallback)
	{
		LoyaltySignUpController.BeginAfterSwipeLoyaltySignUpFlow(continueCheckoutCallback);
	}

	[KernelFunction(Name = "kernel.is_current_card_on_offline_bad_card_list")]
	internal static bool IsCurrentCardOnOfflineBadCardList()
	{
		return Redbox.Rental.Services.Controllers.CheckoutFunctions.IsCurrentCardOnOfflineBadCardList();
	}

	[KernelFunction(Name = "kernel.update_account_on_offline_bad_card_list")]
	internal static void UpdateAccountOnOfflineBadCardList()
	{
		Redbox.Rental.Services.Controllers.CheckoutFunctions.UpdateAccountOnOfflineBadCardList();
	}

	[KernelFunction(Name = "kernel.remove_account_from_offline_bad_card_list")]
	internal static void RemoveAccountFromOfflineBadCardList()
	{
		Redbox.Rental.Services.Controllers.CheckoutFunctions.RemoveAccountFromOfflineBadCardList();
	}

	[KernelFunction(Name = "kernel.check_offline_validations")]
	internal static LuaTable CheckOfflineValidations()
	{
		LuaTable result = null;
		string response = null;
		if (!Redbox.Rental.Services.Controllers.CheckoutFunctions.CheckOfflineValidations(out response))
		{
			result = new Dictionary<string, object>
			{
				["success"] = false,
				["response"] = response
			}.ToLuaTable();
		}
		return result;
	}

	[KernelFunction(Name = "kernel.handle_auth_error")]
	internal static void HandleAuthError(LuaTable properties, string shoppingCartId)
	{
		Redbox.Rental.Services.Controllers.CheckoutFunctions.ProcessAuthError(properties, shoppingCartId);
	}

	[KernelFunction(Name = "kernel.show_multi_disc_vend_view_if_needed")]
	internal static void ShowMultiDiscVendViewIfNeeded(Action continueCheckoutCallback)
	{
		Redbox.Rental.Services.Controllers.CheckoutFunctions.ShowMultiDiscVendViewIfNeeded(continueCheckoutCallback);
	}

	[KernelFunction(Name = "kernel.show_reservation_details_view")]
	internal static void ShowReservationDetailsView(Action continueReservationPickupCallback)
	{
		Redbox.Rental.Services.Controllers.CheckoutFunctions.ShowReservationDetailsView(continueReservationPickupCallback);
	}
}
