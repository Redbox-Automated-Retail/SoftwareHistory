namespace Redbox.HAL.Component.Model;

internal interface IPersistentCounter
{
	string Name { get; }

	int Value { get; }

	int Increment();

	int Decrement();

	void Reset();
}
