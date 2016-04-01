using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class VendDoorImpl : IVendDoor
{
	public IControlResponse Rent()
	{
		return RetryableVendDoorCommand(VendDoorState.Rent, retry: false);
	}

	public IControlResponse Close()
	{
		return RetryableVendDoorCommand(VendDoorState.Closed, retry: true);
	}

	public VendDoorState ReadState()
	{
		ICoreCommandExecutor service = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
		CoreResponse coreResponse = service.ExecuteControllerCommand(CommandType.AuxSensorsRead);
		if (!coreResponse.Success)
		{
			LogHelper.Instance.Log("Read AUX sensors failed.", LogEntryType.Error);
			return VendDoorState.Unknown;
		}
		if (coreResponse.IsBitSet(2))
		{
			return VendDoorState.Closed;
		}
		if (coreResponse.IsBitSet(4))
		{
			return VendDoorState.Rent;
		}
		return VendDoorState.Unknown;
	}

	private bool VendDoorIsInState(VendDoorState state)
	{
		return ReadState() == state;
	}

	private CoreResponse RetryableVendDoorCommand(VendDoorState state, bool retry)
	{
		CommandType commandType = CommandType.VendDoorClose;
		CommandType unknownCommand = CommandType.UnknownVendDoorOpenCommand;
		switch (state)
		{
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
		ICoreCommandExecutor service = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
		for (int i = 0; i < num; i++)
		{
			coreResponse = service.ExecuteControllerCommand(commandType, 5500);
			if (coreResponse.CommError)
			{
				string message = $"Communication error during vend door {state.ToString().ToUpper()}";
				LogHelper.Instance.Log(message, LogEntryType.Error);
				return coreResponse;
			}
			if (coreResponse.TimedOut && state == VendDoorState.Closed)
			{
				PerfFunctions.Wait(70);
				service.ExecuteControllerCommand(CommandType.UnknownVendDoorCloseCommand);
				PerfFunctions.Wait(70);
				service.ExecuteControllerCommand(CommandType.VendDoorKillCommand);
				PerfFunctions.Wait(50);
				continue;
			}
			if (state != VendDoorState.Closed)
			{
				return coreResponse;
			}
			if (VendDoorIsInState(state) || MoveToVendSensor(state, commandType, unknownCommand))
			{
				return coreResponse;
			}
		}
		string message2 = $"SetVendDoor {state.ToString()} timed out after {num} tries.";
		LogHelper.Instance.Log(message2, LogEntryType.Error);
		IControlSystem service2 = ServiceLocator.Instance.GetService<IControlSystem>();
		service2.LogInputs(ControlBoards.Aux, LogEntryType.Error);
		IPersistentCounterService service3 = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		switch (state)
		{
		case VendDoorState.Closed:
			service3.Increment(TimeoutCounters.VendDoorClose);
			break;
		case VendDoorState.Rent:
			service3.Increment(TimeoutCounters.VendDoorRent);
			break;
		}
		return coreResponse;
	}

	private bool MoveToVendSensor(VendDoorState state, CommandType doorCommand, CommandType unknownCommand)
	{
		if (!ControllerConfiguration.Instance.MoveVendDoorToAuxSensor || VendDoorState.Rent == state)
		{
			return true;
		}
		ICoreCommandExecutor service = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
		PerfFunctions.SpinWait(200);
		service.ExecuteControllerCommand(unknownCommand);
		PerfFunctions.SpinWait(100);
		service.ExecuteControllerCommand(CommandType.VendDoorKillCommand);
		PerfFunctions.SpinWait(100);
		CoreResponse coreResponse = service.ExecuteControllerCommand(doorCommand, 5000);
		if (coreResponse.CommError)
		{
			return false;
		}
		PerfFunctions.SpinWait(100);
		return !coreResponse.TimedOut;
	}
}
