using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class ExitInstruction : Instruction
{
	public override string Mnemonic => "EXIT";

	public override bool ExitsContext => true;

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count > 0)
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		if (base.Operands.Count > 0)
		{
			string text = base.Operands[0].Value;
			if (base.Operands[0].IsSymbolOrConst)
			{
				text = (string)ExecutionEngine.Instance.ActiveContext.GetSymbolValue(base.Operands[0].Value, result.Errors);
				if (text == null || result.Errors.Count > 0)
				{
					AddError("E999", "Unspecified context exit; check the logs.", result.Errors);
					return;
				}
			}
			AddError("E999", text, result.Errors);
		}
		else
		{
			AddError("E999", "Unspecified context exit; check the logs.", result.Errors);
		}
	}
}
