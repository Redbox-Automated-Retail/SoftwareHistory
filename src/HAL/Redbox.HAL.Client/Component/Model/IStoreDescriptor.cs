namespace Redbox.HAL.Component.Model;

internal interface IStoreDescriptor
{
	string DataSource { get; }

	StoreLockType LockType { get; }

	IDataStore Store { get; }

	void OnShutdown();
}
