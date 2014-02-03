using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class SecretOptionInstruction : Instruction
{
	private enum Operation
	{
		None,
		Set,
		SetEncrypted,
		Get,
		GetEncrypted,
		Remove
	}

	public override string Mnemonic => "SECRETOPTION";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		int count = tokens.GetTokensAfterMnemonic().Count;
		if (count > 0)
		{
			string value = tokens.GetTokensAfterMnemonic()[0].Value;
			if (OperationRecognized(result, value))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		Operation operation = ComputeOperation(result, base.Operands[0].Value);
		if (!LocateKey(result, out var key))
		{
			AddError("E001", "Missing 'NAME' operand.", result.Errors);
			return;
		}
		string val = null;
		LocateValue(result, out val);
		switch (operation)
		{
		case Operation.Remove:
			SecretOptions.Instance.RemoveOption(key);
			break;
		case Operation.Get:
			PushRetrieved(SecretOptions.Instance.GetValue<string>(key));
			break;
		case Operation.GetEncrypted:
			result.Errors.Add(Error.NewError("E666", "Unsuppported operation.", "The get encrypted operation on SecretOptions is not supported."));
			break;
		case Operation.Set:
			if (string.IsNullOrEmpty(val))
			{
				AddError("E001", "A 'VALUE' operand is expected.", result.Errors);
			}
			else
			{
				SecretOptions.Instance.SetValue(key, val);
			}
			break;
		case Operation.SetEncrypted:
			result.Errors.Add(Error.NewError("E666", "Unsuppported operation.", "The set encrypted operation on SecretOptions is not supported."));
			break;
		}
	}

	private void PushRetrieved(string val)
	{
		ExecutionEngine.Instance.ActiveContext.PushTop(string.IsNullOrEmpty(val) ? "UNKNOWN" : val);
	}

	private bool LocateKey(ExecutionResult result, out string key)
	{
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("NAME");
		key = GetKeyValuePairValue<string>(keyValuePair, result.Errors);
		if (result.Errors.Count <= 0)
		{
			return true;
		}
		return false;
	}

	private bool LocateValue(ExecutionResult result, out string val)
	{
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("VALUE");
		val = GetKeyValuePairValue<string>(keyValuePair, result.Errors);
		if (result.Errors.Count <= 0)
		{
			return true;
		}
		return false;
	}

	private bool OperationRecognized(ExecutionResult result, string op)
	{
		return ComputeOperation(result, op) != Operation.None;
	}

	private Operation ComputeOperation(ExecutionResult result, string operation)
	{
		Operation operation2 = Operation.None;
		try
		{
			return (Operation)Enum.Parse(typeof(Operation), operation, ignoreCase: true);
		}
		catch (ArgumentException)
		{
			string message = $"Unrecognized OPERATION argument: {operation}";
			LogHelper.Instance.Log(message, LogEntryType.Info);
			AddError("E001", message, result.Errors);
			return Operation.None;
		}
	}
}
