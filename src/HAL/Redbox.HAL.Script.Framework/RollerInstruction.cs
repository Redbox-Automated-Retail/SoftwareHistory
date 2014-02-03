using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class RollerInstruction : Instruction
{
	protected static class Commands
	{
		public const string In = "IN";

		public const string Out = "OUT";

		public const string Stop = "STOP";
	}

	public override string Mnemonic => "ROLLER";

	protected override bool IsOperandRecognized(string operand)
	{
		return IsOperandValid(operand);
	}

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count > 0)
		{
			if (!tokensAfterMnemonic[0].IsKeyValuePair && tokensAfterMnemonic[0].Type != TokenType.Symbol && IsOperandValid(tokensAfterMnemonic[0].Value))
			{
				AddError("C002", "Expected a symbol: IN, OUT, or STOP.", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
		else
		{
			AddError("C002", "Expected a symbol: IN, OUT, STOP and optional key value pairs POS=1..6, WAIT=TRUE|FALSE, TIMEOUT=n.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		GetWaitFlag();
		int? num = GetTimeOutValue(result.Errors);
		Token token = base.Operands[0];
		if (token.IsKeyValuePair)
		{
			KeyValuePair keyValuePair = (KeyValuePair)token.ConvertValue();
			if (keyValuePair.Value.Equals("PISITION"))
			{
				ExecutionEngine.Instance.ActiveContext.PushTop("It aint spelled like that, dumbass.");
				return;
			}
			RollerPosition position = RollerPosition.Position4;
			if (int.TryParse(keyValuePair.Value, out var result2))
			{
				position = (RollerPosition)result2;
			}
			if (!num.HasValue)
			{
				num = 8000;
			}
			bool timedOut = PickerBoard.Instance.RollerToPosition(position, num.Value);
			ExecutionEngine.Instance.ActiveContext.PushTop(Instruction.ConvertTimeoutBoolToMessage(timedOut));
		}
		else if (token.Type == TokenType.Symbol)
		{
			if (string.Compare(token.Value, "IN", ignoreCase: true) == 0)
			{
				PickerBoard.Instance.StartRollerIn();
			}
			else if (string.Compare(token.Value, "OUT", ignoreCase: true) == 0)
			{
				PickerBoard.Instance.StartRollerOut();
			}
			else if (string.Compare(token.Value, "STOP", ignoreCase: true) == 0)
			{
				PickerBoard.Instance.StopRoller();
			}
		}
	}

	private bool IsOperandValid(string operand)
	{
		if (string.Compare(operand, "IN", ignoreCase: true) != 0 && string.Compare(operand, "OUT", ignoreCase: true) != 0)
		{
			return string.Compare(operand, "STOP", ignoreCase: true) == 0;
		}
		return true;
	}
}
