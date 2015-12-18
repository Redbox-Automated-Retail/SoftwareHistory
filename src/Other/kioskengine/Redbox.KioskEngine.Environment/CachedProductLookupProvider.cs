using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Redbox.Core;
using Redbox.ProductLookupCatalog;

namespace Redbox.KioskEngine.Environment;

internal class CachedProductLookupProvider : CachedDataStoreProvider
{
	protected override object OnGet(string dataStore, string tableName, string keyName)
	{
		using MemoryArchive memoryArchive = MemoryArchive.Open(DataStoreProvider.GetPath(dataStore));
		if (tableName == "Version")
		{
			if (keyName == "origin_machine")
			{
				return "{ name = '" + memoryArchive.GetOriginMachine().Trim() + "' }";
			}
			if (keyName == "generated_on")
			{
				return "{ date = '" + memoryArchive.GetCreatedOn().Trim() + "' }";
			}
		}
		if (tableName != "BarcodeToProduct")
		{
			return null;
		}
		Inventory inventory = memoryArchive.Find(keyName);
		if (inventory == null)
		{
			return null;
		}
		string arg = inventory.Code.ToString();
		if (inventory.Code == InventoryStatusCode.WrongTitle)
		{
			arg = "Wrong Title";
		}
		return "{ " + $"product_id = {inventory.TitleId}, status = '{arg}', total_rental_count = {inventory.TotalRentalCount}" + " }";
	}

	protected override void OnBulkSet(string dataStore, string tableName, IDictionary<string, object> values)
	{
		if (!(tableName == "Version"))
		{
		}
	}

	protected override ReadOnlyCollection<string> OnGetKeys(string dataStore, string tableName)
	{
		if (!(tableName == "Version"))
		{
			if (tableName == "BarcodeToProduct")
			{
				List<string> list = new List<string>();
				Dictionary<string, Dictionary<string, object>> cachedDataStoreCreate = GetCachedDataStoreCreate(dataStore);
				if (cachedDataStoreCreate.ContainsKey(tableName))
				{
					Dictionary<string, object> dictionary = cachedDataStoreCreate[tableName];
					foreach (KeyValuePair<string, object> item in dictionary)
					{
						list.Add(item.Key);
					}
				}
				return list.AsReadOnly();
			}
			return new List<string>().AsReadOnly();
		}
		List<string> list2 = new List<string>();
		list2.Add("origin_machine");
		list2.Add("generated_on");
		return list2.AsReadOnly();
	}

	protected override ReadOnlyCollection<string> OnGetTables(string dataStore)
	{
		List<string> list = new List<string>();
		list.Add("Version");
		list.Add("BarcodeToProduct");
		List<string> list2 = list;
		return list2.AsReadOnly();
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
			if (!File.Exists(DataStoreProvider.GetPath(dataStore)))
			{
				LogHelper.Instance.Log("CachedProductLookup file '" + dataStore + "' does not exist, returning empty table '" + tableName + "'.");
				return dictionary;
			}
			if (!MemoryArchive.IsValid(DataStoreProvider.GetPath(dataStore)))
			{
				LogHelper.Instance.Log("CachedProductLookup file '" + dataStore + "' is not a valid inventory archive, returning empty table '" + tableName + "'.");
				return dictionary;
			}
			using MemoryArchive memoryArchive = MemoryArchive.Open(DataStoreProvider.GetPath(dataStore));
			if (tableName == "Version")
			{
				dictionary.Add("origin_machine", "{ name = '" + memoryArchive.GetOriginMachine().Trim() + "' }");
				dictionary.Add("generated_on", "{ date = '" + memoryArchive.GetCreatedOn().Trim() + "' }");
			}
			else
			{
				for (int i = 0; i < memoryArchive.Count; i++)
				{
					Inventory inventory = memoryArchive[i];
					if (inventory != null && inventory.Barcode != null)
					{
						string arg = inventory.Code.ToString();
						if (inventory.Code == InventoryStatusCode.WrongTitle)
						{
							arg = "Wrong Title";
						}
						string value = "{ " + $"product_id = {inventory.TitleId}, status = '{arg}', total_rental_count = {inventory.TotalRentalCount}" + " }";
						string barcode = inventory.Barcode;
						dictionary[barcode] = value;
					}
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Exception in CachedProductLookupProvider.InternalGetTable({dataStore},{tableName})", e);
			throw;
		}
		return dictionary;
	}
}
