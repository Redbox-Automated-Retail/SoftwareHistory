using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class SensorInstruction : Instruction
{
	private static class KeyNames
	{
		public const string AuxillarySensor = "AUX-SENSOR";

		public const string PickerSensor = "PICKER-SENSOR";
	}

	private static class Commands
	{
		public const string PickerOn = "PICKER-ON";

		public const string PickerOff = "PICKER-OFF";

		public const string Read = "READ";

		public const string ReadAuxInputs = "READ-AUX-INPUTS";

		public const string ReadPickerInputs = "READ-PICKER-INPUTS";
	}

	public override string Mnemonic => "SENSOR";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens[0].Type != TokenType.Symbol || !IsOperandRecognized(operandTokens[0].Value))
		{
			result.AddInvalidOperandError("Expected a symbol: PICKER-ON, PICKER-OFF, READ-PICKER, READ-AUX-INPUTS, READ-PICKER-INPUTS");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Token token = base.Operands[0];
		if (string.Compare(token.Value, "PICKER-ON", ignoreCase: true) == 0)
		{
			context.PushTop(PickerBoard.SetSensors(on: true));
		}
		else if (string.Compare(token.Value, "PICKER-OFF", ignoreCase: true) == 0)
		{
			context.PushTop(PickerBoard.SetSensors(on: false));
		}
		else if (string.Compare(token.Value, "READ-AUX-INPUTS", ignoreCase: true) == 0)
		{
			string[] array = ControlSystem.Instance.ReadInputs(ControlBoards.Aux);
			int num = 1;
			string[] array2 = array;
			foreach (string arg in array2)
			{
				context.PushTop($"AUX-QLM I/O {num++} = {arg}");
			}
			context.PushTop(array.Length);
		}
		else if (string.Compare(token.Value, "READ-PICKER-INPUTS", ignoreCase: true) == 0)
		{
			string[] array3 = ControlSystem.Instance.ReadInputs(ControlBoards.Picker);
			int num2 = 1;
			string[] array4 = array3;
			foreach (string arg2 in array4)
			{
				context.PushTop($"PICKER CONTROLLER I/O {num2++} = {arg2}");
			}
			context.PushTop(array3.Length);
		}
		else
		{
			if (string.Compare(token.Value, "READ", ignoreCase: true) != 0)
			{
				return;
			}
			PickerSensorReadResult pickerSensorReadResult = PickerBoard.ImmediateReadSensors();
			if (pickerSensorReadResult.ReadError)
			{
				context.PushTop(0);
				return;
			}
			int num3 = 1;
			bool[] sensors = pickerSensorReadResult.Sensors;
			foreach (bool flag in sensors)
			{
				string instance = string.Format("SENSOR{0}-{1}", num3++, flag ? "BLOCKED" : "CLEAR");
				context.PushTop(instance);
			}
			context.PushTop(pickerSensorReadResult.Sensors.Length);
		}
	}

	protected override bool IsOperandRecognized(string op)
	{
		return OperandIsValid(op);
	}

	private bool OperandIsValid(string operand)
	{
		if (string.Compare(operand, "PICKER-ON", ignoreCase: true) != 0 && string.Compare(operand, "PICKER-OFF", ignoreCase: true) != 0 && string.Compare(operand, "READ", ignoreCase: true) != 0 && string.Compare(operand, "READ-PICKER-INPUTS", ignoreCase: true) != 0)
		{
			return string.Compare(operand, "READ-AUX-INPUTS", ignoreCase: true) == 0;
		}
		return true;
	}
}
