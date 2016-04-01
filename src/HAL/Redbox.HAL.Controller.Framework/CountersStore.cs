using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class CountersStore : IStoreDescriptor
{
	public string DataSource => "HALCounters.vdb3";

	public StoreLockType LockType => StoreLockType.Synchronized;

	public IDataStore Store { get; private set; }

	public void OnShutdown()
	{
	}

	internal CountersStore()
	{
		IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
		Store = service.CreateStore(this);
	}
}
