using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class WaitInstruction : Instruction
{
	public override string Mnemonic => "WAIT";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
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
			AddError("C002", "The WAIT mnemonic requires a single numeric literal or symbol.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		int ms = 0;
		object obj = base.Operands[0].Value;
		if (base.Operands[0].Type == TokenType.Symbol)
		{
			obj = ExecutionEngine.Instance.ActiveContext.GetSymbolValue(base.Operands[0].Value, result.Errors);
			if (obj == null)
			{
				return;
			}
		}
		if (int.TryParse(obj.ToString(), out var result2))
		{
			ms = result2;
		}
		PerfFunctions.Wait(ms);
	}
}
