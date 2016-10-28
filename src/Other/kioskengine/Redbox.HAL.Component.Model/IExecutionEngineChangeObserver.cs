namespace Redbox.HAL.Component.Model;

internal interface IExecutionEngineChangeObserver
{
	void OnEngineChange(EngineModes newMode);

	void OnShutdown();
}
