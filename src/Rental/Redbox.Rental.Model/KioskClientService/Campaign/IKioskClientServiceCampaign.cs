namespace Redbox.Rental.Model.KioskClientService.Campaign;

public interface IKioskClientServiceCampaign
{
	KioskInCartDetails GetInCart();

	KioskStartScreenDetails GetStartScreen();
}
