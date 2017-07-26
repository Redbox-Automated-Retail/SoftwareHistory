using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Services;

namespace Redbox.Rental.Kernel;

public static class DataServiceFunctions
{
	public static string CacheTableName = "cache";

	public static string ProfileTableName = "profile";

	[KernelFunction(Name = "kernel.dataserviceget")]
	internal static object DataServiceGet(string dataStore, string tableName, string keyName)
	{
		try
		{
			if (dataStore.Equals(CacheTableName))
			{
				return ServiceLocator.Instance.GetService<IDataStoreService>()?.Get(dataStore, tableName, keyName);
			}
			if (dataStore.Equals(ProfileTableName))
			{
				ErrorList luaTableData = ServiceLocator.Instance.GetService<IDataService>().GetLuaTableData(tableName, out var data);
				if (luaTableData.Count > 0)
				{
					luaTableData.ToLogHelper();
					return null;
				}
				if (!data.Data.TryGetValue(keyName, out var value))
				{
					return null;
				}
				return value.Value;
			}
			return null;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An unhandled exception occured - DataServiceFunctions.DataServiceGet({dataStore},{tableName},{keyName}).", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.dataservicegetall")]
	internal static LuaTable DataServiceGetAll(string dataStore, string tableName)
	{
		try
		{
			if (dataStore.Equals(CacheTableName))
			{
				IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
				if (service == null)
				{
					return null;
				}
				Dictionary<string, object> all = service.GetAll(dataStore, tableName);
				if (all == null)
				{
					return null;
				}
				IKernelService service2 = ServiceLocator.Instance.GetService<IKernelService>();
				LuaTable keysTable = (LuaTable)service2.NewTable();
				LuaTable cacheTable = (LuaTable)service2.NewTable();
				int i = 1;
				all.ForEach(delegate(KeyValuePair<string, object> r)
				{
					keysTable[i] = r.Key;
					cacheTable[r.Key] = r.Value;
					i++;
				});
				cacheTable["keys"] = keysTable;
				return cacheTable;
			}
			if (dataStore.Equals(ProfileTableName))
			{
				ErrorList luaTableData = ServiceLocator.Instance.GetService<IDataService>().GetLuaTableData(tableName, out var data);
				if (luaTableData.Count > 0)
				{
					luaTableData.ToLogHelper();
					return null;
				}
				IKernelService service3 = ServiceLocator.Instance.GetService<IKernelService>();
				LuaTable keysTable2 = (LuaTable)service3.NewTable();
				LuaTable cacheTable2 = (LuaTable)service3.NewTable();
				int i2 = 1;
				data.Data.ForEach(delegate(KeyValuePair<string, ILuaKeyValue> row)
				{
					if (!string.IsNullOrEmpty(row.Key) && !string.IsNullOrEmpty(row.Value.Value))
					{
						keysTable2[i2] = row.Key;
						cacheTable2[row.Key] = row.Value.Value;
						i2++;
					}
				});
				cacheTable2["keys"] = keysTable2;
				return cacheTable2;
			}
			return null;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An unhandled exception occured - DataServiceFunctions.DataServiceGetAll({dataStore},{tableName}).", e);
			throw;
		}
	}

	[KernelFunction(Name = "kernel.dataservicegetkeys")]
	internal static LuaTable DataServiceGetKeys(string dataStore, string tableName)
	{
		try
		{
			if (dataStore.Equals(CacheTableName))
			{
				LuaTable luaTable = (LuaTable)ServiceLocator.Instance.GetService<IKernelService>().NewTable();
				IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
				if (service == null)
				{
					return luaTable;
				}
				ReadOnlyCollection<string> keys = service.GetKeys(dataStore, tableName);
				int num = 1;
				foreach (string item in keys)
				{
					luaTable[num++] = item;
				}
				return luaTable;
			}
			if (dataStore.Equals(ProfileTableName))
			{
				IKernelService service2 = ServiceLocator.Instance.GetService<IKernelService>();
				LuaTable luaTable2 = (LuaTable)service2.NewTable();
				ErrorList luaTableData = ServiceLocator.Instance.GetService<IDataService>().GetLuaTableData(tableName, out var data);
				if (luaTableData.Count > 0)
				{
					luaTableData.ToLogHelper();
					return null;
				}
				int index = 1;
				data.Data.ForEach(delegate(KeyValuePair<string, ILuaKeyValue> row)
				{
					if (!string.IsNullOrEmpty(row.Key))
					{
						luaTable2[index++] = row.Key;
					}
				});
				return luaTable2;
			}
			return null;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An unhandled exception occured - DataServiceFunctions.DataServiceGetKeys({dataStore},{tableName}).", e);
			ServiceLocator.Instance.GetService<IEngineApplication>()?.CorruptDbDetected(dataStore);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.dataservicecount")]
	internal static long DataServiceCount(string dataStore, string tableName)
	{
		try
		{
			if (dataStore.Equals(CacheTableName))
			{
				return ServiceLocator.Instance.GetService<IDataStoreService>()?.Count(dataStore, tableName) ?? 0;
			}
			if (dataStore.Equals(ProfileTableName))
			{
				ErrorList luaTableData = ServiceLocator.Instance.GetService<IDataService>().GetLuaTableData(tableName, out var data);
				if (luaTableData.Count > 0)
				{
					luaTableData.ToLogHelper();
					return 0L;
				}
				return data.Data.Count;
			}
			return 0L;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An unhandled exception occured - DataServiceFunctions.DataServiceCount({dataStore},{tableName}).", e);
			throw;
		}
	}

	[KernelFunction(Name = "kernel.dataserviceclose")]
	internal static void DataServiceClose(string dataStore)
	{
		LogHelper.Instance.Log("DataServiceFunctions.DataServiceClose called");
		if (dataStore.Equals(CacheTableName))
		{
			ServiceLocator.Instance.GetService<IDataStoreService>()?.CloseStore(dataStore);
		}
	}

	[KernelFunction(Name = "kernel.dataserviceremove")]
	internal static void DataServiceRemove(string dataStore, string tableName, string keyName)
	{
		try
		{
			if (dataStore.Equals(CacheTableName))
			{
				IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
				if (service != null)
				{
					service.Remove(dataStore, tableName, keyName);
					ServiceLocator.Instance.GetService<IDataService>().CacheTablesRemove(tableName, keyName);
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An unhandled exception occured - DataServiceFunctions.DataServiceRemove({dataStore},{tableName},{keyName}).", e);
		}
	}

	[KernelFunction(Name = "kernel.dataserviceclear")]
	internal static void DataServiceClear(string dataStore, string tableName)
	{
		try
		{
			if (dataStore.Equals(CacheTableName))
			{
				ServiceLocator.Instance.GetService<IDataStoreService>()?.Clear(dataStore, tableName);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An unhandled exception occured - DataServiceFunctions.DataServiceClear({dataStore},{tableName}).", e);
		}
	}

	[KernelFunction(Name = "kernel.dataserviceset")]
	internal static void DataServiceSet(string dataStore, string tableName, string keyName, object data)
	{
		try
		{
			if (dataStore.Equals(CacheTableName))
			{
				IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
				if (service != null)
				{
					service.Set(dataStore, tableName, keyName, data);
					ServiceLocator.Instance.GetService<IDataService>().CacheTablesUpdate(tableName, keyName, data);
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An unhandled exception occured - DataServiceFunctions.DataServiceSet({dataStore},{tableName},{keyName},{Convert.ToString(data)}).", e);
		}
	}

	[KernelFunction(Name = "kernel.dataserviceregisterimage")]
	internal static bool DataServiceRegisterImage(string name, string cacheName)
	{
		try
		{
			return OrchestrationService.DataServiceRegisterImage(name, cacheName);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An unhandled exception occured - DataServiceFunctions.RegisterImageFromCache({name},{cacheName}).", e);
		}
		return false;
	}
}
