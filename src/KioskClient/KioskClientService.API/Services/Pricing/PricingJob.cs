using System.Threading.Tasks;
using Coravel.Invocable;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Pricing;

public class PricingJob : IInvocable
{
	private ILogger<PricingJob> _logger;

	private readonly IPricingService _pricingService;

	private readonly IKioskConfiguration _kioskConfiguration;

	public PricingJob(ILogger<PricingJob> logger, IPricingService pricingService, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration)
	{
		_logger = logger;
		_pricingService = pricingService;
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
	}

	public async Task Invoke()
	{
		if (!_kioskConfiguration.KioskClientDefault.PricingActive)
		{
			_logger.LogInfoWithSource("Pricing is not active", "Invoke");
			return;
		}
		_logger.LogInfoWithSource("Invoking pricing Job to see if updates are needed", "Invoke");
		await _pricingService.RefreshIfNeeded();
		_logger.LogInfoWithSource("Finished", "Invoke");
	}
}
