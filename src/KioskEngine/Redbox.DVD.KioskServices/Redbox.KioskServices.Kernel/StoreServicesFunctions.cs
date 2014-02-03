using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskServices.Kernel;

internal static class StoreServicesFunctions
{
	[KernelFunction(Name = "kernel.storesvcs_getpendingstates")]
	internal static void GetPendingStates(LuaFunction completeCallback)
	{
		ServiceLocator.Instance.GetService<IStoreServices>()?.GetPendingStates(delegate(IRemoteServiceResult r)
		{
			IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
			LuaTable luaTable = r.AsTable();
			int num = 1;
			LuaTable luaTable2 = (LuaTable)service.NewTable();
			foreach (IState item in (List<IState>)r.Properties["states"])
			{
				LuaTable luaTable3 = (LuaTable)service.NewTable();
				luaTable3["id"] = item.Id;
				luaTable3["name"] = item.Name;
				luaTable2[num++] = luaTable3;
			}
			((LuaTable)luaTable["properties"])["states"] = luaTable2;
			ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new ResultCallbackEntry
			{
				Function = completeCallback,
				Parameters = new LuaTable[1] { luaTable }
			});
		});
	}

	[KernelFunction(Name = "kernel.storesvcs_getpendingbanners")]
	internal static void GetPendingBanners(int stateId, LuaFunction completeCallback)
	{
		ServiceLocator.Instance.GetService<IStoreServices>()?.GetPendingBanners(stateId, delegate(IRemoteServiceResult r)
		{
			IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
			LuaTable luaTable = r.AsTable();
			int num = 1;
			LuaTable luaTable2 = (LuaTable)service.NewTable();
			foreach (IBanner item in (List<IBanner>)r.Properties["banners"])
			{
				LuaTable luaTable3 = (LuaTable)service.NewTable();
				luaTable3["id"] = item.Id;
				luaTable3["name"] = item.Name;
				luaTable2[num++] = luaTable3;
			}
			((LuaTable)luaTable["properties"])["banners"] = luaTable2;
			ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new ResultCallbackEntry
			{
				Function = completeCallback,
				Parameters = new LuaTable[1] { luaTable }
			});
		});
	}

	[KernelFunction(Name = "kernel.storesvcs_getpendingstores")]
	internal static void GetPendingStores(int bannerId, int stateId, LuaFunction completeCallback)
	{
		ServiceLocator.Instance.GetService<IStoreServices>()?.GetPendingStores(bannerId, stateId, delegate(IRemoteServiceResult r)
		{
			IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
			LuaTable luaTable = r.AsTable();
			int num = 1;
			LuaTable luaTable2 = (LuaTable)service.NewTable();
			foreach (IStore item in (List<IStore>)r.Properties["stores"])
			{
				LuaTable luaTable3 = (LuaTable)service.NewTable();
				luaTable3["id"] = item.Id;
				luaTable3["address"] = item.Address;
				luaTable3["city"] = item.City;
				luaTable3["postal_code"] = item.ZipCode;
				luaTable3["market_name"] = item.MarketName;
				luaTable3["kaseya_market_name"] = item.KaseyaMarketName;
				luaTable3["state"] = item.State;
				luaTable3["due_time"] = item.DueTime;
				luaTable2[num++] = luaTable3;
			}
			((LuaTable)luaTable["properties"])["stores"] = luaTable2;
			ServiceLocator.Instance.GetService<ICallbackService>()?.EnqueueCallback(new ResultCallbackEntry
			{
				Function = completeCallback,
				Parameters = new LuaTable[1] { luaTable }
			});
		});
	}

	[KernelFunction(Name = "kernel.storesvcs_shouldreboot")]
	internal static bool ShouldReboot()
	{
		IStoreServices service = ServiceLocator.Instance.GetService<IStoreServices>();
		if (service == null)
		{
			return false;
		}
		DateTime? rebootTime = service.GetRebootTime();
		if (!rebootTime.HasValue)
		{
			return false;
		}
		DateTime now = DateTime.Now;
		DateTime dateTime = rebootTime.Value.AddMinutes(-1.0);
		DateTime dateTime2 = rebootTime.Value.AddMinutes(1.0);
		if (now >= dateTime)
		{
			return now <= dateTime2;
		}
		return false;
	}

	[KernelFunction(Name = "kernel.storesvcs_shouldruninitialsync")]
	internal static bool ShouldRunInitialSync()
	{
		return ServiceLocator.Instance.GetService<IStoreServices>()?.ShouldRunInitialSync() ?? false;
	}

	[KernelFunction(Name = "kernel.storesvcs_setruninitialsync")]
	internal static void SetRunInitialSync(bool flag)
	{
		ServiceLocator.Instance.GetService<IStoreServices>()?.SetRunInitialSync(flag);
	}

	[KernelFunction(Name = "kernel.storesvcs_getinitialsynctype")]
	internal static string GetInitialSyncType()
	{
		return ServiceLocator.Instance.GetService<IStoreServices>()?.GetInitialSyncType();
	}

	[KernelFunction(Name = "kernel.storesvcs_registerinitialsyncconfigcallback")]
	internal static void RegisterInitialSyncChangeCallback(LuaFunction callback)
	{
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RegisterConfigurationCallback("iniital-sync-config-callback", delegate(string store, string action, string path, string key, string newValue)
		{
			if (callback != null)
			{
				IStoreServices service = ServiceLocator.Instance.GetService<IStoreServices>();
				if (service != null && store == "machine" && action == "set" && path == "Store" && (key == "RunInitialSync" || key == "InitialSyncType"))
				{
					callback.Call(service.ShouldRunInitialSync(), service.GetInitialSyncType());
				}
			}
		});
	}

	[KernelFunction(Name = "kernel.storesvcs_clearinitialsyncconfigcallback")]
	internal static void ClearInitialSyncChangeCallback()
	{
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.UnregisterConfigurationCallback("iniital-sync-config-callback");
	}

	[KernelFunction(Name = "kernel.storesvcs_issinglebilling")]
	internal static bool IsSingleBilling(byte? collectionMethod)
	{
		CollectionMethod value = (CollectionMethod)collectionMethod.Value;
		return (value & CollectionMethod.AuthorizeOnly) != 0;
	}
}
