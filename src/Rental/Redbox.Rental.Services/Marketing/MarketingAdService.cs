using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Model.Marketing;

namespace Redbox.Rental.Services.Marketing;

public class MarketingAdService : IMarketingAdService, IAdService
{
	private IMarketingConfigurationManager _marketingConfigurationManager;

	private IStoreManager _storeManager;

	private IResourceBundleService _resourceBundleService;

	private MarketingAdCampaignGroup _marketingAdCampaignGroup = new MarketingAdCampaignGroup();

	private bool _enableMarketingAds;

	private List<MarketingAdCampaign> _marketingAdCampaigns = new List<MarketingAdCampaign>();

	private List<MarketingAdCampaign> _activeCampaigns = new List<MarketingAdCampaign>();

	public MarketingAdService(IMarketingConfigurationManager marketingConfigurationManager, IStoreManager storeManager, IResourceBundleService resourceBundleService)
	{
		_marketingConfigurationManager = marketingConfigurationManager;
		_storeManager = storeManager;
		_resourceBundleService = resourceBundleService;
	}

	public void LoadCampaigns()
	{
		LoadMarketingAdCampaignGroup();
		_marketingAdCampaigns.Clear();
		List<MarketingAdCampaign> campaigns = _marketingAdCampaignGroup.GetCampaigns(_storeManager.KioskId, _storeManager.Banner, _storeManager.Market);
		_marketingAdCampaigns.AddRange(campaigns);
		if (_marketingAdCampaigns.Count == 0)
		{
			LogHelper.Instance.Log("MarketingAdService.StartApplication - no campaigns found");
		}
		_marketingAdCampaigns.ForEach(delegate(MarketingAdCampaign item)
		{
			LogHelper.Instance.Log("MarketingAdService.StartApplication - Found Campaign Id: {0} Name: {1} - Start Date: {2} End Date: {3}, StartTime: {4} EndTime: {5} DaysofWeek: {6}", item.Id, item.Name, item.StartDate, item.EndDate, item.StartTime, item.EndTime, item.DaysOfWeek);
		});
	}

	public void ActivateCampaigns()
	{
		_enableMarketingAds = _marketingConfigurationManager?.EnableMarketingAds ?? false;
		_activeCampaigns = GetActiveCampaigns();
		if (_activeCampaigns == null || _activeCampaigns.Count == 0)
		{
			LogHelper.Instance.Log("MarketingAdService.StartSession - no active campaign");
			return;
		}
		_activeCampaigns.ForEach(delegate(MarketingAdCampaign c)
		{
			LogHelper.Instance.Log("MarketingAdService.StartSession - Active Campaign Id: {0} Name: {1}", c?.Id, c?.Name);
		});
	}

	public bool CheckForNewActiveCampaign()
	{
		List<MarketingAdCampaign> activeCampaigns = GetActiveCampaigns();
		if (activeCampaigns.Count == 0 && _activeCampaigns.Count == 0)
		{
			return false;
		}
		if (activeCampaigns.Count != _activeCampaigns.Count)
		{
			return true;
		}
		return activeCampaigns.Union(_activeCampaigns).Count() != _activeCampaigns.Count;
	}

	public List<MarketingAdCampaign> GetActiveCampaigns()
	{
		DateTime dt = DateTime.Now;
		return _marketingAdCampaigns?.Where((MarketingAdCampaign camp) => camp?.IsActive(dt) ?? false).OrderByDescending((MarketingAdCampaign camp) => camp.StartDate).ToList();
	}

	public bool RecordAdImpression(IAdImpression adImpression)
	{
		return false;
	}

	public async Task<bool> RecordAdImpressionAsync(IAdImpression adImpression)
	{
		return await Task.Run(() => false);
	}

	public async Task<IAd> GetScreenAdAsync(AdLocation marketingAdLocation)
	{
		IAd ad = null;
		await Task.Run(delegate
		{
			if (_enableMarketingAds && _activeCampaigns.Count != 0)
			{
				MarketingAdCampaign marketingAdCampaign = _activeCampaigns.Where((MarketingAdCampaign c) => c.Ads.ContainsKey(marketingAdLocation)).FirstOrDefault();
				if (marketingAdCampaign != null)
				{
					ad = marketingAdCampaign.Ads[marketingAdLocation];
				}
			}
		}).ConfigureAwait(continueOnCapturedContext: false);
		return ad;
	}

	public IAd GetScreenAd(AdLocation marketingAdLocation)
	{
		IAd result = null;
		if (_enableMarketingAds && _activeCampaigns.Count != 0)
		{
			MarketingAdCampaign marketingAdCampaign = _activeCampaigns.Where((MarketingAdCampaign c) => c.Ads.ContainsKey(marketingAdLocation)).FirstOrDefault();
			if (marketingAdCampaign != null)
			{
				result = marketingAdCampaign.Ads[marketingAdLocation];
			}
		}
		return result;
	}

	private void LoadMarketingAdCampaignGroup()
	{
		string jsonFile = _resourceBundleService.GetJsonFile("json-marketingads");
		if (!string.IsNullOrEmpty(jsonFile))
		{
			MarketingAdData adData = jsonFile.ToObject<MarketingAdData>();
			_marketingAdCampaignGroup = ToCampaignGroup(adData);
		}
	}

	private MarketingAdCampaignGroup ToCampaignGroup(MarketingAdData adData)
	{
		MarketingAdCampaignGroup group = new MarketingAdCampaignGroup();
		adData.MarketingAdCampaigns.ForEach(delegate(AdCampaign item)
		{
			MarketingAdCampaign campaign = new MarketingAdCampaign
			{
				Id = item.Id,
				Name = item.Name,
				StartDate = item.StartDate,
				EndDate = item.EndDate,
				StartTime = item.StartTime,
				EndTime = item.EndTime,
				DaysOfWeek = item.DaysOfWeek
			};
			group.MarketingAdCampaigns.Add(campaign);
			if (item.Kiosks != null)
			{
				item.Kiosks.ForEach(delegate(long k)
				{
					campaign.Kiosks.Add(k);
				});
			}
			if (item.Banners != null)
			{
				item.Banners.ForEach(delegate(string b)
				{
					campaign.Banners.Add(b);
				});
			}
			if (item.Markets != null)
			{
				item.Markets.ForEach(delegate(string m)
				{
					campaign.Markets.Add(m);
				});
			}
			if (item.ExcludeKiosks != null)
			{
				item.ExcludeKiosks.ForEach(delegate(long k)
				{
					campaign.ExcludeKiosks.Add(k);
				});
			}
			if (item.ExcludeBanners != null)
			{
				item.ExcludeBanners.ForEach(delegate(string b)
				{
					campaign.ExcludeBanners.Add(b);
				});
			}
			if (item.ExcludeMarkets != null)
			{
				item.ExcludeMarkets.ForEach(delegate(string m)
				{
					campaign.ExcludeMarkets.Add(m);
				});
			}
			if (item.Ads != null)
			{
				item.Ads.ForEach(delegate(Ad a)
				{
					campaign.Ads[a.Location] = new MarketingAd
					{
						ImageName = a.ImageName,
						MinimumDisplay = a.MinimumDisplay,
						Campaign = campaign
					};
				});
			}
		});
		return group;
	}
}
