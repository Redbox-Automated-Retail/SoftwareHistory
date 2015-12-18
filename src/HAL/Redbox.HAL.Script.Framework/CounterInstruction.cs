using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class CounterInstruction : Instruction
{
	private enum Ops
	{
		None,
		Increment,
		Decrement,
		Get,
		Create,
		Reset,
		ShowTimeouts
	}

	public override string Mnemonic => "COUNTER";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (!operandTokens[0].IsKeyValuePair)
		{
			result.AddInvalidOperandError($"The {Mnemonic} instruction expected at least a key-value pair.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("NAME");
		string keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair, result.Errors, context);
		string value = base.Operands[1].Value;
		switch (Enum<Ops>.ParseIgnoringCase(value, Ops.None))
		{
		case Ops.None:
		{
			string message = $"Unrecognized operation: {value}";
			AddError("E001", message, result.Errors);
			break;
		}
		case Ops.Create:
			context.PushTop(CounterManager.Instance.Exists(keyValuePairValue) ? "SUCCESS" : "FAILURE");
			break;
		case Ops.Decrement:
		{
			Counter counter = CounterManager.Instance.Decrement(keyValuePairValue);
			if (counter == null)
			{
				context.PushTop("FAILURE");
				break;
			}
			context.PushTop(counter.Value);
			context.PushTop("SUCCESS");
			break;
		}
		case Ops.Increment:
		{
			Counter counter = CounterManager.Instance.Find(keyValuePairValue);
			if (counter == null)
			{
				context.PushTop("FAILURE");
				break;
			}
			context.PushTop(counter.Value);
			context.PushTop("SUCCESS");
			break;
		}
		case Ops.Get:
		{
			if (keyValuePairValue.Equals("DUMPBIN", StringComparison.CurrentCultureIgnoreCase))
			{
				context.PushTop(DumpBin.Instance.CurrentCount());
				context.PushTop("SUCCESS");
				break;
			}
			Counter counter = CounterManager.Instance.Find(keyValuePairValue);
			if (counter == null)
			{
				context.PushTop("FAILURE");
				break;
			}
			context.PushTop(counter.Value);
			context.PushTop("SUCCESS");
			break;
		}
		case Ops.Reset:
			context.PushTop((CounterManager.Instance.Reset(keyValuePairValue) != null) ? "SUCCESS" : "FAILURE");
			break;
		case Ops.ShowTimeouts:
			HardwareSupport.LogCurrent();
			break;
		}
	}
}
