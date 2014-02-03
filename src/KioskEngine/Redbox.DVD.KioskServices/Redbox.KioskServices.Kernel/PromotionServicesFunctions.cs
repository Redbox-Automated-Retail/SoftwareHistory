using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskServices.Kernel;

internal static class PromotionServicesFunctions
{
	[KernelFunction(Name = "kernel.promosvcs_validatecode")]
	internal static void ValidatePromotionCode(string storeNumber, string promotionCode, LuaFunction validationCallback)
	{
		IPromotionServices service = ServiceLocator.Instance.GetService<IPromotionServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			service.ValidatePromotionCode(storeNumber, promotionCode, delegate(IRemoteServiceResult r)
			{
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = validationCallback,
					Parameters = new LuaTable[1] { r.AsTable() }
				});
			});
		}
	}

	[KernelFunction(Name = "kernel.promosvcs_kioskoptin")]
	internal static void KioskOptIn(string storeNumber, LuaTable optInItems, LuaFunction callback)
	{
		InnerKioskOptIn(storeNumber, optInItems, sendCardHash: false, callback);
	}

	[KernelFunction(Name = "kernel.promosvcs_kioskoptin2")]
	internal static void KioskOptIn(string storeNumber, LuaTable optInItems, bool sendCardHash, LuaFunction callback)
	{
		InnerKioskOptIn(storeNumber, optInItems, sendCardHash, callback);
	}

	private static void InnerKioskOptIn(string storeNumber, LuaTable optInItems, bool sendCardHash, LuaFunction callback)
	{
		IPromotionServices service = ServiceLocator.Instance.GetService<IPromotionServices>();
		if (service == null)
		{
			return;
		}
		List<IOptInItem> list = new List<IOptInItem>();
		foreach (object key in optInItems.Keys)
		{
			LuaTable luaTable = (LuaTable)optInItems[key];
			if (luaTable != null)
			{
				IOptInItem optInItem = service.NewOptInItem();
				optInItem.ItemId = long.Parse((string)luaTable["item_id"]);
				optInItem.Item_Type = Enum<Item_Type>.Parse((string)luaTable["item_type"]);
				optInItem.OptInMethodType = Enum<OptInMethodType>.Parse((string)luaTable["optin_method_type"]);
				optInItem.OptInMethodData = (string)luaTable["optin_method_data"];
				optInItem.TemplateName = (string)luaTable["template_name"];
				optInItem.CampaignName = (string)luaTable["campaign_name"];
				list.Add(optInItem);
			}
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService == null)
		{
			return;
		}
		RemoteServiceCallback callback2 = null;
		if (callback != null)
		{
			callback2 = delegate(IRemoteServiceResult r)
			{
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = callback,
					Parameters = new LuaTable[1] { r.AsTable() }
				});
			};
		}
		service.KioskOptIn(storeNumber, list, sendCardHash, callback2);
		optInItems.Dispose();
	}
}
