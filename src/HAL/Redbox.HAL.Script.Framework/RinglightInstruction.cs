using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class RinglightInstruction : Instruction
{
	public override string Mnemonic => "RINGLIGHT";

	protected override bool IsOperandRecognized(string operand)
	{
		return IsOperandValid(operand);
	}

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count == 1)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.Symbol || !IsOperandValid(tokensAfterMnemonic[0].Value))
			{
				AddError("C002", "Expected a symbol: ON, OFF.", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
		else
		{
			AddError("C002", "Expected a symbol: ON, OFF.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		RinglightState state = FromString(base.Operands[0].Value);
		ControllerService.Instance.ToggleRingLight(state, null);
	}

	private bool IsOperandValid(string op)
	{
		if (FromString(op) != RinglightState.None)
		{
			return true;
		}
		return false;
	}

	private RinglightState FromString(string operand)
	{
		try
		{
			return (RinglightState)Enum.Parse(typeof(RinglightState), operand, ignoreCase: true);
		}
		catch (Exception)
		{
			return RinglightState.None;
		}
	}
}
