using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class QuickReturnInstruction : Instruction
{
	internal enum Forms
	{
		Unknown,
		PollStatus
	}

	public override string Mnemonic => "QUICKRETURN";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		Forms forms = CheckForm(operandTokens[0].Value);
		if (forms == Forms.Unknown)
		{
			result.AddInvalidOperandError($"Unknown form of QuickReturn: {operandTokens[0].Value}");
			return;
		}
		string text = string.Empty;
		Forms forms2 = forms;
		if (forms2 != Forms.PollStatus)
		{
			text = "Only the POLLSTATUS form of QUICKRETURN is supported.";
		}
		if (!string.IsNullOrEmpty(text))
		{
			result.AddInvalidOperandError(text);
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		if (base.Operands.Count == 0)
		{
			AddError("E002", "There are no operands for this instruction.", result.Errors);
			return;
		}
		Forms forms = CheckForm(base.Operands[0].Value);
		if (forms == Forms.Unknown)
		{
			AddError("E002", "There is an unknown form of this instruction.", result.Errors);
			return;
		}
		Forms forms2 = forms;
		if (forms2 == Forms.PollStatus)
		{
			context.PushTop("INACTIVE");
		}
		else
		{
			result.AddError(ExecutionErrors.InvalidOperand, "The QUICKRETURN operand is no longer supported.");
		}
	}

	private Forms CheckForm(string token)
	{
		Forms forms = Enum<Forms>.ParseIgnoringCase(token, Forms.Unknown);
		if (forms == Forms.Unknown)
		{
			string message = $"Unrecognized FORM argument: {token}";
			LogHelper.Instance.Log(message, LogEntryType.Error);
		}
		return forms;
	}
}
