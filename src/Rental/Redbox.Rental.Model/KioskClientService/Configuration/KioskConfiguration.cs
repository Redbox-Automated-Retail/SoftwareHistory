namespace Redbox.Rental.Model.KioskClientService.Configuration;

public class KioskConfiguration : BaseConfiguration
{
	public DeviceServiceConfiguration DeviceService { get; set; } = new DeviceServiceConfiguration();

	public BrowseConfiguration Browse { get; set; } = new BrowseConfiguration();

	public CMSConfiguration CMS { get; set; } = new CMSConfiguration();
}
