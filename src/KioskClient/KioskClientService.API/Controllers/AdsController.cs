using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.Ads;
using KioskClientService.API.Services.Ads;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Controllers;

[Route("api/ads")]
[ApiController]
public class AdsController : ControllerBase
{
	private readonly ILogger _logger;

	private readonly IAdService _adService;

	private readonly IAssetService _assetService;

	public AdsController(ILogger<AdsController> logger, IAdService adService, IAssetService assetService)
	{
		_logger = logger;
		_adService = adService;
		_assetService = assetService;
	}

	[HttpGet]
	public async Task<ActionResult<GetAdResponse>> GetAd(AdLocation location)
	{
		_logger?.LogInformation("AdController.GetAd");
		return new OkObjectResult(await _adService.GetAd(new GetAdRequest
		{
			AdLocation = location
		}));
	}

	[HttpPost]
	[Route("preload")]
	public async Task<ActionResult<List<GetAdsResponse>>> PostPreload([FromBody] List<AdLocation> locations)
	{
		_logger?.LogInformation("AdController.Preload - " + string.Join(',', locations));
		return new OkObjectResult(await _adService.GetAds(locations, isPreload: true));
	}

	[HttpPost]
	[Route("cache")]
	public async Task<ActionResult<CacheResponse>> PostCache([FromBody] CacheRequest cacheRequest)
	{
		_logger?.LogInformation("AdController.PostCache");
		return new OkObjectResult(await _assetService.Cache());
	}

	[HttpPost]
	[Route("{advertisementId}/proofofplay")]
	public async Task<ActionResult<ProofOfPlayResponse>> PostProofOfPlay(string advertisementId, [FromBody] ProofOfPlayRequest proofOfPlayRequest)
	{
		_logger?.LogInformation("AdController.PostProofOfPlay");
		return new OkObjectResult(await _adService.ProofOfPlay(advertisementId, proofOfPlayRequest));
	}

	[HttpPost]
	[Route("{advertisementId}/expire")]
	public async Task<ActionResult<ExpireResponse>> PostExpire(string advertisementId)
	{
		_logger?.LogInformation("AdController.PostExpire");
		return new OkObjectResult(await _adService.Expire(advertisementId));
	}
}
