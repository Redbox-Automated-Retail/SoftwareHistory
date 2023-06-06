namespace KioskClientService.API.Services.Configuration;

[Category(Name = "Browse")]
public class BrowseConfiguration : BaseCategorySetting
{
	public bool ShowDealsBrowseFilterButton { get; set; }

	public bool Show99CentsBrowseFilterButton { get; set; }

	public bool EnableSonySweepstakes { get; set; }
}
