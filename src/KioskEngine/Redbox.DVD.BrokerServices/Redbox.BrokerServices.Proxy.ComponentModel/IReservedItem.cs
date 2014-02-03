using Redbox.KioskEngine.ComponentModel;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

internal interface IReservedItem
{
	string Barcode { get; set; }

	LineItemStatus Status { get; set; }

	int ProductId { get; set; }
}
