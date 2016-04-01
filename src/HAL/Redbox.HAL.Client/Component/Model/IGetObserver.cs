namespace Redbox.HAL.Component.Model;

internal interface IGetObserver
{
	void OnStuck(IGetResult result);

	bool OnEmpty(IGetResult result);
}
