using Redbox.Rental.Model.KioskClientService.Ads;
using Redbox.Rental.Model.KioskClientService.Application;
using Redbox.Rental.Model.KioskClientService.Authentication;
using Redbox.Rental.Model.KioskClientService.CustomerProfile;
using Redbox.Rental.Model.KioskClientService.Data;
using Redbox.Rental.Model.KioskClientService.Installer;
using Redbox.Rental.Model.KioskClientService.Inventory;
using Redbox.Rental.Model.KioskClientService.Loyalty;
using Redbox.Rental.Model.KioskClientService.Marketing;
using Redbox.Rental.Model.KioskClientService.Personalization;
using Redbox.Rental.Model.KioskClientService.Planogram;
using Redbox.Rental.Model.KioskClientService.Products;
using Redbox.Rental.Model.KioskClientService.Promotion;
using Redbox.Rental.Model.KioskClientService.Session;
using Redbox.Rental.Model.KioskClientService.Subscriptions;
using Redbox.Rental.Model.KioskClientService.TestData;
using Redbox.Rental.Model.KioskClientService.Transactions;
using Redbox.Rental.Model.KioskClientService.Utilities;

namespace Redbox.Rental.Model.KioskClientService;

public interface IKioskClientService
{
	IKioskClientServiceAds Ads { get; }

	IKioskClientServiceApplication Application { get; }

	IKioskClientServiceAuthentication Authentication { get; }

	IKioskClientServiceCustomerProfile CustomerProfile { get; }

	IKioskClientServiceData Data { get; }

	IKioskClientServiceInstaller Installer { get; }

	IKioskClientServiceInventory Inventory { get; }

	IKioskClientServiceLoyalty Loyalty { get; }

	IKioskClientServiceMarketing Marketing { get; }

	IKioskClientServicePersonalization Personalization { get; }

	IKioskClientServiceProductGroups ProductGroups { get; }

	IKioskClientServicePromotion Promotion { get; }

	IKioskClientServiceQRCodeUtilities QRCodeUtilities { get; }

	IKioskClientServiceSession Session { get; }

	IKioskClientServiceSubscriptions Subscriptions { get; }

	IKioskClientServiceTestData TestData { get; }

	IKioskClientServiceTransaction Transaction { get; }

	IKioskClientServicePlanogram Planogram { get; }
}
