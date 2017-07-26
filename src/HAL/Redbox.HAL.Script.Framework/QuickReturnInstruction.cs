using System;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class QuickReturnInstruction : Instruction
{
	internal enum Forms
	{
		Unknown,
		DefState,
		State,
		LedState,
		Reset,
		CheckAttButton,
		ClearAttButton,
		LogReturn,
		GetConfig,
		GetCurrentPage,
		DisableUI,
		UpdateEEPROM,
		HaltPoll,
		PollStatus,
		Help,
		Restart,
		IsInstalled,
		DumpCounters,
		ClosePort,
		DumpStateTable,
		FindDevice
	}

	internal enum Attributes
	{
		Unknown,
		File,
		Text,
		Mode,
		Name,
		Led,
		LedStates,
		Displayable
	}

	private static string m_statsDirectory;

	public override string Mnemonic => "QUICKRETURN";

	private static string StatisticsDirectory
	{
		get
		{
			if (!string.IsNullOrEmpty(m_statsDirectory))
			{
				return m_statsDirectory;
			}
			m_statsDirectory = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().CreateSubpath("Return");
			return m_statsDirectory;
		}
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		Forms forms = CheckForm(operandTokens[0].Value);
		if (forms == Forms.Unknown)
		{
			result.AddInvalidOperandError($"Unknown form of QuickReturn: {operandTokens[0].Value}");
			return;
		}
		string text = string.Empty;
		switch (forms)
		{
		case Forms.State:
			if (operandTokens.Count < 2 || operandTokens.GetKeyValuePair(Attributes.Name.ToString().ToUpper()) == null)
			{
				text = $"{Forms.State.ToString().ToUpper()} form requires a {Attributes.Name.ToString()} operand.";
			}
			break;
		case Forms.LedState:
		{
			if (operandTokens.Count < 2)
			{
				text = $"{Forms.LedState.ToString().ToUpper()} form requires a {Attributes.Mode.ToString()} operand and {Attributes.Led.ToString()} operand.";
				break;
			}
			bool flag = false;
			bool flag2 = false;
			if (operandTokens.GetKeyValuePair(Attributes.Mode.ToString().ToUpper()) != null)
			{
				flag = true;
			}
			if (operandTokens.GetKeyValuePair(Attributes.Led.ToString().ToUpper()) != null)
			{
				flag2 = true;
			}
			if (!flag2 || !flag)
			{
				text = "Expected both LED and MODE key-value pair operands.";
			}
			break;
		}
		case Forms.DefState:
			if (operandTokens.Count < 2 || operandTokens.GetKeyValuePair(Attributes.Name.ToString().ToUpper()) == null)
			{
				text = string.Format("{0} form requires at least a {0} operand and {1} operand (optional {2} operand).", Forms.DefState.ToString(), Attributes.Name.ToString(), Attributes.File.ToString(), Attributes.Text.ToString());
			}
			break;
		case Forms.GetConfig:
			if (operandTokens.Count < 2)
			{
				text = $"{forms.ToString().ToUpper()} form requires a configuration option operand.";
			}
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			result.AddInvalidOperandError(text);
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		if (base.Operands.Count == 0)
		{
			AddError("E002", "There are no operands for this instruction.", result.Errors);
			return;
		}
		switch (CheckForm(base.Operands[0].Value))
		{
		case Forms.Unknown:
			AddError("E002", "There is an unknown form of this instruction.", result.Errors);
			break;
		case Forms.DumpStateTable:
			QuickReturnDevice.Instance.DumpStateTable();
			break;
		case Forms.IsInstalled:
			context.PushTop(QuickReturnDevice.Instance.IsConfigured.ToString().ToUpper());
			break;
		case Forms.Restart:
			if (QuickReturnPoller.Instance.Running)
			{
				context.PushTop("SUCCESS");
				break;
			}
			QuickReturnPoller.Instance.Start();
			context.PushTop(QuickReturnPoller.Instance.Running ? "SUCCESS" : "FAILURE");
			break;
		case Forms.Help:
			context.PushTop("NOT IMPLEMENTED");
			break;
		case Forms.PollStatus:
			context.PushTop(QuickReturnPoller.Instance.Running ? "ACTIVE" : "INACTIVE");
			break;
		case Forms.DisableUI:
		case Forms.HaltPoll:
			QuickReturnPoller.Instance.Shutdown();
			context.PushTop("SUCCESS");
			break;
		case Forms.GetConfig:
			_ = base.Operands[1].Value;
			AddError("E777", "The 'GetConfig' Operand is not supported.", result.Errors);
			break;
		case Forms.LogReturn:
			LogHelper.Instance.Log($"Return via QuickReturn terminal on: {DateTime.Now}", LogEntryType.Info);
			break;
		case Forms.CheckAttButton:
		{
			bool flag5 = QuickReturnDevice.Instance.CheckAttButton(out var commError);
			if (commError)
			{
				context.PushTop("FAILURE");
				break;
			}
			context.PushTop(flag5 ? "SET" : "CLEAR");
			context.PushTop("SUCCESS");
			break;
		}
		case Forms.ClearAttButton:
			context.PushTop(QuickReturnDevice.Instance.ClearAttentionButtonStatus() ? "SUCCESS" : "FAILURE");
			break;
		case Forms.Reset:
		{
			bool flag4 = QuickReturnDevice.Instance.Reset();
			context.PushTop(flag4);
			break;
		}
		case Forms.State:
		{
			string keyValuePairValue4 = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair(Attributes.Name.ToString().ToUpper()), result.Errors, context);
			if (keyValuePairValue4 == null || !QuickReturnDevice.Instance.DisplayState(keyValuePairValue4))
			{
				AddError("E002", "Unable to set state: maybe state is not defined?", result.Errors);
				QuickReturnPoller.Instance.Shutdown();
			}
			else
			{
				context.PushTop("SUCCESS");
			}
			break;
		}
		case Forms.DefState:
		{
			string keyValuePairValue = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair(Attributes.Name.ToString().ToUpper()), result.Errors, context);
			string keyValuePairValue2 = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair(Attributes.File.ToString().ToUpper()), result.Errors, context);
			string keyValuePairValue3 = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair(Attributes.LedStates.ToString().ToUpper()), result.Errors, context);
			bool flag3 = QuickReturnDevice.Instance.CreateState(keyValuePairValue, keyValuePairValue2, keyValuePairValue3);
			context.PushTop(flag3);
			break;
		}
		case Forms.LedState:
		{
			bool flag = false;
			try
			{
				QuickReturnDevice.LED lED = GetLED(result, context);
				if (lED == QuickReturnDevice.LED.Unknown)
				{
					AddError("E001", "Invalid LED key-value pair.", result.Errors);
					break;
				}
				QuickReturnDevice.LEDMode mode = GetMode(result, context);
				if (mode == QuickReturnDevice.LEDMode.Unknown)
				{
					AddError("E001", "Invalid MODE token.", result.Errors);
					break;
				}
				bool flag2 = false;
				bool blink = false;
				switch (mode)
				{
				case QuickReturnDevice.LEDMode.Blink:
					flag2 = (blink = true);
					break;
				case QuickReturnDevice.LEDMode.On:
					flag2 = true;
					break;
				}
				flag = ((!flag2) ? QuickReturnDevice.Instance.TurnOffLED(lED) : QuickReturnDevice.Instance.TurnOnLED(lED, blink));
				break;
			}
			finally
			{
				context.PushTop(flag);
			}
		}
		case Forms.UpdateEEPROM:
		{
			int num = QuickReturnDevice.Instance.UpdateDeviceImages();
			context.PushTop(num);
			break;
		}
		case Forms.ClosePort:
			QuickReturnDevice.Instance.ClosePort();
			break;
		case Forms.FindDevice:
		{
			string instance = QuickReturnDevice.Instance.ProbePort();
			context.PushTop(instance);
			break;
		}
		}
	}

	private void LogCounterValue(TextWriter writer, string counterName, string friendlyName)
	{
		IPersistentCounter persistentCounter = ServiceLocator.Instance.GetService<IPersistentCounterService>().Find(counterName);
		int num = 0;
		if (persistentCounter != null)
		{
			num = persistentCounter.Value;
		}
		writer.WriteLine("{0} counter has a value of {1}", friendlyName, num);
	}

	private Forms CheckForm(string token)
	{
		Forms forms = Enum<Forms>.ParseIgnoringCase(token, Forms.Unknown);
		if (forms != Forms.Unknown)
		{
			return forms;
		}
		string message = $"Unrecognized FORM argument: {token}";
		LogHelper.Instance.Log(message, LogEntryType.Error);
		return Forms.Unknown;
	}

	private QuickReturnDevice.LED GetLED(ExecutionResult result, ExecutionContext context)
	{
		try
		{
			return Enum<QuickReturnDevice.LED>.ParseIgnoringCase(GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair(Attributes.Led.ToString().ToUpper()), result.Errors, context), QuickReturnDevice.LED.Unknown);
		}
		catch (Exception)
		{
			return QuickReturnDevice.LED.Unknown;
		}
	}

	private QuickReturnDevice.LEDMode GetMode(ExecutionResult result, ExecutionContext context)
	{
		try
		{
			return Enum<QuickReturnDevice.LEDMode>.ParseIgnoringCase(GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair(Attributes.Mode.ToString().ToUpper()), result.Errors, context), QuickReturnDevice.LEDMode.Unknown);
		}
		catch (Exception)
		{
			return QuickReturnDevice.LEDMode.Unknown;
		}
	}

	private Attributes CheckAttribute(string token)
	{
		Attributes attributes = Enum<Attributes>.ParseIgnoringCase(token, Attributes.Unknown);
		if (attributes != Attributes.Unknown)
		{
			return attributes;
		}
		string message = $"Unrecognized Attribute argument: {token}";
		LogHelper.Instance.Log(message, LogEntryType.Error);
		return Attributes.Unknown;
	}
}
