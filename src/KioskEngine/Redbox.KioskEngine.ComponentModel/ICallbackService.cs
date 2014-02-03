namespace Redbox.KioskEngine.ComponentModel;

public interface ICallbackService
{
	void Reset();

	void Flush();

	void Resume();

	void Suspend();

	bool InvokeNextCallback();

	void EnqueueCallback(ICallbackEntry callbackEntry);
}
