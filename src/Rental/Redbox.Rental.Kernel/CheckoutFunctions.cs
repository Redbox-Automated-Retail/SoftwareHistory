using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment.KioskServices;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel;

public class CheckoutFunctions
{
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

	[KernelFunction(Name = "kernel.device_service_report_auth_result")]
	internal static void DeviceServiceReportAuthorizeResult(bool authorizeSuccessful)
	{
		Redbox.Rental.Services.Controllers.CheckoutFunctions.ReportAuthorizeResultToDeviceService(authorizeSuccessful);
	}

	[KernelFunction(Name = "kernel.set_read_card_attempt_authorize_result")]
	internal static void SetReadCardAttemptAuthorizeResult(LuaTable authorizeResultsLuaTable)
	{
		if (authorizeResultsLuaTable == null)
		{
			return;
		}
		IAuthorizeResult authorizeResult = new AuthorizeResult();
		if (GetBooleanValue(authorizeResultsLuaTable, "success", out var value))
		{
			authorizeResult.Success = value;
		}
		if (authorizeResultsLuaTable["properties"] is LuaTable luaTable)
		{
			if (GetBooleanValue(luaTable, "is_online", out var value2))
			{
				authorizeResult.IsOnline = value2;
			}
			if (GetBooleanValue(luaTable, "stand_alone", out var value3))
			{
				authorizeResult.StandAlone = value3;
			}
			if (GetStringValue(luaTable, "server_name", out var value4))
			{
				authorizeResult.ServerName = value4;
			}
			if (GetStringValue(luaTable, "response", out var value5))
			{
				authorizeResult.Response = value5;
			}
			if (GetStringValue(luaTable, "email", out var value6))
			{
				authorizeResult.Email = value6;
			}
			if (GetStringValue(luaTable, "reference_number", out var value7))
			{
				authorizeResult.ReferenceNumber = value7;
			}
			if (GetBooleanValue(luaTable, "has_profile", out var value8))
			{
				authorizeResult.HasProfile = value8;
			}
			if (GetBooleanValue(luaTable, "is_rbi", out var value9))
			{
				authorizeResult.IsRBI = value9;
			}
			if (GetStringValue(luaTable, "first_name", out var value10))
			{
				authorizeResult.FirstName = value10;
			}
			if (GetStringValue(luaTable, "last_name", out var value11))
			{
				authorizeResult.LastName = value11;
			}
			if (GetStringValue(luaTable, "confirmation_status", out var value12))
			{
				authorizeResult.ConfirmationStatus = value12;
			}
			if (GetStringValue(luaTable, "account_number", out var value13))
			{
				authorizeResult.AccountNumber = value13;
			}
			if (GetStringValue(luaTable, "customer_profile_number", out var value14))
			{
				authorizeResult.CustomerProfileNumber = value14;
			}
			if (GetStringValue(luaTable, "store_number", out var value15))
			{
				authorizeResult.StoreNumber = value15;
			}
		}
		Redbox.Rental.Services.Controllers.CheckoutFunctions.SetReadCardAttemptAuthorizeResult(authorizeResult);
	}

	private static bool GetBooleanValue(LuaTable luaTable, string propertyName, out bool value)
	{
		bool result = false;
		value = false;
		if (luaTable != null)
		{
			object obj = luaTable[propertyName];
			if (obj != null && bool.TryParse(obj.ToString(), out var result2))
			{
				value = result2;
				result = true;
			}
		}
		return result;
	}

	private static bool GetStringValue(LuaTable luaTable, string propertyName, out string value)
	{
		bool result = false;
		value = null;
		if (luaTable != null)
		{
			object obj = luaTable[propertyName];
			if (obj != null)
			{
				value = obj.ToString();
				result = true;
			}
		}
		return result;
	}

	private static bool GetIntegerValue(LuaTable luaTable, string propertyName, out int value)
	{
		bool result = false;
		value = 0;
		if (luaTable != null)
		{
			object obj = luaTable[propertyName];
			if (obj != null && int.TryParse(obj.ToString(), out var result2))
			{
				value = result2;
				result = true;
			}
		}
		return result;
	}
}
