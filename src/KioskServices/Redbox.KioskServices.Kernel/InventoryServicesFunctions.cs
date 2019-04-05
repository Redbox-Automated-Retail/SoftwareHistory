using System;
using System.Collections.Generic;
using System.Globalization;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskServices.Kernel;

internal static class InventoryServicesFunctions
{
	[KernelFunction(Name = "kernel.invsvcs_return")]
	internal static void Return(string storeNumber, string returnType, string barcode, int deck, int slot, string returnTime)
	{
		InnerReturn(storeNumber, returnType, barcode, deck, slot, returnTime, failedSecurityRead: false, null, null);
	}

	[KernelFunction(Name = "kernel.invsvcs_return2")]
	internal static void Return2(string storeNumber, string returnType, string barcode, int deck, int slot, string returnTime, bool failedSecurityRead, string fileName)
	{
		InnerReturn(storeNumber, returnType, barcode, deck, slot, returnTime, failedSecurityRead, fileName, null);
	}

	[KernelFunction(Name = "kernel.invsvcs_snapshot")]
	internal static void Snapshot(string storeNumber, LuaTable items)
	{
		List<IInventoryItem> list = new List<IInventoryItem>();
		IInventoryServices service = ServiceLocator.Instance.GetService<IInventoryServices>();
		if (service != null)
		{
			InnerSnapshot(storeNumber, items, list);
			service.Snapshot(storeNumber, list.AsReadOnly(), null);
		}
	}

	[KernelFunction(Name = "kernel.invsvcs_snapshotimmediate")]
	internal static void Snapshot(string storeNumber, LuaTable items, LuaFunction callback)
	{
		List<IInventoryItem> list = new List<IInventoryItem>();
		IInventoryServices service = ServiceLocator.Instance.GetService<IInventoryServices>();
		if (service == null)
		{
			return;
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
		InnerSnapshot(storeNumber, items, list);
		service.SnapshotImmediate(storeNumber, list.AsReadOnly(), callback2);
	}

	[KernelFunction(Name = "kernel.invsvcs_getrentalcount")]
	internal static void GetRentalCount(string storeNumber, LuaFunction completeCallback)
	{
		IInventoryServices service = ServiceLocator.Instance.GetService<IInventoryServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			service.GetRentalCount(storeNumber, delegate(IRemoteServiceResult r)
			{
				ICallbackService callbackService2 = callbackService;
				ResultCallbackEntry resultCallbackEntry = new ResultCallbackEntry
				{
					Function = completeCallback
				};
				object[] parameters = new LuaTable[1] { r.AsTable() };
				resultCallbackEntry.Parameters = parameters;
				callbackService2.EnqueueCallback(resultCallbackEntry);
			});
		}
	}

	[KernelFunction(Name = "kernel.updatefraudstatus")]
	internal static void UpdateFraudStatus(string storeNumber, string barcode, string isFraud, string readTime, LuaFunction completeCallback)
	{
		IInventoryServices service = ServiceLocator.Instance.GetService<IInventoryServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			service.UpdateFraudStatus(storeNumber, barcode, isFraud, readTime, delegate(IRemoteServiceResult r)
			{
				ICallbackService callbackService2 = callbackService;
				ResultCallbackEntry resultCallbackEntry = new ResultCallbackEntry
				{
					Function = completeCallback
				};
				object[] parameters = new LuaTable[1] { r.AsTable() };
				resultCallbackEntry.Parameters = parameters;
				callbackService2.EnqueueCallback(resultCallbackEntry);
			});
		}
	}

	[KernelFunction(Name = "kernel.fmadiscaction")]
	internal static void FmaDiscAction(string storeNumber, string username, string jobName, string jobTimeStamp, LuaTable barcode_list, LuaFunction completeCallback)
	{
		IInventoryServices service = ServiceLocator.Instance.GetService<IInventoryServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
			for (int i = 1; i <= barcode_list.Keys.Count; i++)
			{
				LuaTable luaTable = (LuaTable)barcode_list[i];
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("Barcode", luaTable[1] as string);
				dictionary.Add("Action", luaTable[2] as string);
				list.Add(dictionary);
			}
			service.FmaDiscAction(storeNumber, username, jobName, jobTimeStamp, list, delegate(IRemoteServiceResult r)
			{
				ICallbackService callbackService2 = callbackService;
				ResultCallbackEntry resultCallbackEntry = new ResultCallbackEntry
				{
					Function = completeCallback
				};
				object[] parameters = new LuaTable[1] { r.AsTable() };
				resultCallbackEntry.Parameters = parameters;
				callbackService2.EnqueueCallback(resultCallbackEntry);
			});
		}
	}

	private static void InnerReturn(string storeNumber, string returnType, string barcode, int deck, int slot, string returnTime, bool failedSecurityRead, string fileName, LuaFunction callback)
	{
		IInventoryServices service = ServiceLocator.Instance.GetService<IInventoryServices>();
		if (service == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(barcode))
		{
			LogHelper.Instance.Log("A null or empty string was passed for barcode in InventoryServicesFunctions.Return; skipping message enqueue.", LogEntryType.Error);
			return;
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
		if (!DateTime.TryParse(returnTime, null, DateTimeStyles.AssumeLocal, out var result))
		{
			LogHelper.Instance.Log("Invalid date time passed to InnerReturn : {0}.", returnTime);
			result = DateTime.Now;
		}
		service.Return(storeNumber, Enum<ReturnType>.ParseIgnoringCase(returnType, ReturnType.KioskReturn), barcode, deck, slot, result.ToString(), failedSecurityRead, fileName, callback2);
	}

	private static void InnerSnapshot(string storeNumber, LuaTable items, List<IInventoryItem> inventoryItems)
	{
		IInventoryServices service = ServiceLocator.Instance.GetService<IInventoryServices>();
		if (service == null || items == null)
		{
			return;
		}
		foreach (object key in items.Keys)
		{
			LuaTable luaTable = (LuaTable)items[key];
			if (luaTable == null)
			{
				LogHelper.Instance.Log($"InventoryServicesFunctions.InnerShapshot unable to cast item key {key} to a LuaTable; skipping this item.", LogEntryType.Error);
				continue;
			}
			object obj = luaTable["deck"];
			int? num = null;
			if (obj != null)
			{
				num = Convert.ToInt32(obj);
			}
			object obj2 = luaTable["slot"];
			int? num2 = null;
			if (obj2 != null)
			{
				num2 = Convert.ToInt32(obj2);
			}
			string text = Convert.ToString(luaTable["barcode"]);
			if (string.IsNullOrEmpty(text))
			{
				LogHelper.Instance.Log($"A null or empty barcode was passed into InventoryServicesFunctions.Snapshot where deck = {num} and slot = {num2}; skipping this item.", LogEntryType.Error);
				continue;
			}
			inventoryItems.Add(service.CreateInventoryItem(text, num, num2, Enum<InventoryItemStatus>.ParseIgnoringCase(Convert.ToString(luaTable["status"]), InventoryItemStatus.In)));
			luaTable.Dispose();
		}
		items.Dispose();
	}
}
