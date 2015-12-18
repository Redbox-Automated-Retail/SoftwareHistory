using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class ESPInstruction : Instruction
{
	public override string Mnemonic => "ESP";

	protected override void ParseOperands(TokenList operands, ExecutionResult result)
	{
		if (operands.Count != 1)
		{
			result.AddInvalidOperandError($"The {Mnemonic} instruction expects one operand.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
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
