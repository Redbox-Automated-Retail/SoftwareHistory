using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Kernel;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Lua;
using Redbox.TicketServices.Proxy.ComponentModel;

namespace Redbox.TicketServices.Kernel;

internal static class TicketServicesFunctions
{
	[KernelFunction(Name = "kernel.ticketsvcs_gettickets")]
	internal static void GetTickets(string storeNumber, string productionId, LuaTable priceCodeQtyTable, LuaTable lockGuidsToRelease, LuaFunction callback)
	{
		ITicketServices service = ServiceLocator.Instance.GetService<ITicketServices>();
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
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = callback,
					Parameters = new LuaTable[1] { r.AsTableExtended() }
				});
			};
		}
		List<object> priceCodeQtyTable2 = LuaTableToListStrings(priceCodeQtyTable);
		List<object> lockGuidsToRelease2 = LuaTableToListStrings(lockGuidsToRelease);
		service.GetTickets(storeNumber, productionId, priceCodeQtyTable2, lockGuidsToRelease2, callback2);
	}

	[KernelFunction(Name = "kernel.ticketsvcs_placeorder")]
	internal static void PlaceOrder(string storeNumber, string sessionId, LuaTable customer, LuaTable orderHeader, LuaTable orderItems, LuaTable unlockTickets, LuaTable promotions, LuaFunction callback)
	{
		ITicketServices service = ServiceLocator.Instance.GetService<ITicketServices>();
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
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = callback,
					Parameters = new LuaTable[1] { r.AsTableExtended() }
				});
			};
		}
		Dictionary<string, string> customer2 = LuaTableToDictionaryStringString(customer);
		Dictionary<string, string> orderHeader2 = LuaTableToDictionaryStringString(orderHeader);
		List<object> orderItems2 = LuaTableToListObjects(orderItems);
		List<object> unlockTickets2 = LuaTableToListObjects(unlockTickets);
		List<int> list = new List<int>();
		foreach (object key in promotions.Keys)
		{
			int item = int.Parse(promotions[key].ToString());
			list.Add(item);
		}
		service.PlaceOrder(storeNumber, sessionId, customer2, orderHeader2, orderItems2, unlockTickets2, list.ToArray(), callback2);
	}

	internal static List<object> LuaTableToListStrings(LuaTable table)
	{
		List<object> list = new List<object>();
		foreach (object key in table.Keys)
		{
			LuaTable luaTable = table[key] as LuaTable;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (object key2 in luaTable.Keys)
			{
				dictionary.Add(key2.ToString(), luaTable[key2].ToString());
			}
			list.Add(dictionary);
		}
		return list;
	}

	internal static List<object> LuaTableToListObjects(LuaTable table)
	{
		List<object> list = new List<object>();
		foreach (object key in table.Keys)
		{
			LuaTable luaTable = table[key] as LuaTable;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			foreach (object key2 in luaTable.Keys)
			{
				if (!(luaTable[key2] is LuaTable table2))
				{
					dictionary.Add(key2.ToString(), luaTable[key2]);
				}
				else
				{
					dictionary.Add(key2.ToString(), LuaTableToListObjects(table2));
				}
			}
			list.Add(dictionary);
		}
		return list;
	}

	internal static Dictionary<string, string> LuaTableToDictionaryStringString(LuaTable table)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (object key in table.Keys)
		{
			string text = (key as string) ?? string.Empty;
			string text2 = (table[key] as string) ?? string.Empty;
			if (text != string.Empty && text2 != string.Empty)
			{
				dictionary.Add(text, text2);
			}
		}
		return dictionary;
	}

	[KernelFunction(Name = "kernel.ticketsvcs_updateticketlock")]
	internal static void UpdateTicketLock(string storeNumber, int ticketGroupId, string lockGuid, int quantity, LuaFunction callback)
	{
		ITicketServices service = ServiceLocator.Instance.GetService<ITicketServices>();
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
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = callback,
					Parameters = new LuaTable[1] { r.AsTable() }
				});
			};
		}
		service.UpdateTicketLock(storeNumber, ticketGroupId, lockGuid, quantity, callback2);
	}

	[KernelFunction(Name = "kernel.ticketsvcs_releaseticketlock")]
	internal static void ReleaseTicketLock(string storeNumber, int ticketGroupId, string lockGuid, int quantity, LuaFunction callback)
	{
		ITicketServices service = ServiceLocator.Instance.GetService<ITicketServices>();
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
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = callback,
					Parameters = new LuaTable[1] { r.AsTable() }
				});
			};
		}
		service.ReleaseTicketLock(storeNumber, ticketGroupId, lockGuid, quantity, callback2);
	}

	[KernelFunction(Name = "kernel.ticketsvcs_getemailbycard")]
	internal static void GetEmailByCard(string storeNumber, LuaFunction callback)
	{
		ITicketServices service = ServiceLocator.Instance.GetService<ITicketServices>();
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
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = callback,
					Parameters = new LuaTable[1] { r.AsTable() }
				});
			};
		}
		service.GetEmailByCard(storeNumber, callback2);
	}

	[KernelFunction(Name = "kernel.ticketsvcs_sendnotification")]
	internal static void EmailEvent(string storeNumber, LuaTable subscriptionTable, string email, LuaFunction callback)
	{
		ITicketServices service = ServiceLocator.Instance.GetService<ITicketServices>();
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
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = callback,
					Parameters = new LuaTable[1] { r.AsTable() }
				});
			};
		}
		List<object> subscriptionTable2 = LuaTableToListStrings(subscriptionTable);
		service.SendNotification(storeNumber, subscriptionTable2, email, callback2);
	}
}
