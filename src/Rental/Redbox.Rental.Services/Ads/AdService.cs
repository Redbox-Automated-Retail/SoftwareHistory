using System.Collections.Generic;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Model.Marketing;

namespace Redbox.Rental.Services.Ads;

public class AdService : IAdService
{
	private List<IAdService> _adServices = new List<IAdService>();

	private IMarketingAdService _marketingAdService;

	private IVistarAdService _vistarAdService;

	public AdService(IMarketingAdService marketingAdService, IVistarAdService vistarAdService)
	{
		_marketingAdService = marketingAdService;
		_vistarAdService = vistarAdService;
		_adServices.Add(_marketingAdService);
		_adServices.Add(_vistarAdService);
	}

	public IAd GetScreenAd(AdLocation adLocation)
	{
		LogHelper.Instance.Log("AdService.GetScreenAd - started");
		IAd ad = _vistarAdService?.GetScreenAd(adLocation);
		if (ad == null)
		{
			ad = _marketingAdService?.GetScreenAd(adLocation);
		}
		bool flag = ad != null;
		LogHelper.Instance.Log($"AdService.GetScreenAd - finished HasResult: {flag}");
		return ad;
	}

	public async Task<IAd> GetScreenAdAsync(AdLocation adLocation)
	{
		IAd ad = await (_vistarAdService?.GetScreenAdAsync(adLocation)).ConfigureAwait(continueOnCapturedContext: false);
		if (ad == null)
		{
			ad = await (_marketingAdService?.GetScreenAdAsync(adLocation)).ConfigureAwait(continueOnCapturedContext: false);
		}
		return ad;
	}

	public bool RecordAdImpression(IAdImpression adImpression)
	{
		bool result = false;
		if (adImpression?.Ad is IVistarAd && _vistarAdService != null)
		{
			result = _vistarAdService.RecordAdImpression(adImpression);
		}
		else if (adImpression?.Ad is MarketingAd && _marketingAdService != null)
		{
			result = _marketingAdService.RecordAdImpression(adImpression);
		}
		else
		{
			LogHelper.Instance.Log($"Error in AdService.CreateImpression.  Invalid adImpression type: {adImpression?.Ad?.GetType()}");
		}
		return result;
	}

	public async Task<bool> RecordAdImpressionAsync(IAdImpression adImpression)
	{
		bool result = false;
		if (adImpression?.Ad is IVistarAd)
		{
			result = await (_vistarAdService?.RecordAdImpressionAsync(adImpression)).ConfigureAwait(continueOnCapturedContext: false);
		}
		else if (adImpression?.Ad is MarketingAd)
		{
			result = await (_marketingAdService?.RecordAdImpressionAsync(adImpression)).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			LogHelper.Instance.Log($"Error in AdService.CreateImpression.  Invalid adImpression type: {adImpression?.Ad?.GetType()}");
		}
		return result;
	}
}
