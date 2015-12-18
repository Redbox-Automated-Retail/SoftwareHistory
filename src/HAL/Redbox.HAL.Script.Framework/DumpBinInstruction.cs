using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class DumpBinInstruction : Instruction
{
	private enum ExpectedOperand
	{
		None,
		IsConfigured,
		SetCapacity,
		GetCapacity,
		ResetCounter,
		GetCurrentCount,
		ResetCounterMove,
		FileDisk,
		DumpInventory
	}

	public override string Mnemonic => "DUMPBIN";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count < 1 || !IsValidOperand(operandTokens[0].Value))
		{
			result.AddInvalidOperandError($"The {Mnemonic} instruction expects at lest one operand.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		ExpectedOperand expectedOperand = Enum<ExpectedOperand>.ParseIgnoringCase(base.Operands[0].Value, ExpectedOperand.None);
		DumpBin dumpBin = SpecialLocationLocatorService.Instance.FindDumpBin();
		switch (expectedOperand)
		{
		case ExpectedOperand.None:
		{
			string text = $"DumpBinInstruction: unrecognized operand '{base.Operands[0].Value}'.";
			context.ServiceLog(text);
			result.Errors.Add(Error.NewError("E001", "Unrecognized operand.", text));
			break;
		}
		case ExpectedOperand.IsConfigured:
			context.PushTop(dumpBin.Active);
			break;
		case ExpectedOperand.GetCapacity:
			context.PushTop(dumpBin.Capacity);
			break;
		case ExpectedOperand.SetCapacity:
			result.Errors.Add(Error.NewError("E011", "Unsupported operand.", "The SetCapacity operand is no longer supported."));
			break;
		case ExpectedOperand.GetCurrentCount:
			context.PushTop(dumpBin.CurrentCount());
			break;
		case ExpectedOperand.ResetCounter:
			dumpBin.ResetCount();
			break;
		case ExpectedOperand.FileDisk:
		{
			string iD = GetID(result, context);
			if (result.Errors.ContainsError() || string.IsNullOrEmpty(iD))
			{
				context.PushTop(MerchandizeResult.LookupFailure);
				break;
			}
			DumpBinThinHelper dumpBinThinHelper = new DumpBinThinHelper(result, context);
			context.PushTop(dumpBinThinHelper.ThinDisk(iD, MerchFlags.Thin).ToString().ToUpper());
			break;
		}
		case ExpectedOperand.ResetCounterMove:
			dumpBin.ResetCount();
			VMZHelperFunctions.MoveViewable(context);
			break;
		case ExpectedOperand.DumpInventory:
			DumpBin.Instance.DumpContents(context.AppLog);
			break;
		}
	}

	private bool IsValidOperand(string op)
	{
		return Enum<ExpectedOperand>.ParseIgnoringCase(op, ExpectedOperand.None) != ExpectedOperand.None;
	}
}
