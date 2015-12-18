using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

public static class Track
{
	private enum TrackState
	{
		Closed,
		Open
	}

	private const int OpenBit = 3;

	private const int ClosedBit = 2;

	private static TrackState State;

	public static CoreResponse TestClose()
	{
		return RetryableTrackCommand(TrackState.Closed);
	}

	public static CoreResponse TestOpen()
	{
		if (ControllerMode.Emulated != ControllerConfiguration.Instance.Mode)
		{
			return RetryableTrackCommand(TrackState.Open);
		}
		return CoreResponse.MakeErrorResponse();
	}

	public static bool Close()
	{
		if (ControllerMode.Emulated != ControllerConfiguration.Instance.Mode)
		{
			return RetryableTrackCommand(TrackState.Closed).TimedOut;
		}
		return false;
	}

	public static bool Open()
	{
		if (ControllerMode.Emulated != ControllerConfiguration.Instance.Mode)
		{
			return RetryableTrackCommand(TrackState.Open).TimedOut;
		}
		return false;
	}

	public static bool Cycle()
	{
		if (!RetryableTrackCommand(TrackState.Open).TimedOut)
		{
			return RetryableTrackCommand(TrackState.Closed).TimedOut;
		}
		return true;
	}

	public static bool IsTrackOpen(bool read)
	{
		return TrackState.Open == State;
	}

	internal static bool OpenIfClosed()
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

	internal static bool CloseIfOpen()
	{
		if (State != TrackState.Closed)
		{
			return RetryableTrackCommand(TrackState.Closed).TimedOut;
		}
		return false;
	}

	private static CoreResponse RetryableTrackCommand(TrackState state)
	{
		CoreResponse coreResponse = null;
		CommandType type = ((state == TrackState.Open) ? CommandType.TrackOpen : CommandType.TrackClose);
		for (int i = 0; i < 2; i++)
		{
			coreResponse = ControlSystem.Instance.ExecuteControllerCommand(type, 5000);
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
		ControlSystem.Instance.LogInputs(ControlBoards.Picker, LogEntryType.Error);
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
}
