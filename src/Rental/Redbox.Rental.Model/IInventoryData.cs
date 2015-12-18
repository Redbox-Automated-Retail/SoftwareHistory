using System;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Model;

public interface IInventoryData
{
	void CheckForNewFiles(out bool rapidStatusChanged);

	Inventory GetRapidStatus(string barcode);

	ProductLookupCatalogModel GetRapidStatus();

	InventoryActionFile GetInventoryActionFile();

	DateTime? GetLastCompletedDate();

	void SetLastCompletedDate(DateTime dt);

	DateTime? GetLastRunThinJobDate();

	void SetLastRunThinJobDate(DateTime dt);

	DateTime? GetInventoryActionModificationTime();

	DateTime? GetRapidStatusModificationTime();
}
