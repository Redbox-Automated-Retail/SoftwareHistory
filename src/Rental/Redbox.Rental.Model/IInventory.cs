using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Model;

public interface IInventory
{
	DateTime? InventoryActionModificationTime { get; }

	DateTime? RapidStatusModificationTime { get; }

	void CheckForNewFiles(Action inventoryCompletion, Action newRapidStatus);

	Inventory GetRapidStatus(string barcode);

	ProductLookupCatalogModel GetRapidStatus();

	List<RebalancesInModel> GetRebalancesIn();

	int GetFillQuantity();

	Inventory GetInventory(string barcode);

	Dictionary<string, Inventory> GetInventories(List<string> barcodes);

	string GetInventoryDataMachineOrigin();

	string GetInventoryDataCreatedOn();

	long GetInventoryDataCount();
}
