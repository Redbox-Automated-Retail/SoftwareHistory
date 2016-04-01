using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal class InventoryStore : IStoreDescriptor
{
	public string DataSource => "HALData.vdb3";

	public StoreLockType LockType => StoreLockType.NoLock;

	public IDataStore Store { get; private set; }

	public void OnShutdown()
	{
	}

	internal InventoryStore()
	{
		IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
		Store = service.CreateStore(this);
	}
}
