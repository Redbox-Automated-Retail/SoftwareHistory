using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.Services.WelcomeScreen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WelcomeScreenAssetsController : ControllerBase
{
	private readonly IWelcomeScreenAssetsService _welcomeScreenAssetsService;

	private readonly ILogger<WelcomeScreenAssetsController> _logger;

	public WelcomeScreenAssetsController(IWelcomeScreenAssetsService welcomeScreenAssetsService, ILogger<WelcomeScreenAssetsController> logger)
	{
		_welcomeScreenAssetsService = welcomeScreenAssetsService;
		_logger = logger;
	}

	[HttpGet]
	[ProducesResponseType(typeof(List<GetWelcomeScreenAssetsResponse>), 200)]
	[ProducesResponseType(typeof(List<GetWelcomeScreenAssetsResponse>), 404)]
	public async Task<ActionResult<GetWelcomeScreenAssetsResponse>> GetActiveWelcomeScreenAssets()
	{
		return (await _welcomeScreenAssetsService.GetActiveAssets()).ToObjectResult();
	}

	[Route("syncassets")]
	[HttpPost]
	[ProducesResponseType(typeof(SyncWelcomeScreenAssetsResponse), 200)]
	[ProducesResponseType(typeof(SyncWelcomeScreenAssetsResponse), 500)]
	public async Task<ActionResult<SyncWelcomeScreenAssetsResponse>> SyncAssets()
	{
		return (await _welcomeScreenAssetsService.SyncAssets()).ToObjectResult();
	}

	[Route("refreshifneeded")]
	[HttpPost]
	[ProducesResponseType(typeof(SyncWelcomeScreenAssetsResponse), 200)]
	public async Task<ActionResult<SyncWelcomeScreenAssetsResponse>> RefreshIfNeeded()
	{
		return (await _welcomeScreenAssetsService.RefreshIfNeeded()).ToObjectResult();
	}

	[Route("deletecache")]
	[HttpPost]
	[ProducesResponseType(200)]
	public ActionResult DeleteCache()
	{
		_welcomeScreenAssetsService.DeleteCache();
		return Ok();
	}
}
