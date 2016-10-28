namespace Redbox.HAL.Component.Model;

internal interface ITransferResult
{
	bool ReturnedToSource { get; }

	bool Transferred { get; }

	ErrorCodes TransferError { get; }

	ILocation Source { get; }

	ILocation Destination { get; }
}
