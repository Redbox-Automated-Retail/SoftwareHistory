using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class RinglightInstruction : Instruction
{
	public override string Mnemonic => "RINGLIGHT";

	protected override bool IsOperandRecognized(string operand)
	{
		return IsOperandValid(operand);
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 1)
		{
			result.AddMissingOperandError("Expected a symbol: ON, OFF.");
		}
		else if (operandTokens[0].Type != TokenType.Symbol || !IsOperandValid(operandTokens[0].Value))
		{
			result.AddInvalidOperandError("Expected a symbol: ON, OFF.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		RinglightState state = Enum<RinglightState>.ParseIgnoringCase(base.Operands[0].Value, RinglightState.None);
		PickerBoard.ToggleRingLight(state, null);
	}

	private bool IsOperandValid(string op)
	{
		if (Enum<RinglightState>.ParseIgnoringCase(op, RinglightState.None) != RinglightState.None)
		{
			return true;
		}
		return false;
	}
}
