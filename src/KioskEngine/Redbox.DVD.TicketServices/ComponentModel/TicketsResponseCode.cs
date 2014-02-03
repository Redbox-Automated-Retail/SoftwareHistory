namespace Redbox.TicketServices.Proxy.ComponentModel;

internal enum TicketsResponseCode
{
	Success,
	Error,
	[ResponseCode(Name = "101")]
	TicketsNotAvailable,
	[ResponseCode(Name = "102")]
	NoContiguousSeats,
	[ResponseCode(Name = "103")]
	TicketsAreSoldOut,
	[ResponseCode(Name = "151")]
	CardNotAuthorized,
	[ResponseCode(Name = "152")]
	CardNotAuthorizedNSF,
	[ResponseCode(Name = "153")]
	CustomerFraudRisk,
	[ResponseCode(Name = "154")]
	LockExpiredRelockFailed,
	[ResponseCode(Name = "155")]
	MerchantServiceFailure,
	[ResponseCode(Name = "156")]
	TransactionLimitExceeded,
	[ResponseCode(Name = "160")]
	AccountNotAuthorizedInCP,
	[ResponseCode(Name = "161")]
	CardDeclined
}
