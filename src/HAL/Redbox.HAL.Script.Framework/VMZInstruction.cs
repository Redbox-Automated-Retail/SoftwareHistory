using System;
using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class VMZInstruction : Instruction
{
	public override string Mnemonic => "VMZ";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens[0].Type != TokenType.Symbol)
		{
			result.AddInvalidOperandError("Expected symbol: GET-THIN, GET-UNLOAD or MARK-EMPTY.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		if (string.Compare(base.Operands[0].Value, "IS-CONFIGURED", ignoreCase: true) == 0)
		{
			context.PushTop(ControllerConfiguration.Instance.IsVMZMachine);
		}
		else if (string.Compare(base.Operands[0].Value, "REGISTER-CLEAN", ignoreCase: true) == 0)
		{
			CleanManager.Instance.Register(context);
		}
		else if (string.Compare(base.Operands[0].Value, "RANDOMIZE-CLEAN", ignoreCase: true) == 0)
		{
			CleanManager.Instance.RandomizeCleanTime();
		}
		else if (string.Compare(base.Operands[0].Value, "FETCH-LAST-THIN", ignoreCase: true) == 0)
		{
			ErrorList errors = new ErrorList();
			int? timeOutValue = GetTimeOutValue(errors, context);
			PerfFunctions.Wait(timeOutValue.HasValue ? timeOutValue.Value : 8000);
			VMZSearchResult result2 = default(VMZSearchResult);
			VMZ.Instance.FindHighestLocation(MerchFlags.Thin, ref result2);
			if (!result2.Found)
			{
				VMZHelperFunctions.MoveViewable(context);
				context.PushTop("NONE");
				return;
			}
			MotionResult motionResult = ControllerService.Instance.MoveTo(result2.Deck, result2.Slot, MoveMode.Get, context.AppLog);
			if (motionResult != MotionResult.Success)
			{
				context.PushTop(motionResult.ToString().ToUpper());
				return;
			}
			GetResult getResult = ControllerService.Instance.Get(context.AppLog);
			if (getResult.Success)
			{
				VMZHelperFunctions.MoveViewable(context);
				context.PushTop("SUCCESS");
			}
			else
			{
				context.PushTop(getResult.ToString().ToUpper());
			}
		}
		else if (string.Compare(base.Operands[0].Value, "FETCH-LAST-REBALANCE", ignoreCase: true) == 0)
		{
			ErrorList errors2 = new ErrorList();
			int? timeOutValue2 = GetTimeOutValue(errors2, context);
			PerfFunctions.Wait(timeOutValue2.HasValue ? timeOutValue2.Value : 8000);
			VMZSearchResult result3 = default(VMZSearchResult);
			VMZ.Instance.FindHighestLocation(MerchFlags.Rebalance, ref result3);
			if (!result3.Found)
			{
				VMZHelperFunctions.MoveViewable(context);
				context.PushTop("NONE");
				return;
			}
			MotionResult motionResult2 = ControllerService.Instance.MoveTo(result3.Deck, result3.Slot, MoveMode.Get, context.AppLog);
			if (motionResult2 != MotionResult.Success)
			{
				context.PushTop(motionResult2.ToString().ToUpper());
				return;
			}
			GetResult getResult2 = ControllerService.Instance.Get(context.AppLog);
			if (getResult2.Success)
			{
				VMZHelperFunctions.MoveViewable(context);
				context.PushTop("SUCCESS");
			}
			else
			{
				context.PushTop(getResult2.ToString().ToUpper());
			}
		}
		else if (string.Compare(base.Operands[0].Value, "NON-THIN-COUNT", ignoreCase: true) == 0)
		{
			AddError("E101", "NON-THIN-COUNT is not supported.", result.Errors);
		}
		else if (string.Compare(base.Operands[0].Value, "AGGREGATE-CLEAN", ignoreCase: true) == 0)
		{
			AggregateCleanData(context);
		}
		else if (string.Compare(base.Operands[0].Value, "CLEAN-SLOT", ignoreCase: true) == 0)
		{
			if (context.IsAutoScheduled && CleanManager.Instance.ScheduledCleanIsRunningOutsideWindow())
			{
				context.PushTop("CLEAN");
				return;
			}
			CleanResult result4 = default(CleanResult);
			CleanManager.Instance.CleanSlot(ref result4, context, SegmentManager.Instance);
			ProcessAnyTransfers(result4, context);
			if (result4.VisitedAllSlots)
			{
				context.PushTop("CLEAN");
			}
			else if (result4.NoEmptySlots)
			{
				context.PushTop(MerchandizeResult.MachineFull.ToString().ToUpper());
			}
			else
			{
				context.PushTop((result4.TransferFailures > 0) ? "TRANSFERFAILURE" : "SUCCESS");
			}
		}
		else if (string.Compare(base.Operands[0].Value, "MARK-UNLOAD", ignoreCase: true) == 0)
		{
			int num = 0;
			KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("LAST-DECK");
			int? keyValuePairValue = GetKeyValuePairValue<int?>(keyValuePair, result.Errors, context);
			if (result.Errors.Count > 0 || !keyValuePairValue.HasValue)
			{
				context.PushTop("ERROR");
				return;
			}
			num = keyValuePairValue.Value;
			context.PushTop(VMZ.Instance.ReserveUnload(num));
		}
		else if (string.Compare(base.Operands[0].Value, "MARK-EMPTY", ignoreCase: true) == 0)
		{
			VMZSearchResult high = default(VMZSearchResult);
			SegmentManager.Instance.FindHighest(ref high);
			if (!high.Found)
			{
				context.PushTop(0);
			}
			else
			{
				context.PushTop(VMZ.Instance.ResetCompressedZone(ref high));
			}
		}
		else if (string.Compare(base.Operands[0].Value, "DUMP-ZONE", ignoreCase: true) == 0)
		{
			AddError("E002", "The operand DUMP-ZONE is no longer supported.", result.Errors);
		}
		else if (string.Compare(base.Operands[0].Value, "DUMP-ZONE-TO-APPLOG", ignoreCase: true) == 0)
		{
			VMZ.Instance.DumpZone(context.AppLog);
		}
		else if (string.Compare(base.Operands[0].Value, "THIN-DISC", ignoreCase: true) == 0)
		{
			ThinType(result, context, MerchFlags.Thin);
		}
		else if (string.Compare(base.Operands[0].Value, "THIN-REBALANCE", ignoreCase: true) == 0)
		{
			ThinType(result, context, MerchFlags.Rebalance);
		}
		else if (string.Compare(base.Operands[0].Value, "THIN-REDEPLOY", ignoreCase: true) == 0)
		{
			ThinType(result, context, MerchFlags.ThinRedeploy);
		}
		else if (string.Compare(base.Operands[0].Value, "UNLOAD-DISC", ignoreCase: true) == 0)
		{
			MerchandizeResult merchandizeResult = VMZHelperFunctions.Instance.UnloadDisc(result, context);
			context.PushTop(merchandizeResult.ToString().ToUpper());
		}
		else if (string.Compare(base.Operands[0].Value, "GET-LAST-THIN", ignoreCase: true) == 0)
		{
			LocateLast(MerchFlags.Thin, context);
		}
		else if (string.Compare(base.Operands[0].Value, "GET-LAST-REBALANCE", ignoreCase: true) == 0)
		{
			LocateLast(MerchFlags.Rebalance, context);
		}
		else if (string.Compare(base.Operands[0].Value, "GET-LAST-THIN-REDEPLOY", ignoreCase: true) == 0)
		{
			LocateLast(MerchFlags.ThinRedeploy, context);
		}
		else if (string.Compare(base.Operands[0].Value, "THIN-ZONE-COUNT", ignoreCase: true) == 0)
		{
			PushCount(MerchFlags.Thin, context);
		}
		else if (string.Compare(base.Operands[0].Value, "REBALANCE-COUNT", ignoreCase: true) == 0)
		{
			PushCount(MerchFlags.Rebalance, context);
		}
		else if (string.Compare(base.Operands[0].Value, "THIN-REDEPLOY-COUNT", ignoreCase: true) == 0)
		{
			PushCount(MerchFlags.ThinRedeploy, context);
		}
		else if (string.Compare(base.Operands[0].Value, "MOVE-VIEWABLE", ignoreCase: true) == 0)
		{
			context.PushTop(VMZHelperFunctions.MoveViewable(context).ToString().ToUpper());
		}
		else if (string.Compare(base.Operands[0].Value, "ZONE-NON-THIN-COUNT", ignoreCase: true) == 0)
		{
			List<Location> nonThinItemsInCompressedZone = SegmentManager.Instance.GetNonThinItemsInCompressedZone();
			context.PushTop(nonThinItemsInCompressedZone.Count.ToString());
			nonThinItemsInCompressedZone.Clear();
		}
	}

	private void PushCount(MerchFlags flags, ExecutionContext context)
	{
		MerchSegment merchSegment = MerchSegmentFactory.Get(flags);
		int num = ((context != null) ? merchSegment.ItemCount() : 0);
		context.PushTop(num);
	}

	private void ThinType(ExecutionResult result, ExecutionContext context, MerchFlags flags)
	{
		string iD = GetID(result, context);
		MerchandizeResult merchandizeResult = ThinHelper.MakeInstance(result, context).ThinDisk(iD, flags);
		if (merchandizeResult != MerchandizeResult.Success)
		{
			context.ServiceLog("Thin {0} returned status {1}", flags.ToString(), merchandizeResult);
		}
		context.PushTop(merchandizeResult.ToString().ToUpper());
	}

	private void ProcessAnyTransfers(CleanResult result, ExecutionContext context)
	{
		if (result.Transfers != 0)
		{
			int num = 0;
			ErrorList errorList = new ErrorList();
			object symbolValue = context.GetSymbolValue("TRANSFER-COUNT", errorList);
			if (errorList.Count == 0)
			{
				num = (int)symbolValue;
			}
			num += result.Transfers;
			context.SetSymbolValue("TRANSFER-COUNT", num);
		}
	}

	private void LocateLast(MerchFlags flags, ExecutionContext context)
	{
		VMZSearchResult result = default(VMZSearchResult);
		VMZ.Instance.FindHighestLocation(flags, ref result);
		if (!result.Found)
		{
			context.PushTop("NONE");
			return;
		}
		context.PushTop(result.Slot);
		context.PushTop(result.Deck);
		context.PushTop("FOUND");
	}

	private void AggregateCleanData(ExecutionContext ctx)
	{
		int num = 0;
		ErrorList errorList = new ErrorList();
		object symbolValue = ctx.GetSymbolValue("TRANSFER-COUNT", errorList);
		if (errorList.Count == 0)
		{
			num = (int)symbolValue;
		}
		ctx.CreateInfoResult("TransferredDiskCount", num.ToString());
		foreach (MerchFlags item in SegmentManager.Instance.DecreasingPriorityOrder)
		{
			MerchSegment merchSegment = MerchSegmentFactory.Get(item);
			ctx.CreateInfoResult($"{item.ToString()}CountInZone", merchSegment.ItemCount().ToString());
			VMZSearchResult result = default(VMZSearchResult);
			VMZ.Instance.FindHighestLocation(item, ref result);
			ctx.CreateInfoResult($"High{item.ToString()}Location", result.ToString());
		}
	}
}
