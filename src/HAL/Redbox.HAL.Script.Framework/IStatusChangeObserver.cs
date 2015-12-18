namespace Redbox.HAL.Script.Framework;

public interface IStatusChangeObserver
{
	void StatusChange(ExecutionContextStatus newStatus);
}
