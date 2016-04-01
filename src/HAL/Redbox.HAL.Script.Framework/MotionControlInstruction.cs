using System;
using Redbox.HAL.Component.Model;
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
		Homeall
	}

	public override string Mnemonic => "MOTIONCONTROL";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		Operand operand = Enum<Operand>.ParseIgnoringCase(operandTokens[0].Value, Operand.None);
		if (operandTokens.Count < 1 || operand == Operand.None)
		{
			result.AddError(ExecutionErrors.MissingOperand, "MOTIONCONTROL instruction expects an operand.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Operand operand = Enum<Operand>.ParseIgnoringCase(base.Operands[0].Value, Operand.None);
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		switch (operand)
		{
		case Operand.Readlimits:
		{
			string[] array = service.ReadLimits();
			string[] array2 = array;
			foreach (string instance in array2)
			{
				context.PushTop(instance);
			}
			context.PushTop(array.Length);
			break;
		}
		case Operand.Comstatus:
			context.PushTop(service.CommunicationOk() ? "OK" : "COMM ERROR");
			break;
		case Operand.Initstatus:
			context.PushTop(service.IsInitialized ? "OK" : "ERROR");
			break;
		case Operand.Positions:
		{
			IControllerPosition controllerPosition = service.ReadPositions();
			if (!controllerPosition.ReadOk)
			{
				context.PushTop("MOTIONCONTROL ERROR");
			}
			else
			{
				context.PushTop($"X = {controllerPosition.XCoordinate.Value} Y = {controllerPosition.YCoordinate.Value}");
			}
			break;
		}
		case Operand.XPos:
		{
			IControllerPosition controllerPosition = service.ReadPositions();
			if (!controllerPosition.ReadOk)
			{
				context.PushTop("MOTIONCONTROL ERROR");
			}
			else
			{
				context.PushTop(controllerPosition.XCoordinate.Value);
			}
			break;
		}
		case Operand.YPos:
		{
			IControllerPosition controllerPosition = service.ReadPositions();
			if (!controllerPosition.ReadOk)
			{
				context.PushTop("MOTIONCONTROL ERROR");
			}
			else
			{
				context.PushTop(controllerPosition.YCoordinate.Value);
			}
			break;
		}
		case Operand.Homeall:
			if (!service.CommunicationOk())
			{
				bool flag = service.Reset(context.AppLog, quick: true);
				LogHelper.Instance.Log("The motion controller is not responsive; driver reset returned {0}.", flag ? "SUCCESS" : "FAILURE");
				context.PushTop(flag);
			}
			else
			{
				ErrorCodes errorCodes = service.InitAxes(context.AppLog);
				context.PushTop(ErrorCodes.Success == errorCodes);
			}
			break;
		}
	}
}
