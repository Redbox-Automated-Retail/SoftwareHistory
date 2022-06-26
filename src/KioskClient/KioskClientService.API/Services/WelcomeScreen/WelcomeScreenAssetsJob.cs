using System.Threading.Tasks;
using Coravel.Invocable;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.WelcomeScreen;

public class WelcomeScreenAssetsJob : IInvocable
{
	private ILogger<WelcomeScreenAssetsJob> _logger;

	private readonly IWelcomeScreenAssetsService _welcomeScreenAssetService;

	private readonly IOptionsSnapshotKioskConfiguration _configuration;

	public WelcomeScreenAssetsJob(ILogger<WelcomeScreenAssetsJob> logger, IWelcomeScreenAssetsService planogramService, IOptionsSnapshotKioskConfiguration configuration)
	{
		_logger = logger;
		_welcomeScreenAssetService = planogramService;
		_configuration = configuration;
	}

	public async Task Invoke()
	{
		if (!_configuration.MarketingWelcomeScreen.EnableAssetsSync)
		{
			_logger.LogInfoWithSource("Welcome Screen Assets sync is not enabled", "Invoke");
		}
		else
		{
			await _welcomeScreenAssetService.RefreshIfNeeded();
		}
	}
}
