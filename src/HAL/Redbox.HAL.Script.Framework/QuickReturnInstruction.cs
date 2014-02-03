using System;
using System.IO;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
internal class QuickReturnInstruction : Instruction
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
		DumpStateTable
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
			m_statsDirectory = Path.Combine(ControllerConfiguration.Instance.KioskLogsPath, "Return");
			return m_statsDirectory;
		}
	}

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count < 1)
		{
			AddError("C002", "QuickReturn expects some arguments", result.Errors);
			return;
		}
		Forms forms = CheckForm(tokensAfterMnemonic[0].Value);
		if (forms == Forms.Unknown)
		{
			AddError("C002", $"Unknown form of QuickReturn: {tokensAfterMnemonic[0].Value}", result.Errors);
			return;
		}
		string text = string.Empty;
		switch (forms)
		{
		case Forms.State:
			if (tokensAfterMnemonic.Count < 2 || tokensAfterMnemonic.GetKeyValuePair(Attributes.Name.ToString().ToUpper()) == null)
			{
				text = $"{Forms.State.ToString().ToUpper()} form requires a {Attributes.Name.ToString()} operand.";
			}
			break;
		case Forms.LedState:
		{
			if (tokensAfterMnemonic.Count < 2)
			{
				text = $"{Forms.LedState.ToString().ToUpper()} form requires a {Attributes.Mode.ToString()} operand and {Attributes.Led.ToString()} operand.";
				break;
			}
			bool flag = false;
			bool flag2 = false;
			if (tokensAfterMnemonic.GetKeyValuePair(Attributes.Mode.ToString().ToUpper()) != null)
			{
				flag = true;
			}
			if (tokensAfterMnemonic.GetKeyValuePair(Attributes.Led.ToString().ToUpper()) != null)
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
			if (tokensAfterMnemonic.Count < 2 || tokensAfterMnemonic.GetKeyValuePair(Attributes.Name.ToString().ToUpper()) == null)
			{
				text = string.Format("{0} form requires at least a {0} operand and {1} operand (optional {2} operand).", Forms.DefState.ToString(), Attributes.Name.ToString(), Attributes.File.ToString(), Attributes.Text.ToString());
			}
			break;
		case Forms.GetConfig:
			if (tokensAfterMnemonic.Count < 2)
			{
				text = $"{forms.ToString().ToUpper()} form requires a configuration option operand.";
			}
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			AddError("C002", text, result.Errors);
		}
		else
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
	}

	protected override void OnExecute(ExecutionResult result)
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
			ExecutionEngine.Instance.ActiveContext.PushTop(QuickReturnDevice.Instance.IsConfigured.ToString().ToUpper());
			break;
		case Forms.Restart:
			if (QuickReturnPoller.Instance.QuickReturnPollerEnabled)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop("SUCCESS");
				break;
			}
			QuickReturnPoller.Instance.Startup();
			ExecutionEngine.Instance.ActiveContext.PushTop(QuickReturnPoller.Instance.QuickReturnPollerEnabled ? "SUCCESS" : "FAILURE");
			break;
		case Forms.Help:
			ExecutionEngine.Instance.ActiveContext.PushTop("NOT IMPLEMENTED");
			break;
		case Forms.PollStatus:
			ExecutionEngine.Instance.ActiveContext.PushTop(QuickReturnPoller.Instance.QuickReturnPollerEnabled ? "ACTIVE" : "INACTIVE");
			break;
		case Forms.DisableUI:
		case Forms.HaltPoll:
			QuickReturnPoller.Instance.Shutdown();
			ExecutionEngine.Instance.ActiveContext.PushTop("SUCCESS");
			break;
		case Forms.GetConfig:
		{
			string value = base.Operands[1].Value;
			ExecutionEngine.Instance.ActiveContext.PushTop(QuickReturnDevice.Instance.GetOption(value));
			break;
		}
		case Forms.LogReturn:
			LogHelper.Instance.Log($"Return via QuickReturn terminal on: {DateTime.Now}", LogEntryType.Info);
			break;
		case Forms.CheckAttButton:
		{
			bool flag5 = QuickReturnDevice.Instance.CheckAttButton(out var commError);
			if (commError)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop("FAILURE");
				break;
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(flag5 ? "SET" : "CLEAR");
			ExecutionEngine.Instance.ActiveContext.PushTop("SUCCESS");
			break;
		}
		case Forms.ClearAttButton:
			ExecutionEngine.Instance.ActiveContext.PushTop(QuickReturnDevice.Instance.ClearAttentionButtonStatus() ? "SUCCESS" : "FAILURE");
			break;
		case Forms.Reset:
		{
			bool flag4 = QuickReturnDevice.Instance.Reset();
			ExecutionEngine.Instance.ActiveContext.PushTop(flag4);
			break;
		}
		case Forms.State:
		{
			string keyValuePairValue4 = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair(Attributes.Name.ToString().ToUpper()), result.Errors);
			if (keyValuePairValue4 == null || !QuickReturnDevice.Instance.DisplayState(keyValuePairValue4))
			{
				AddError("E002", "Unable to set state: maybe state is not defined?", result.Errors);
				QuickReturnPoller.Instance.Shutdown();
			}
			else
			{
				ExecutionEngine.Instance.ActiveContext.PushTop("SUCCESS");
			}
			break;
		}
		case Forms.DefState:
		{
			string keyValuePairValue = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair(Attributes.Name.ToString().ToUpper()), result.Errors);
			string keyValuePairValue2 = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair(Attributes.File.ToString().ToUpper()), result.Errors);
			string keyValuePairValue3 = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair(Attributes.LedStates.ToString().ToUpper()), result.Errors);
			bool flag3 = QuickReturnDevice.Instance.CreateState(keyValuePairValue, keyValuePairValue2, keyValuePairValue3);
			ExecutionEngine.Instance.ActiveContext.PushTop(flag3);
			break;
		}
		case Forms.LedState:
		{
			bool flag = false;
			try
			{
				QuickReturnDevice.LED lED = GetLED(result);
				if (lED == QuickReturnDevice.LED.Unknown)
				{
					AddError("E001", "Invalid LED key-value pair.", result.Errors);
					break;
				}
				QuickReturnDevice.Mode mode = GetMode(result);
				if (mode == QuickReturnDevice.Mode.Unknown)
				{
					AddError("E001", "Invalid MODE token.", result.Errors);
					break;
				}
				bool flag2 = false;
				bool blink = false;
				switch (mode)
				{
				case QuickReturnDevice.Mode.Blink:
					flag2 = (blink = true);
					break;
				case QuickReturnDevice.Mode.On:
					flag2 = true;
					break;
				}
				flag = ((!flag2) ? QuickReturnDevice.Instance.TurnOffLED(lED) : QuickReturnDevice.Instance.TurnOnLED(lED, blink));
				break;
			}
			finally
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(flag);
			}
		}
		case Forms.UpdateEEPROM:
		{
			int num = QuickReturnDevice.Instance.UpdateDeviceImages();
			ExecutionEngine.Instance.ActiveContext.PushTop(num);
			break;
		}
		case Forms.ClosePort:
			QuickReturnDevice.Instance.ClosePort();
			break;
		case Forms.DumpCounters:
		{
			string path = Path.Combine(StatisticsDirectory, "QRstats.log");
			if (File.Exists(path))
			{
				try
				{
					FileHelper.CreateBackup(path, BackupAction.Move);
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("Unable to backup statistics file.", e);
					break;
				}
			}
			using StreamWriter writer = new StreamWriter(path);
			LogCounterValue(writer, "QuickReturnAttention", "Button presses");
			LogCounterValue(writer, "QuickReturnNoInsertDisk", "No disk inserts");
			LogCounterValue(writer, "QuickReturnSuccessful", "Successful returns");
			break;
		}
		}
	}

	private void LogCounterValue(TextWriter writer, string counterName, string friendlyName)
	{
		Counter counter = CounterManager.Instance.Find(counterName);
		int num = 0;
		if (counter != null)
		{
			num = counter.Value;
		}
		writer.WriteLine("{0} counter has a value of {1}", friendlyName, num);
	}

	private Forms CheckForm(string token)
	{
		Forms forms = Forms.Unknown;
		try
		{
			return (Forms)Enum.Parse(typeof(Forms), token, ignoreCase: true);
		}
		catch (ArgumentException)
		{
			string message = $"Unrecognized FORM argument: {token}";
			LogHelper.Instance.Log(message, LogEntryType.Error);
			return Forms.Unknown;
		}
	}

	private QuickReturnDevice.LED GetLED(ExecutionResult result)
	{
		try
		{
			string keyValuePairValue = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair(Attributes.Led.ToString().ToUpper()), result.Errors);
			return (QuickReturnDevice.LED)Enum.Parse(typeof(QuickReturnDevice.LED), keyValuePairValue, ignoreCase: true);
		}
		catch (Exception)
		{
			return QuickReturnDevice.LED.Unknown;
		}
	}

	private QuickReturnDevice.Mode GetMode(ExecutionResult result)
	{
		try
		{
			string keyValuePairValue = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair(Attributes.Mode.ToString().ToUpper()), result.Errors);
			return (QuickReturnDevice.Mode)Enum.Parse(typeof(QuickReturnDevice.Mode), keyValuePairValue, ignoreCase: true);
		}
		catch (Exception)
		{
			return QuickReturnDevice.Mode.Unknown;
		}
	}

	private Attributes CheckAttribute(string token)
	{
		Attributes attributes = Attributes.Unknown;
		try
		{
			return (Attributes)Enum.Parse(typeof(Attributes), token, ignoreCase: true);
		}
		catch (ArgumentException)
		{
			string message = $"Unrecognized Attribute argument: {token}";
			LogHelper.Instance.Log(message, LogEntryType.Error);
			return Attributes.Unknown;
		}
	}
}
