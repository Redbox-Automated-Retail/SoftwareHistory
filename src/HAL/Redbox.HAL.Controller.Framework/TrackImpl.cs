using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal class TrackImpl : ITrack
{
	private const int TrackOpenBit = 3;

	private const int TrackClosedBit = 2;

	private const int OpTries = 2;

	private readonly ControlSystem ControlSystem;

	private TrackState m_trackState;

	public TrackState TrackState => m_trackState;

	public IControlResponse Open()
	{
		return RetryableTrackCommand(TrackState.Open);
	}

	public IControlResponse Close()
	{
		return RetryableTrackCommand(TrackState.Closed);
	}

	public IControlResponse Cycle()
	{
		IControlResponse controlResponse = RetryableTrackCommand(TrackState.Open);
		if (!controlResponse.Success)
		{
			return controlResponse;
		}
		return RetryableTrackCommand(TrackState.Closed);
	}

	internal TrackImpl(ControlSystem controlSystem)
	{
		ControlSystem = controlSystem;
	}

	private IControlResponse RetryableTrackCommand(TrackState state)
	{
		IControlResponse controlResponse = null;
		m_trackState = TrackState.Unknown;
		CommandType type = ((state == TrackState.Open) ? CommandType.TrackOpen : CommandType.TrackClose);
		for (int i = 0; i < 2; i++)
		{
			controlResponse = ControlSystem.ExecuteControllerCommand(type, 5000);
			if (controlResponse.Success)
			{
				m_trackState = state;
				return controlResponse;
			}
			if (controlResponse.CommError)
			{
				LogHelper.Instance.Log($"Communication error during track {state.ToString().ToUpper()} operation.", LogEntryType.Error);
				return controlResponse;
			}
		}
		LogHelper.Instance.Log($"Timeout error during track {state.ToString().ToUpper()} operation.", LogEntryType.Error);
		ControlSystem.LogInputs(ControlBoards.Picker, LogEntryType.Error);
		IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		if (TrackState.Closed == state)
		{
			service.Increment(TimeoutCounters.TrackClose);
		}
		else if (TrackState.Open == state)
		{
			service.Increment(TimeoutCounters.TrackOpen);
		}
		return controlResponse;
	}
}
