namespace Redbox.HAL.Component.Model;

internal interface IComponentErrorThreshold
{
	bool Exceeded { get; }

	bool Correct();

	int Increment();

	void Reset();
}
