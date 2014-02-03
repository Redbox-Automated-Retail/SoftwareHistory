using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Redbox.IPC.Framework;

internal class PerformanceCounterSetup : IDisposable
{
	private readonly Dictionary<string, string> m_counters = new Dictionary<string, string>();

	private readonly string m_groupName;

	public PerformanceCounterSetup(string groupName)
	{
		m_groupName = groupName;
		PerformanceCounterHelper.Instance.Initialize(m_groupName);
	}

	public void AddCounter(string command, string commandForm)
	{
		m_counters.Add(PerformanceCounterHelper.Instance.FormatName(command, commandForm), PerformanceCounterHelper.Instance.FormatDescription(command, commandForm));
	}

	public void Dispose()
	{
		RemovePerformanceCounters();
		CounterCreationDataCollection counterCreationDataCollection = new CounterCreationDataCollection();
		foreach (KeyValuePair<string, string> counter in m_counters)
		{
			counterCreationDataCollection.Add(new CounterCreationData(counter.Key, counter.Value, PerformanceCounterType.RateOfCountsPerSecond64));
		}
		counterCreationDataCollection.Add(new CounterCreationData("Total Commands/second", "Total Commands executed per second.", PerformanceCounterType.RateOfCountsPerSecond64));
		PerformanceCounterCategory.Create(m_groupName, "Performance couners for the Redbox Update Service", PerformanceCounterCategoryType.SingleInstance, counterCreationDataCollection);
	}

	private void RemovePerformanceCounters()
	{
		if (PerformanceCounterCategory.Exists(m_groupName))
		{
			PerformanceCounterCategory.Delete(m_groupName);
		}
	}
}
