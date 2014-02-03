using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class ESPInstruction : Instruction
{
	public override string Mnemonic => "ESP";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		base.Operands.AddRange(tokensAfterMnemonic);
	}

	protected override void OnExecute(ExecutionResult result)
	{
		if (string.Compare(base.Operands[0].Value, "DUMP", ignoreCase: true) == 0)
		{
			ControllerConfiguration.Instance.DumpESP(dumpStore: false);
		}
		else if (string.Compare(base.Operands[0].Value, "DUMP-STORE", ignoreCase: true) == 0)
		{
			ControllerConfiguration.Instance.DumpESP(dumpStore: true);
		}
	}
}
