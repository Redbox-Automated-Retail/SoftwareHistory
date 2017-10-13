namespace Redbox.KioskServices.Proxy.ComponentModel;

internal interface IInventoryItem
{
	int? Deck { get; set; }

	int? Slot { get; set; }

	string Barcode { get; set; }

	InventoryItemStatus Status { get; set; }
}
