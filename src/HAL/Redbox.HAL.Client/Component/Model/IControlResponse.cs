namespace Redbox.HAL.Component.Model;

internal interface IControlResponse
{
	bool Success { get; }

	bool TimedOut { get; }

	bool CommError { get; }

	string Diagnostic { get; }
}
