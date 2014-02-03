using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class QlmDoorInstruction : Instruction
{
	protected static class Commands
	{
		public const string Lock = "LOCK";

		public const string Unlock = "UNLOCK";

		public const string Status = "STATUS";
	}

	public override string Mnemonic => "QLMDOOR";

	private QLM QLM => QLM.Instance;

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count == 1)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.Symbol || (string.Compare(tokensAfterMnemonic[0].Value, "LOCK", ignoreCase: true) != 0 && string.Compare(tokensAfterMnemonic[0].Value, "UNLOCK", ignoreCase: true) != 0 && string.Compare(tokensAfterMnemonic[0].Value, "STATUS", ignoreCase: true) != 0))
			{
				AddError("C002", "Expected a symbol: LOCK, UNLOCK, or STATUS.", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
		else
		{
			AddError("C002", "Expected a symbol: LOCK, UNLOCK.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		Token token = base.Operands[0];
		if (token.Type == TokenType.Symbol)
		{
			if (string.Compare(token.Value, "LOCK", ignoreCase: true) == 0)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(Instruction.ConvertTimeoutBoolToMessage(QLM.SetDoorLock(locked: true)));
			}
			else if (string.Compare(token.Value, "UNLOCK", ignoreCase: true) == 0)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(Instruction.ConvertTimeoutBoolToMessage(QLM.SetDoorLock(locked: false)));
			}
			else if (string.Compare(token.Value, "STATUS", ignoreCase: true) == 0)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(QLM.GetDoorStatus().ToString().ToUpper());
			}
		}
	}
}
