using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class PersistentCounterService : IPersistentCounterService
{
	private readonly IDataTable<IPersistentCounter> CounterTable;

	private readonly IDataTable<IHardwareCorrectionStatistic> StatsTable;

	private readonly List<string> WeeklyResettableCounters = new List<string>();

	private readonly Dictionary<string, IPersistentCounter> Counters = new Dictionary<string, IPersistentCounter>();

	public bool Exists(string name)
	{
		return Counters.ContainsKey(name);
	}

	public IPersistentCounter Find(string name)
	{
		if (Counters.ContainsKey(name))
		{
			return Counters[name];
		}
		IPersistentCounter persistentCounter = ServiceLocator.Instance.GetService<ITableTypeFactory>().NewCounter(name, 0);
		if (CounterTable.Insert(persistentCounter))
		{
			Counters[name] = persistentCounter;
			return persistentCounter;
		}
		LogHelper.Instance.Log("Unable to create counter {0}", name);
		return null;
	}

	public IPersistentCounter Find(TimeoutCounters counter)
	{
		return Find(KeyFrom(counter));
	}

	public IPersistentCounter Increment(string name)
	{
		try
		{
			IPersistentCounter persistentCounter = Find(name);
			if (persistentCounter == null)
			{
				return null;
			}
			persistentCounter.Increment();
			return CounterTable.Update(persistentCounter) ? persistentCounter : null;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"FindAndIncrementCounter ( name = {name} ) caught an exception.", e);
			return null;
		}
	}

	public IPersistentCounter Increment(TimeoutCounters counter)
	{
		return Increment(KeyFrom(counter));
	}

	public IPersistentCounter Decrement(string name)
	{
		try
		{
			IPersistentCounter persistentCounter = Find(name);
			if (persistentCounter == null)
			{
				return null;
			}
			persistentCounter.Decrement();
			return CounterTable.Update(persistentCounter) ? persistentCounter : null;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"[PersistentCounterService] Decrement counter ( name = {name} ) caught an exception.", e);
			return null;
		}
	}

	public IPersistentCounter Decrement(TimeoutCounters counter)
	{
		return Decrement(KeyFrom(counter));
	}

	public bool Reset(IPersistentCounter counter)
	{
		if (counter == null)
		{
			return false;
		}
		if (counter.Value == 0)
		{
			return true;
		}
		counter.Reset();
		return CounterTable.Update(counter);
	}

	public bool Reset(string name)
	{
		IPersistentCounter persistentCounter = Find(name);
		if (persistentCounter == null)
		{
			return false;
		}
		return Reset(persistentCounter);
	}

	public bool Insert(HardwareCorrectionStatistic type, IExecutionContext context, bool correctionOk)
	{
		if (!ControllerConfiguration.Instance.TrackHardwareCorrections)
		{
			return true;
		}
		IHardwareCorrectionStatistic p = ServiceLocator.Instance.GetService<ITableTypeFactory>().NewStatistic(type, context.ProgramName, correctionOk, DateTime.Now);
		return StatsTable.Insert(p);
	}

	public bool RemoveAll(HardwareCorrectionStatistic stat)
	{
		try
		{
			List<IHardwareCorrectionStatistic> list = StatsTable.LoadEntries();
			List<IHardwareCorrectionStatistic> list2 = new List<IHardwareCorrectionStatistic>();
			using (new DisposeableList<IHardwareCorrectionStatistic>(list2))
			{
				using (new DisposeableList<IHardwareCorrectionStatistic>(list))
				{
					foreach (IHardwareCorrectionStatistic item in list)
					{
						if (item.Statistic == stat)
						{
							list2.Add(item);
						}
					}
				}
				return StatsTable.Delete(list2);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Failed to delete HardwareCorrection info.", e);
			return false;
		}
	}

	public List<IHardwareCorrectionStatistic> GetStats(HardwareCorrectionStatistic key)
	{
		List<IHardwareCorrectionStatistic> list = StatsTable.LoadEntries();
		using (new DisposeableList<IHardwareCorrectionStatistic>(list))
		{
			return list.FindAll((IHardwareCorrectionStatistic each) => each.Statistic == key);
		}
	}

	public void AddWeeklyResettable(IPersistentCounter counter)
	{
		if (!WeeklyResettableCounters.Contains(counter.Name))
		{
			WeeklyResettableCounters.Add(counter.Name);
		}
	}

	public void ResetWeekly()
	{
		List<IPersistentCounter> list = new List<IPersistentCounter>();
		using (new DisposeableList<IPersistentCounter>(list))
		{
			foreach (string weeklyResettableCounter in WeeklyResettableCounters)
			{
				IPersistentCounter persistentCounter = Find(weeklyResettableCounter);
				if (persistentCounter != null && persistentCounter.Value != 0)
				{
					persistentCounter.Reset();
					list.Add(persistentCounter);
				}
			}
			CounterTable.Update(list);
		}
	}

	internal PersistentCounterService()
	{
		foreach (TimeoutCounters value in Enum.GetValues(typeof(TimeoutCounters)))
		{
			string item = KeyFrom(value);
			if (!WeeklyResettableCounters.Contains(item))
			{
				WeeklyResettableCounters.Add(item);
			}
		}
		IDataTableService service = ServiceLocator.Instance.GetService<IDataTableService>();
		CounterTable = service.GetTable<IPersistentCounter>();
		StatsTable = service.GetTable<IHardwareCorrectionStatistic>();
		List<IPersistentCounter> list = CounterTable.LoadEntries();
		using (new DisposeableList<IPersistentCounter>(list))
		{
			foreach (IPersistentCounter item2 in list)
			{
				Counters[item2.Name] = item2;
			}
		}
	}

	private string KeyFrom(TimeoutCounters counter)
	{
		return $"{counter.ToString()}Timeout";
	}
}
