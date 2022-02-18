using System.Threading.Tasks;
using KioskClientService.API.Models;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/kiosk")]
[ApiController]
public class KioskController : ControllerBase
{
	private readonly ILogger<InstallerController> _logger;

	private readonly IProxyApi _proxyApi;

	public KioskController(ILogger<InstallerController> logger, IProxyApi proxyApi)
	{
		_logger = logger;
		_proxyApi = proxyApi;
	}

	[Route("authenticate")]
	[HttpPost]
	[ProducesResponseType(typeof(KioskModels.KioskAuthenticateResponse), 200)]
	public async Task<ActionResult<KioskModels.KioskAuthenticateResponse>> Authenticate(KioskModels.KioskAuthenticateRequest request)
	{
		_logger?.LogInfoWithSource("KioskController.Authenticate called.", "Authenticate");
		return new ObjectResult((await _proxyApi.Authenticate(request))?.Response);
	}
}
