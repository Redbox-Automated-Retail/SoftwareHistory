using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

public static class VendDoor
{
	public static CoreResponse TestRent()
	{
		return RetryableVendDoorCommand(VendDoorState.Rent, retry: true);
	}

	public static CoreResponse TestClose()
	{
		return RetryableVendDoorCommand(VendDoorState.Closed, retry: true);
	}

	public static CoreResponse TestOpen()
	{
		return CoreResponse.MakeErrorResponse();
	}

	public static bool IsClosed()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedIsVendDoorClosedResult;
		}
		return VendDoorIsInState(VendDoorState.Closed);
	}

	public static bool Close()
	{
		return SetVendDoorInner(VendDoorState.Closed, retry: true);
	}

	public static bool Open()
	{
		return true;
	}

	public static bool Rent()
	{
		return SetVendDoorInner(VendDoorState.Rent, retry: false);
	}

	internal static bool VendDoorIsInState(VendDoorState state)
	{
		int bit = -1;
		switch (state)
		{
		case VendDoorState.Closed:
			bit = 2;
			break;
		case VendDoorState.Open:
			bit = 3;
			break;
		case VendDoorState.Rent:
			bit = 4;
			break;
		}
		CoreResponse coreResponse = ControlSystem.Instance.ExecuteControllerCommand(CommandType.AuxSensorsRead);
		if (coreResponse.CommunicationError)
		{
			LogHelper.Instance.Log("Read AUX sensors failed.", LogEntryType.Error);
			return false;
		}
		return coreResponse.IsBitSet(bit);
	}

	private static bool SetVendDoorInner(VendDoorState state, bool retry)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedSetVendDoorResult;
		}
		if (!VendDoorIsInState(state))
		{
			return RetryableVendDoorCommand(state, retry).TimedOut;
		}
		return false;
	}

	private static CoreResponse RetryableVendDoorCommand(VendDoorState state, bool retry)
	{
		CommandType commandType = CommandType.VendDoorClose;
		CommandType unknownCommand = CommandType.UnknownVendDoorOpenCommand;
		switch (state)
		{
		case VendDoorState.Open:
			commandType = CommandType.VendDoorOpen;
			unknownCommand = CommandType.UnknownVendDoorOpenCommand;
			break;
		case VendDoorState.Rent:
			commandType = CommandType.VendDoorRent;
			break;
		case VendDoorState.Closed:
			commandType = CommandType.VendDoorClose;
			unknownCommand = CommandType.UnknownVendDoorCloseCommand;
			break;
		}
		int num = ((!retry) ? 1 : 2);
		CoreResponse coreResponse = null;
		for (int i = 0; i < num; i++)
		{
			coreResponse = ControlSystem.Instance.ExecuteControllerCommand(commandType, 5500);
			if (coreResponse.CommunicationError)
			{
				string message = $"Communication error during vend door {state.ToString().ToUpper()}";
				LogHelper.Instance.Log(message, LogEntryType.Error);
				return coreResponse;
			}
			if (coreResponse.TimedOut)
			{
				if (state == VendDoorState.Closed)
				{
					PerfFunctions.Wait(70);
					ControlSystem.Instance.ExecuteControllerCommand(CommandType.UnknownVendDoorCloseCommand);
					PerfFunctions.Wait(70);
					ControlSystem.Instance.ExecuteControllerCommand(CommandType.VendDoorKillCommand);
					PerfFunctions.Wait(50);
				}
			}
			else
			{
				if (state != VendDoorState.Closed)
				{
					return coreResponse;
				}
				if (VendDoorIsInState(state) || MoveToVendSensor(state, commandType, unknownCommand))
				{
					return coreResponse;
				}
			}
		}
		string message2 = $"SetVendDoor {state.ToString()} timed out after {num} tries.";
		LogHelper.Instance.Log(message2, LogEntryType.Error);
		ControlSystem.Instance.LogInputs(ControlBoards.Aux, LogEntryType.Error);
		switch (state)
		{
		case VendDoorState.Closed:
			CounterManager.Instance.Increment("VendDoorCloseTimeout");
			break;
		case VendDoorState.Rent:
			CounterManager.Instance.Increment("VendDoorRentTimeout");
			break;
		}
		return coreResponse;
	}

	private static bool MoveToVendSensor(VendDoorState state, CommandType doorCommand, CommandType unknownCommand)
	{
		if (!ControllerConfiguration.Instance.MoveVendDoorToAuxSensor)
		{
			return true;
		}
		if (state == VendDoorState.Rent)
		{
			return true;
		}
		PerfFunctions.SpinWait(200);
		ControlSystem.Instance.ExecuteControllerCommand(unknownCommand);
		PerfFunctions.SpinWait(100);
		ControlSystem.Instance.ExecuteControllerCommand(CommandType.VendDoorKillCommand);
		PerfFunctions.SpinWait(100);
		CoreResponse coreResponse = ControlSystem.Instance.ExecuteControllerCommand(doorCommand, 5000);
		if (coreResponse.CommunicationError)
		{
			return false;
		}
		PerfFunctions.SpinWait(100);
		return !coreResponse.TimedOut;
	}
}
