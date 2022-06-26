namespace KioskClientService.API.Services.Configuration;

public interface IKioskConfiguration
{
	ZeroTouch ZeroTouch { get; }

	MarketingGeneral MarketingGeneral { get; }

	MarketingWelcomeScreen MarketingWelcomeScreen { get; }
}
