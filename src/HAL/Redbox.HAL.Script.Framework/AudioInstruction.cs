using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class AudioInstruction : Instruction
{
	private enum AudioOperands
	{
		None,
		On,
		Off
	}

	public override string Mnemonic => "AUDIO";

	protected override bool IsOperandRecognized(string operand)
	{
		return AudioOperands.None != Enum<AudioOperands>.ParseIgnoringCase(operand, AudioOperands.None);
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (Enum<AudioOperands>.ParseIgnoringCase(operandTokens[0].Value, AudioOperands.None) == AudioOperands.None)
		{
			result.Errors.Add(Error.NewError("C001", "Unexpected form", $"The operand {operandTokens[0].Value} for AUDIO isn't recognized."));
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		AudioOperands audioOperands = Enum<AudioOperands>.ParseIgnoringCase(base.Operands[0].Value, AudioOperands.None);
		ControlSystem.Instance.ExecuteControllerCommand((AudioOperands.Off == audioOperands) ? CommandType.AudioOff : CommandType.AudioOn);
	}
}
