using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Marketing;

namespace Redbox.Rental.Kernel;

public static class MarketingAdFunctions
{
	[KernelFunction(Name = "kernel.marketing_ad_start_application")]
	internal static void MarketingAdStartApplication()
	{
		try
		{
			ServiceLocator.Instance.GetService<IMarketingAdService>().StartApplication();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("MarketingAdFunctions.MarketingAdStartApplication - an unhanded exception occurred.", e);
		}
	}

	[KernelFunction(Name = "kernel.marketing_ad_start_session")]
	internal static void MarketingAdStartSession()
	{
		try
		{
			ServiceLocator.Instance.GetService<IMarketingAdService>().StartSession();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("MarketingAdFunctions.MarketingAdStartSession - an unhanded exception occurred.", e);
		}
	}

	[KernelFunction(Name = "kernel.marketing_ad_get_screen_ad")]
	internal static LuaTable MarketingAdGetScreenAd(string marketingAdLocation, bool createImpression)
	{
		try
		{
			MarketingAd ad = new MarketingAd
			{
				Visible = false
			};
			if (Enum.TryParse<MarketingAdLocation>(marketingAdLocation, ignoreCase: true, out var result))
			{
				MarketingAd screenAd = ServiceLocator.Instance.GetService<IMarketingAdService>().GetScreenAd(result, createImpression);
				if (screenAd != null)
				{
					ad = screenAd;
				}
			}
			return ad.ToLuaTable();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("MarketingAdFunctions.MarketingAdGetScreenAd - an unhanded exception occurred.", e);
			return new MarketingAd
			{
				Visible = false
			}.ToLuaTable();
		}
	}

	[KernelFunction(Name = "kernel.marketing_ad_check_for_new_active_campaign")]
	internal static bool MarketingAdCheckForNewActiveCampaign()
	{
		try
		{
			return ServiceLocator.Instance.GetService<IMarketingAdService>().CheckForNewActiveCampaign();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("MarketingAdFunctions.MarketingAdCheckForNewActiveCampaign - an unhanded exception occurred.", e);
			return false;
		}
	}

	private static LuaTable ToLuaTable(this MarketingAd ad)
	{
		return new Dictionary<string, object>
		{
			["visible"] = ad.Visible,
			["image_name"] = ad.ImageName,
			["minimum_display"] = ad.MinimumDisplay
		}.ToLuaTable();
	}
}
