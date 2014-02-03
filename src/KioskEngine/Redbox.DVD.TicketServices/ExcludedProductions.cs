using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.TicketServices.Proxy.ComponentModel;

namespace Redbox.TicketServices.Proxy;

internal class ExcludedProductions : IExcludeProductions
{
	private const string LocalDataStore = "local";

	private const string ExcludedProductionsTableName = "ExcludedProductions";

	private List<long> _excludedProductions = new List<long>();

	private object _lock = new object();

	public static ExcludedProductions Instance => Singleton<ExcludedProductions>.Instance;

	public List<long> GetList()
	{
		List<long> list = new List<long>();
		lock (_lock)
		{
			_excludedProductions.ForEach(list.Add);
			return list;
		}
	}

	public void RemoveIfNotIn(List<long> list)
	{
		List<long> list2 = GetList();
		IEnumerable<long> items = list2.Except(list);
		items.ForEach(delegate(long p)
		{
			Remove(p);
		});
	}

	public void RemoveAll()
	{
		List<long> list = GetList();
		list.ForEach(delegate(long p)
		{
			Remove(p);
		});
	}

	public void Remove(long production_id)
	{
		try
		{
			IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
			if (service == null)
			{
				LogHelper.Instance.Log("ExcludedProductions.Remove failed. IDataStoreService not found.");
				return;
			}
			service.Remove("local", "ExcludedProductions", production_id.ToString());
			lock (_lock)
			{
				if (_excludedProductions.Exists((long p) => p == production_id))
				{
					_excludedProductions.Remove(production_id);
					LogHelper.Instance.Log("ExcludedProductions.Remove( production_id = {0})", production_id);
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in ExcludedProductions.Add (production_id={production_id})", e);
		}
	}

	public void Add(long production_id)
	{
		try
		{
			IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
			if (service == null)
			{
				LogHelper.Instance.Log("ExcludedProductions.Add failed. IDataStoreService not found.");
				return;
			}
			service.Set("local", "ExcludedProductions", production_id.ToString(), "{production_id=" + production_id + "}");
			lock (_lock)
			{
				if (!_excludedProductions.Exists((long p) => p == production_id))
				{
					_excludedProductions.Add(production_id);
					LogHelper.Instance.Log("ExcludedProductions.Add( production_id = {0})", production_id);
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in ExcludedProductions.Add (production_id={production_id})", e);
		}
	}

	private Dictionary<string, object> GetDataStoreTable(string dataStore, string tableName)
	{
		try
		{
			IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
			if (service == null)
			{
				LogHelper.Instance.Log("ExcludedProductions.GetDataStoreTable failed. IDataStoreService not found.");
				return null;
			}
			return service.GetAll(dataStore, tableName);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"An exception was raised in ExcludedProductions.GetDataStoreTable({dataStore},{tableName}).", e);
			return null;
		}
	}

	private void Refresh()
	{
		LogHelper.Instance.Log("ExcludedProductions.Refresh Started.");
		Dictionary<string, object> dataStoreTable = GetDataStoreTable("local", "ExcludedProductions");
		if (dataStoreTable == null)
		{
			LogHelper.Instance.Log("ExcludedProductions.Refresh failed.");
			return;
		}
		List<long> list = new List<long>();
		dataStoreTable?.Keys.ForEach(delegate(string key)
		{
			list.Add(Convert.ToInt32(key));
		});
		lock (_lock)
		{
			_excludedProductions = list;
		}
		LogHelper.Instance.Log("ExcludedProductions.Refresh Finished. RowCount={0}", list.Count);
	}

	private ExcludedProductions()
	{
		ServiceLocator.Instance.AddService(typeof(IExcludeProductions), this);
		Refresh();
	}
}
