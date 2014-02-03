using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class LookupInstruction : Instruction
{
	protected static class ResultCodes
	{
		public const string Found = "FOUND";

		public const string NotFound = "NOT FOUND";
	}

	public override string Mnemonic => "LOOKUP";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count == 1)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.StringLiteral && tokensAfterMnemonic[0].Type != TokenType.Symbol)
			{
				AddError("C002", "Expected a string literal or symbol.", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		string text = null;
		if (base.Operands.Count == 1)
		{
			Token token = base.Operands[0];
			if (token.Type == TokenType.Symbol)
			{
				text = ExecutionEngine.Instance.ActiveContext.GetSymbolValue(token.Value, result.Errors) as string;
				if (result.Errors.Count > 0)
				{
					return;
				}
			}
			else if (token.Type == TokenType.StringLiteral)
			{
				text = token.Value;
			}
		}
		else if (ExecutionEngine.Instance.ActiveContext.StackDepth > 0)
		{
			text = ExecutionEngine.Instance.ActiveContext.PopTop() as string;
		}
		if (text != null)
		{
			Location location = ControllerService.Instance.Lookup(text);
			if (location == null)
			{
				ExecutionEngine.Instance.ActiveContext.AppLog.LogLookupFailure(text);
				ExecutionEngine.Instance.ActiveContext.PushTop("NOT FOUND");
			}
			else
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(location.Slot);
				ExecutionEngine.Instance.ActiveContext.PushTop(location.Deck);
				ExecutionEngine.Instance.ActiveContext.PushTop("FOUND");
			}
		}
		else
		{
			AddError("E004", "LOOKUP expects a valid string on the stack, symbol, or string literal representing the ID to find.", result.Errors);
		}
	}
}
