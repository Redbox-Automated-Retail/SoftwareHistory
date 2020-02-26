using Redbox.Rental.Model.KioskClientService.Ads;
using Redbox.Rental.Model.KioskClientService.Application;
using Redbox.Rental.Model.KioskClientService.Products;
using Redbox.Rental.Model.KioskClientService.Session;

namespace Redbox.Rental.Model.KioskClientService;

public interface IKioskClientService
{
	IKioskClientServiceAds Ads { get; }

	IKioskClientServiceApplication Application { get; }

	IKioskClientServiceSession Session { get; }

	IKioskClientServiceProductGroups ProductGroups { get; }
}
