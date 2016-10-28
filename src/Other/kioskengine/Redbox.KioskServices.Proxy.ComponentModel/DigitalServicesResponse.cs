namespace Redbox.KioskServices.Proxy.ComponentModel;

public enum DigitalServicesResponse
{
	Valid,
	[ResponseCode(Name = "(A002)")]
	CustomerDisabled,
	[ResponseCode(Name = "(A003)")]
	RentalLimitExceeded,
	[ResponseCode(Name = "(GCA002)")]
	GCRentalLimitExceeded,
	[ResponseCode(Name = "(C999)")]
	RemotingException,
	[ResponseCode(Name = "(U001)")]
	[ResponseCode(Name = "(A009)")]
	Error
}
