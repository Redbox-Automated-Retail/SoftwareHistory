using System;
using System.Diagnostics;
using Redbox.Core;

namespace Redbox.IPC.Framework;

internal class PerformanceCounterHelper
{
	private string m_groupName;

	private volatile bool m_countersEnabled;

	public static PerformanceCounterHelper Instance => Singleton<PerformanceCounterHelper>.Instance;

	public void Initialize(string groupName)
	{
		m_countersEnabled = true;
		m_groupName = groupName;
	}

	public void IncrementCommandPerSecond(string command, string commandForm)
	{
		if (!m_countersEnabled)
		{
			return;
		}
		try
		{
			using (PerformanceCounter performanceCounter = new PerformanceCounter(m_groupName, FormatName(command, commandForm), readOnly: false))
			{
				performanceCounter.Increment();
			}
			using PerformanceCounter performanceCounter2 = new PerformanceCounter(m_groupName, "Total Commands/second", readOnly: false);
			performanceCounter2.Increment();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception raised in IncrementCounter: ", e);
			m_countersEnabled = false;
		}
	}

	public string FormatName(string command, string commandForm)
	{
		return $"{command} {commandForm}/second.";
	}

	public string FormatDescription(string command, string commandForm)
	{
		return $"{command} {commandForm}/second.";
	}

	private PerformanceCounterHelper()
	{
	}
}
