using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class MessageInstruction : Instruction
{
	public override string Mnemonic => "MSG";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count >= 1)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.StringLiteral && tokensAfterMnemonic[0].Type != TokenType.ConstSymbol)
			{
				AddError("C002", "Expected a string literal and any number of optional subtitution parameters.", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
		else
		{
			AddError("C002", "Expected a string literal and any number of optional subtitution parameters.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		if (base.Operands.Count > 0)
		{
			object[] args = ConvertSymbolsToParameterArray(result.Errors);
			if (!result.Errors.ContainsError())
			{
				string format = ((base.Operands[0].Type == TokenType.StringLiteral) ? base.Operands[0].Value : ((string)ExecutionEngine.Instance.ActiveContext.GetSymbolValue(base.Operands[0].Value, result.Errors)));
				string message = string.Format(format, args);
				ExecutionEngine.Instance.ActiveContext.Send(message);
			}
		}
	}
}
