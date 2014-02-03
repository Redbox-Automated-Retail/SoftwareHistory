namespace Redbox.HAL.Script.Framework;

public enum ExecutionContextStatus
{
	Suspended,
	Pending,
	Running,
	Errored,
	Stopped,
	Garbage,
	Completed
}
