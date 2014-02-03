using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class InspectInstruction : Instruction
{
	protected static class KeyNames
	{
		public const string Deck = "DECK";

		public const string Slot = "SLOT";
	}

	public override string Mnemonic => "INSPECT";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count > 0 && tokensAfterMnemonic.Count <= 3)
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
		else
		{
			AddError("C002", "Expected key value pair DECK=y, SLOT=x.  The values of x and y may be numeric literals or symbols.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		if (!GetLocation(result, out var deck, out var slot))
		{
			AddError("E004", "Deck or slot are undefined, reissue the INSPECT instruction specifying both deck and slot values.", result.Errors);
		}
		else
		{
			ExecutionEngine.Instance.ActiveContext.PushTop(ControllerService.Instance.Inspect(deck, slot));
		}
	}
}
