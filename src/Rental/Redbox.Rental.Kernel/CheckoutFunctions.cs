using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Services;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Kernel;

public class CheckoutFunctions
{
	[KernelFunction(Name = "kernel.get_customer_has_profile")]
	internal static bool GetCustomerHasProfile()
	{
		return Redbox.Rental.Services.Controllers.CheckoutFunctions.GetMostRecentCardReadAttemptAuthorizeResult()?.HasProfile ?? false;
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

	[KernelFunction(Name = "kernel.show_authorizing_message")]
	internal static void ShowAuthorizingMessage()
	{
		OrchestrationService.ViewShow("general_message_view", new GeneralMessageViewModel
		{
			DisableIdleTimerOnEnter = false,
			EnableIdleTimerOnLeave = false,
			MessageText = Resources.authorization_message
		});
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
