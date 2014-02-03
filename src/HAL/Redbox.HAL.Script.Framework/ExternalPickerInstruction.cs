using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class ExternalPickerInstruction : Instruction
{
	private enum Operation
	{
		None,
		PushOut,
		Accept,
		KioskPush,
		KioskAccept
	}

	public override string Mnemonic => "EXTPICKER";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count == 1)
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
		else
		{
			result.Errors.Add(Error.NewError("E001", "Invalid operands.", "EXTPICKER expects an operand specifying the operation."));
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		Token token = base.Operands[0];
		Operation operation;
		try
		{
			operation = Enum<Operation>.ParseIgnoringCase(token.Value, Operation.None);
		}
		catch (Exception)
		{
			result.Errors.Add(Error.NewError("E001", "Unrecognized operation operand.", $"The option {token.Value} is not a valid operation."));
			return;
		}
		switch (operation)
		{
		case Operation.None:
			result.Errors.Add(Error.NewError("E001", "Unrecognized operation option.", $"The option {token.Value} is not a valid operation."));
			break;
		case Operation.PushOut:
			try
			{
				Gripper.Instance.SetFinger(GripperFingerState.Rent);
				PickerBoard.Instance.RollerToPosition(RollerPosition.Position2, 3000);
				Track.Instance.Cycle();
				ControllerService.Instance.ClearDiskFromPicker(RollerDirection.In);
				int pushTime2 = ControllerConfiguration.Instance.PushTime;
				Track.Instance.Open();
				Gripper instance2 = Gripper.Instance;
				instance2.SetFinger(GripperFingerState.Closed);
				instance2.TimedExtend(pushTime2);
				instance2.SetFinger(GripperFingerState.Rent);
				instance2.RetractArm();
				instance2.SetFinger(GripperFingerState.Closed);
				instance2.TimedExtend(pushTime2);
				instance2.SetFinger(GripperFingerState.Rent);
				instance2.RetractArm();
				ExecutionEngine.Instance.ActiveContext.PushTop((!PickerBoard.Instance.IsFull()) ? "OK" : "FAIL");
				break;
			}
			finally
			{
				PickerBoard.Instance.StopRoller();
			}
		case Operation.KioskPush:
			PickerBoard.Instance.RollerToPosition(RollerPosition.Position4, 3000);
			VendDoor.Instance.Rent();
			ControllerService.Instance.GripperPushOut();
			Track.Instance.Open();
			ExecutionEngine.Instance.ActiveContext.PushTop("OK");
			break;
		case Operation.KioskAccept:
			VendDoor.Instance.Rent();
			Track.Instance.Close();
			ExecutionEngine.Instance.ActiveContext.PushTop(ControllerService.Instance.TakeDiskAtDoor().ToString().ToUpper());
			break;
		default:
			try
			{
				int pushTime = ControllerConfiguration.Instance.PushTime;
				Gripper instance = Gripper.Instance;
				instance.SetFinger(GripperFingerState.Rent);
				PickerBoard.Instance.StartRollerOut();
				Track.Instance.Open();
				instance.TimedExtend(pushTime);
				instance.SetFinger(GripperFingerState.Closed);
				instance.RetractArm();
				instance.SetFinger(GripperFingerState.Rent);
				instance.TimedExtend(pushTime);
				instance.SetFinger(GripperFingerState.Closed);
				instance.RetractArm();
				instance.SetFinger(GripperFingerState.Rent);
				Track.Instance.Close();
				PickerBoard.Instance.RollerToPosition(RollerPosition.Position4, 4000);
				ExecutionEngine.Instance.ActiveContext.PushTop(PickerBoard.Instance.IsFull() ? "OK" : "FAIL");
				break;
			}
			finally
			{
				PickerBoard.Instance.StopRoller();
			}
		}
	}

	private bool Cleared(PickerSensorReadResult res)
	{
		if (res.ReadError)
		{
			return false;
		}
		if (!res.Sensors[1] && !res.Sensors[2] && !res.Sensors[3])
		{
			return !res.Sensors[4];
		}
		return false;
	}
}
