namespace Redbox.HAL.Script.Framework;

internal interface IPollerService
{
	ScriptPoller Locate(string name);

	void InitializePollers();
}
