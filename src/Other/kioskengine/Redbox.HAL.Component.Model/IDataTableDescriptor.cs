namespace Redbox.HAL.Component.Model;

internal interface IDataTableDescriptor
{
	string Source { get; }

	bool ExclusiveReadWrite { get; }

	bool SupportsPooling { get; }

	bool UseTransaction { get; }
}
