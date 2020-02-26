using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Models;
using KioskClientService.API.Services;
using KioskClientService.API.Services.Ads;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Vistar;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Ads;

public class AdService : IAdService
{
	private ILogger<AdService> _logger;

	private IApplicationConfiguration _applicationConfiguration;

	private IVistarAPIClient _vistarAPI;

	private IStoreService _storeService;

	private IAssetService _assetService;

	private readonly IAnalyticsService _analytics;

	private bool _adsActive;

	private List<AdLocation> _preloadedAds;

	private List<DisplayArea> _displayAreas;

	private static Timer _timer;

	private static AdService _adService;

	private static bool _inSession = false;

	private static ConcurrentDictionary<string, CachedAdvertisement> Cached { get; } = new ConcurrentDictionary<string, CachedAdvertisement>();

	public AdService(IApplicationConfiguration applicationConfiguration, ILogger<AdService> logger, IVistarAPIClient vistarAPI, IStoreService storeService, IAssetService assetService, IAnalyticsService analytics)
	{
		_applicationConfiguration = applicationConfiguration;
		_logger = logger;
		_vistarAPI = vistarAPI;
		_storeService = storeService;
		_assetService = assetService;
		_analytics = analytics;
		_adsActive = _applicationConfiguration.AdsActive;
		_preloadedAds = _applicationConfiguration.AdsToPreload;
		_logger?.LogInformation($"AdService - AdsActive - {_adsActive}");
	}

	public async Task<ApplicationResponse> StartApplication()
	{
		_logger?.LogInformation("AdService.StartApplication");
		_assetService.Cache();
		if (_timer == null)
		{
			_logger?.LogInformation("AdService.EndApplication - creating and starting timer");
			int dueTime = 1800000;
			int period = 60000;
			_adService = this;
			_timer = new Timer(delegate
			{
				ExpireTimer();
			}, null, dueTime, period);
		}
		return new ApplicationResponse();
	}

	public async Task<ApplicationResponse> EndApplication()
	{
		_logger?.LogInformation("AdService.EndApplication");
		await ExpireAll();
		if (_timer != null)
		{
			_logger?.LogInformation("AdService.EndApplication - stopping timer");
			_timer.Change(-1, -1);
			_timer = null;
		}
		return new ApplicationResponse();
	}

	public async Task<SessionResponse> StartSession()
	{
		_logger?.LogInformation("AdService.StartSession");
		_adService = this;
		_inSession = true;
		PreLoadAds();
		return new SessionResponse();
	}

	public async Task<SessionResponse> EndSession()
	{
		_inSession = false;
		_logger?.LogInformation("AdService.EndSession");
		return new SessionResponse();
	}

	public async Task<ProofOfPlayResponse> ProofOfPlay(string advertisementId, ProofOfPlayRequest proofOfPlayRequest)
	{
		_logger?.LogInformation("AdService.ProofOfPlay");
		ProofOfPlayResponse result = new ProofOfPlayResponse();
		CachedAdvertisement ad = GetAdvertisement(advertisementId, remove: true);
		if (ad == null)
		{
			result.Success = false;
			result.StatusCode = 404;
			_analytics.VistarProofOfPlay(proofOfPlayRequest, result);
			return result;
		}
		(await _vistarAPI.GetProofOfPlay(new VistarProofOfPlayRequest
		{
			ProofOfPlayUrl = ad.Advertisement.ProofOfPlayUrl
		})).CopyTo(result);
		_analytics.VistarProofOfPlay(proofOfPlayRequest, result, ad);
		return result;
	}

	public async Task<ExpireResponse> Expire(string advertisementId)
	{
		_logger?.LogInformation("AdService.Expire");
		ExpireResponse result = new ExpireResponse();
		CachedAdvertisement ad = GetAdvertisement(advertisementId, remove: true);
		if (ad == null)
		{
			result.Success = false;
			result.StatusCode = 404;
			_analytics.VistarExpire(new VistarExpirationRequest(), result, null, AdExpirationSource.Controller);
			return result;
		}
		VistarExpirationRequest request = new VistarExpirationRequest
		{
			ExpirationUrl = ad.Advertisement.ExpirationUrl
		};
		(await _vistarAPI.GetExpiration(request)).CopyTo(result);
		_analytics.VistarExpire(request, result, ad, AdExpirationSource.Controller);
		return result;
	}

	public async Task<GetAdResponse> GetAd(GetAdRequest request)
	{
		try
		{
			_logger?.LogInformation("AdService.GetAd");
			GetAdResponse result = new GetAdResponse
			{
				AdLocation = request.AdLocation
			};
			if (!_adsActive)
			{
				result.Success = false;
				return result;
			}
			CachedAdvertisement advertisement = GetAdvertisement(request.AdLocation);
			if (advertisement != null)
			{
				advertisement.Requested = true;
				result.CopyFrom(advertisement);
				return result;
			}
			if (!(await GetAdAndCache(request.AdLocation)))
			{
				result.Success = false;
				return result;
			}
			advertisement = GetAdvertisement(request.AdLocation);
			if (advertisement != null)
			{
				advertisement.Requested = true;
				result.CopyFrom(advertisement);
				return result;
			}
			return result;
		}
		finally
		{
			PreLoadAds();
		}
	}

	public async Task<GetAdsResponse> GetAds(List<AdLocation> locations, bool isPreload = false)
	{
		try
		{
			_logger?.LogInfoWithSource(locations.ToJson(), "GetAds");
			GetAdsResponse result = new GetAdsResponse();
			if (!_adsActive)
			{
				result.Success = false;
				return result;
			}
			Dictionary<AdLocation, CachedAdvertisement> advertisements = GetAdvertisements(locations, isPreload);
			List<AdLocation> locationsToCache = new List<AdLocation>();
			foreach (AdLocation location in locations)
			{
				if (advertisements.TryGetValue(location, out var value))
				{
					value.Requested = !isPreload;
					result.GetAdResponses.Add(new GetAdResponse().CopyFrom(value));
				}
				else
				{
					locationsToCache.Add(location);
				}
			}
			if (locationsToCache.Any())
			{
				await GetAdsAndCache(locationsToCache);
				advertisements = GetAdvertisements(locationsToCache, isPreload);
				foreach (AdLocation item in locationsToCache)
				{
					if (advertisements.TryGetValue(item, out var value2))
					{
						value2.Requested = !isPreload;
						result.GetAdResponses.Add(new GetAdResponse().CopyFrom(value2));
					}
				}
			}
			result.Success = result.GetAdResponses.Any();
			return result;
		}
		finally
		{
			if (!isPreload)
			{
				PreLoadAds();
			}
		}
	}

	public async Task<GetAdsResponse> GetAds(string view, bool isPreload = false)
	{
		List<AdLocation> adLocations = GetAdLocations(view);
		if (!adLocations.Any())
		{
			return new GetAdsResponse
			{
				StatusCode = 204
			};
		}
		return await GetAds(adLocations, isPreload);
	}

	private async Task<bool> GetAdAndCache(AdLocation adLocation)
	{
		APIResponse<VistarGetAdResponse> aPIResponse = await _vistarAPI.GetAd(_storeService.KioskId, new List<VistarDisplayArea> { DisplayArea.GetDisplayArea(adLocation).ToVistarDisplayArea() });
		_analytics.VistarGetAd(adLocation, aPIResponse);
		if (!aPIResponse.IsSuccessStatusCode)
		{
			return false;
		}
		bool result = false;
		List<Advertisement> list = aPIResponse.Response.advertisement.ToAdvertisements();
		list.ForEach(delegate(Advertisement a)
		{
			CachedAdvertisement cachedAdvertisement = new CachedAdvertisement
			{
				AdLocation = adLocation,
				Advertisement = a,
				RequestedTime = DateTime.Now
			};
			Asset asset = _assetService.GetAsset(cachedAdvertisement.Advertisement.AssetId);
			if (asset == null)
			{
				_logger?.LogError("DownloadAd succeeded but asset was not cached.  Expiring get ad request now.");
				Expire(cachedAdvertisement);
				_assetService.DownloadAsset(cachedAdvertisement.Advertisement.ToAsset());
			}
			else
			{
				cachedAdvertisement.AssetPath = _assetService.GetAssetCreativeFilePath(asset);
				AddAdvertisement(cachedAdvertisement);
				result = true;
			}
		});
		return result;
	}

	private async Task<bool> GetAdsAndCache(List<AdLocation> adLocations)
	{
		_logger?.LogInfoWithSource(adLocations.ToJson(), "GetAdsAndCache");
		List<VistarDisplayArea> displayArea = adLocations.Select((AdLocation adLocation) => DisplayArea.GetDisplayArea(adLocation).ToVistarDisplayArea()).ToList();
		APIResponse<VistarGetAdResponse> aPIResponse = await _vistarAPI.GetAd(_storeService.KioskId, displayArea);
		_analytics.VistarGetAd((adLocations.Count == 1) ? adLocations[0] : AdLocation.Multiple, aPIResponse);
		if (!aPIResponse.IsSuccessStatusCode)
		{
			return false;
		}
		List<Advertisement> list = aPIResponse.Response.advertisement.ToAdvertisements();
		list.ForEach(delegate(Advertisement a)
		{
			CachedAdvertisement cachedAdvertisement = new CachedAdvertisement
			{
				AdLocation = Enum.Parse<AdLocation>(a.DisplayAreaId),
				Advertisement = a,
				RequestedTime = DateTime.Now
			};
			Asset asset = _assetService.GetAsset(cachedAdvertisement.Advertisement.AssetId);
			if (asset == null)
			{
				_logger?.LogError("DownloadAd succeeded but asset was not cached.  Expiring get ad request now.");
				Expire(cachedAdvertisement);
				_assetService.DownloadAsset(cachedAdvertisement.Advertisement.ToAsset());
			}
			else
			{
				cachedAdvertisement.AssetPath = _assetService.GetAssetCreativeFilePath(asset);
				AddAdvertisement(cachedAdvertisement);
			}
		});
		return true;
	}

	private async Task<bool> ExpireAdAndRemoveFromCache(string advertisementId)
	{
		_logger?.LogInformation("AdService.Expire");
		CachedAdvertisement advertisement = GetAdvertisement(advertisementId, remove: true);
		if (advertisement == null)
		{
			return false;
		}
		return await Expire(advertisement);
	}

	private async Task<bool> Expire(CachedAdvertisement ad)
	{
		ExpireResponse result = new ExpireResponse();
		VistarExpirationRequest request = new VistarExpirationRequest
		{
			ExpirationUrl = ad.Advertisement.ExpirationUrl
		};
		APIResponse<VistarExpirationResponse> aPIResponse = await _vistarAPI.GetExpiration(request);
		aPIResponse.CopyTo(result);
		_analytics.VistarExpire(request, result, ad, AdExpirationSource.ClientService);
		return aPIResponse.IsSuccessStatusCode;
	}

	private async Task<bool> ExpireAll()
	{
		_logger?.LogInformation("AdService.ExpireAll");
		Cached.Values.ToList().ForEach(async delegate(CachedAdvertisement x)
		{
			await ExpireAdAndRemoveFromCache(x.Advertisement.Id);
		});
		Cached.Clear();
		return true;
	}

	private CachedAdvertisement GetAdvertisement(AdLocation adLocation, bool isPreload = false)
	{
		List<CachedAdvertisement> list = Cached.Values.Where((CachedAdvertisement x) => x.AdLocation == adLocation).ToList();
		CachedAdvertisement cachedAdvertisement = (from x in list
			where !x.Requested || (DateTime.Now - x.RequestedTime).TotalSeconds >= 120.0
			orderby x.RequestedTime
			select x).FirstOrDefault();
		if (cachedAdvertisement == null && !isPreload)
		{
			list.ForEach(delegate(CachedAdvertisement x)
			{
				x.Requested = false;
			});
			return null;
		}
		return cachedAdvertisement;
	}

	private CachedAdvertisement GetAdvertisement(string advertisementId, bool remove)
	{
		CachedAdvertisement value;
		if (remove)
		{
			Cached.TryRemove(advertisementId, out value);
		}
		else
		{
			Cached.TryGetValue(advertisementId, out value);
		}
		return value;
	}

	private Dictionary<AdLocation, CachedAdvertisement> GetAdvertisements(List<AdLocation> adLocations, bool isPreload = false)
	{
		Dictionary<AdLocation, CachedAdvertisement> result = new Dictionary<AdLocation, CachedAdvertisement>();
		adLocations.ForEach(delegate(AdLocation location)
		{
			List<CachedAdvertisement> list = Cached.Values.Where((CachedAdvertisement ca) => ca.AdLocation == location).ToList();
			CachedAdvertisement cachedAdvertisement = (from ca in list
				where !ca.Requested || (DateTime.Now - ca.RequestedTime).TotalSeconds >= 120.0
				orderby ca.RequestedTime
				select ca).FirstOrDefault();
			if (cachedAdvertisement == null && !isPreload)
			{
				list.ForEach(delegate(CachedAdvertisement ca)
				{
					ca.Requested = false;
				});
			}
			else if (cachedAdvertisement != null)
			{
				cachedAdvertisement.Requested = true;
				result[location] = cachedAdvertisement;
			}
		});
		return result;
	}

	private void AddAdvertisement(CachedAdvertisement ca)
	{
		Cached[ca.Advertisement.Id] = ca;
	}

	private void PreLoadAds()
	{
		if (!_adsActive || !_inSession)
		{
			return;
		}
		DateTime now = DateTime.Now;
		if (now.Hour < 8 || now.Hour >= 23)
		{
			return;
		}
		Task.Run(delegate
		{
			Dictionary<AdLocation, CachedAdvertisement> advertisements = GetAdvertisements(_preloadedAds, isPreload: true);
			List<AdLocation> list = _preloadedAds.Except(advertisements.Keys).ToList();
			if (list.Any())
			{
				GetAdsAndCache(list);
			}
		});
	}

	private async void ExpireCachedAds()
	{
		DateTime age = DateTime.Now.AddMinutes(-30.0);
		List<CachedAdvertisement> list = (from x in Cached.Values
			where x.RequestedTime < age
			orderby x.RequestedTime
			select x).ToList();
		if (list.Count != 0)
		{
			_logger?.LogInformation($"AdService.ExpireCachedAds - Found {list.Count} ads to expire");
			list.ForEach(async delegate(CachedAdvertisement ad)
			{
				await ExpireAdAndRemoveFromCache(ad.Advertisement.Id);
				_logger?.LogInformation($"AdService.ExpireCachedAd - expiring {ad.Advertisement.Id}, requested {ad.Requested}, cached {ad.RequestedTime}");
			});
		}
	}

	private static void ExpireTimer()
	{
		if (_adService != null)
		{
			_adService.ExpireCachedAds();
		}
	}

	private List<AdLocation> GetAdLocations(string view)
	{
		_applicationConfiguration.AdsToPreloadByView.TryGetValue(view, out var value);
		return value ?? new List<AdLocation>();
	}
}
