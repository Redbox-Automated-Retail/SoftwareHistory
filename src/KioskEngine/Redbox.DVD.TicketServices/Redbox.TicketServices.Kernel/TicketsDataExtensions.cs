using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.TicketServices.Proxy.ComponentModel;

namespace Redbox.TicketServices.Kernel;

internal static class TicketsDataExtensions
{
	internal static LuaTable ToLuaDataTable(this ITicketDataTable all)
	{
		try
		{
			IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
			LuaTable keysTable = (LuaTable)kernelService.NewTable();
			LuaTable cacheTable = (LuaTable)kernelService.NewTable();
			List<string> columnNames = new List<string>();
			all.ColumnNames.ForEach(delegate(string c)
			{
				if (!c.Equals("key", StringComparison.CurrentCultureIgnoreCase))
				{
					columnNames.Add(c);
				}
			});
			int i = 1;
			all.Data.ForEach(delegate(KeyValuePair<string, object[]> r)
			{
				keysTable[i++] = r.Key;
				LuaTable row = (LuaTable)kernelService.NewTable();
				int col = 0;
				columnNames.ForEach(delegate(string c)
				{
					bool flag = false;
					object obj = r.Value[col++];
					if (obj is DateTime)
					{
						row[c] = ((DateTime)obj).ToString("yyyyMMddHHmmss");
					}
					else if (obj is ICollection)
					{
						LuaTable luaTable = (LuaTable)kernelService.NewTable();
						int num = 1;
						foreach (object item in (IEnumerable)obj)
						{
							if (item is Dictionary<string, object>)
							{
								LuaTable luaTable2 = (LuaTable)kernelService.NewTable();
								foreach (string key in ((Dictionary<string, object>)item).Keys)
								{
									luaTable2[key] = ((Dictionary<string, object>)item)[key];
								}
								luaTable[num++] = luaTable2;
							}
							else
							{
								luaTable[num++] = item;
							}
						}
						row[c] = luaTable;
					}
					else
					{
						row[c] = (flag ? null : obj);
					}
				});
				cacheTable[r.Key] = row;
			});
			cacheTable["keys"] = keysTable;
			return cacheTable;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in TicketServiceFunctions.TicketDataTableGetAll({all.TableName}).", e);
			return null;
		}
	}

	internal static LuaTable ToLuaTable<T>(this ITicketDataTable all, List<T> filter)
	{
		try
		{
			IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
			LuaTable cacheTable = (LuaTable)kernelService.NewTable();
			List<string> columnNames = new List<string>();
			all.ColumnNames.ForEach(delegate(string c)
			{
				if (!c.Equals("key", StringComparison.CurrentCultureIgnoreCase))
				{
					columnNames.Add(c);
				}
			});
			int pos = 0;
			var orderedFilter = filter.Select((T a) => new
			{
				order = pos++,
				FilterItem = a
			}).ToList();
			List<KeyValuePair<string, object[]>> list = (from a in all.Data
				from d in orderedFilter
				where a.Key == d.FilterItem.ToString()
				orderby d.order
				select a).ToList();
			int i = 1;
			list.ForEach(delegate(KeyValuePair<string, object[]> r)
			{
				LuaTable row = (LuaTable)kernelService.NewTable();
				int col = 0;
				columnNames.ForEach(delegate(string c)
				{
					bool flag = false;
					object obj = r.Value[col++];
					if (obj is DateTime)
					{
						row[c] = ((DateTime)obj).ToString("yyyyMMddHHmmss");
					}
					else if (obj is ICollection)
					{
						LuaTable luaTable = (LuaTable)kernelService.NewTable();
						int num = 1;
						foreach (object item in (IEnumerable)obj)
						{
							luaTable[num++] = item;
						}
						row[c] = luaTable;
					}
					else
					{
						row[c] = (flag ? null : obj);
					}
				});
				cacheTable[i] = row;
				i++;
			});
			return cacheTable;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in TicketServiceFunctions.TicketDataTableGetAll({all.TableName}).", e);
			return null;
		}
	}
}
