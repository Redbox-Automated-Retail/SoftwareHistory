using Redbox.KioskEngine.ComponentModel;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

public interface IReservedItem
{
	string Barcode { get; set; }

	LineItemStatus Status { get; set; }

	int ProductId { get; set; }
}
