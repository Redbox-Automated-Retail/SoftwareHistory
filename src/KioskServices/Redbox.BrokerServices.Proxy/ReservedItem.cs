using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.BrokerServices.Proxy;

public class ReservedItem : IReservedItem
{
	public string Barcode { get; set; }

	public LineItemStatus Status { get; set; }

	public int ProductId { get; set; }
}
