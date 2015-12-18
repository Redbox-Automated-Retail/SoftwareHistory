using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.ProductLookupCatalog;

namespace Redbox.KioskEngine.Environment;

internal class ProductLookupDataStoreProvider : DataStoreProvider
{
	protected override long OnCount(string dataStore, string tableName)
	{
		if (tableName == "Version")
		{
			return 2L;
		}
		using Archive archive = Archive.Open(DataStoreProvider.GetPath(dataStore));
		return archive.GetNumberOfRecords();
	}

	protected override object OnGet(string dataStore, string tableName, string keyName)
	{
		using Archive archive = Archive.Open(DataStoreProvider.GetPath(dataStore));
		if (tableName == "Version")
		{
			if (keyName == "origin_machine")
			{
				return "{ name = '" + archive.GetOriginMachine().Trim() + "' }";
			}
			if (keyName == "generated_on")
			{
				return "{ date = '" + archive.GetCreatedOn().Trim() + "' }";
			}
		}
		if (tableName != "BarcodeToProduct")
		{
			return null;
		}
		Inventory inventory = archive.Find(keyName);
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
				using (Archive archive = Archive.Open(DataStoreProvider.GetPath(dataStore)))
				{
					for (int i = 0; i < Math.Min(1000L, archive.GetNumberOfRecords()); i++)
					{
						list.Add(archive[i].Barcode);
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
}
