using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class SensorInstruction : Instruction
{
	protected static class KeyNames
	{
		public const string AuxillarySensor = "AUX-SENSOR";

		public const string PickerSensor = "PICKER-SENSOR";
	}

	protected static class Commands
	{
		public const string PickerOn = "PICKER-ON";

		public const string PickerOff = "PICKER-OFF";

		public const string Read = "READ";

		public const string ReadAuxInputs = "READ-AUX-INPUTS";

		public const string ReadPickerInputs = "READ-PICKER-INPUTS";

		public const string WaitClear = "WAIT-CLEAR";

		public const string WaitBlock = "WAIT-BLOCK";
	}

	public override string Mnemonic => "SENSOR";

	private static AuxBoard AuxBoard => AuxBoard.Instance;

	private static PickerBoard PickerBoard => PickerBoard.Instance;

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count >= 1)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.Symbol || !IsOperandRecognized(tokensAfterMnemonic[0].Value))
			{
				AddError("C002", "Expected a symbol: PICKER-ON, PICKER-OFF, LOG-STATE, READ, WAIT-CLEAR, WAIT-BLOCK; one of: PICKER-SENSOR=x[..y] or AUX-SENSOR=x[..y]; optionally, TIMEOUT=m", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
		else
		{
			AddError("C002", "Expected a symbol: PICKER-ON, PICKER-OFF, READ, READ-ALL-AUX, READ-ALL-PICKER, WAIT-CLEAR, WAIT-BLOCK; one of: PICKER-SENSOR=x[..y] or AUX-SENSOR=x[..y]; optionally, TIMEOUT=m", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		Token token = base.Operands[0];
		GetTimeOutValue(result.Errors);
		if (string.Compare(token.Value, "PICKER-ON", ignoreCase: true) == 0)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop(PickerBoard.SetSensors(on: true));
			return;
		}
		if (string.Compare(token.Value, "PICKER-OFF", ignoreCase: true) == 0)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop(PickerBoard.SetSensors(on: false));
			return;
		}
		if (string.Compare(token.Value, "READ-AUX-INPUTS", ignoreCase: true) == 0)
		{
			string[] array = AuxBoard.ImmediateReadInputs();
			int num = 1;
			string[] array2 = array;
			foreach (string arg in array2)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop($"AUX-QLM I/O {num++} = {arg}");
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(array.Length);
			return;
		}
		if (string.Compare(token.Value, "READ-PICKER-INPUTS", ignoreCase: true) == 0)
		{
			string[] array3 = PickerBoard.Instance.ReadInputs();
			int num2 = 1;
			string[] array4 = array3;
			foreach (string arg2 in array4)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop($"PICKER CONTROLLER I/O {num2++} = {arg2}");
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(array3.Length);
			return;
		}
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("AUX-SENSOR");
		KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair("PICKER-SENSOR");
		if (keyValuePair != null && keyValuePair2 != null)
		{
			AddError("E004", "Only one of: PICKER-SENSOR=x[..y] or AUX-SENSOR=x[..y] may be used per SENSOR instruction.", result.Errors);
			return;
		}
		if (keyValuePair == null && keyValuePair2 == null)
		{
			AddError("E004", "At least one of: PICKER-SENSOR=x[..y] or AUX-SENSOR=x[..y] must be used per SENSOR instruction.", result.Errors);
			return;
		}
		if (keyValuePair != null)
		{
			Range sensorRange = GetSensorRange(keyValuePair);
			if (string.Compare(token.Value, "READ", ignoreCase: true) != 0)
			{
				return;
			}
			foreach (int item in sensorRange.GetNextInRnage())
			{
				bool flag = AuxBoard.Instance.IsSensorBlocked((AuxSensor)item);
				string instance = string.Format("{0}-{1}", ((AuxSensor)item).ToString().ToUpper(), flag ? "BLOCKED" : "CLEAR");
				ExecutionEngine.Instance.ActiveContext.PushTop(instance);
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(sensorRange.Size);
			return;
		}
		Range sensorRange2 = GetSensorRange(keyValuePair2);
		if (string.Compare(token.Value, "READ", ignoreCase: true) == 0)
		{
			bool[] array5 = null;
			PickerSensorReadResult pickerSensorReadResult = PickerBoard.Instance.ImmediateReadSensors();
			if (pickerSensorReadResult.ReadError)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(0);
				return;
			}
			array5 = pickerSensorReadResult.Sensors;
			int num3 = 0;
			foreach (int item2 in sensorRange2.GetNextInRnage())
			{
				string instance2 = string.Format("{0}-{1}", ((PickerSensor)item2).ToString().ToUpper(), array5[num3++] ? "BLOCKED" : "CLEAR");
				ExecutionEngine.Instance.ActiveContext.PushTop(instance2);
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(sensorRange2.Size);
		}
		else if (string.Compare(token.Value, "WAIT-CLEAR", ignoreCase: true) == 0)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop("TIMEOUT");
		}
		else if (string.Compare(token.Value, "WAIT-BLOCK", ignoreCase: true) == 0)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop("TIMEOUT");
		}
	}

	protected override bool IsOperandRecognized(string op)
	{
		return OperandIsValid(op);
	}

	private bool OperandIsValid(string operand)
	{
		if (string.Compare(operand, "PICKER-ON", ignoreCase: true) != 0 && string.Compare(operand, "PICKER-OFF", ignoreCase: true) != 0 && string.Compare(operand, "READ", ignoreCase: true) != 0 && string.Compare(operand, "READ-PICKER-INPUTS", ignoreCase: true) != 0 && string.Compare(operand, "READ-AUX-INPUTS", ignoreCase: true) != 0 && string.Compare(operand, "WAIT-CLEAR", ignoreCase: true) != 0)
		{
			return string.Compare(operand, "WAIT-BLOCK", ignoreCase: true) == 0;
		}
		return true;
	}

	private static Range GetSensorRange(KeyValuePair sensorValue)
	{
		if (sensorValue.Value.IndexOf("..") != -1)
		{
			return new Range(sensorValue.Value);
		}
		int num = int.Parse(sensorValue.Value);
		return new Range(num, num);
	}
}
