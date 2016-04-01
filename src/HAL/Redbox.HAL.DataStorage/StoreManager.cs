using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

public sealed class StoreManager : IDataStoreService
{
	private readonly List<IStoreDescriptor> Descriptors = new List<IStoreDescriptor>();

	private readonly Dictionary<string, IDataStore> Cache = new Dictionary<string, IDataStore>();

	private readonly IRuntimeService RuntimeService;

	public string StoreDirectory { get; private set; }

	public bool Shutdown()
	{
		lock (Cache)
		{
			foreach (string key in Cache.Keys)
			{
				IDataStore dataStore = Cache[key];
				dataStore.CloseConnection(force: true);
			}
			foreach (IStoreDescriptor descriptor in Descriptors)
			{
				descriptor.OnShutdown();
			}
			Cache.Clear();
			Descriptors.Clear();
		}
		return true;
	}

	public IDataStore CreateStore(IStoreDescriptor descriptor)
	{
		lock (Cache)
		{
			string key = descriptor.DataSource.ToUpper();
			if (Cache.ContainsKey(key))
			{
				return Cache[key];
			}
			string source = Path.Combine(StoreDirectory, descriptor.DataSource);
			IDataStore dataStore = null;
			dataStore = ((StoreLockType.NoLock != descriptor.LockType) ? ((IDataStore)new ThreadSafeVistaDBStore(source)) : ((IDataStore)new NoLockVistaDBStore(source)));
			Cache[key] = dataStore;
			Descriptors.Add(descriptor);
			return dataStore;
		}
	}

	public StoreManager()
	{
		RuntimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
		StoreDirectory = Path.Combine(RuntimeService.AssemblyDirectory, "data");
	}
}
