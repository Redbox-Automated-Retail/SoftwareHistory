namespace Redbox.HAL.Component.Model;

internal interface IDataStoreService
{
	string StoreDirectory { get; }

	IDataStore CreateStore(IStoreDescriptor descriptor);

	bool Shutdown();
}
