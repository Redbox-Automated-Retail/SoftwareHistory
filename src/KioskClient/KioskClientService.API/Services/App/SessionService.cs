using KioskClientService.API.Ads;
using KioskClientService.API.App;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.App;

public class SessionService : ISessionService
{
	private ILogger<SessionService> _logger;

	private IAdService _adService;

	public SessionService(ILogger<SessionService> logger, IAdService adService)
	{
		_logger = logger;
		_adService = adService;
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
