using System;

namespace Redbox.TicketServices.Proxy.ComponentModel;

internal class Production
{
	public long ProductionId { get; set; }

	public int ProductionTypeId { get; set; }

	public long PrimaryEventId { get; set; }

	public long VenueId { get; set; }

	public DateTime RBOnSaleDateTimeUTC { get; set; }

	public DateTime RBOffSaleDateTimeUTC { get; set; }

	public short ContentRatingTypeId { get; set; }

	public bool? HasGATicketGroups { get; set; }

	public decimal? MinSupplierPrice { get; set; }

	public decimal? MaxSupplierPrice { get; set; }

	public short? TicketLimitPerOrder { get; set; }

	public long? SecondaryEventId { get; set; }

	public string ShortNote { get; set; }

	public DateTime? ProductionStartDateTimeLocal { get; set; }

	public DateTime? ProductionEndDateTimeLocal { get; set; }

	public DateTime? ProductionStartDateTimeUTC { get; set; }

	public DateTime? ProductionEndDateTimeUTC { get; set; }

	public bool IsThisWeekend()
	{
		DateTime today = DateTime.Today;
		DateTime dateTime = today.AddDays((double)((today.DayOfWeek == DayOfWeek.Sunday) ? ((DayOfWeek)(-2)) : (5 - today.DayOfWeek)));
		DateTime value = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59).AddDays(2.0);
		DateTime? productionEndDateTimeLocal = ProductionEndDateTimeLocal;
		DateTime dateTime2 = dateTime;
		if (!productionEndDateTimeLocal.HasValue || !(productionEndDateTimeLocal.GetValueOrDefault() >= dateTime2) || !(ProductionEndDateTimeLocal <= value))
		{
			DateTime? productionStartDateTimeLocal = ProductionStartDateTimeLocal;
			DateTime dateTime3 = dateTime;
			if (!productionStartDateTimeLocal.HasValue || !(productionStartDateTimeLocal.GetValueOrDefault() >= dateTime3) || !(ProductionEndDateTimeLocal <= value))
			{
				return !ProductionEndDateTimeLocal.HasValue;
			}
		}
		return true;
	}

	public bool IsNextWeek()
	{
		DateTime today = DateTime.Today;
		DateTime dateTime = today.AddDays((double)((today.DayOfWeek == DayOfWeek.Sunday) ? DayOfWeek.Monday : (7 - (today.DayOfWeek - 1))));
		DateTime value = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59).AddDays(6.0);
		DateTime? productionEndDateTimeLocal = ProductionEndDateTimeLocal;
		DateTime dateTime2 = dateTime;
		if (!productionEndDateTimeLocal.HasValue || !(productionEndDateTimeLocal.GetValueOrDefault() >= dateTime2) || !(ProductionEndDateTimeLocal <= value))
		{
			DateTime? productionStartDateTimeLocal = ProductionStartDateTimeLocal;
			DateTime dateTime3 = dateTime;
			if (!productionStartDateTimeLocal.HasValue || !(productionStartDateTimeLocal.GetValueOrDefault() >= dateTime3) || !(ProductionEndDateTimeLocal <= value))
			{
				return !ProductionEndDateTimeLocal.HasValue;
			}
		}
		return true;
	}

	public bool IsExpired()
	{
		return RBOffSaleDateTimeUTC <= DateTime.UtcNow;
	}
}
