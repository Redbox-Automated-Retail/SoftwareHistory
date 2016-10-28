namespace Redbox.HAL.Component.Model;

internal interface IPeekResult
{
	ILocation PeekLocation { get; }

	bool TestOk { get; }

	bool IsFull { get; }

	ErrorCodes Error { get; }
}
