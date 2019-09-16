namespace DeviceService.ComponentModel;

public enum CardReadExitType
{
	GoodRead,
	BadRead,
	Cancelled,
	ButtonPressed,
	ClessCardFloorLimitExceeded,
	MaxClessFloorLimitExceeded,
	InvalidPrompt,
	EncryptionFailed,
	BadKeyCard,
	BadFormatOf23,
	AmountWasNotSetAndContactlessReaderWasNotEnabled,
	AtLeastOneSpecifiedReaderIsDisabled
}
