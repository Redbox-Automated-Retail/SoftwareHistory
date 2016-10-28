namespace Redbox.HAL.Component.Model;

internal interface IInputReadResult
{
	ErrorCodes Error { get; }

	string[] Result { get; }

	void Log();

	void Log(LogEntryType type);
}
