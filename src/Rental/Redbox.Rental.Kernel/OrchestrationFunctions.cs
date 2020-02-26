using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Services;

namespace Redbox.Rental.Kernel;

public static class OrchestrationFunctions
{
	[KernelFunction(Name = "kernel.is_card_blocklisted")]
	internal static bool IsCardBlocklisted()
	{
		return OrchestrationService.IsCardBlocklisted();
	}

	[KernelFunction(Name = "kernel.view_analytics_start")]
	internal static void ViewAnalyticsStart(string userTriggerAction, string sessionType)
	{
		OrchestrationService.CreateStartViewAnalytics(userTriggerAction, sessionType);
	}

	[KernelFunction(Name = "kernel.register_pricing_service")]
	internal static void RegisterPricingService(Action pricingSetChangeCallback)
	{
		OrchestrationService.RegisterPricingService(pricingSetChangeCallback);
	}

	[KernelFunction(Name = "kernel.register_idle_timer_expire_session_callback")]
	internal static void RegisterIdleTimerExpireSessionCallback(Action idleTimerExpireSessionCallback)
	{
		OrchestrationService.RegisterIdleTimerExpireSessionCallback(idleTimerExpireSessionCallback);
	}

	[KernelFunction(Name = "kernel.register_start_new_rental_session_callback")]
	internal static void RegisterStartNewRentalSessionCallback(Action startNewRentalSessionCallback)
	{
		OrchestrationService.RegisterStartNewRentalSessionCallback(startNewRentalSessionCallback);
	}

	[KernelFunction(Name = "kernel.register_start_idle_timer_callback")]
	internal static void RegisterStartIdleTimerCallback(Action startIdleTimerCallback)
	{
		OrchestrationService.RegisterStartIdleTimerCallback(startIdleTimerCallback);
	}

	[KernelFunction(Name = "kernel.register_localization_reload_callback")]
	internal static void RegisterLocalizationReloadCallback(Action callback)
	{
		OrchestrationService.RegisterLocalizationReloadCallback(callback);
	}

	[KernelFunction(Name = "kernel.register_finish_vend_callback")]
	internal static void RegisterFinishVendCallback(Action finishVendCallback)
	{
		OrchestrationService.RegisterFinishVendCallback(finishVendCallback);
	}

	[KernelFunction(Name = "kernel.register_preposition_vend_callback")]
	internal static void RegisterPrepositionVendCallback(Action prepositionVendCallback)
	{
		OrchestrationService.RegisterPrepositionVendCallback(prepositionVendCallback);
	}

	[KernelFunction(Name = "kernel.register_preposition_vend_abort_callback")]
	internal static void RegisterPrepositionVendAbortCallback(Action prepositionVendAbortCallback)
	{
		OrchestrationService.RegisterPrepositionVendAbortCallback(prepositionVendAbortCallback);
	}

	[KernelFunction(Name = "kernel.register_is_vend_job_completed_callback")]
	internal static void RegisterIsVendJobCompletedCallback(Func<bool> isVendJobCompleteCallback)
	{
		OrchestrationService.RegisterIsVendJobCompletedCallback(isVendJobCompleteCallback);
	}

	[KernelFunction(Name = "kernel.register_view_definitions")]
	internal static void RegisterViewDefinitions()
	{
		OrchestrationService.RegisterViewDefinitions();
	}

	[KernelFunction(Name = "kernel.register_environment_change_callback")]
	internal static void RegisterEnvironmentChangeCallback()
	{
		OrchestrationService.RegisterEnvironmentChangeCallback();
	}

	[KernelFunction(Name = "kernel.reload_pricing_service")]
	internal static void ReloadPricingService()
	{
		OrchestrationService.ReloadPricingService();
	}

	private static void AssignPriceValuesFromLuaTable(IPricingRecord pricingRecord, LuaTable luaTable)
	{
		if (GetDecimalValue(luaTable, "initial_night", out var value))
		{
			pricingRecord.InitialNight = value;
		}
		if (GetDecimalValue(luaTable, "extra_night", out value))
		{
			pricingRecord.ExtraNight = value;
		}
		if (GetDecimalValue(luaTable, "non_return", out value))
		{
			pricingRecord.NonReturn = value;
		}
		if (GetDecimalValue(luaTable, "non_return_days", out value))
		{
			pricingRecord.NonReturnDays = value;
		}
		if (GetDecimalValue(luaTable, "expiration_price", out value))
		{
			pricingRecord.ExpirationPrice = value;
		}
	}

	private static bool GetDecimalValue(LuaTable luaTable, string propertyName, out decimal value)
	{
		bool result = false;
		value = 0m;
		if (luaTable != null)
		{
			object obj = luaTable[propertyName];
			if (obj != null && decimal.TryParse(obj.ToString(), out var result2))
			{
				value = result2;
				result = true;
			}
		}
		return result;
	}

	[KernelFunction(Name = "kernel.does_shopping_cart_contain_all_reserved_barcodes")]
	internal static bool DoesShoppingCartContainAllReservedBarcodes()
	{
		return OrchestrationService.DoesShoppingCartContainAllReservedBarcodes();
	}

	[KernelFunction(Name = "kernel.shopping_cart_commit")]
	internal static void ShoppingCartCommit(string sessionId, string reason, string shouldEndAndSave)
	{
		bool result = false;
		bool.TryParse(shouldEndAndSave, out result);
		OrchestrationService.ShoppingCartCommit(sessionId, reason, result);
	}

	[KernelFunction(Name = "kernel.shopping_cart_end_session")]
	internal static void ShoppingCartEndSession(string sessionId)
	{
		OrchestrationService.ShoppingCartEndSession(sessionId);
	}

	[KernelFunction(Name = "kernel.load_email_optin_campaign")]
	internal static void LoadEmailOptInCampaign()
	{
		OrchestrationService.LoadEmailOptInCampaign();
	}
}
