using System;

namespace Redbox.Rental.Model;

public class InventoryDataFile
{
	public DateTime? LastCompletedDate { get; set; }

	public DateTime? LastRunThinJobDate { get; set; }
}
