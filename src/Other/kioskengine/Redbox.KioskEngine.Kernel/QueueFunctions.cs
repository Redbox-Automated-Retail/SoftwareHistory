using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class QueueFunctions
{
	[KernelFunction(Name = "kernel.queuependingcounts")]
	internal static LuaTable GetPendingCounts()
	{
		Dictionary<string, int> counts = new Dictionary<string, int>();
		counts["movie_rental_count"] = 0;
		counts["movie_purchase_count"] = 0;
		counts["game_rental_count"] = 0;
		counts["game_purchase_count"] = 0;
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service == null)
		{
			return BuildCountsTable(counts);
		}
		ITrackDataService service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
		if (service2 == null || !service2.HasValidData())
		{
			return BuildCountsTable(counts);
		}
		string currentCardHashId = service2.GenerateCardHashId();
		service.ForEach(delegate(IMessage m)
		{
			Dictionary<string, object> dictionary = Encoding.ASCII.GetString(m.Data).ToDictionary();
			if (dictionary.ContainsKey("CreditCard"))
			{
				IDictionary<string, object> dictionary2 = (IDictionary<string, object>)dictionary["CreditCard"];
				if ((!dictionary2.ContainsKey("CardId") || string.Compare((string)dictionary2["CardId"], currentCardHashId) == 0) && dictionary.ContainsKey("Contents"))
				{
					IDictionary<string, object> dictionary3 = (IDictionary<string, object>)dictionary["Contents"];
					if (dictionary3.ContainsKey("rental"))
					{
						foreach (IDictionary<string, object> item in (ArrayList)((IDictionary<string, object>)dictionary3["rental"])["Items"])
						{
							if (!item.ContainsKey("VendStatus") || string.Compare((string)item["VendStatus"], "Vended", ignoreCase: true) == 0)
							{
								if (item.ContainsKey("MultiDisc") && string.Compare(item["MultiDisc"].ToString(), "true", ignoreCase: true) == 0 && item.ContainsKey("Prices"))
								{
									IDictionary<string, object> dictionary5 = (IDictionary<string, object>)item["Prices"];
									if (dictionary5.ContainsKey("initialnight") && (string.Compare(dictionary5["initialnight"].ToString(), "0") == 0 || string.Compare(dictionary5["initialnight"].ToString(), "0.0") == 0))
									{
										continue;
									}
								}
								if (item.ContainsKey("ProductFamily") && string.Compare((string)item["ProductFamily"], "Movies", ignoreCase: true) == 0)
								{
									counts["movie_rental_count"]++;
								}
								if (item.ContainsKey("ProductFamily") && string.Compare((string)item["ProductFamily"], "Games", ignoreCase: true) == 0)
								{
									counts["game_rental_count"]++;
								}
							}
						}
					}
					if (dictionary3.ContainsKey("purchase"))
					{
						foreach (IDictionary<string, object> item2 in (ArrayList)((IDictionary<string, object>)dictionary3["purchase"])["Items"])
						{
							if (!item2.ContainsKey("VendStatus") || string.Compare((string)item2["VendStatus"], "Vended", ignoreCase: true) == 0)
							{
								if (item2.ContainsKey("ProductFamily") && string.Compare((string)item2["ProductFamily"], "Movies", ignoreCase: true) == 0)
								{
									counts["movie_purchase_count"]++;
								}
								if (item2.ContainsKey("ProductFamily") && string.Compare((string)item2["ProductFamily"], "Games", ignoreCase: true) == 0)
								{
									counts["game_purchase_count"]++;
								}
							}
						}
					}
				}
			}
		}, "Reconcile");
		return BuildCountsTable(counts);
	}

	[KernelFunction(Name = "kernel.queuecounts")]
	internal static LuaTable GetQueueCounts()
	{
		ErrorList errors;
		return InnerQueueCounts(out errors);
	}

	[KernelFunction(Name = "kernel.queuecounts2")]
	internal static LuaTable GetQueueCounts2()
	{
		LuaTable value = InnerQueueCounts(out var errors);
		LuaTable luaTable = BuildResultTable(errors);
		luaTable["counts"] = value;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.queuestopworker")]
	internal static void StopQueueWorker()
	{
		ServiceLocator.Instance.GetService<IQueueService>()?.StopQueueWorker();
	}

	[KernelFunction(Name = "kernel.queuestartworker")]
	internal static void StartQueueWorker()
	{
		ServiceLocator.Instance.GetService<IQueueService>()?.StartQueueWorker();
	}

	[KernelFunction(Name = "kernel.queueclear")]
	internal static LuaTable ClearQueue()
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			return BuildResultTable(service.Clear());
		}
		return null;
	}

	[KernelFunction(Name = "kernel.queuegetallmessages")]
	internal static LuaTable GetAllMessages()
	{
		LuaTable table = new LuaTable(KernelService.Instance.LuaRuntime);
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service == null)
		{
			return table;
		}
		int index = 1;
		BuildResultTable(service.ForEach(delegate(IMessage m)
		{
			table[index++] = CreateMessageTable(m);
		}))["messages"] = table;
		return table;
	}

	[KernelFunction(Name = "kernel.queuegetmessages")]
	internal static LuaTable GetMessagesByType(string type)
	{
		LuaTable table = new LuaTable(KernelService.Instance.LuaRuntime);
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service == null)
		{
			return table;
		}
		int index = 1;
		BuildResultTable(service.ForEach(delegate(IMessage m)
		{
			table[index++] = CreateMessageTable(m);
		}, type))["messages"] = table;
		return table;
	}

	[KernelFunction(Name = "kernel.queuedequeue")]
	internal static LuaTable Dequeue(LuaFunction predicate)
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service == null)
		{
			return null;
		}
		return BuildResultTable(service.Dequeue(delegate(IMessage m)
		{
			if (predicate == null)
			{
				return false;
			}
			ReadOnlyCollection<object> readOnlyCollection = predicate.Call(CreateMessageTable(m));
			return readOnlyCollection != null && readOnlyCollection.Count > 0 && (bool)readOnlyCollection[0];
		}));
	}

	[KernelFunction(Name = "kernel.queueisworkerstarted")]
	internal static bool IsWorkerStarted()
	{
		return ServiceLocator.Instance.GetService<IQueueService>()?.IsWorkerStarted() ?? false;
	}

	[KernelFunction(Name = "kernel.queuedeletemessage")]
	internal static LuaTable DeleteMessage(int id)
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			return BuildResultTable(service.DeleteMessage(id));
		}
		return null;
	}

	[KernelFunction(Name = "kernel.queuedeletemessagesbytype")]
	internal static LuaTable DeleteMessageByType(string type)
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			return BuildResultTable(service.DeleteMessageByType(type));
		}
		return null;
	}

	private static LuaTable InnerQueueCounts(out ErrorList errors)
	{
		errors = new ErrorList();
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service == null)
		{
			return luaTable;
		}
		Dictionary<string, int> counts = new Dictionary<string, int>();
		errors = service.ForEach(delegate(IMessage m)
		{
			if (counts.ContainsKey(m.Type))
			{
				int num = counts[m.Type];
				counts[m.Type] = num + 1;
			}
			else
			{
				counts[m.Type] = 1;
			}
		});
		foreach (KeyValuePair<string, int> item in counts)
		{
			luaTable[item.Key] = item.Value;
		}
		return luaTable;
	}

	private static LuaTable CreateMessageTable(IMessage message)
	{
		return new LuaTable(KernelService.Instance.LuaRuntime)
		{
			["id"] = message.ID,
			["data"] = message.Data,
			["type"] = message.Type,
			["priority"] = message.Priority,
			["created_on"] = message.CreatedOn.ToString("yyyyMMddHHmmss")
		};
	}

	private static LuaTable BuildResultTable(ErrorList errors)
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		luaTable["success"] = !errors.ContainsError();
		LuaTable luaTable2 = (LuaTable)(luaTable["errors"] = new LuaTable(KernelService.Instance.LuaRuntime));
		if (!errors.ContainsError())
		{
			return luaTable;
		}
		int num = 1;
		foreach (Redbox.KioskEngine.ComponentModel.Error error in errors)
		{
			LuaTable luaTable4 = new LuaTable(KernelService.Instance.LuaRuntime);
			luaTable4["code"] = error.Code;
			luaTable4["description"] = error.Description;
			luaTable4["details"] = error.Details;
			luaTable2[num++] = luaTable4;
		}
		return luaTable;
	}

	private static LuaTable BuildCountsTable(Dictionary<string, int> counts)
	{
		return new LuaTable(KernelService.Instance.LuaRuntime)
		{
			["movie_rental_count"] = counts["movie_rental_count"],
			["movie_purchase_count"] = counts["movie_purchase_count"],
			["game_rental_count"] = counts["game_rental_count"],
			["game_purchase_count"] = counts["game_purchase_count"],
			["total_count"] = counts["movie_rental_count"] + counts["movie_purchase_count"] + counts["game_rental_count"] + counts["game_purchase_count"]
		};
	}
}
