using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Campaign;

public interface IKioskClientServiceCampaign
{
	bool IsKCSInCartEnabled { get; }

	bool IsKCSStartScreenEnabled { get; }

	int KCSLoadCampaignTimerDuration { get; }

	void LoadCampaign();

	KioskInCartDetails GetInCart();

	KioskStartScreenDetails GetStartScreen();

	ICarouselControl GetCarouselControl();

	IList<IStartAssetControl> GetStartAssetControls();
}
