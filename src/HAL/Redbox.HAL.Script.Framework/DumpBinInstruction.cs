using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class DumpBinInstruction : Instruction
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

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count <= 0)
		{
			result.Errors.Add(Error.NewError("C002", "Expected a 'GetCapacity', 'IsConfigured' or 'SetCapacity' operand.", "Correct the statement and recompile."));
		}
		else if (IsValidOperand(tokensAfterMnemonic[0].Value))
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
		else
		{
			result.Errors.Add(Error.NewError("C002", "Expected a 'Query' or 'IsConfigured' operand.", "Correct the statement and recompile."));
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		ExpectedOperand expectedOperand = ConvertToOperand(base.Operands[0].Value);
		DumpBin dumpBin = SpecialLocationLocatorService.Instance.FindDumpBin();
		switch (expectedOperand)
		{
		case ExpectedOperand.None:
		{
			string text = $"DumpBinInstruction: unrecognized operand '{base.Operands[0].Value}'.";
			ServiceLogHelper.Instance.WriteInfoMessage(text);
			result.Errors.Add(Error.NewError("E001", "Unrecognized operand.", text));
			break;
		}
		case ExpectedOperand.IsConfigured:
			ExecutionEngine.Instance.ActiveContext.PushTop(dumpBin.Active);
			break;
		case ExpectedOperand.GetCapacity:
			ExecutionEngine.Instance.ActiveContext.PushTop(dumpBin.Capacity);
			break;
		case ExpectedOperand.SetCapacity:
			result.Errors.Add(Error.NewError("E011", "Unsupported operand.", "The SetCapacity operand is no longer supported."));
			break;
		case ExpectedOperand.GetCurrentCount:
			ExecutionEngine.Instance.ActiveContext.PushTop(dumpBin.CurrentCount());
			break;
		case ExpectedOperand.ResetCounter:
			dumpBin.ResetCount();
			break;
		case ExpectedOperand.FileDisk:
		{
			string iD = GetID(result);
			if (result.Errors.ContainsError() || string.IsNullOrEmpty(iD))
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(MerchandizeResult.LookupFailure);
				break;
			}
			DumpBinThinHelper dumpBinThinHelper = new DumpBinThinHelper(result);
			ExecutionEngine.Instance.ActiveContext.PushTop(dumpBinThinHelper.ThinDisk(iD, MerchFlags.Thin).ToString().ToUpper());
			break;
		}
		case ExpectedOperand.ResetCounterMove:
			dumpBin.ResetCount();
			VMZHelperFunctions.MoveViewable();
			break;
		case ExpectedOperand.DumpInventory:
			DumpBin.Instance.DumpContents(ExecutionEngine.Instance.ActiveContext.AppLog);
			break;
		}
	}

	private bool IsValidOperand(string op)
	{
		return ConvertToOperand(op) != ExpectedOperand.None;
	}

	private ExpectedOperand ConvertToOperand(string operand)
	{
		ExpectedOperand expectedOperand = ExpectedOperand.None;
		try
		{
			return (ExpectedOperand)Enum.Parse(typeof(ExpectedOperand), operand, ignoreCase: true);
		}
		catch (ArgumentException)
		{
			return ExpectedOperand.None;
		}
	}
}
