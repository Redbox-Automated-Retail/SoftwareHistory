using System;
using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class InventoryInstruction : Instruction
{
	private enum InventoryOperands
	{
		None,
		Deck,
		Slot,
		SetUnknown,
		SetEmpty,
		GetReturnTime,
		SoftSync,
		IsDuplicate,
		ClearAll,
		EmptyCount
	}

	public override string Mnemonic => "INVENTORY";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (Enum<InventoryOperands>.ParseIgnoringCase(operandTokens[0].Value, InventoryOperands.None) == InventoryOperands.None && !operandTokens[0].Value.Equals("MARK-EMPTY-UNKNOWN", StringComparison.CurrentCultureIgnoreCase))
		{
			result.Errors.Add(Error.NewError("C001", "Unexpected form", $"The operand {operandTokens[0].Value} for INVENTORY isn't recognized."));
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		InventoryOperands inventoryOperands = Enum<InventoryOperands>.ParseIgnoringCase(base.Operands[0].Value, InventoryOperands.None);
		Location location;
		if (inventoryOperands == InventoryOperands.None && base.Operands[0].Value.Equals("MARK-EMPTY-UNKNOWN", StringComparison.CurrentCultureIgnoreCase))
		{
			int? deckOperand = GetDeckOperand(result.Errors, context);
			if (!deckOperand.HasValue)
			{
				context.PushTop("ERROR");
			}
			Deck deck = DecksManager.Instance.GetDeck(deckOperand.Value);
			Range range = new Range(1, deck.NumberOfSlots);
			List<int> list = deck.SwapEmptyWith("UNKNOWN", MerchFlags.None, range);
			context.PushTop(list);
			LogHelper.Instance.Log("Inventory: reset {0} empty slots on deck {1} to UNKNOWN", list, deckOperand.Value);
		}
		else if (InventoryOperands.IsDuplicate == inventoryOperands)
		{
			KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("ID");
			string keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair, result.Errors, context);
			if (result.Errors.Count > 0)
			{
				context.PushTop("FAILURE");
			}
			else
			{
				context.PushTop(InventoryService.Instance.IsBarcodeDuplicate(keyValuePairValue, out var _));
			}
		}
		else if (InventoryOperands.ClearAll == inventoryOperands)
		{
			DecksManager.Instance.ForAllDecksDo(delegate(Deck _d0)
			{
				_d0.MarkInventory("EMPTY");
				return true;
			});
		}
		else if (InventoryOperands.EmptyCount == inventoryOperands)
		{
			context.PushTop(InventoryService.Instance.GetEmptyCount());
		}
		else if (!GetLocation(result, context, out location))
		{
			AddError("E004", "Deck or slot are undefined, reissue the INVENTORY instruction specifying both deck and slot values.", result.Errors);
			context.PushTop("ERROR");
		}
		else if (InventoryOperands.GetReturnTime == inventoryOperands)
		{
			DateTime? returnTime = location.ReturnTime;
			context.PushTop((!returnTime.HasValue) ? "NONE" : returnTime.Value.ToString());
		}
		else if (InventoryOperands.SetUnknown == inventoryOperands)
		{
			location.MarkUnknown();
			context.PushTop("SUCCESS");
		}
		else if (InventoryOperands.SetEmpty == inventoryOperands)
		{
			location.Reset();
			context.PushTop("SUCCESS");
		}
		else
		{
			result.Errors.Add(Error.NewError("E999", "Invalid operand.", $"The operand {base.Operands[0].Value} is invalid."));
		}
	}
}
