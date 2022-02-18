using KioskClientService.API.Services.Ads;
using KioskClientService.API.Services.App;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/session")]
[ApiController]
public class SessionController : ControllerBase
{
	private readonly ILogger _logger;

	private readonly ISessionService _sessionService;

	public SessionController(ILogger<AdsController> logger, ISessionService sessionService)
	{
		_logger = logger;
		_sessionService = sessionService;
	}

	[HttpPost("start")]
	public ActionResult<SessionResponse> StartSession(SessionType sessionType)
	{
		_logger?.LogInformation("SessionController.StartSession");
		_sessionService?.Start(sessionType);
		return new OkObjectResult(new SessionResponse());
	}

	[HttpPost("end")]
	public ActionResult<SessionResponse> EndSession()
	{
		_logger?.LogInformation("SessionController.EndSession");
		_sessionService?.End();
		return new OkObjectResult(new SessionResponse());
	}
}
