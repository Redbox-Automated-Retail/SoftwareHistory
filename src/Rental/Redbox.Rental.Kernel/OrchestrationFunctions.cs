using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.ShoppingCart;
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

	[KernelFunction(Name = "kernel.pricing_service_get_adjusted_price")]
	internal static decimal? GetAdjustedPrice(string priceValue, string actionValue)
	{
		decimal? result = null;
		if (decimal.TryParse(priceValue, out var result2) && Enum.TryParse<RentalShoppingCartItemAction>(actionValue, out var result3))
		{
			IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
			if (service != null)
			{
				result = service.GetAdjustedPrice(result2, result3, TitleType.All);
			}
		}
		return result;
	}

	[KernelFunction(Name = "kernel.pricing_service_get_pricing_record")]
	internal static LuaTable GetProductPricingRecord(string productIdValue)
	{
		LuaTable result = null;
		if (long.TryParse(productIdValue, out var result2))
		{
			IPricingRecord pricingRecord = ServiceLocator.Instance.GetService<IPricingService>()?.GetOneDayProductPricingRecord(result2);
			if (pricingRecord != null)
			{
				result = pricingRecord.ToDictionary().ToLuaTable();
			}
		}
		return result;
	}

	[KernelFunction(Name = "kernel.pricing_service_get_current_price_set")]
	internal static LuaTable GetCurrentPriceSet()
	{
		LuaTable result = null;
		IPricingSet pricingSet = ServiceLocator.Instance.GetService<IPricingService>()?.CurrentPriceSet;
		if (pricingSet != null)
		{
			List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
			pricingSet.PriceRecords.ForEach(delegate(IPricingRecord x)
			{
				rows.Add(x.ToDictionary());
			});
			result = new Dictionary<string, object>
			{
				["PriceSetID"] = pricingSet.PriceSetId,
				["Rows"] = rows
			}.ToLuaTable();
		}
		return result;
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

	[KernelFunction(Name = "kernel.pricing_service_get_price_setting")]
	internal static decimal? GetPriceSetting(string titleFamilyValue, string titleTypeValue, string settingName, string actionValue)
	{
		decimal? result = null;
		if (Enum.TryParse<TitleFamily>(titleFamilyValue, ignoreCase: true, out var result2))
		{
			TitleType result3 = TitleType.All;
			titleTypeValue = titleTypeValue.Replace("-", string.Empty);
			if (titleTypeValue == "*")
			{
				titleTypeValue = "All";
			}
			Enum.TryParse<TitleType>(titleTypeValue, ignoreCase: true, out result3);
			if (Enum.TryParse<RentalShoppingCartItemAction>(actionValue, out var result4))
			{
				IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
				if (service != null)
				{
					return service.GetPriceSetting(result2, result3, settingName, result4);
				}
			}
		}
		else
		{
			LogHelper.Instance.Log("Error in OrchestrationFunctions.GetPriceSetting.  Unable to parse title family {0}", titleFamilyValue);
		}
		return result;
	}

	[KernelFunction(Name = "kernel.pricing_service_get_product_type_price")]
	internal static LuaTable GetProductTypePrice(string titleFamilyValue, string titleTypeValue, string forRecommendationOnPickupFlowValue)
	{
		LuaTable result = null;
		IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
		if (Enum.TryParse<TitleFamily>(titleFamilyValue, ignoreCase: true, out var result2))
		{
			TitleType result3 = TitleType.All;
			titleTypeValue = titleTypeValue.Replace("-", string.Empty);
			if (titleTypeValue == "*")
			{
				titleTypeValue = "All";
			}
			Enum.TryParse<TitleType>(titleTypeValue, ignoreCase: true, out result3);
			bool result4 = false;
			bool.TryParse(forRecommendationOnPickupFlowValue, out result4);
			IPricingRecord productTypePricingRecord = service.GetProductTypePricingRecord(result2, result3, !result4);
			if (productTypePricingRecord != null)
			{
				result = productTypePricingRecord.ToDictionary().ToLuaTable();
			}
		}
		else
		{
			LogHelper.Instance.Log("Error in OrchestrationFunctions.GetProductTypePrice.  Unable to parse title family {0}", titleFamilyValue);
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
