using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.Core;
using VistaDB.DDA;

namespace Redbox.KioskEngine.Environment;

internal class CachedVistaDBDataStoreProvider : CachedDataStoreProvider
{
	protected override ReadOnlyCollection<string> OnGetTables(string dataStore)
	{
		List<string> list = new List<string>();
		IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
		if (workStore == null)
		{
			return list.AsReadOnly();
		}
		try
		{
			foreach (string item in workStore.EnumTables())
			{
				list.Add(item);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in CachedVistaDbDataStoreProvider.OnGetTables({dataStore})", e);
			throw;
		}
		finally
		{
			DataStoreCache.CloseStore(DataStoreProvider.GetPath(dataStore));
		}
		return list.AsReadOnly();
	}

	protected override Dictionary<string, object> GetCachedTableCreate(string dataStore, string tableName)
	{
		Dictionary<string, Dictionary<string, object>> cachedDataStoreCreate = GetCachedDataStoreCreate(dataStore);
		if (cachedDataStoreCreate.ContainsKey(tableName))
		{
			return cachedDataStoreCreate[tableName];
		}
		Dictionary<string, object> dictionary = InternalGetTable(dataStore, tableName);
		if (dictionary == null)
		{
			dictionary = new Dictionary<string, object>();
		}
		cachedDataStoreCreate[tableName] = dictionary;
		return cachedDataStoreCreate[tableName];
	}

	private Dictionary<string, object> InternalGetTable(string dataStore, string tableName)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		try
		{
			IVistaDBDatabase workStore = DataStoreCache.GetWorkStore(DataStoreProvider.GetPath(dataStore));
			if (workStore == null)
			{
				return dictionary;
			}
			if (!workStore.ContainsTable(tableName))
			{
				return dictionary;
			}
			using IVistaDBTable vistaDBTable = workStore.OpenTable(tableName, exclusive: false, readOnly: true);
			vistaDBTable.Next();
			while (!vistaDBTable.EndOfTable)
			{
				string text = vistaDBTable.CurrentRow["Key"].Value as string;
				string value = vistaDBTable.CurrentRow["Value"].Value as string;
				if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(value))
				{
					dictionary[text] = value;
					vistaDBTable.Next();
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in CachedVistaDbDataStoreProvider.OnGetKeys({dataStore},{tableName})", e);
			throw;
		}
		finally
		{
			DataStoreCache.CloseStore(DataStoreProvider.GetPath(dataStore));
		}
		return dictionary;
	}
}
