namespace Redbox.KioskServices.Proxy.ComponentModel;

public enum PromotionValidationResponse
{
	Valid,
	[ResponseCode(Name = "(D001)")]
	CodeNotValid,
	[ResponseCode(Name = "(D003)")]
	CodeAlreadyUsed,
	[ResponseCode(Name = "(D002)")]
	CodeNotValidAtThisLocation,
	[ResponseCode(Name = "(D005)")]
	KioskMissing,
	[ResponseCode(Name = "(U001)")]
	Error
}
