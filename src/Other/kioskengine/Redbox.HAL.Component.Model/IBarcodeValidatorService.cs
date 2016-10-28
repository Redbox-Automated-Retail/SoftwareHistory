namespace Redbox.HAL.Component.Model;

internal interface IBarcodeValidatorService
{
	bool IsValid(string barcode);
}
