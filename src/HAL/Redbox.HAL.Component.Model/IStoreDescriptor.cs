namespace Redbox.HAL.Component.Model;

public interface IStoreDescriptor
{
	string DataSource { get; }

	StoreLockType LockType { get; }

	IDataStore Store { get; }

	void OnShutdown();
}
