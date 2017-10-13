namespace Redbox.KioskServices.Proxy.ComponentModel;

internal enum ReturnType
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
