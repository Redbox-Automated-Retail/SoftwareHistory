using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Rental.Model.Ads;

namespace Redbox.Rental.Model.Marketing;

public class MarketingAdCampaign
{
	private List<string> _banners = new List<string>();

	private List<string> _excludeBanners = new List<string>();

	private List<string> _markets = new List<string>();

	private List<string> _excludeMarkets = new List<string>();

	private List<long> _kiosks = new List<long>();

	private List<long> _excludeKiosks = new List<long>();

	private Dictionary<AdLocation, MarketingAd> _ads = new Dictionary<AdLocation, MarketingAd>();

	public Guid Id { get; set; }

	public string Name { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime EndDate { get; set; }

	public TimeSpan? StartTime { get; set; }

	public TimeSpan? EndTime { get; set; }

	public DaysOfWeek? DaysOfWeek { get; set; }

	public List<string> Banners => _banners;

	public List<string> ExcludeBanners => _excludeBanners;

	public List<long> Kiosks => _kiosks;

	public List<long> ExcludeKiosks => _excludeKiosks;

	public List<string> Markets => _markets;

	public List<string> ExcludeMarkets => _excludeMarkets;

	public Dictionary<AdLocation, MarketingAd> Ads => _ads;

	private bool HasKiosksSet
	{
		get
		{
			if (Kiosks != null)
			{
				return Kiosks.Count > 0;
			}
			return false;
		}
	}

	private bool HasBannersSet
	{
		get
		{
			if (Banners != null)
			{
				return Banners.Count > 0;
			}
			return false;
		}
	}

	private bool HasMarketsSet
	{
		get
		{
			if (Markets != null)
			{
				return Markets.Count > 0;
			}
			return false;
		}
	}

	private bool HasExcludeKiosksSet
	{
		get
		{
			if (ExcludeKiosks != null)
			{
				return ExcludeKiosks.Count > 0;
			}
			return false;
		}
	}

	private bool HasExcludeBannersSet
	{
		get
		{
			if (ExcludeBanners != null)
			{
				return ExcludeBanners.Count > 0;
			}
			return false;
		}
	}

	private bool HasExcludeMarketsSet
	{
		get
		{
			if (ExcludeMarkets != null)
			{
				return ExcludeMarkets.Count > 0;
			}
			return false;
		}
	}

	public bool IsValidIn(long kioskId, string banner, string market)
	{
		bool result = true;
		if (HasKiosksSet)
		{
			result = false;
			if (Kiosks.Contains(kioskId))
			{
				return true;
			}
		}
		if (HasExcludeKiosksSet && ExcludeKiosks.Contains(kioskId))
		{
			return false;
		}
		if (HasBannersSet)
		{
			result = false;
			if (Banners.Contains(banner, StringComparer.CurrentCultureIgnoreCase))
			{
				return true;
			}
		}
		if (HasExcludeBannersSet && ExcludeBanners.Contains(banner, StringComparer.CurrentCultureIgnoreCase))
		{
			return false;
		}
		if (HasMarketsSet)
		{
			result = false;
			if (Markets.Contains(market, StringComparer.CurrentCultureIgnoreCase))
			{
				return true;
			}
		}
		if (HasExcludeMarketsSet && ExcludeMarkets.Contains(market, StringComparer.CurrentCultureIgnoreCase))
		{
			return false;
		}
		return result;
	}

	public bool IsActive(DateTime dt)
	{
		if (dt < StartDate || dt > EndDate)
		{
			return false;
		}
		if (DaysOfWeek.HasValue && DaysOfWeek != Redbox.Rental.Model.Marketing.DaysOfWeek.NotSet && !IsDayOfWeek(DaysOfWeek.Value, dt.DayOfWeek))
		{
			return false;
		}
		if (StartTime.HasValue && EndTime.HasValue)
		{
			TimeSpan timeOfDay;
			TimeSpan? startTime;
			if (StartTime <= EndTime)
			{
				timeOfDay = dt.TimeOfDay;
				startTime = StartTime;
				if (!(timeOfDay < startTime))
				{
					timeOfDay = dt.TimeOfDay;
					startTime = EndTime;
					if (!(timeOfDay > startTime))
					{
						goto IL_017b;
					}
				}
				return false;
			}
			timeOfDay = dt.TimeOfDay;
			startTime = StartTime;
			if (timeOfDay < startTime)
			{
				timeOfDay = dt.TimeOfDay;
				startTime = EndTime;
				if (timeOfDay > startTime)
				{
					return false;
				}
			}
		}
		goto IL_017b;
		IL_017b:
		return true;
	}

	private bool IsDayOfWeek(DaysOfWeek daysOfWeek, DayOfWeek dayOfWeek)
	{
		return dayOfWeek switch
		{
			DayOfWeek.Sunday => (daysOfWeek & Redbox.Rental.Model.Marketing.DaysOfWeek.Sunday) > Redbox.Rental.Model.Marketing.DaysOfWeek.NotSet, 
			DayOfWeek.Monday => (daysOfWeek & Redbox.Rental.Model.Marketing.DaysOfWeek.Monday) > Redbox.Rental.Model.Marketing.DaysOfWeek.NotSet, 
			DayOfWeek.Tuesday => (daysOfWeek & Redbox.Rental.Model.Marketing.DaysOfWeek.Tuesday) > Redbox.Rental.Model.Marketing.DaysOfWeek.NotSet, 
			DayOfWeek.Wednesday => (daysOfWeek & Redbox.Rental.Model.Marketing.DaysOfWeek.Wednesday) > Redbox.Rental.Model.Marketing.DaysOfWeek.NotSet, 
			DayOfWeek.Thursday => (daysOfWeek & Redbox.Rental.Model.Marketing.DaysOfWeek.Thursday) > Redbox.Rental.Model.Marketing.DaysOfWeek.NotSet, 
			DayOfWeek.Friday => (daysOfWeek & Redbox.Rental.Model.Marketing.DaysOfWeek.Friday) > Redbox.Rental.Model.Marketing.DaysOfWeek.NotSet, 
			DayOfWeek.Saturday => (daysOfWeek & Redbox.Rental.Model.Marketing.DaysOfWeek.Saturday) > Redbox.Rental.Model.Marketing.DaysOfWeek.NotSet, 
			_ => false, 
		};
	}
}
