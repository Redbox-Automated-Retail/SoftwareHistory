using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class FindEmptyInstruction : Instruction
{
	protected static class ResultCodes
	{
		public const string MachineFull = "MACHINE FULL";

		public const string Found = "FOUND";
	}

	public override string Mnemonic => "FINDEMPTY";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count == 0)
		{
			return;
		}
		if (tokensAfterMnemonic.Count == 1)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.NumericLiteral && tokensAfterMnemonic[0].Type != TokenType.Symbol)
			{
				AddError("C002", "Expected a numeric literal or symbol.", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
		else
		{
			AddError("C002", "The DO mnemonic supports an optional single numeric literal or symbol.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		using EmptySearchResult emptySearchResult = new EmptySearchResult();
		if (!emptySearchResult.FindOneEmpty())
		{
			ExecutionEngine.Instance.ActiveContext.AppLog.LogMachineFull();
			ExecutionEngine.Instance.ActiveContext.PushTop("MACHINE FULL");
			return;
		}
		Location location = emptySearchResult.EmptyLocations[0];
		ExecutionEngine.Instance.ActiveContext.PushTop(location.Slot);
		ExecutionEngine.Instance.ActiveContext.PushTop(location.Deck);
		ExecutionEngine.Instance.ActiveContext.PushTop("FOUND");
	}
}
