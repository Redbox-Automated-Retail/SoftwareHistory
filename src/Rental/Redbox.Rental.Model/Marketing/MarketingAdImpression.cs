using System;

namespace Redbox.Rental.Model.Marketing;

public class MarketingAdImpression
{
	public Guid CampaignId { get; set; }

	public string Name { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime EndDate { get; set; }

	public TimeSpan? StartTime { get; set; }

	public TimeSpan? EndTime { get; set; }

	public int? DaysOfWeek { get; set; }

	public DateTime ImpressionStartDate { get; set; }

	public DateTime ImpressionEndDate { get; set; }

	public int Count { get; set; }

	public MarketingAdLocation AdLocation { get; set; }
}
