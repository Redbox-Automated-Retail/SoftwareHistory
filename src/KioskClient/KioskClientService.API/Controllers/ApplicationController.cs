using System.Threading.Tasks;
using KioskClientService.API.Ads;
using KioskClientService.API.App;
using KioskClientService.API.Services.Ads;
using KioskClientService.API.Services.App;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/application")]
[ApiController]
public class ApplicationController : ControllerBase
{
	private readonly ILogger<ApplicationController> _logger;

	private readonly IApplicationService _applicationService;

	private readonly IAdService _adService;

	private readonly IAssetService _assetService;

	public ApplicationController(ILogger<ApplicationController> logger, IApplicationService applicationService)
	{
		_logger = logger;
		_applicationService = applicationService;
	}

	[HttpPost("start")]
	public async Task<ActionResult<ApplicationResponse>> StartApplication()
	{
		_logger?.LogInformation("ApplicationController.StartApplication");
		_applicationService.Start();
		return new OkObjectResult(new ApplicationResponse());
	}

	[HttpPost("end")]
	public async Task<ActionResult<ApplicationResponse>> EndApplication()
	{
		_logger?.LogInformation("ApplicationController.EndApplication");
		_applicationService.End();
		return new OkObjectResult(new ApplicationResponse());
	}

	[HttpPost("viewchangedevent")]
	public async Task<ActionResult<ViewShowResponse>> ViewChangedEvent(ViewStateChangedEvent e)
	{
		_logger?.LogInfoWithSource(e.ToJson(), "ViewChangedEvent");
		await _applicationService.ViewChangedEvent(e);
		return new OkObjectResult(new ViewShowResponse());
	}
}
