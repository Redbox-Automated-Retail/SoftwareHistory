using System;
using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class InventoryInstruction : Instruction
{
	private enum InventoryOperands
	{
		None,
		Deck,
		Slot,
		SetUnknown,
		SetEmpty,
		GetUnknown,
		GetReturnTime,
		SoftSync,
		IsDuplicate,
		ClearAll,
		EmptyCount,
		CalcFile,
		SoftSyncResultStore
	}

	public override string Mnemonic => "INVENTORY";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count < 1)
		{
			result.Errors.Add(Error.NewError("C001", "Unexpected form", "The INVENTORY instruction expects a deck, slot and SET operands."));
		}
		else if (!ValidateOperand(tokensAfterMnemonic[0].Value) && !tokensAfterMnemonic[0].Value.Equals("MARK-EMPTY-UNKNOWN", StringComparison.CurrentCultureIgnoreCase))
		{
			result.Errors.Add(Error.NewError("C001", "Unexpected form", $"The operand {tokensAfterMnemonic[0].Value} for INVENTORY isn't recognized."));
		}
		else
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		InventoryOperands inventoryOperands = Enum<InventoryOperands>.ParseIgnoringCase(base.Operands[0].Value, InventoryOperands.None);
		if (inventoryOperands == InventoryOperands.None && base.Operands[0].Value.Equals("MARK-EMPTY-UNKNOWN", StringComparison.CurrentCultureIgnoreCase))
		{
			int? deckOperand = GetDeckOperand(result.Errors);
			if (!deckOperand.HasValue)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop("ERROR");
			}
			Deck deck = DecksManager.Instance.GetDeck(deckOperand.Value);
			List<int> list = deck.SwapEmptyWith("UNKNOWN", MerchFlags.None);
			ExecutionEngine.Instance.ActiveContext.PushTop(list);
			LogHelper.Instance.Log("Inventory: reset {0} empty slots on deck {1} to UNKNOWN", list, deckOperand.Value);
		}
		else if (InventoryOperands.GetUnknown == inventoryOperands)
		{
			List<Location> unknowns = ControllerConfiguration.Instance.GetUnknowns();
			foreach (Location item in unknowns)
			{
				Deck deck2 = DecksManager.Instance.GetDeck(item.Deck);
				DateTime? returnTime = deck2.GetReturnTime(item.Slot);
				if (!returnTime.HasValue)
				{
					ExecutionEngine.Instance.ActiveContext.PushTop("NONE");
				}
				else
				{
					ExecutionEngine.Instance.ActiveContext.PushTop(returnTime.Value);
				}
				ExecutionEngine.Instance.ActiveContext.PushTop(item.Slot);
				ExecutionEngine.Instance.ActiveContext.PushTop(item.Deck);
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(unknowns.Count);
		}
		else if (InventoryOperands.IsDuplicate == inventoryOperands)
		{
			KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("ID");
			string keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair, result.Errors);
			if (result.Errors.Count > 0)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop("FAILURE");
			}
			else
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(ControllerConfiguration.Instance.IsBarcodeDuplicate(keyValuePairValue, out var _));
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
			ExecutionEngine.Instance.ActiveContext.PushTop(ControllerConfiguration.Instance.GetEmptyCount());
		}
		else
		{
			if (InventoryOperands.SoftSyncResultStore == inventoryOperands)
			{
				return;
			}
			if (InventoryOperands.CalcFile == inventoryOperands)
			{
				using (EmptySearchResult emptySearchResult = new EmptySearchResult())
				{
					if (!emptySearchResult.FindMultipleEmptyLocations())
					{
						ServiceLogHelper.Instance.WriteInfoMessage("The machine is full, can't continue.");
						ExecutionEngine.Instance.ActiveContext.PushTop("MACHINEFULL");
					}
					else
					{
						ExecutionEngine.Instance.ActiveContext.PushTop(emptySearchResult.FoundEmpty);
					}
					return;
				}
			}
			int? nullableNumericOperand = GetNullableNumericOperand(result, "DECK");
			int? nullableNumericOperand2 = GetNullableNumericOperand(result, "SLOT");
			if (!nullableNumericOperand.HasValue || !nullableNumericOperand2.HasValue)
			{
				AddError("E004", "Deck or slot are undefined, reissue the INVENTORY instruction specifying both deck and slot values.", result.Errors);
				ExecutionEngine.Instance.ActiveContext.PushTop("ERROR");
				return;
			}
			Deck deck3 = DecksManager.Instance.GetDeck(nullableNumericOperand.Value);
			if (InventoryOperands.GetReturnTime == inventoryOperands)
			{
				DateTime? returnTime2 = deck3.GetReturnTime(nullableNumericOperand2.Value);
				ExecutionEngine.Instance.ActiveContext.PushTop((!returnTime2.HasValue) ? "NONE" : returnTime2.Value.ToString());
			}
			else if (InventoryOperands.SetUnknown == inventoryOperands)
			{
				deck3.MarkSlotUnknown(nullableNumericOperand2.Value);
				ExecutionEngine.Instance.ActiveContext.PushTop("SUCCESS");
			}
			else if (InventoryOperands.SetEmpty == inventoryOperands)
			{
				deck3.MarkSlotEmpty(nullableNumericOperand2.Value);
				ExecutionEngine.Instance.ActiveContext.PushTop("SUCCESS");
			}
			else
			{
				result.Errors.Add(Error.NewError("E999", "Invalid operand.", $"The operand {base.Operands[0].Value} is invalid."));
			}
		}
	}

	private bool ValidateOperand(string operand)
	{
		try
		{
			Enum.Parse(typeof(InventoryOperands), operand, ignoreCase: true);
			return true;
		}
		catch
		{
			return false;
		}
	}
}
