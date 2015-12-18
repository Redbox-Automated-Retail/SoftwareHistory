using System;
using System.Collections.Generic;
using System.IO;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.ProductLookupCatalog;

namespace Redbox.KioskEngine.Environment;

public class ProductLookupCatalogService : IProductLookupCatalogService
{
	public ProductLookupCatalogModel GetProductLookupCatalog(string path)
	{
		new Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory>();
		try
		{
			ProductLookupCatalogModel productLookupCatalogModel = new ProductLookupCatalogModel();
			productLookupCatalogModel.Barcodes = new Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory>();
			ProductLookupCatalogModel productLookupCatalogModel2 = productLookupCatalogModel;
			if (!File.Exists(path))
			{
				LogHelper.Instance.Log("ProductLookupCatalogModel.GetProductLookupCatalog - ProductLookup file ({0}) does not exist", path);
				return null;
			}
			if (!MemoryArchive.IsValid(path))
			{
				LogHelper.Instance.LogError("ProductLookupCatalogModel.GetProductLookupCatalog", $"ProductLookup file ({path}) is not valid");
				return null;
			}
			using (MemoryArchive memoryArchive = MemoryArchive.Open(path))
			{
				productLookupCatalogModel2.OriginMachine = memoryArchive.GetOriginMachine().Trim();
				productLookupCatalogModel2.GeneratedOn = memoryArchive.GetCreatedOn().Trim();
				for (int i = 0; i < memoryArchive.Count; i++)
				{
					Redbox.ProductLookupCatalog.Inventory inventory = memoryArchive[i];
					if (inventory != null && inventory.Barcode != null && !productLookupCatalogModel2.Barcodes.ContainsKey(inventory.Barcode))
					{
						productLookupCatalogModel2.Barcodes.Add(inventory.Barcode, new Redbox.KioskEngine.ComponentModel.Inventory
						{
							Barcode = inventory.Barcode,
							TitleId = inventory.TitleId,
							Code = (Redbox.KioskEngine.ComponentModel.InventoryStatusCode)inventory.Code,
							TotalRentalCount = inventory.TotalRentalCount
						});
					}
				}
			}
			LogHelper.Instance.Log("ProductLookupCatalogModel.GetProductLookupCatalog - ProductLookup file {0} contains {1} barcodes.", path, productLookupCatalogModel2.Barcodes.Count);
			return productLookupCatalogModel2;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"ProductLookupCatalogModel.GetProductLookupCatalog - An unhandled exception occurred", e);
		}
		return null;
	}
}
