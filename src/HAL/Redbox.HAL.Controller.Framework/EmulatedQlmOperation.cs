using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class EmulatedQlmOperation : IQlm
{
	public QlmStatus GetStatus()
	{
		return ControllerEmulationState.Instance.ExpectedGetQlmStatusResult;
	}

	public ErrorCodes Engage(bool home, IFormattedLog log)
	{
		return ControllerEmulationState.Instance.ExpectedEngageQlmResult;
	}

	public ErrorCodes Disengage(bool home, IFormattedLog log)
	{
		return ControllerEmulationState.Instance.ExpectedDisengageQlmResult;
	}

	public void Drop()
	{
	}

	public void Lift()
	{
	}

	public bool HaltOperation()
	{
		return ControllerEmulationState.Instance.ExpectedQlmHaltResult;
	}
}
