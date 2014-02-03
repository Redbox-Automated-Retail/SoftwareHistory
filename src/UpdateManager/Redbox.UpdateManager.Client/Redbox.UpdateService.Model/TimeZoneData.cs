using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.UpdateService.Model;

internal class TimeZoneData : IEquatable<TimeZoneData>
{
	private readonly TimeSpan m_daylightSavingsTimeOffset;

	private readonly TimeSpan m_greenwichMeanTimeOffset;

	private readonly string m_displayText;

	private static readonly Dictionary<TimeZoneData, TimeZoneInfo> HardCoded = new Dictionary<TimeZoneData, TimeZoneInfo>
	{
		{
			new TimeZoneData(-4m, -5m, "Eastern"),
			TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")
		},
		{
			new TimeZoneData(-5m, -6m, "Central"),
			TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
		},
		{
			new TimeZoneData(-6m, -7m, "Mountain"),
			TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time")
		},
		{
			new TimeZoneData(-7m, -8m, "Pacific"),
			TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time")
		},
		{
			new TimeZoneData(-7m, -7m, "Arizona"),
			TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time")
		},
		{
			new TimeZoneData(-4m, -4m, "Atlantic"),
			TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time")
		},
		{
			new TimeZoneData(-10m, -10m, "Hawaii"),
			TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time")
		},
		{
			new TimeZoneData(-8m, -9m, "Alaska"),
			TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time")
		}
	};

	public TimeSpan DaylightSavingsTimeOffset => m_daylightSavingsTimeOffset;

	public TimeSpan GreenwichMeanTimeOffset => m_greenwichMeanTimeOffset;

	public string DisplayText => m_displayText;

	public TimeSpan DaylightDelta => m_daylightSavingsTimeOffset - m_greenwichMeanTimeOffset;

	private static TimeZoneInfo.TransitionTime UsDstStart
	{
		get
		{
			DateTime timeOfDay = new DateTime(1, 1, 1, 2, 0, 0);
			return TimeZoneInfo.TransitionTime.CreateFloatingDateRule(timeOfDay, 3, 2, DayOfWeek.Sunday);
		}
	}

	private static TimeZoneInfo.TransitionTime UsDstEnd
	{
		get
		{
			DateTime timeOfDay = new DateTime(1, 1, 1, 2, 0, 0);
			return TimeZoneInfo.TransitionTime.CreateFloatingDateRule(timeOfDay, 11, 1, DayOfWeek.Sunday);
		}
	}

	public TimeZoneData(decimal daylightSavingsTimeOffset, decimal greenwichMeanTimeOffset, string displayText)
	{
		m_daylightSavingsTimeOffset = TimeSpan.FromHours((double)daylightSavingsTimeOffset);
		m_greenwichMeanTimeOffset = TimeSpan.FromHours((double)greenwichMeanTimeOffset);
		m_displayText = displayText;
	}

	public TimeZoneData()
	{
	}

	public override string ToString()
	{
		return m_displayText;
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if (object.ReferenceEquals(this, obj))
		{
			return true;
		}
		if (obj.GetType() == typeof(TimeZoneData))
		{
			return Equals((TimeZoneData)obj);
		}
		return false;
	}

	public bool Equals(TimeZoneData other)
	{
		if (object.ReferenceEquals(null, other))
		{
			return false;
		}
		if (object.ReferenceEquals(this, other))
		{
			return true;
		}
		if (TimeSpan.Equals(other.m_greenwichMeanTimeOffset, m_greenwichMeanTimeOffset) && TimeSpan.Equals(other.m_daylightSavingsTimeOffset, m_daylightSavingsTimeOffset))
		{
			return string.Equals(other.m_displayText, m_displayText, StringComparison.CurrentCultureIgnoreCase);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int hashCode = m_daylightSavingsTimeOffset.GetHashCode();
		hashCode = (hashCode * 397) ^ m_greenwichMeanTimeOffset.GetHashCode();
		return (hashCode * 397) ^ m_displayText.GetHashCode();
	}

	public bool TryFindTimeZoneInfo(out TimeZoneInfo timeZoneInfo)
	{
		TimeZoneInfo value = HardCoded.FirstOrDefault((KeyValuePair<TimeZoneData, TimeZoneInfo> zoneInfo) => zoneInfo.Key.Equals(this)).Value;
		if (value != null)
		{
			timeZoneInfo = value;
			return true;
		}
		List<TimeZoneInfo> source = (from tzi in TimeZoneInfo.GetSystemTimeZones()
			where AreTheseTimeZonesTheSame(this, tzi)
			select tzi).ToList();
		timeZoneInfo = source.Where((TimeZoneInfo tzi) => tzi.DisplayName.Contains(m_displayText ?? string.Empty)).DefaultIfEmpty(source.FirstOrDefault()).First();
		return source.Any();
	}

	public TimeZoneInfo ToTimeZoneInfo()
	{
		string id = "RB " + m_displayText;
		TimeSpan greenwichMeanTimeOffset = m_greenwichMeanTimeOffset;
		string displayText = m_displayText;
		string standardDisplayName = m_displayText + " Standard Time";
		string daylightDisplayName = m_displayText + " Daylight Time";
		DateTime date = DateTime.MinValue.Date;
		DateTime date2 = DateTime.MaxValue.Date;
		TimeSpan daylightDelta = DaylightDelta;
		TimeZoneInfo.TransitionTime usDstStart = UsDstStart;
		TimeZoneInfo.TransitionTime usDstEnd = UsDstEnd;
		TimeZoneInfo.AdjustmentRule adjustmentRule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(date, date2, daylightDelta, usDstStart, usDstEnd);
		TimeZoneInfo.AdjustmentRule[] adjustmentRules = new TimeZoneInfo.AdjustmentRule[1] { adjustmentRule };
		bool disableDaylightSavingTime = m_daylightSavingsTimeOffset == m_greenwichMeanTimeOffset;
		return TimeZoneInfo.CreateCustomTimeZone(id, greenwichMeanTimeOffset, displayText, standardDisplayName, daylightDisplayName, adjustmentRules, disableDaylightSavingTime);
	}

	public static bool AreTheseTimeZonesTheSame(TimeZoneData timeZoneData, TimeZoneInfo timeZoneInfo)
	{
		TimeSpan baseUtcOffset = timeZoneInfo.BaseUtcOffset;
		TimeZoneInfo.AdjustmentRule[] adjustmentRules = timeZoneInfo.GetAdjustmentRules();
		TimeZoneInfo.AdjustmentRule adjustmentRule = adjustmentRules.FirstOrDefault((TimeZoneInfo.AdjustmentRule ar) => ar.DateStart < DateTime.UtcNow && ar.DateEnd > DateTime.UtcNow);
		TimeSpan t = ((adjustmentRule != null) ? (baseUtcOffset + adjustmentRule.DaylightDelta) : baseUtcOffset);
		if (TimeSpan.Equals(timeZoneData.m_greenwichMeanTimeOffset, baseUtcOffset))
		{
			return TimeSpan.Equals(timeZoneData.m_daylightSavingsTimeOffset, t);
		}
		return false;
	}

	public bool AreTheseTimeZonesTheSame(TimeZoneInfo timeZoneInfo)
	{
		return AreTheseTimeZonesTheSame(this, timeZoneInfo);
	}
}
