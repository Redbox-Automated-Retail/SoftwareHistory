using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class AudioInstruction : Instruction
{
	protected static class Commands
	{
		public const string On = "ON";

		public const string Off = "OFF";
	}

	public override string Mnemonic => "AUDIO";

	protected override bool IsOperandRecognized(string operand)
	{
		return IsValidOperand(operand);
	}

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count == 1)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.Symbol || !IsValidOperand(tokensAfterMnemonic[0].Value))
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
		Token token = base.Operands[0];
		if (token.Type == TokenType.Symbol)
		{
			if (string.Compare(token.Value, "ON", ignoreCase: true) == 0)
			{
				ControllerService.Instance.SetAudio(on: true);
			}
			else if (string.Compare(token.Value, "OFF", ignoreCase: true) == 0)
			{
				ControllerService.Instance.SetAudio(on: false);
			}
		}
	}

	private bool IsValidOperand(string op)
	{
		if (string.Compare(op, "ON", ignoreCase: true) != 0)
		{
			return string.Compare(op, "OFF", ignoreCase: true) == 0;
		}
		return true;
	}
}
