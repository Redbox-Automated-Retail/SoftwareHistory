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
}
