namespace Redbox.HAL.Component.Model;

public interface IDataStoreService
{
	string StoreDirectory { get; }

	IDataStore CreateStore(IStoreDescriptor descriptor);

	bool Shutdown();
}
