using System;
using System.Collections.Generic;
using System.Linq;

namespace ProductPricing.Model;

public static class PricingExtensions
{
	private static List<T> GetActiveProductPricing<T>(this List<T> ppList, DateTime kioskDT) where T : IPricing
	{
		return ppList.Where(delegate(T p)
		{
			if (p.StartDate > kioskDT)
			{
				return false;
			}
			if (p.EndDate.HasValue && kioskDT > p.EndDate.Value)
			{
				return false;
			}
			if (p.DaysOfWeek.HasValue)
			{
				DaysOfWeek value = (DaysOfWeek)p.DaysOfWeek.Value;
				if (kioskDT.DayOfWeek == DayOfWeek.Sunday && !value.HasFlag(DaysOfWeek.Sunday))
				{
					return false;
				}
				if (kioskDT.DayOfWeek == DayOfWeek.Monday && !value.HasFlag(DaysOfWeek.Monday))
				{
					return false;
				}
				if (kioskDT.DayOfWeek == DayOfWeek.Tuesday && !value.HasFlag(DaysOfWeek.Tuesday))
				{
					return false;
				}
				if (kioskDT.DayOfWeek == DayOfWeek.Wednesday && !value.HasFlag(DaysOfWeek.Wednesday))
				{
					return false;
				}
				if (kioskDT.DayOfWeek == DayOfWeek.Thursday && !value.HasFlag(DaysOfWeek.Thursday))
				{
					return false;
				}
				if (kioskDT.DayOfWeek == DayOfWeek.Friday && !value.HasFlag(DaysOfWeek.Friday))
				{
					return false;
				}
				if (kioskDT.DayOfWeek == DayOfWeek.Saturday && !value.HasFlag(DaysOfWeek.Saturday))
				{
					return false;
				}
			}
			return (!p.StartTime.HasValue || !p.EndTime.HasValue || TimeBetween(kioskDT, p.StartTime.Value, p.EndTime.Value)) ? true : false;
		}).ToList();
	}

	private static bool TimeBetween(DateTime datetime, TimeSpan start, TimeSpan end)
	{
		TimeSpan timeOfDay = datetime.TimeOfDay;
		if (start < end)
		{
			if (start <= timeOfDay)
			{
				return timeOfDay <= end;
			}
			return false;
		}
		if (end < timeOfDay)
		{
			return !(timeOfDay < start);
		}
		return true;
	}

	public static T GetActiveDefaultRentalPricing<T>(this List<T> ppList, DateTime kioskDT, long titleId, PriceLevelType plt) where T : IRentalPricing
	{
		return (from p in ppList.GetActiveProductPricing(kioskDT)
			where p.PriceLevelType == plt || (p.PriceLevelType == PriceLevelType.Title && Convert.ToInt32(p.PriceLevelTypeValue) == titleId)
			orderby p.StartDate descending, p.LocationRank, p.InitialNight
			select p).FirstOrDefault();
	}

	public static T GetActiveRentalPricing<T>(this List<T> ppList, DateTime kioskDT, long titleId, PriceLevelType plt) where T : IRentalPricing
	{
		return (from p in ppList.GetActiveProductPricing(kioskDT)
			where (p.PriceLevelType == plt || (p.PriceLevelType == PriceLevelType.Title && Convert.ToInt32(p.PriceLevelTypeValue) == titleId)) && p.InitialDays == 1
			orderby p.StartDate descending, p.InitialNight
			select p).FirstOrDefault();
	}

	public static List<T> GetActiveMNPRentalPricing<T>(this List<T> ppList, DateTime kioskDT, long titleId, PriceLevelType plt) where T : IRentalPricing
	{
		return (from p in ppList.GetActiveProductPricing(kioskDT)
			where (p.PriceLevelType == plt || (p.PriceLevelType == PriceLevelType.Title && Convert.ToInt32(p.PriceLevelTypeValue) == titleId)) && p.InitialDays > 1
			orderby p.StartDate descending, p.InitialNight
			select p).ToList();
	}

	public static T GetActivePurchasePricing<T>(this List<T> ppList, DateTime kioskDT, long titleId, bool useLowestPriceSortOrder) where T : IPurchasePricing
	{
		List<T> activeProductPricing = ppList.GetActiveProductPricing(kioskDT);
		if (useLowestPriceSortOrder)
		{
			return (from p in activeProductPricing
				where p.ProductId == titleId
				orderby p.PricingId descending, p.PurchasePrice
				select p).FirstOrDefault();
		}
		return (from p in activeProductPricing
			where p.ProductId == titleId
			orderby p.PricingId descending, p.LocationRank, p.StartDate descending, p.PurchasePrice
			select p).FirstOrDefault();
	}
}
