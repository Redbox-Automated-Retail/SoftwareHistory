using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public class Track
{
	private enum TrackState
	{
		Closed,
		Open
	}

	private const int OpenBit = 3;

	private const int ClosedBit = 2;

	public static Track Instance => Singleton<Track>.Instance;

	private TrackState State { get; set; }

	public CoreResponse TestClose()
	{
		return RetryableTrackCommand(TrackState.Closed);
	}

	public CoreResponse TestOpen()
	{
		if (ControllerMode.Emulated != ControllerConfiguration.Instance.Mode)
		{
			return RetryableTrackCommand(TrackState.Open);
		}
		return CoreResponse.MakeEmulated(timeout: false);
	}

	public bool Close()
	{
		if (ControllerMode.Emulated != ControllerConfiguration.Instance.Mode)
		{
			return RetryableTrackCommand(TrackState.Closed).TimedOut;
		}
		return false;
	}

	public bool Open()
	{
		if (ControllerMode.Emulated != ControllerConfiguration.Instance.Mode)
		{
			return RetryableTrackCommand(TrackState.Open).TimedOut;
		}
		return false;
	}

	public bool Cycle()
	{
		if (!RetryableTrackCommand(TrackState.Open).TimedOut)
		{
			return RetryableTrackCommand(TrackState.Closed).TimedOut;
		}
		return true;
	}

	public bool IsTrackOpen(bool read)
	{
		if (!read)
		{
			return TrackState.Open == State;
		}
		return ReadTrackState(3);
	}

	internal bool OpenIfClosed()
	{
		if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode)
		{
			return ControllerEmulationState.Instance.ExpectedIsTrackOpenResult;
		}
		if (TrackState.Open != State)
		{
			return RetryableTrackCommand(TrackState.Open).TimedOut;
		}
		return false;
	}

	internal bool CloseIfOpen()
	{
		if (State != TrackState.Closed)
		{
			return RetryableTrackCommand(TrackState.Closed).TimedOut;
		}
		return false;
	}

	private CoreResponse RetryableTrackCommand(TrackState state)
	{
		CoreResponse coreResponse = null;
		CommandType type = ((state == TrackState.Open) ? CommandType.TrackOpen : CommandType.TrackClose);
		for (int i = 0; i < 2; i++)
		{
			coreResponse = CoreController.ExecuteSingleWaitableCommand(type, 5000);
			if (coreResponse.CommunicationError)
			{
				LogHelper.Instance.Log($"Communication error during track {state.ToString().ToUpper()} operation.", LogEntryType.Error);
				return coreResponse;
			}
			if (!coreResponse.TimedOut)
			{
				State = state;
				return coreResponse;
			}
		}
		LogHelper.Instance.Log($"Timeout error during track {state.ToString().ToUpper()} operation.", LogEntryType.Error);
		PickerBoard.Instance.LogInputs(LogEntryType.Error);
		if (state == TrackState.Closed)
		{
			CounterManager.Instance.Increment("TrackCloseTimeout");
		}
		else if (TrackState.Open == state)
		{
			CounterManager.Instance.Increment("TrackOpenTimeout");
		}
		return coreResponse;
	}

	private bool ReadTrackState(int bit)
	{
		return PickerBoard.Instance.IsInputBlocked(bit);
	}

	private Track()
	{
	}
}
