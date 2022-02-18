using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskServices.Kernel;

internal static class PromotionServicesFunctions
{
	[KernelFunction(Name = "kernel.promosvcs_kioskoptin")]
	internal static void KioskOptIn(string storeNumber, LuaTable optInItems, LuaFunction callback)
	{
		InnerKioskOptIn(storeNumber, optInItems, callback);
	}

	private static void InnerKioskOptIn(string storeNumber, LuaTable optInItems, LuaFunction callback)
	{
		IMarketingServices service = ServiceLocator.Instance.GetService<IMarketingServices>();
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
				optInItem.Item_Type = Enum<KioskOptIntItemType>.Parse((string)luaTable["item_type"]);
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
				ICallbackService callbackService2 = callbackService;
				ResultCallbackEntry resultCallbackEntry = new ResultCallbackEntry
				{
					Function = callback
				};
				object[] parameters = new LuaTable[1] { r.AsTable() };
				resultCallbackEntry.Parameters = parameters;
				callbackService2.EnqueueCallback(resultCallbackEntry);
			};
		}
		service.KioskOptIn(long.Parse(storeNumber), list, callback2);
		optInItems.Dispose();
	}
}
