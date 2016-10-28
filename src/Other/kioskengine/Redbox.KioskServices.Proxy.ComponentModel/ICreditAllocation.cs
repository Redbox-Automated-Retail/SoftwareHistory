namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface ICreditAllocation
{
	string Barcode { get; }

	decimal Amount { get; }

	int NumberApplied { get; }
}
