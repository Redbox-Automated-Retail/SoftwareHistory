namespace Redbox.UpdateManager.ComponentModel;

internal enum TransferStatus : byte
{
	Queued,
	Connecting,
	Transfering,
	Suspended,
	Error,
	TransientError,
	Transferred,
	Acknowledged,
	Cancelled
}
