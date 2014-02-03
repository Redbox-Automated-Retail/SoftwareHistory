using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class PushFormatInstruction : Instruction
{
	protected static class StackEnds
	{
		public const string Bottom = "bottom";

		public const string Top = "top";
	}

	protected static class KeyNames
	{
		public const string End = "END";
	}

	public override string Mnemonic => "PUSHF";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count >= 1)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.StringLiteral)
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
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("END");
		bool flag = keyValuePair != null && string.Compare(keyValuePair.Value, "bottom", ignoreCase: true) == 0;
		object[] args = ConvertSymbolsToParameterArray(result.Errors);
		if (result.Errors.Count <= 0)
		{
			string instance = string.Format(base.Operands[0].Value, args);
			if (flag)
			{
				ExecutionEngine.Instance.ActiveContext.PushBottom(instance);
			}
			else
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(instance);
			}
		}
	}
}
