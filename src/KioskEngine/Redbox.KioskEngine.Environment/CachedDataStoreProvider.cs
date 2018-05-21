using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.Core;

namespace Redbox.KioskEngine.Environment;

internal class CachedDataStoreProvider : DataStoreProvider
{
	private Dictionary<string, Dictionary<string, Dictionary<string, object>>> _cache = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();

	protected override void OnClear(string dataStore, string tableName)
	{
		RemoveCachedTable(dataStore, tableName);
	}

	protected override void OnCloseAllStores()
	{
		_cache = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
	}

	protected override void OnCloseStore(string dataStore)
	{
		RemoveCachedDataStore(dataStore);
	}

	protected override long OnCount(string dataStore, string tableName)
	{
		Dictionary<string, object> cachedTable = GetCachedTable(dataStore, tableName);
		if (cachedTable == null)
		{
			return 0L;
		}
		return cachedTable.Count;
	}

	protected override object OnGet(string dataStore, string tableName, string keyName)
	{
		Dictionary<string, object> cachedTableCreate = GetCachedTableCreate(dataStore, tableName);
		if (cachedTableCreate.ContainsKey(keyName))
		{
			return cachedTableCreate[keyName];
		}
		return null;
	}

	protected override Dictionary<string, object> OnGetAll(string dataStore, string tableName)
	{
		return GetCachedTableCreate(dataStore, tableName);
	}

	protected override void OnDrop(string dataStore, string tableName)
	{
		RemoveCachedTable(dataStore, tableName);
	}

	protected override ReadOnlyCollection<string> OnGetKeys(string dataStore, string tableName)
	{
		List<string> keys = new List<string>();
		GetCachedTableCreate(dataStore, tableName).ForEach(delegate(KeyValuePair<string, object> pair)
		{
			keys.Add(pair.Key);
		});
		return keys.AsReadOnly();
	}

	protected override void OnRemove(string dataStore, string tableName, string keyName)
	{
		Dictionary<string, object> cachedTable = GetCachedTable(dataStore, tableName);
		if (cachedTable != null && cachedTable.ContainsKey(keyName))
		{
			cachedTable.Remove(keyName);
		}
	}

	protected override void OnSet(string dataStore, string tableName, string keyName, object data)
	{
		GetCachedTableCreate(dataStore, tableName)[keyName] = data as string;
	}

	protected override void OnBulkSet(string dataStore, string tableName, IDictionary<string, object> values)
	{
		Dictionary<string, object> table = GetCachedTableCreate(dataStore, tableName);
		values.ForEach(delegate(KeyValuePair<string, object> pair)
		{
			table[pair.Key] = pair.Value as string;
		});
	}

	protected override ReadOnlyCollection<string> OnGetTables(string dataStore)
	{
		List<string> tables = new List<string>();
		Dictionary<string, Dictionary<string, object>> cachedDataStore = GetCachedDataStore(dataStore);
		if (cachedDataStore == null)
		{
			return tables.AsReadOnly();
		}
		cachedDataStore.ForEach(delegate(KeyValuePair<string, Dictionary<string, object>> pair)
		{
			tables.Add(pair.Key);
		});
		return tables.AsReadOnly();
	}

	private bool IsDataStoreCached(string dataStore)
	{
		return _cache.ContainsKey(dataStore);
	}

	private bool IsTableCached(string dataStore, string tableName)
	{
		if (!IsDataStoreCached(dataStore))
		{
			return false;
		}
		return _cache[dataStore].ContainsKey(tableName);
	}

	private Dictionary<string, Dictionary<string, object>> GetCachedDataStore(string dataStore)
	{
		if (IsDataStoreCached(dataStore))
		{
			return _cache[dataStore];
		}
		return null;
	}

	protected Dictionary<string, Dictionary<string, object>> GetCachedDataStoreCreate(string dataStore)
	{
		if (!IsDataStoreCached(dataStore))
		{
			_cache[dataStore] = new Dictionary<string, Dictionary<string, object>>();
		}
		return _cache[dataStore];
	}

	private Dictionary<string, object> GetCachedTable(string dataStore, string tableName)
	{
		Dictionary<string, Dictionary<string, object>> cachedDataStore = GetCachedDataStore(dataStore);
		if (cachedDataStore == null)
		{
			return null;
		}
		if (cachedDataStore.ContainsKey(tableName))
		{
			return cachedDataStore[tableName];
		}
		return null;
	}

	protected virtual Dictionary<string, object> GetCachedTableCreate(string dataStore, string tableName)
	{
		Dictionary<string, Dictionary<string, object>> cachedDataStoreCreate = GetCachedDataStoreCreate(dataStore);
		if (cachedDataStoreCreate.ContainsKey(tableName))
		{
			return cachedDataStoreCreate[tableName];
		}
		cachedDataStoreCreate[tableName] = new Dictionary<string, object>();
		return cachedDataStoreCreate[tableName];
	}

	private void RemoveCachedDataStore(string dataStore)
	{
		if (_cache.ContainsKey(dataStore))
		{
			_cache.Remove(dataStore);
		}
	}

	private void RemoveCachedTable(string dataStore, string tableName)
	{
		Dictionary<string, Dictionary<string, object>> cachedDataStore = GetCachedDataStore(dataStore);
		if (cachedDataStore != null && cachedDataStore.ContainsKey(tableName))
		{
			cachedDataStore.Remove(tableName);
		}
	}
}
