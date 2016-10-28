namespace Redbox.HAL.Component.Model;

internal interface IComponentErrorThresholdService
{
	bool Register(string key, IComponentErrorThreshold thresh);

	bool Remove(string key);

	IComponentErrorThreshold Find(string key);
}
