namespace Redbox.HAL.Component.Model;

internal interface IPersistentOption
{
	string Key { get; }

	string Value { get; }

	void UpdateValue(string value);
}
