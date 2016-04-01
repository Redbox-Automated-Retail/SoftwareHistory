using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class PersistentCounterService : IPersistentCounterService
{
	private const string Table = "Counters";

	private const string CorrectionTable = "HardwareCorrection";

	private readonly List<string> WeeklyResettableCounters = new List<string>();

	private readonly Dictionary<TimeoutCounters, string> TimeoutCounterMap = new Dictionary<TimeoutCounters, string>();

	private readonly Dictionary<string, IPersistentCounter> Counters = new Dictionary<string, IPersistentCounter>();

	private readonly object SyncObject = new object();

	private readonly IDataStore Store;

	public bool UpdateTable()
	{
		return true;
	}

	public void Shutdown()
	{
		lock (SyncObject)
		{
			Counters.Clear();
		}
	}

	public bool Exists(string name)
	{
		lock (SyncObject)
		{
			return FindUnderLock(name, createIfNotFound: false) != null;
		}
	}

	public IPersistentCounter Find(string name)
	{
		lock (SyncObject)
		{
			return FindUnderLock(name, createIfNotFound: true);
		}
	}

	public IPersistentCounter Find(TimeoutCounters counter)
	{
		return Find(TimeoutCounterMap[counter]);
	}

	public IPersistentCounter Increment(string name)
	{
		lock (SyncObject)
		{
			try
			{
				IPersistentCounter persistentCounter = FindUnderLock(name, createIfNotFound: true);
				if (persistentCounter == null)
				{
					return null;
				}
				persistentCounter.Increment();
				return UpdateCounterInner(persistentCounter) ? persistentCounter : null;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"FindAndIncrementCounter ( name = {name} ) caught an exception.", e);
				return null;
			}
		}
	}

	public IPersistentCounter Increment(TimeoutCounters counter)
	{
		return Increment(TimeoutCounterMap[counter]);
	}

	public IPersistentCounter Decrement(string name)
	{
		lock (SyncObject)
		{
			try
			{
				IPersistentCounter persistentCounter = FindUnderLock(name, createIfNotFound: true);
				if (persistentCounter == null)
				{
					return null;
				}
				persistentCounter.Decrement();
				return UpdateCounterInner(persistentCounter) ? persistentCounter : null;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"FindAndDecrementCounter ( name = {name} ) caught an exception.", e);
				return null;
			}
		}
	}

	public bool ResetCounter(IPersistentCounter counter)
	{
		lock (SyncObject)
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
			return UpdateCounterInner(counter);
		}
	}

	public IPersistentCounter Reset(string name)
	{
		lock (SyncObject)
		{
			return ResetUnderLock(name);
		}
	}

	public IPersistentCounter Reset(TimeoutCounters ctr)
	{
		return Reset(TimeoutCounterMap[ctr]);
	}

	public void LogTimeouts(TextWriter log)
	{
		foreach (TimeoutCounters value in Enum.GetValues(typeof(TimeoutCounters)))
		{
			LogTimeout(value, log);
		}
	}

	public void LogTimeout(TimeoutCounters counter, TextWriter writer)
	{
		LogCounter(TimeoutCounterMap[counter], writer);
	}

	public void LogCounter(string counter, TextWriter writer)
	{
		int num = 0;
		IPersistentCounter persistentCounter = Find(counter);
		if (persistentCounter != null)
		{
			num = persistentCounter.Value;
		}
		writer.WriteLine($" {counter}: {num}");
	}

	public bool Insert(HardwareCorrectionStatistic type, IExecutionContext context, bool correctionOk)
	{
		if (!ControllerConfiguration.Instance.TrackHardwareCorrections)
		{
			return true;
		}
		string insert = string.Format("INSERT INTO [{0}] (Type, Job, CorrectionTime, Success) VALUES ('{1}', '{2}', '{3}', '{4}')", "HardwareCorrection", (int)type, context.ProgramName, DateTime.Now, correctionOk ? 1 : 0);
		return Store.Insert(insert);
	}

	public int RemoveAll(HardwareCorrectionStatistic stat)
	{
		try
		{
			string deleteQuery = string.Format("DELETE FROM [{0}] where Type = {1}", "HardwareCorrection", (int)stat);
			return Store.Delete(deleteQuery);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Failed to delete HardwareCorrection info.", e);
			return 0;
		}
	}

	public void LogStats(HardwareCorrectionStatistic key, TextWriter log)
	{
		string query = string.Format("SELECT Job, CorrectionTime, Success FROM [{0}] WHERE Type = '{1}'", "HardwareCorrection", (int)key);
		Store.ExectuteSelectQuery(delegate(IDataReader reader)
		{
			string arg = reader.ReadString(0);
			DateTime? dateTime = reader.ReadDateTime(1);
			bool flag = reader.ReadBoolean(2);
			log.WriteLine("{0},{1},{2}", dateTime.ToString(), arg, flag ? "SUCCESS" : "FAILURE");
		}, query);
	}

	public void AddWeeklyResettable(IPersistentCounter counter)
	{
		lock (SyncObject)
		{
			if (!WeeklyResettableCounters.Contains(counter.Name))
			{
				WeeklyResettableCounters.Add(counter.Name);
			}
		}
	}

	public void AddWeeklyResettable(TimeoutCounters counter)
	{
		lock (SyncObject)
		{
			string item = TimeoutCounterMap[counter];
			if (!WeeklyResettableCounters.Contains(item))
			{
				WeeklyResettableCounters.Add(TimeoutCounterMap[counter]);
			}
		}
	}

	public void ResetWeekly()
	{
		lock (SyncObject)
		{
			foreach (string weeklyResettableCounter in WeeklyResettableCounters)
			{
				ResetUnderLock(weeklyResettableCounter);
			}
		}
	}

	internal PersistentCounterService(IDataStore store)
	{
		BuildTimeoutMap();
		Store = store;
		string query = string.Format("SELECT Name, Value FROM [{0}]", "Counters");
		IDataStore store2 = Store;
		HandleSelectResult callback = delegate(IDataReader reader)
		{
			Counter counter = new Counter(reader.ReadString(0), reader.ReadInt(1));
			Counters[counter.Name] = counter;
		};
		store2.ExectuteSelectQuery(callback, query);
	}

	private IPersistentCounter ResetUnderLock(string name)
	{
		IPersistentCounter persistentCounter = FindUnderLock(name, createIfNotFound: true);
		if (persistentCounter == null)
		{
			return null;
		}
		if (persistentCounter.Value == 0)
		{
			return persistentCounter;
		}
		persistentCounter.Reset();
		if (!UpdateCounterInner(persistentCounter))
		{
			return null;
		}
		return persistentCounter;
	}

	private IPersistentCounter InsertCounter(string name, int defaultValue)
	{
		string insert = string.Format("INSERT INTO [{0}] (Name, Value) VALUES ('{1}', '{2}')", "Counters", name, defaultValue);
		if (Store.Insert(insert))
		{
			return new Counter(name, defaultValue);
		}
		LogHelper.Instance.Log($"Unable to create counter {name}", LogEntryType.Info);
		return null;
	}

	private void BuildTimeoutMap()
	{
		TimeoutCounterMap[TimeoutCounters.FingerClose] = "FingerCloseTimeout";
		TimeoutCounterMap[TimeoutCounters.FingerOpen] = "FingerOpenTimeout";
		TimeoutCounterMap[TimeoutCounters.FingerRent] = "FingerRentTimeout";
		TimeoutCounterMap[TimeoutCounters.GripperExtend] = "GripperExtendTimeout";
		TimeoutCounterMap[TimeoutCounters.GripperRetract] = "GripperRetractTimeout";
		TimeoutCounterMap[TimeoutCounters.TrackClose] = "TrackCloseTimeout";
		TimeoutCounterMap[TimeoutCounters.TrackOpen] = "TrackOpenTimeout";
		TimeoutCounterMap[TimeoutCounters.VendDoorClose] = "VendDoorCloseTimeout";
		TimeoutCounterMap[TimeoutCounters.VendDoorRent] = "VendDoorRentTimeout";
		TimeoutCounterMap[TimeoutCounters.QlmDisengage] = "QlmDisengageTimeout";
		TimeoutCounterMap[TimeoutCounters.QlmEngage] = "QlmEngageTimeout";
	}

	private IPersistentCounter FindUnderLock(string name, bool createIfNotFound)
	{
		if (Counters.ContainsKey(name))
		{
			return Counters[name];
		}
		if (!createIfNotFound)
		{
			return null;
		}
		IPersistentCounter persistentCounter = InsertCounter(name, 0);
		if (persistentCounter != null)
		{
			Counters[name] = persistentCounter;
		}
		return persistentCounter;
	}

	private bool UpdateCounterInner(IPersistentCounter counter)
	{
		string update = string.Format("UPDATE {0} SET VALUE = '{1}' WHERE NAME = '{2}'", "Counters", counter.Value, counter.Name);
		return Store.Update(update);
	}
}
