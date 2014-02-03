namespace Redbox.KioskServices.Proxy.ComponentModel;

internal enum AuthorizeResponse
{
	Approved,
	[ResponseCode(Name = "(A001)")]
	MissingKiosk,
	[ResponseCode(Name = "(A002)")]
	CustomerDisabled,
	[ResponseCode(Name = "(A003)")]
	RentalLimitExceeded,
	[ResponseCode(Name = "(A004)")]
	CreditCardError,
	[ResponseCode(Name = "(A005)")]
	AvsFailure,
	[ResponseCode(Name = "(A006)")]
	Declined,
	[ResponseCode(Name = "(D001)")]
	CodeNotValid,
	[ResponseCode(Name = "(D003)")]
	CodeAlreadyUsed,
	[ResponseCode(Name = "(D002)")]
	CodeNotValidAtThisLocation,
	[ResponseCode(Name = "(A008)")]
	ProcesorError,
	[ResponseCode(Name = "(A009)")]
	[ResponseCode(Name = "(U001)")]
	Error,
	[ResponseCode(Name = "(D004)")]
	NewCustomerOnly,
	[ResponseCode(Name = "(A010)")]
	GameLimitExceeded,
	[ResponseCode(Name = "(A014)")]
	CheckIfCustomerWantsToUseCredits
}
