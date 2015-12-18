using System;
using System.IO;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class DoorSensorInstruction : Instruction
{
	private enum ExpectedOperand
	{
		None,
		IsConfigured,
		Query,
		Status,
		SoftwareOverride
	}

	public override string Mnemonic => "DOORSENSORS";

	protected override bool IsOperandRecognized(string operand)
	{
		return IsValidOperand(operand);
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (!IsValidOperand(operandTokens[0].Value))
		{
			result.AddInvalidOperandError("Expected a 'Query' or 'IsConfigured' operand.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		switch (Enum<ExpectedOperand>.ParseIgnoringCase(base.Operands[0].Value, ExpectedOperand.None))
		{
		case ExpectedOperand.None:
			context.ServiceLog("DoorSensor instruction: converted '{0}' to operand None", base.Operands[0].Value);
			break;
		case ExpectedOperand.IsConfigured:
			context.PushTop(DoorSensors.Instance.SensorsEnabled);
			break;
		case ExpectedOperand.SoftwareOverride:
		{
			if (!GetKeyValuePairValue<string>("OVERRIDE", out var val, result.Errors, context))
			{
				val = "off";
			}
			bool flag = val.Equals("off", StringComparison.CurrentCultureIgnoreCase);
			DoorSensors.Instance.SoftwareOverride = flag;
			string path = Path.Combine(ControllerConfiguration.Instance.KioskLogsPath, "Service\\DoorSensorConfiguration.log");
			using (StreamWriter streamWriter = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read)))
			{
				streamWriter.WriteLine(flag ? $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}: Door sensors have a software override in place: they are not active - *UNCHECKED MOVEMENT IS POSSIBLE*." : $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}: Door sensors do not have a software override; they are active.");
			}
			context.PushTop(DoorSensors.Instance.SensorsEnabled);
			break;
		}
		case ExpectedOperand.Query:
			switch (DoorSensors.Instance.QueryStateForDisplay())
			{
			case DoorSensorResult.AuxReadError:
				context.PushTop("AUX board not responsive.");
				break;
			case DoorSensorResult.FrontDoor:
				context.PushTop("FRONT DOOR OPEN");
				break;
			case DoorSensorResult.Ok:
				context.PushTop("FRONT DOOR CLOSED");
				break;
			case DoorSensorResult.SoftwareOverride:
				context.PushTop("OVERRIDE IN PLACE.");
				break;
			}
			context.PushTop(1);
			break;
		case ExpectedOperand.Status:
			context.PushTop(DoorSensors.Instance.Query().ToString().ToUpper());
			break;
		}
	}

	private bool IsValidOperand(string operand)
	{
		return Enum<ExpectedOperand>.ParseIgnoringCase(operand, ExpectedOperand.None) != ExpectedOperand.None;
	}
}
