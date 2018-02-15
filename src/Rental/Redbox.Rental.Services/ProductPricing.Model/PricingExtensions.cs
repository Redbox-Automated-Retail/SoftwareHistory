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

	public static List<T> GetActiveDefaultRentalPricing<T>(this List<T> ppList, DateTime kioskDT) where T : IRentalPricing
	{
		return (from g in (from p in ppList.GetActiveProductPricing(kioskDT)
				group p by p.PriceLevelType into g
				select (g)).ToList()
			select (from p in g
				orderby p.StartDate descending, p.LocationRank, p.InitialNight descending
				select p).First()).ToList();
	}

	public static T GetActiveDefaultRentalPricing<T>(this List<T> ppList, DateTime kioskDT, long titleId, PriceLevelType plt) where T : IRentalPricing
	{
		return (from p in ppList.GetActiveProductPricing(kioskDT)
			where p.PriceLevelType == plt || (p.PriceLevelType == PriceLevelType.Title && Convert.ToInt32(p.PriceLevelTypeValue) == titleId)
			orderby p.StartDate descending, p.LocationRank, p.InitialNight
			select p).FirstOrDefault();
	}

	public static T GetActiveDefaultRentalPricing<T>(this List<T> ppList, DateTime kioskDT, PriceLevelType plt) where T : IRentalPricing
	{
		if (plt != PriceLevelType.Dvd && plt != PriceLevelType.Bluray && plt != PriceLevelType.Games)
		{
			return default(T);
		}
		return (from p in ppList.GetActiveProductPricing(kioskDT)
			where p.PriceLevelType == plt
			orderby p.StartDate descending, p.LocationRank, p.InitialNight
			select p).FirstOrDefault();
	}

	public static List<T> GetActiveRentalPricing<T>(this List<T> ppList, DateTime kioskDT) where T : IRentalPricing
	{
		return (from g in (from p in ppList.GetActiveProductPricing(kioskDT)
				where p.InitialDays == 1
				group p by p.PriceLevelType into g
				select (g)).ToList()
			select (from p in g
				orderby p.LocationRank, p.ProductPriceId descending
				select p).First()).ToList();
	}

	public static List<T> GetActiveMNPRentalPricing<T>(this List<T> ppList, DateTime kioskDT) where T : IRentalPricing
	{
		return (from g in (from p in ppList.GetActiveProductPricing(kioskDT)
				where p.InitialDays > 1
				group p by p.PriceLevelType into g
				select (g)).ToList()
			select (from p in g
				orderby p.LocationRank, p.ProductPriceId descending
				select p).First()).ToList();
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

	public static List<T> GetActivePurchasePricing<T>(this List<T> ppList, DateTime kioskDT) where T : IPurchasePricing
	{
		return (from g in (from p in ppList.GetActiveProductPricing(kioskDT)
				group p by p.ProductId into g
				select (g)).ToList()
			select (from o in (from i in g
					group i by i.PricingId into source
					select source.ToList()).Last()
				orderby o.PurchasePrice
				select o).First()).ToList();
	}

	public static T GetActivePurchasePricing<T>(this List<T> ppList, DateTime kioskDT, long titleId) where T : IPurchasePricing
	{
		return (from p in ppList.GetActiveProductPricing(kioskDT)
			where p.ProductId == titleId
			orderby p.PricingId descending, p.PurchasePrice
			select p).FirstOrDefault();
	}
}
