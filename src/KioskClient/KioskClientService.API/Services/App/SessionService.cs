using System.Threading.Tasks;
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

	public async Task<ApplicationResponse> Start(SessionType sessionType)
	{
		_logger?.LogInformation($"SessionService.Start - SessionType:{sessionType}");
		_adService.StartSession();
		return new ApplicationResponse();
	}

	public async Task<ApplicationResponse> End()
	{
		_logger?.LogInformation("SessionService.End");
		_adService.EndSession();
		return new ApplicationResponse();
	}
}
