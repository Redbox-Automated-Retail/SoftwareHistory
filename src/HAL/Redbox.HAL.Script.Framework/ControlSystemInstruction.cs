using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class ControlSystemInstruction : Instruction
{
	private enum Ops
	{
		None,
		Center
	}

	public override string Mnemonic => "CONTROLSYSTEM";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 1)
		{
			result.AddInvalidOperandError($"The {Mnemonic} instruction expects one operand.");
		}
		else if (Enum<Ops>.ParseIgnoringCase(operandTokens[0].Value, Ops.None) == Ops.None)
		{
			result.AddInvalidOperandError($"The operand {operandTokens[0].Value} is not recognized by {Mnemonic}.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Ops ops = Enum<Ops>.ParseIgnoringCase(base.Operands[0].Value, Ops.None);
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		Ops ops2 = ops;
		if (ops2 == Ops.Center)
		{
			service.Center(CenterDiskMethod.VendDoorAndBack);
		}
	}
}
