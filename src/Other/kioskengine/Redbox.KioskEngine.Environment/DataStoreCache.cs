using System;
using System.Collections.Generic;
using System.IO;
using Redbox.Core;
using VistaDB;
using VistaDB.DDA;

namespace Redbox.KioskEngine.Environment;

public class DataStoreCache
{
	private sealed class DatabaseCache
	{
		public IVistaDBDDA DDA;

		public IVistaDBDatabase Instance;
	}

	private static readonly IDictionary<string, DatabaseCache> m_databases = new Dictionary<string, DatabaseCache>();

	public static void CloseAll()
	{
		LogHelper.Instance.Log("Close all data store caches.");
		List<string> list = new List<string>(m_databases.Keys);
		foreach (string item in list)
		{
			LogHelper.Instance.Log("...Close data store: {0}", item);
			CloseStore(item);
		}
	}

	public static void CloseStore(string path)
	{
		if (m_databases.ContainsKey(path))
		{
			DatabaseCache databaseCache = m_databases[path];
			databaseCache.Instance.Dispose();
			databaseCache.DDA.Dispose();
			m_databases.Remove(path);
		}
	}

	public static IVistaDBDatabase GetWorkStore(string path)
	{
		return GetStore(path, VistaDBDatabaseOpenMode.NonexclusiveReadWrite);
	}

	public static IVistaDBDatabase GetProfileStore(string path)
	{
		return GetStore(path, VistaDBDatabaseOpenMode.SharedReadOnly);
	}

	private static IVistaDBDatabase GetStore(string path, VistaDBDatabaseOpenMode mode)
	{
		try
		{
			if (m_databases.ContainsKey(path))
			{
				return m_databases[path].Instance;
			}
			DatabaseCache databaseCache = new DatabaseCache();
			databaseCache.DDA = VistaDBEngine.Connections.OpenDDA();
			DatabaseCache databaseCache2 = databaseCache;
			if (!File.Exists(path))
			{
				if (mode == VistaDBDatabaseOpenMode.SharedReadOnly)
				{
					return null;
				}
				databaseCache2.Instance = databaseCache2.DDA.CreateDatabase(path, stayExclusive: false, null, 0, 0, caseSensitive: false);
			}
			if (databaseCache2.Instance == null)
			{
				databaseCache2.Instance = databaseCache2.DDA.OpenDatabase(path, mode, null);
			}
			m_databases[path] = databaseCache2;
			return databaseCache2.Instance;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception in DataStoreCache.GetStore.", e);
			throw;
		}
	}
}
