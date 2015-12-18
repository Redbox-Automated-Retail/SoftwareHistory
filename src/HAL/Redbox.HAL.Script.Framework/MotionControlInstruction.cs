using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class MotionControlInstruction : Instruction
{
	private enum Operand
	{
		None,
		Readlimits,
		Comstatus,
		Initstatus,
		Positions,
		XPos,
		YPos,
		Homeall,
		Dumpstats,
		Clearstats
	}

	public override string Mnemonic => "MOTIONCONTROL";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count < 1 || ConvertToOperand(operandTokens[0].Value) == Operand.None)
		{
			result.AddError(ExecutionErrors.MissingOperand, "MOTIONCONTROL instruction expects an operand.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Operand operand = ConvertToOperand(base.Operands[0].Value);
		MotionController controller = MotionController.GetController();
		switch (operand)
		{
		case Operand.Readlimits:
		{
			string[] array = controller.ReadLimits();
			string[] array2 = array;
			foreach (string instance in array2)
			{
				context.PushTop(instance);
			}
			context.PushTop(array.Length);
			break;
		}
		case Operand.Comstatus:
			context.PushTop(controller.CommunicationOk() ? "OK" : "COMM ERROR");
			break;
		case Operand.Initstatus:
			context.PushTop(controller.IsInitialized() ? "OK" : "ERROR");
			break;
		case Operand.Positions:
		{
			string motionControllerPosition2 = ControllerService.Instance.GetMotionControllerPosition(Axis.X);
			string motionControllerPosition3 = ControllerService.Instance.GetMotionControllerPosition(Axis.Y);
			context.PushTop($"X = {motionControllerPosition2} Y = {motionControllerPosition3}");
			break;
		}
		case Operand.XPos:
		{
			string motionControllerPosition = ControllerService.Instance.GetMotionControllerPosition(Axis.X);
			context.PushTop(motionControllerPosition);
			break;
		}
		case Operand.YPos:
		{
			string motionControllerPosition4 = ControllerService.Instance.GetMotionControllerPosition(Axis.Y);
			context.PushTop(motionControllerPosition4);
			break;
		}
		case Operand.Homeall:
			if (!controller.CommunicationOk())
			{
				bool flag = MotionController.Reset(context.AppLog, quick: true);
				LogHelper.Instance.Log("The motion controller is not responsive; driver reset returned {0}.", flag ? "SUCCESS" : "FAILURE");
				context.PushTop(flag);
			}
			else
			{
				context.PushTop(controller.InitAxes(context.AppLog, quick: false).Succeded);
			}
			break;
		case Operand.Dumpstats:
			HardwareCorrectionStats.DumpStats(CorrectionType.Arcus, "ArcusCorrectionStats.txt");
			break;
		case Operand.Clearstats:
			HardwareCorrectionStats.RemoveStats(CorrectionType.Arcus);
			break;
		}
	}

	private Operand ConvertToOperand(string operand)
	{
		return Enum<Operand>.ParseIgnoringCase(operand, Operand.None);
	}
}
