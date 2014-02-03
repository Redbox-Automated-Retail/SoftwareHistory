using System;
using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class CounterManager
{
	private Dictionary<string, Counter> Counters;

	public static CounterManager Instance => Singleton<CounterManager>.Instance;

	public bool Exists(string name)
	{
		lock (this)
		{
			return FindInner(name, createIfNotFound: false) != null;
		}
	}

	public Counter Find(string name)
	{
		lock (this)
		{
			return FindInner(name, createIfNotFound: true);
		}
	}

	public Counter Increment(string name)
	{
		lock (this)
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
		lock (this)
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
		lock (this)
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
		lock (this)
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

	private CounterManager()
	{
		Counters = new Dictionary<string, Counter>();
		List<Counter> list = CountersDBManager.Instance.LoadCounters();
		foreach (Counter item in list)
		{
			Counters[item.Name] = item;
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
		Counter counter = CountersDBManager.Instance.InsertCounter(name, 0);
		if (counter != null)
		{
			Counters[name] = counter;
		}
		return counter;
	}

	private bool UpdateCounterInner(Counter counter)
	{
		string query = $"UPDATE {CountersDBManager.Instance.CountersTableName} SET VALUE = '{counter.Value}' WHERE NAME = '{counter.Name}'";
		return CountersDBManager.Instance.MakeUpdate(query);
	}
}
