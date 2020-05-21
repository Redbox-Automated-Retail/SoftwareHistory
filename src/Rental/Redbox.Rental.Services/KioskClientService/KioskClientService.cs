using Redbox.Core;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Ads;
using Redbox.Rental.Model.KioskClientService.Application;
using Redbox.Rental.Model.KioskClientService.Inventory;
using Redbox.Rental.Model.KioskClientService.Products;
using Redbox.Rental.Model.KioskClientService.Session;
using Redbox.Rental.Services.KioskClientService.Ads;
using Redbox.Rental.Services.KioskClientService.Application;
using Redbox.Rental.Services.KioskClientService.Inventory;
using Redbox.Rental.Services.KioskClientService.Products;
using Redbox.Rental.Services.KioskClientService.Session;

namespace Redbox.Rental.Services.KioskClientService;

public class KioskClientService : IKioskClientService
{
	private IKioskClientServiceAds _ads;

	private IKioskClientServiceApplication _application;

	private IKioskClientServiceSession _session;

	private IKioskClientServiceProductGroups _productGroups;

	private IKioskClientServiceInventory _inventory;

	public static KioskClientService Instance => Singleton<KioskClientService>.Instance;

	public IKioskClientServiceAds Ads => _ads;

	public IKioskClientServiceApplication Application => _application;

	public IKioskClientServiceSession Session => _session;

	public IKioskClientServiceProductGroups ProductGroups => _productGroups;

	public IKioskClientServiceInventory Inventory => _inventory;

	private KioskClientService()
	{
		_ads = new KioskClientServiceAds();
		_application = new KioskClientServiceApplication();
		_session = new KioskClientServiceSession();
		_productGroups = new KioskClientServiceProductGroups();
		_inventory = new KioskClientServiceInventory();
	}
}
