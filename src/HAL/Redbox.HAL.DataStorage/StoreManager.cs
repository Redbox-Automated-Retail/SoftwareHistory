using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataStorage;

public sealed class StoreManager
{
	public readonly string DataDirectory;

	private readonly Dictionary<PersistentMapTypes, IPersistentMap> ObjectMap = new Dictionary<PersistentMapTypes, IPersistentMap>();

	private readonly Dictionary<string, IDataStore> Cache = new Dictionary<string, IDataStore>();

	public static StoreManager Instance => Singleton<StoreManager>.Instance;

	public void CloseStores()
	{
		lock (Cache)
		{
			foreach (string key in Cache.Keys)
			{
				IDataStore dataStore = Cache[key];
				dataStore.CloseConnection(force: true);
			}
			Cache.Clear();
		}
	}

	public void Initialize()
	{
		lock (Cache)
		{
			if (Cache.Keys.Count <= 0)
			{
				string source = Path.Combine(DataDirectory, "HALData.vdb3");
				Cache["HALData.vdb3"] = new NoLockVistaDBStore(source);
				source = Path.Combine(DataDirectory, "HALCounters.vdb3");
				ThreadSafeVistaDBStore threadSafeVistaDBStore = new ThreadSafeVistaDBStore(source);
				Cache["HALCounters.vdb3"] = threadSafeVistaDBStore;
				ObjectMap[PersistentMapTypes.Database] = new SecretOptions(threadSafeVistaDBStore);
				ObjectMap[PersistentMapTypes.Engine] = new RegistryStore("SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store");
				ObjectMap[PersistentMapTypes.Registry] = new RegistryStore("SOFTWARE\\Redbox\\HAL");
			}
		}
	}

	public IDataStore FindByName(string store)
	{
		lock (Cache)
		{
			if (Cache.ContainsKey(store))
			{
				return Cache[store];
			}
			return null;
		}
	}

	public bool UpgradeTables(TextWriter writer)
	{
		IDataStore dataStore = FindByName("HALCounters.vdb3");
		if (dataStore == null)
		{
			writer.WriteLine("[UpgradeTables] Cannot locate a counters database. EPIC FAILURE.");
			return false;
		}
		CountersStore countersStore = new CountersStore();
		foreach (string table in countersStore.Tables)
		{
			TestAndCreate(dataStore, table, writer);
		}
		dataStore = FindByName("HALData.vdb3");
		if (dataStore == null)
		{
			writer.WriteLine("[UpgradeTables] Cannot locate a Data database. EPIC FAILURE.");
			return false;
		}
		InventoryStore inventoryStore = new InventoryStore();
		foreach (string table2 in inventoryStore.Tables)
		{
			TestAndCreate(dataStore, table2, writer);
		}
		return true;
	}

	public IPersistentMap GetDataStore(PersistentMapTypes type)
	{
		if (ObjectMap.ContainsKey(type))
		{
			return ObjectMap[type];
		}
		return null;
	}

	public IPersistentMap GetDataStore()
	{
		return GetDataStore(PersistentMapTypes.Database);
	}

	private bool TestAndCreate(IDataStore store, string table, TextWriter writer)
	{
		if (store.TableExists(table))
		{
			writer.WriteLine("The table '{0}' already exists - take no action.", table);
			return true;
		}
		if (!TableNames.CreateMap.ContainsKey(table))
		{
			return true;
		}
		if (!store.CreateTable(TableNames.CreateMap[table]))
		{
			writer.WriteLine("Unable to create new database table '{0}'.", table);
			return false;
		}
		writer.WriteLine("The table '{0}' was successfully created.", table);
		return true;
	}

	private StoreManager()
	{
		DataDirectory = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "data");
	}
}
