using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.Ads;
using KioskClientService.API.App;
using KioskClientService.API.Services.Ads.AdServer;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.App;

public class ApplicationService : IApplicationService
{
	private ILogger<ApplicationService> _logger;

	private IAdService _adService;

	public ApplicationService(ILogger<ApplicationService> logger, IEnumerable<IAdService> adServices, IApplicationConfiguration config)
	{
		_logger = logger;
		_adService = (config.UseAdServer ? adServices.GetImplementation<AdServerService>() : adServices.GetImplementation<AdService>());
	}

	public async Task<ApplicationResponse> Start()
	{
		_logger?.LogInformation("ApplicationService.Start");
		await (_adService?.StartApplication());
		return new ApplicationResponse();
	}

	public async Task<ApplicationResponse> End()
	{
		_logger?.LogInformation("ApplicationService.End");
		await _adService.EndApplication();
		return new ApplicationResponse();
	}

	public async Task<ViewShowResponse> ViewChangedEvent(ViewStateChangedEvent e)
	{
		if (e.ToView != null)
		{
			await _adService.GetAds(e.ToView, isPreload: true);
		}
		return new ViewShowResponse();
	}
}
