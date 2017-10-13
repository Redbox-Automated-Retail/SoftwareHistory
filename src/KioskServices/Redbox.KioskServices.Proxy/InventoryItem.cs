using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.Proxy;

public class InventoryItem : IInventoryItem
{
	public int? Deck { get; set; }

	public int? Slot { get; set; }

	public string Barcode { get; set; }

	public InventoryItemStatus Status { get; set; }
}
