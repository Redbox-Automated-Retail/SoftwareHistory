using System;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class ReadInstruction : Instruction
{
	private static string[] OptionalOperands = new string[2] { "FULL", "SHOWSCANTIME" };

	public override string Mnemonic => "READ";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		switch (tokens.GetTokensAfterMnemonic().Count)
		{
		case 0:
			break;
		case 1:
		{
			string value = tokens.GetTokensAfterMnemonic()[0].Value;
			if (TokenIsRecognized(value))
			{
				TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
				base.Operands.AddRange(tokensAfterMnemonic);
			}
			else
			{
				AddError("C002", $"Unrecognized operand {value} to READ", result.Errors);
			}
			break;
		}
		default:
			AddError("C002", "The READ mnemonic supports one optional operand.", result.Errors);
			break;
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		if (!(ExecutionEngine.Instance.ActiveContext.PopTop() is string text) || !text.EndsWith(".jpg"))
		{
			AddError("E004", "The READ instruction expects a valid string on the stack which represents a path to a valid JPEG. CAMERA CAPTURE will place such a string on the stack for you.", result.Errors);
			return;
		}
		bool flag = false;
		foreach (Token operand in base.Operands)
		{
			if (operand.Value.Equals("SHOWSCANTIME"))
			{
				flag = true;
			}
		}
		ScanResult scanResult = DataMatrixHelper.Instance.ScanFrame(text);
		if (scanResult.ScannedCodes == 0)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop("UNKNOWN");
		}
		else
		{
			for (int i = 0; i < scanResult.ScannedCodes; i++)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(scanResult.ScannedMatrix);
			}
		}
		ExecutionEngine.Instance.ActiveContext.PushTop(scanResult.ScannedCodes);
		if (flag)
		{
			string instance = $"{scanResult.ExecutionTime.Seconds}.{scanResult.ExecutionTime.Milliseconds} s";
			ExecutionEngine.Instance.ActiveContext.PushTop(instance);
		}
	}

	private bool TokenIsRecognized(string token)
	{
		string[] optionalOperands = OptionalOperands;
		foreach (string text in optionalOperands)
		{
			if (text.Equals(token, StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}
}
