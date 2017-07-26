using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Marketing;

public class AdCampaign
{
	public Guid Id { get; set; }

	public string Name { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime EndDate { get; set; }

	public TimeSpan? StartTime { get; set; }

	public TimeSpan? EndTime { get; set; }

	public DaysOfWeek? DaysOfWeek { get; set; }

	public List<string> Banners { get; set; }

	public List<string> ExcludeBanners { get; set; }

	public List<long> Kiosks { get; set; }

	public List<long> ExcludeKiosks { get; set; }

	public List<string> Markets { get; set; }

	public List<string> ExcludeMarkets { get; set; }

	public List<Ad> Ads { get; set; }
}
