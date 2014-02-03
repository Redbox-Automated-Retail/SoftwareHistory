namespace Redbox.KioskServices.Proxy.ComponentModel;

internal interface ICreditAllocation
{
	string Barcode { get; }

	decimal Amount { get; }

	int NumberApplied { get; }
}
