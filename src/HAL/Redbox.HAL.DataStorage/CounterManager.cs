using System;
using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataStorage;

public sealed class CounterManager
{
	private readonly Dictionary<string, Counter> Counters = new Dictionary<string, Counter>();

	private readonly object SyncObject = new object();

	public static CounterManager Instance => Singleton<CounterManager>.Instance;

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
			return FindInner(name, createIfNotFound: false) != null;
		}
	}

	public Counter Find(string name)
	{
		lock (SyncObject)
		{
			return FindInner(name, createIfNotFound: true);
		}
	}

	public Counter Increment(string name)
	{
		lock (SyncObject)
		{
			try
			{
				Counter counter = FindInner(name, createIfNotFound: true);
				if (counter == null)
				{
					return null;
				}
				counter.Increment();
				return UpdateCounterInner(counter) ? counter : null;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"FindAndIncrementCounter ( name = {name} ) caught an exception.", e);
				return null;
			}
		}
	}

	public Counter Decrement(string name)
	{
		lock (SyncObject)
		{
			try
			{
				Counter counter = FindInner(name, createIfNotFound: true);
				if (counter == null)
				{
					return null;
				}
				counter.Decrement();
				return UpdateCounterInner(counter) ? counter : null;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"FindAndDecrementCounter ( name = {name} ) caught an exception.", e);
				return null;
			}
		}
	}

	public bool ResetCounter(Counter counter)
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
			counter.Value = 0;
			return UpdateCounterInner(counter);
		}
	}

	public Counter Reset(string name)
	{
		lock (SyncObject)
		{
			Counter counter = FindInner(name, createIfNotFound: true);
			if (counter == null)
			{
				return null;
			}
			if (counter.Value == 0)
			{
				return counter;
			}
			counter.Value = 0;
			return UpdateCounterInner(counter) ? counter : null;
		}
	}

	internal Counter InsertCounter(string name, int defaultValue)
	{
		IDataStore dataStore = StoreManager.Instance.FindByName("HALCounters.vdb3");
		if (dataStore == null)
		{
			return null;
		}
		string insert = string.Format("INSERT INTO [{0}] (Name, Value) VALUES ('{1}', '{2}')", "Counters", name, defaultValue);
		if (dataStore.Insert(insert))
		{
			Counter counter = new Counter();
			counter.Name = name;
			counter.Value = defaultValue;
			return counter;
		}
		LogHelper.Instance.Log($"Unable to create counter {name}", LogEntryType.Info);
		return null;
	}

	private CounterManager()
	{
		IDataStore dataStore = StoreManager.Instance.FindByName("HALCounters.vdb3");
		if (dataStore != null)
		{
			string query = string.Format("SELECT Name, Value FROM [{0}]", "Counters");
			dataStore.ExectuteSelectQuery(delegate(IDataReader reader)
			{
				Counter counter = new Counter
				{
					Name = reader.ReadString(0),
					Value = reader.ReadInt(1)
				};
				Counters[counter.Name] = counter;
			}, query);
		}
	}

	private Counter FindInner(string name, bool createIfNotFound)
	{
		if (Counters.ContainsKey(name))
		{
			return Counters[name];
		}
		if (!createIfNotFound)
		{
			return null;
		}
		Counter counter = InsertCounter(name, 0);
		if (counter != null)
		{
			Counters[name] = counter;
		}
		return counter;
	}

	private bool UpdateCounterInner(Counter counter)
	{
		IDataStore dataStore = StoreManager.Instance.FindByName("HALCounters.vdb3");
		if (dataStore == null)
		{
			return false;
		}
		string update = string.Format("UPDATE {0} SET VALUE = '{1}' WHERE NAME = '{2}'", "Counters", counter.Value, counter.Name);
		return dataStore.Update(update);
	}
}
