using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class VendDoorInstruction : Instruction
{
	protected static class Commands
	{
		public const string Open = "OPEN";

		public const string Rent = "RENT";

		public const string Close = "CLOSE";

		public const string Status = "STATUS";
	}

	public override string Mnemonic => "VENDDOOR";

	private static VendDoor VendDoor => VendDoor.Instance;

	protected override bool IsOperandRecognized(string operand)
	{
		return IsOperandValid(operand);
	}

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count == 1)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.Symbol || !IsOperandValid(tokensAfterMnemonic[0].Value))
			{
				AddError("C002", "Expected a symbol: OPEN, CLOSE, RENT, or STATUS.", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
		else
		{
			AddError("C002", "Expected a symbol: OPEN, CLOSE, RENT, or STATUS.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		Token token = base.Operands[0];
		if (token.Type == TokenType.Symbol)
		{
			if (string.Compare(token.Value, "STATUS", ignoreCase: true) == 0)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(VendDoor.IsClosed() ? "CLOSED" : "OPEN");
			}
			else if (string.Compare(token.Value, "OPEN", ignoreCase: true) == 0)
			{
				HandleTimeoutOrCommmError(VendDoor.TestOpen());
			}
			else if (string.Compare(token.Value, "RENT", ignoreCase: true) == 0)
			{
				HandleTimeoutOrCommmError(VendDoor.TestRent());
			}
			else if (string.Compare(token.Value, "CLOSE", ignoreCase: true) == 0)
			{
				HandleTimeoutOrCommmError(VendDoor.TestClose());
			}
		}
	}

	private bool IsOperandValid(string op)
	{
		if (string.Compare(op, "OPEN", ignoreCase: true) != 0 && string.Compare(op, "RENT", ignoreCase: true) != 0 && string.Compare(op, "CLOSE", ignoreCase: true) != 0)
		{
			return string.Compare(op, "STATUS", ignoreCase: true) == 0;
		}
		return true;
	}
}
