namespace Redbox.HAL.Component.Model;

internal interface IPutObserver
{
	void OnSuccessfulPut(IPutResult result, IFormattedLog log);

	void OnFailedPut(IPutResult result, IFormattedLog log);
}
