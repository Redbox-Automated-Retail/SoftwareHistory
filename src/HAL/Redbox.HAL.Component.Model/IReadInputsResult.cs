namespace Redbox.HAL.Component.Model;

public interface IReadInputsResult
{
	ErrorCodes Error { get; }

	bool Success { get; }

	string[] Inputs { get; }

	bool SensorSet(int idx);
}
