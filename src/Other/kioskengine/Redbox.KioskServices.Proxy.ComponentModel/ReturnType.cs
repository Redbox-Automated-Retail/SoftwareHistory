namespace Redbox.KioskServices.Proxy.ComponentModel;

public enum ReturnType
{
	Unknown = 1,
	ServiceMenu,
	InventorySync,
	Qlm,
	ExceptionReturn,
	KioskReturn,
	QuickReturn,
	BackwardsDiscReturn,
	UnplayableReturn,
	CrossBorderReturn,
	FailedSecurityRead
}
