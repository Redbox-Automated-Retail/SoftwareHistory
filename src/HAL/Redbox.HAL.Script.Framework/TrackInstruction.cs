using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class TrackInstruction : Instruction
{
	protected static class Commands
	{
		public const string Open = "OPEN";

		public const string Close = "CLOSE";

		public const string Status = "STATUS";

		public const string Cycle = "CYCLE";
	}

	public override string Mnemonic => "TRACK";

	private static Track Track => Track.Instance;

	protected override bool IsOperandRecognized(string operand)
	{
		return IsValidOperand(operand);
	}

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count == 1)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.Symbol || !IsValidOperand(tokensAfterMnemonic[0].Value))
			{
				AddError("C002", "Expected a symbol: OPEN, CLOSE, CYCLE or STATUS.", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
		else
		{
			AddError("C002", "Expected a symbol: OPEN, CLOSE, CYCLE or STATUS.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		_ = ExecutionEngine.Instance.ActiveContext.ProgramName;
		Token token = base.Operands[0];
		if (token.Type == TokenType.Symbol)
		{
			if (string.Compare(token.Value, "STATUS", ignoreCase: true) == 0)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(Track.IsTrackOpen(read: false) ? "OPEN" : "CLOSED");
			}
			else if (string.Compare(token.Value, "OPEN", ignoreCase: true) == 0)
			{
				HandleTimeoutOrCommmError(Track.TestOpen());
			}
			else if (string.Compare(token.Value, "CLOSE", ignoreCase: true) == 0)
			{
				HandleTimeoutOrCommmError(Track.TestClose());
			}
			else if (string.Compare(token.Value, "CYCLE", ignoreCase: true) == 0)
			{
				bool timedOut = Track.Cycle();
				ExecutionEngine.Instance.ActiveContext.PushTop(Instruction.ConvertTimeoutBoolToMessage(timedOut));
			}
		}
	}

	private bool IsValidOperand(string operand)
	{
		if (string.Compare(operand, "OPEN", ignoreCase: true) != 0 && string.Compare(operand, "CLOSE", ignoreCase: true) != 0 && string.Compare(operand, "STATUS", ignoreCase: true) != 0)
		{
			return string.Compare(operand, "CYCLE", ignoreCase: true) == 0;
		}
		return true;
	}
}
