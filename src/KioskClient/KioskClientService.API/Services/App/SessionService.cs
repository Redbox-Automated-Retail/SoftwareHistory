using System.Collections.Generic;
using KioskClientService.API.Ads;
using KioskClientService.API.App;
using KioskClientService.API.Services.Ads.AdServer;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.App;

public class SessionService : ISessionService
{
	private ILogger<SessionService> _logger;

	private IAdService _adService;

	public SessionService(ILogger<SessionService> logger, IEnumerable<IAdService> adServices, IApplicationConfiguration config)
	{
		_logger = logger;
		_adService = (config.UseAdServer ? adServices.GetImplementation<AdServerService>() : adServices.GetImplementation<AdService>());
	}

	public ApplicationResponse Start(SessionType sessionType)
	{
		_logger?.LogInfoWithSource($"SessionType:{sessionType}", "Start");
		_adService.StartSession();
		return new ApplicationResponse();
	}

	public ApplicationResponse End()
	{
		_adService.EndSession();
		return new ApplicationResponse();
	}
}
