using System;
using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Common.GUI.Functions.Services;

public sealed class ClientControlSystem : IControlSystem
{
	private readonly HardwareService Service;

	private readonly byte[] ReadImmediateInstruction;

	public VendDoorState VendDoorState
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public TrackState TrackState
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IControlResponse Initialize()
	{
		return From("SERIALBOARD RESET");
	}

	public bool Shutdown()
	{
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		return instructionHelper.ExecuteGeneric("SERIALBOARD CLOSEPORT").Equals("SUCCESS", StringComparison.CurrentCultureIgnoreCase);
	}

	public IControlResponse SetAudio(AudioChannelState newState)
	{
		return From($"AUDIO {newState.ToString()}");
	}

	public IControlResponse ToggleRingLight(bool on, int? sleepAfter)
	{
		return From(string.Format("CONTROLSYSTEM RINGLIGHT{0}", on ? "ON" : "OFF"));
	}

	public IControlResponse VendDoorRent()
	{
		return From("VENDDOOR RENT");
	}

	public IControlResponse VendDoorClose()
	{
		return From("VENDDOOR CLOSE");
	}

	public VendDoorState ReadVendDoorPosition()
	{
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		string value = instructionHelper.ExecuteGeneric("VENDDOOR STATUS");
		return (!string.IsNullOrEmpty(value)) ? Enum<VendDoorState>.ParseIgnoringCase(value, VendDoorState.Unknown) : VendDoorState.Unknown;
	}

	public IControlResponse TrackOpen()
	{
		return From("TRACK OPEN");
	}

	public IControlResponse TrackClose()
	{
		return From("TRACK CLOSE");
	}

	public IControlResponse TrackCycle()
	{
		return From("TRACK CYCLE");
	}

	public void TimedExtend()
	{
	}

	public void TimedExtend(int timeout)
	{
	}

	public IControlResponse ExtendArm()
	{
		return From("GRIPPER EXTEND");
	}

	public IControlResponse ExtendArm(int timeout)
	{
		throw new NotImplementedException();
	}

	public IControlResponse RetractArm()
	{
		return From("GRIPPER RETRACT");
	}

	public IControlResponse SetFinger(GripperFingerState state)
	{
		string instruction = string.Format("GRIPPER {0}", (GripperFingerState.Closed == state) ? "CLOSE" : state.ToString());
		return From(instruction);
	}

	public ErrorCodes Center(CenterDiskMethod method)
	{
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		return instructionHelper.ExecuteErrorCode("CONTROLSYSTEM CENTER");
	}

	public string GetBoardVersion(ControlBoards board)
	{
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		return instructionHelper.ExecuteGeneric($"VERSION {board.ToString().ToUpper()}");
	}

	public string[] GetBoardVersions()
	{
		throw new NotImplementedException();
	}

	public string[] ReadInputs(ControlBoards board)
	{
		string text = string.Empty;
		if (ControlBoards.Aux == board)
		{
			text = "SENSOR READ-AUX-INPUTS";
		}
		else if (ControlBoards.Picker == board)
		{
			text = "SENSOR READ-PICKER-INPUTS";
		}
		HardwareJob val = default(HardwareJob);
		if (!((ClientCommandResult)(object)Service.ExecuteImmediate(text, ref val)).Success)
		{
			return new string[0];
		}
		string topOfStack = HardwareJobExtensions.GetTopOfStack(val);
		int result = 0;
		if (!int.TryParse(topOfStack, out result) || result == 0)
		{
			return new string[0];
		}
		string[] stackEntries = HardwareJobExtensions.GetStackEntries(val, result + 1);
		string[] array = new string[result];
		Array.Copy(stackEntries, 1, array, 0, result);
		return array;
	}

	public void LogPickerSensorState()
	{
	}

	public void LogPickerSensorState(LogEntryType type)
	{
	}

	public void LogInputs(ControlBoards board)
	{
	}

	public void LogInputs(ControlBoards board, LogEntryType type)
	{
	}

	public IControlResponse SetSensors(bool on)
	{
		return From(string.Format("CONTROLSYSTEM PICKERSENSORS{0}", on ? "ON" : "OFF"));
	}

	public IPickerSensorReadResult ReadSensors()
	{
		throw new NotImplementedException();
	}

	public IPickerSensorReadResult ReadImmediateSensors()
	{
		string text = "SENSOR READ PICKER-SENSOR=1..6";
		HardwareJob val = default(HardwareJob);
		if (!((ClientCommandResult)(object)Service.ExecuteImmediate(text, ref val)).Success)
		{
			return new ClientPickerSensorReadResult();
		}
		Stack<string> stack = default(Stack<string>);
		if (!((ClientCommandResult)(object)val.GetStack(ref stack)).Success || stack.Count == 0)
		{
			return new ClientPickerSensorReadResult();
		}
		if (int.Parse(stack.Pop()) >= 6)
		{
			return new ClientPickerSensorReadResult(stack);
		}
		return new ClientPickerSensorReadResult();
	}

	public IControlResponse StopRoller()
	{
		return From("ROLLER STOP");
	}

	public IControlResponse StartRollerIn()
	{
		return From("ROLLER IN");
	}

	public IControlResponse StartRollerOut()
	{
		return From("ROLLER OUT");
	}

	public IControlResponse RollerToPosition(RollerPosition position)
	{
		return RollerToPosition(position, 5000);
	}

	public IControlResponse RollerToPosition(RollerPosition position, int opTimeout)
	{
		return RollerToPosition(position, opTimeout, logSensors: false);
	}

	public IControlResponse RollerToPosition(RollerPosition position, int opTimeout, bool logSensors)
	{
		int num = (int)position;
		return From($"ROLLER POS={num.ToString()} TIMEOUT={opTimeout} WAIT=TRUE");
	}

	public QlmStatus GetQlmStatus()
	{
		throw new NotImplementedException();
	}

	public ErrorCodes EngageQlm(IFormattedLog log)
	{
		return EngageQlm(home: true, log);
	}

	public ErrorCodes EngageQlm(bool home, IFormattedLog log)
	{
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		return instructionHelper.ExecuteErrorCode("QLM ENGAGE");
	}

	public ErrorCodes DisengageQlm(IFormattedLog log)
	{
		return DisengageQlm(home: true, log);
	}

	public ErrorCodes DisengageQlm(bool home, IFormattedLog log)
	{
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		return instructionHelper.ExecuteErrorCode("QLM DISENGAGE");
	}

	public IControlResponse DropQlm()
	{
		return From("CONTROLSYSTEM DROPQLM");
	}

	public IControlResponse LiftQlm()
	{
		return From("CONTROLSYSTEM LIFTQLM");
	}

	public IControlResponse HaltQlmOperation()
	{
		return From("CONTROLSYSTEM HALTQLMOPERATION");
	}

	public ClientControlSystem(HardwareService s)
	{
		Service = s;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("CLEAR");
		stringBuilder.AppendLine(" SENSOR READ PICKER-SENSOR=1..6");
		ReadImmediateInstruction = Encoding.ASCII.GetBytes(stringBuilder.ToString());
	}

	private IControlResponse From(string instruction)
	{
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		return instructionHelper.ExecuteWithResponse(instruction);
	}
}
