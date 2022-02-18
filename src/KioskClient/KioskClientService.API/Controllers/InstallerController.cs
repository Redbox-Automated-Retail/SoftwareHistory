using System.Threading.Tasks;
using KioskClientService.API.Models;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/installer")]
[ApiController]
public class InstallerController : ControllerBase
{
	private readonly ILogger<InstallerController> _logger;

	private readonly IProxyApi _proxyApi;

	public InstallerController(ILogger<InstallerController> logger, IProxyApi proxyApi)
	{
		_logger = logger;
		_proxyApi = proxyApi;
	}

	[Route("getopsmarket")]
	[HttpGet]
	[ProducesResponseType(typeof(InstallerModels.OpsMarketResponse), 200)]
	public async Task<ActionResult<InstallerModels.OpsMarketResponse>> GetOpsMarket(int kioskid)
	{
		_logger?.LogInfoWithSource("InstallerController.GetOpsMarket called.", "GetOpsMarket");
		return new ObjectResult((await _proxyApi.GetOpsMarket(kioskid))?.Response);
	}

	[Route("getpendingbanners")]
	[HttpGet]
	[ProducesResponseType(typeof(InstallerModels.PendingBannersResponse), 200)]
	public async Task<ActionResult<InstallerModels.PendingBannersResponse>> GetPendingBanners(int stateid)
	{
		_logger?.LogInfoWithSource("InstallerController.GetPendingBanners called.", "GetPendingBanners");
		return new ObjectResult((await _proxyApi.GetPendingBanners(stateid))?.Response);
	}

	[Route("getpendingkiosks")]
	[HttpGet]
	[ProducesResponseType(typeof(InstallerModels.PendingKiosksResponse), 200)]
	public async Task<ActionResult<InstallerModels.PendingKiosksResponse>> GetPendingKiosks(short stateid, int bannerId)
	{
		_logger?.LogInfoWithSource("InstallerController.GetPendingKiosks called.", "GetPendingKiosks");
		return new ObjectResult((await _proxyApi.GetPendingKiosks(stateid, bannerId))?.Response);
	}

	[Route("getpendingstates")]
	[HttpGet]
	[ProducesResponseType(typeof(InstallerModels.PendingStatesResponse), 200)]
	public async Task<ActionResult<InstallerModels.PendingStatesResponse>> GetPendingStates()
	{
		_logger?.LogInfoWithSource("InstallerController.GetPendingStates called.", "GetPendingStates");
		return new ObjectResult((await _proxyApi.GetPendingStates())?.Response);
	}
}
