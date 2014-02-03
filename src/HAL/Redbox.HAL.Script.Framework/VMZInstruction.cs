using System;
using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class VMZInstruction : Instruction
{
	public override string Mnemonic => "VMZ";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic[0].Type != TokenType.Symbol)
		{
			AddError("C002", "Expected symbol: GET-THIN, GET-UNLOAD or MARK-EMPTY.", result.Errors);
		}
		else
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		if (string.Compare(base.Operands[0].Value, "IS-CONFIGURED", ignoreCase: true) == 0)
		{
			activeContext.PushTop(ControllerConfiguration.Instance.MerchandizingMode != MerchandizingMode.QLM);
		}
		else if (string.Compare(base.Operands[0].Value, "FETCH-LAST-THIN", ignoreCase: true) == 0)
		{
			ErrorList errors = new ErrorList();
			int? timeOutValue = GetTimeOutValue(errors);
			PerfFunctions.Wait(timeOutValue.HasValue ? timeOutValue.Value : 8000);
			VMZSearchResult result2 = default(VMZSearchResult);
			VMZ.Instance.FindLastLocation(MerchFlags.Thin, ref result2);
			if (!result2.Found)
			{
				VMZHelperFunctions.MoveViewable();
				ExecutionEngine.Instance.ActiveContext.PushTop("NONE");
				return;
			}
			MotionResult motionResult = ControllerService.Instance.MoveTo(result2.Deck, result2.Slot, MoveMode.Get, ExecutionEngine.Instance.ActiveContext.AppLog);
			if (motionResult != MotionResult.Success)
			{
				activeContext.PushTop(motionResult.ToString().ToUpper());
				return;
			}
			GetResult getResult = ControllerService.Instance.Get(testOnEmpty: false, activeContext.AppLog);
			if (getResult.Success)
			{
				VMZHelperFunctions.MoveViewable();
				activeContext.PushTop("SUCCESS");
			}
			else
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(getResult.ToString().ToUpper());
			}
		}
		else if (string.Compare(base.Operands[0].Value, "FETCH-LAST-REBALANCE", ignoreCase: true) == 0)
		{
			ErrorList errors2 = new ErrorList();
			int? timeOutValue2 = GetTimeOutValue(errors2);
			PerfFunctions.Wait(timeOutValue2.HasValue ? timeOutValue2.Value : 8000);
			VMZSearchResult result3 = default(VMZSearchResult);
			VMZ.Instance.FindLastLocation(MerchFlags.Rebalance, ref result3);
			if (!result3.Found)
			{
				VMZHelperFunctions.MoveViewable();
				ExecutionEngine.Instance.ActiveContext.PushTop("NONE");
				return;
			}
			MotionResult motionResult2 = ControllerService.Instance.MoveTo(result3.Deck, result3.Slot, MoveMode.Get, ExecutionEngine.Instance.ActiveContext.AppLog);
			if (motionResult2 != MotionResult.Success)
			{
				activeContext.PushTop(motionResult2.ToString().ToUpper());
				return;
			}
			GetResult getResult2 = ControllerService.Instance.Get(testOnEmpty: false, activeContext.AppLog);
			if (getResult2.Success)
			{
				VMZHelperFunctions.MoveViewable();
				activeContext.PushTop("SUCCESS");
			}
			else
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(getResult2.ToString().ToUpper());
			}
		}
		else if (string.Compare(base.Operands[0].Value, "NON-THIN-COUNT", ignoreCase: true) == 0)
		{
			AddError("E101", "NON-THIN-COUNT is not supported.", result.Errors);
		}
		else if (string.Compare(base.Operands[0].Value, "AGGREGATE-CLEAN", ignoreCase: true) == 0)
		{
			AggregateCleanData();
		}
		else if (string.Compare(base.Operands[0].Value, "CLEAN-SLOT", ignoreCase: true) == 0)
		{
			if (activeContext.SymbolTable.ContainsKey("_CHECK_RUNTIME") && CleanManager.Instance.ScheduledCleanIsRunningOutsideWindow())
			{
				activeContext.PushTop("CLEAN");
				return;
			}
			CleanResult result4 = CleanManager.Instance.CleanSlot();
			ProcessAnyTransfers(result4);
			if (result4.VisitedAllSlots)
			{
				activeContext.PushTop("CLEAN");
			}
			else if (result4.NoEmptySlots)
			{
				activeContext.PushTop(MerchandizeResult.MachineFull.ToString().ToUpper());
			}
			else if (result4.SourceDiskStuck)
			{
				activeContext.PushTop("EMPTYSTUCK");
			}
			else if (result4.HardwareError || PickerBoard.Instance.IsFull())
			{
				activeContext.PushTop("HARDWAREERROR");
			}
			else
			{
				activeContext.PushTop((result4.TransferFailures > 0) ? "TRANSFERFAILURE" : "SUCCESS");
			}
		}
		else if (string.Compare(base.Operands[0].Value, "MARK-UNLOAD", ignoreCase: true) == 0)
		{
			int num = 0;
			KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("LAST-DECK");
			int? keyValuePairValue = GetKeyValuePairValue<int?>(keyValuePair, result.Errors);
			if (result.Errors.Count > 0 || !keyValuePairValue.HasValue)
			{
				activeContext.PushTop("ERROR");
				return;
			}
			num = keyValuePairValue.Value;
			activeContext.PushTop(VMZ.Instance.ReserveUnload(num));
		}
		else if (string.Compare(base.Operands[0].Value, "GET-UNLOAD-PATTERN", ignoreCase: true) == 0)
		{
			result.Errors.Add(Error.NewError("E777", "Unsupported operand.", "The GET-UNLOAD-PATTERN is not a valid operand."));
			activeContext.PushTop(0);
		}
		else if (string.Compare(base.Operands[0].Value, "MARK-EMPTY", ignoreCase: true) == 0)
		{
			VMZSearchResult result5 = default(VMZSearchResult);
			VMZ.Instance.FindLastLocation(MerchFlags.Thin, ref result5);
			if (!result5.Found)
			{
				VMZ.Instance.FindLastLocation(MerchFlags.Rebalance, ref result5);
				if (!result5.Found)
				{
					ExecutionEngine.Instance.ActiveContext.PushTop(0);
					return;
				}
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(VMZ.Instance.ResetCompressedZone(ref result5));
		}
		else if (string.Compare(base.Operands[0].Value, "DUMP-ZONE", ignoreCase: true) == 0)
		{
			VMZ.Instance.DumpZone();
		}
		else if (string.Compare(base.Operands[0].Value, "DUMP-ZONE-TO-APPLOG", ignoreCase: true) == 0)
		{
			VMZ.Instance.DumpZone(ExecutionEngine.Instance.ActiveContext.AppLog);
		}
		else if (string.Compare(base.Operands[0].Value, "THIN-DISC", ignoreCase: true) == 0)
		{
			string iD = GetID(result);
			MerchandizeResult merchandizeResult = ThinHelper.MakeInstance(result).ThinDisk(iD, MerchFlags.Thin);
			if (merchandizeResult != MerchandizeResult.Success)
			{
				ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Info, "Thin-Disc returned status {0}", merchandizeResult);
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(merchandizeResult.ToString().ToUpper());
		}
		else if (string.Compare(base.Operands[0].Value, "THIN-REBALANCE", ignoreCase: true) == 0)
		{
			string iD2 = GetID(result);
			MerchandizeResult merchandizeResult2 = ThinHelper.MakeInstance(result).ThinDisk(iD2, MerchFlags.Rebalance);
			if (merchandizeResult2 != MerchandizeResult.Success)
			{
				ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Info, "Thin-Rebalance returned status {0}", merchandizeResult2);
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(merchandizeResult2.ToString().ToUpper());
		}
		else if (string.Compare(base.Operands[0].Value, "UNLOAD-DISC", ignoreCase: true) == 0)
		{
			MerchandizeResult merchandizeResult3 = VMZHelperFunctions.Instance.UnloadDisc(result);
			ExecutionEngine.Instance.ActiveContext.PushTop(merchandizeResult3.ToString().ToUpper());
		}
		else if (string.Compare(base.Operands[0].Value, "GET-LAST-THIN", ignoreCase: true) == 0)
		{
			LocateLast(MerchFlags.Thin);
		}
		else if (string.Compare(base.Operands[0].Value, "GET-LAST-REBALANCE", ignoreCase: true) == 0)
		{
			LocateLast(MerchFlags.Rebalance);
		}
		else if (string.Compare(base.Operands[0].Value, "THIN-ZONE-COUNT", ignoreCase: true) == 0)
		{
			int countInZone = VMZ.Instance.GetCountInZone(MerchFlags.Thin);
			ExecutionEngine.Instance.ActiveContext.PushTop((countInZone >= 0) ? countInZone : 0);
		}
		else if (string.Compare(base.Operands[0].Value, "REBALANCE-COUNT", ignoreCase: true) == 0)
		{
			int countInZone2 = VMZ.Instance.GetCountInZone(MerchFlags.Rebalance);
			ExecutionEngine.Instance.ActiveContext.PushTop((countInZone2 >= 0) ? countInZone2 : 0);
		}
		else if (string.Compare(base.Operands[0].Value, "GET-STATE", ignoreCase: true) == 0)
		{
			List<string> state = VMZ.Instance.GetState();
			foreach (string item in state)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(item);
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(state.Count);
			state.Clear();
		}
		else if (string.Compare(base.Operands[0].Value, "HALT-CLEAN", ignoreCase: true) == 0)
		{
			if (CleanManager.Instance.ScheduledCleanIsRunning())
			{
				CleanManager.Instance.HaltScheduledClean();
			}
		}
		else if (string.Compare(base.Operands[0].Value, "MOVE-VIEWABLE", ignoreCase: true) == 0)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop(VMZHelperFunctions.MoveViewable().ToString().ToUpper());
		}
		else if (string.Compare(base.Operands[0].Value, "ZONE-NON-THIN-COUNT", ignoreCase: true) == 0)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop(VMZ.Instance.GetNonThinItemsInCompressedZone().Count.ToString());
		}
	}

	private void ProcessAnyTransfers(CleanResult result)
	{
		if (result.Transfers != 0)
		{
			int num = 0;
			ErrorList errorList = new ErrorList();
			object symbolValue = ExecutionEngine.Instance.ActiveContext.GetSymbolValue("TRANSFER-COUNT", errorList);
			if (errorList.Count == 0)
			{
				num = (int)symbolValue;
			}
			num += result.Transfers;
			ExecutionEngine.Instance.ActiveContext.SetSymbolValue("TRANSFER-COUNT", num);
		}
	}

	private void LocateLast(MerchFlags flags)
	{
		VMZSearchResult result = default(VMZSearchResult);
		VMZ.Instance.FindLastLocation(flags, ref result);
		if (!result.Found)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop("NONE");
			return;
		}
		ExecutionEngine.Instance.ActiveContext.PushTop(result.Slot);
		ExecutionEngine.Instance.ActiveContext.PushTop(result.Deck);
		ExecutionEngine.Instance.ActiveContext.PushTop("FOUND");
	}

	private void AggregateCleanData()
	{
		int num = 0;
		ErrorList errorList = new ErrorList();
		object symbolValue = ExecutionEngine.Instance.ActiveContext.GetSymbolValue("TRANSFER-COUNT", errorList);
		if (errorList.Count == 0)
		{
			num = (int)symbolValue;
		}
		ResultHelper.Instance.CreateInfoResult("TransferredDiskCount", num.ToString());
		int countInZone = VMZ.Instance.GetCountInZone(MerchFlags.Thin);
		ResultHelper.Instance.CreateInfoResult("ThinCountInZone", countInZone.ToString());
		int countInZone2 = VMZ.Instance.GetCountInZone(MerchFlags.Rebalance);
		ResultHelper.Instance.CreateInfoResult("RebalanceCountInZone", countInZone2.ToString());
		VMZSearchResult result = default(VMZSearchResult);
		VMZ.Instance.FindLastLocation(MerchFlags.Thin, ref result);
		ResultHelper.Instance.CreateInfoResult("HighThinLocation", result.ToString());
		result.Clear();
		VMZ.Instance.FindLastLocation(MerchFlags.Rebalance, ref result);
		ResultHelper.Instance.CreateInfoResult("HighRebalanceLocation", result.ToString());
	}
}
