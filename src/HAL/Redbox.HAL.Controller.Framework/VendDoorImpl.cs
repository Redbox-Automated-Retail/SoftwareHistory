using Redbox.HAL.Component.Model;

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
		CoreResponse coreResponse = ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(CommandType.AuxSensorsRead);
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
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
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
		ICoreCommandExecutor service2 = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
		for (int i = 0; i < num; i++)
		{
			coreResponse = service2.ExecuteControllerCommand(commandType, 5500);
			if (coreResponse.CommError)
			{
				LogHelper.Instance.Log(LogEntryType.Error, "Communication error during vend door {0}", state.ToString().ToUpper());
				return coreResponse;
			}
			if (coreResponse.TimedOut)
			{
				if (VendDoorState.Closed == state)
				{
					service.Wait(70);
					service2.ExecuteControllerCommand(CommandType.UnknownVendDoorCloseCommand);
					service.Wait(70);
					service2.ExecuteControllerCommand(CommandType.VendDoorKillCommand);
					service.Wait(50);
				}
			}
			else
			{
				if (state != VendDoorState.Closed)
				{
					return coreResponse;
				}
				if (VendDoorIsInState(state) || MoveToVendSensor(state, commandType, unknownCommand, service))
				{
					return coreResponse;
				}
			}
		}
		LogHelper.Instance.Log(LogEntryType.Error, "SetVendDoor {0} timed out after {1} tries.", state.ToString(), num);
		ServiceLocator.Instance.GetService<IControlSystem>().LogInputs(ControlBoards.Aux, LogEntryType.Error);
		return coreResponse;
	}

	private bool MoveToVendSensor(VendDoorState state, CommandType doorCommand, CommandType unknownCommand, IRuntimeService rts)
	{
		if (!ControllerConfiguration.Instance.MoveVendDoorToAuxSensor || VendDoorState.Rent == state)
		{
			return true;
		}
		ICoreCommandExecutor service = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
		rts.SpinWait(200);
		service.ExecuteControllerCommand(unknownCommand);
		rts.SpinWait(100);
		service.ExecuteControllerCommand(CommandType.VendDoorKillCommand);
		rts.SpinWait(100);
		CoreResponse coreResponse = service.ExecuteControllerCommand(doorCommand, 5000);
		if (coreResponse.CommError)
		{
			return false;
		}
		rts.SpinWait(100);
		return !coreResponse.TimedOut;
	}
}
