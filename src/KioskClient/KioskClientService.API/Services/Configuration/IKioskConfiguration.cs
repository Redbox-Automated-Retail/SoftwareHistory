namespace KioskClientService.API.Services.Configuration;

public interface IKioskConfiguration
{
	long ConfigurationVersion { get; }

	MarketingWelcomeScreen MarketingWelcomeScreen { get; }

	CampaignConfiguration Campaigns { get; set; }

	KioskClientAds KioskClientAds { get; }

	KioskClientDefault KioskClientDefault { get; }

	KioskClientProxyApi KioskClientProxyApi { get; }

	KioskClientSubscription KioskClientSubscriptions { get; }

	TestData TestData { get; }

	void Log(ConfigLogParameters loggingParameters = null);
}
