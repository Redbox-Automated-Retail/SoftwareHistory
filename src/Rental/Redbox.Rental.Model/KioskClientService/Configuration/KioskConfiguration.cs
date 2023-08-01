namespace Redbox.Rental.Model.KioskClientService.Configuration;

public class KioskConfiguration : BaseConfiguration
{
	public DeviceServiceConfiguration DeviceService { get; set; } = new DeviceServiceConfiguration();

	public BrowseConfiguration Browse { get; set; } = new BrowseConfiguration();

	public CMSConfiguration CMS { get; set; } = new CMSConfiguration();

	public _KioskConfiguration Kiosk { get; set; } = new _KioskConfiguration();

	public DualConfiguration Dual { get; set; } = new DualConfiguration();

	public InventoryConfiguration Inventory { get; set; } = new InventoryConfiguration();
}
