using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal class TrackImpl : ITrack
{
	private readonly ControlSystem ControlSystem;

	private const int TrackOpenBit = 3;

	private const int TrackClosedBit = 2;

	private const int OpTries = 2;

	public IControlResponse Open()
	{
		return RetryableTrackCommand(TrackState.Open);
	}

	public IControlResponse Close()
	{
		return RetryableTrackCommand(TrackState.Closed);
	}

	internal TrackImpl(ControlSystem controlSystem)
	{
		ControlSystem = controlSystem;
	}

	private IControlResponse RetryableTrackCommand(TrackState state)
	{
		IControlResponse controlResponse = null;
		CommandType type = ((state == TrackState.Open) ? CommandType.TrackOpen : CommandType.TrackClose);
		for (int i = 0; i < 2; i++)
		{
			controlResponse = ControlSystem.ExecuteControllerCommand(type, 5000);
			if (controlResponse.Success)
			{
				return controlResponse;
			}
			if (controlResponse.CommError)
			{
				LogHelper.Instance.Log($"Communication error during track {state.ToString().ToUpper()} operation.", LogEntryType.Error);
				return controlResponse;
			}
		}
		LogHelper.Instance.Log(LogEntryType.Error, "Timeout error during track {0} operation.", state.ToString().ToUpper());
		ControlSystem.LogInputs(ControlBoards.Picker, LogEntryType.Error);
		return controlResponse;
	}
}
