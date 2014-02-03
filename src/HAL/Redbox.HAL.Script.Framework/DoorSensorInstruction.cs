using System;
using System.IO;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class DoorSensorInstruction : Instruction
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

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count <= 0)
		{
			result.Errors.Add(Error.NewError("C002", "Expected a 'Query' or 'IsConfigured' operand.", "Correct the statement and recompile."));
		}
		else if (IsValidOperand(tokensAfterMnemonic[0].Value))
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
		else
		{
			result.Errors.Add(Error.NewError("C002", "Expected a 'Query' or 'IsConfigured' operand.", "Correct the statement and recompile."));
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		switch (ConvertToOperand(base.Operands[0].Value))
		{
		case ExpectedOperand.None:
			ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Info, "DoorSensor instruction: converted '{0}' to operand None", base.Operands[0].Value);
			break;
		case ExpectedOperand.IsConfigured:
			ExecutionEngine.Instance.ActiveContext.PushTop(DoorSensors.Instance.SensorsEnabled);
			break;
		case ExpectedOperand.SoftwareOverride:
		{
			if (!GetKeyValuePairValue<string>("OVERRIDE", out var val, result.Errors))
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
			ExecutionEngine.Instance.ActiveContext.PushTop(DoorSensors.Instance.SensorsEnabled);
			break;
		}
		case ExpectedOperand.Query:
		{
			string[] array = DoorSensors.Instance.QueryStateForDisplay();
			string[] array2 = array;
			foreach (string instance in array2)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(instance);
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(array.Length);
			break;
		}
		case ExpectedOperand.Status:
			ExecutionEngine.Instance.ActiveContext.PushTop(DoorSensors.Instance.Query().ToString().ToUpper());
			break;
		}
	}

	private bool IsValidOperand(string operand)
	{
		return ConvertToOperand(operand) != ExpectedOperand.None;
	}

	private ExpectedOperand ConvertToOperand(string operand)
	{
		ExpectedOperand expectedOperand = ExpectedOperand.None;
		try
		{
			return (ExpectedOperand)Enum.Parse(typeof(ExpectedOperand), operand, ignoreCase: true);
		}
		catch (ArgumentException)
		{
			return ExpectedOperand.None;
		}
	}
}
