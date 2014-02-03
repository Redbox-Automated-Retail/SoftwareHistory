using System;
using System.IO;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class CounterInstruction : Instruction
{
	protected enum Ops
	{
		None,
		Increment,
		Decrement,
		Get,
		Create,
		Reset,
		ShowTimeouts
	}

	private const string LogName = "Timeouts.log";

	private readonly string[] SupportCounters = new string[12]
	{
		"FingerCloseTimeout", "FingerOpenTimeout", "FingerRentTimeout", "GripperExtendTimeout", "GripperRetractTimeout", "QlmDisengageTimeout", "QlmEngageTimeout", "TrackCloseTimeout", "TrackOpenTimeout", "VendDoorCloseTimeout",
		"VendDoorRentTimeout", "HomeYOnInventoryError"
	};

	public override string Mnemonic => "COUNTER";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count > 0 && tokensAfterMnemonic[0].IsKeyValuePair)
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("NAME");
		string keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair, result.Errors);
		string value = base.Operands[1].Value;
		Ops ops = Ops.None;
		try
		{
			ops = (Ops)Enum.Parse(typeof(Ops), value, ignoreCase: true);
		}
		catch (ArgumentException)
		{
			string text = $"Unrecognized operation: {value}";
			ServiceLogHelper.Instance.WriteInfoMessage(text);
			AddError("E001", text, result.Errors);
			return;
		}
		switch (ops)
		{
		case Ops.Create:
			ExecutionEngine.Instance.ActiveContext.PushTop(CounterManager.Instance.Exists(keyValuePairValue) ? "SUCCESS" : "FAILURE");
			break;
		case Ops.Decrement:
		{
			Counter counter3 = CounterManager.Instance.Decrement(keyValuePairValue);
			if (counter3 == null)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop("FAILURE");
				break;
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(counter3.Value);
			ExecutionEngine.Instance.ActiveContext.PushTop("SUCCESS");
			break;
		}
		case Ops.Increment:
		{
			Counter counter3 = CounterManager.Instance.Find(keyValuePairValue);
			if (counter3 == null)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop("FAILURE");
				break;
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(counter3.Value);
			ExecutionEngine.Instance.ActiveContext.PushTop("SUCCESS");
			break;
		}
		case Ops.Get:
		{
			Counter counter3 = CounterManager.Instance.Find(keyValuePairValue);
			if (counter3 == null)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop("FAILURE");
				break;
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(counter3.Value);
			ExecutionEngine.Instance.ActiveContext.PushTop("SUCCESS");
			break;
		}
		case Ops.Reset:
			ExecutionEngine.Instance.ActiveContext.PushTop((CounterManager.Instance.Reset(keyValuePairValue) != null) ? "SUCCESS" : "FAILURE");
			break;
		case Ops.ShowTimeouts:
		{
			string path = Path.Combine(ControllerConfiguration.Instance.KioskLogsPath, "ErrorLogs");
			string path2 = Path.Combine(path, "Timeouts.log");
			using StreamWriter streamWriter = new StreamWriter(File.Open(path2, FileMode.Append, FileAccess.Write, FileShare.Read));
			streamWriter.WriteLine("Counter sample on {0} {1}", DateTime.Now.ToShortTimeString(), DateTime.Now.ToShortDateString());
			string[] supportCounters = SupportCounters;
			foreach (string text2 in supportCounters)
			{
				int num = 0;
				Counter counter = CounterManager.Instance.Find(text2);
				if (counter != null)
				{
					num = counter.Value;
				}
				streamWriter.WriteLine($" {text2}: {num}");
			}
			Counter counter2 = CounterManager.Instance.Find("EmptyOrStuckCount");
			int num2 = 0;
			if (counter2 != null)
			{
				num2 = counter2.Value;
			}
			streamWriter.WriteLine(string.Format(" {0}: {1}", "EmptyOrStuckCount", num2));
			streamWriter.WriteLine();
			break;
		}
		}
	}
}
