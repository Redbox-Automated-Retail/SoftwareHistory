using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class PrintInstruction : Instruction
{
	public override string Mnemonic => "PRINT";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count >= 1 && tokensAfterMnemonic[0].Type != TokenType.StringLiteral)
		{
			AddError("C002", "Expected a string literal and any number of optional subtitution parameters.", result.Errors);
		}
		else
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		if (base.Operands.Count > 0)
		{
			object[] arg = ConvertSymbolsToParameterArray(result.Errors);
			if (result.Errors.Count <= 0)
			{
				Console.WriteLine(base.Operands[0].Value, arg);
			}
		}
		else if (ExecutionEngine.Instance.ActiveContext.StackDepth > 0)
		{
			object obj = ExecutionEngine.Instance.ActiveContext.PopTop();
			if (obj != null)
			{
				Console.WriteLine(obj);
			}
		}
	}
}
