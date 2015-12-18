using Redbox.KioskEngine.ComponentModel;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

internal interface IProductItem
{
	string Barcode { get; set; }

	bool MultiDisc { get; set; }

	LineItemStatus Status { get; set; }
}
