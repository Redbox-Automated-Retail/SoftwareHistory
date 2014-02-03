using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class LocInstruction : Instruction
{
	private const string ExcludedSlotsKey = "__ExcludedSlots__";

	private static int PearsonCount;

	public override string Mnemonic => "LOC";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		int count = tokens.GetTokensAfterMnemonic().Count;
		if (count > 0)
		{
			string value = tokens.GetTokensAfterMnemonic()[0].Value;
			if (count == 1 && value.Equals("GET-EXCLUDED", StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.Add(tokens.GetTokensAfterMnemonic()[0]);
			}
			else if (value.Equals("SLOTTYPE", StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("IS-EXCLUDED".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("COMPUTE-EXCLUDED-LOCS".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("AT-DRUM-LOC".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("CLEARRETURNTIME".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("MARKRETURNTIME".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("GETRETURNTIME".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("SETRETURNTIME".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("SETINVENTORY".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("GET-INFO".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("RESET".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("RESETRT".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("SETID".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("SYNC".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("MOVE-GET".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("TIMESTAMP".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("MOVE-PUT".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("MOVE-PUT".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("INC-STUCK".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else if ("CLEAR-STUCK".Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				base.Operands.AddRange(tokens.GetTokensAfterMnemonic());
			}
			else
			{
				AddError("C002", "The LOC instruction supports an optional GET-EXCLUDED or SLOTTYPE operand.", result.Errors);
			}
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		if (base.Operands.Count >= 1)
		{
			Location location;
			if (base.Operands[0].Value.Equals("CLEARRETURNTIME"))
			{
				SetReturnTime(result, null, leaveResult: true);
			}
			else if (base.Operands[0].Value.Equals("INC-STUCK"))
			{
				if (!GetLocation(result, out location))
				{
					ExecutionEngine.Instance.ActiveContext.PushTop("ERROR");
				}
				else
				{
					ExecutionEngine.Instance.ActiveContext.PushTop(location.IncrementEmptyStuck() ? "SUCCESS" : "ERROR");
				}
			}
			else if (base.Operands[0].Value.Equals("CLEAR-STUCK"))
			{
				if (!GetLocation(result, out location))
				{
					ExecutionEngine.Instance.ActiveContext.PushTop("ERROR");
				}
				else
				{
					ExecutionEngine.Instance.ActiveContext.PushTop(location.ResetEmptyStuck() ? "SUCCESS" : "ERROR");
				}
			}
			else if (base.Operands[0].Value.Equals("RESET"))
			{
				GetLocation(result, out var deck, out var slot);
				Deck deck2 = DecksManager.Instance.GetDeck(deck);
				ExecutionEngine.Instance.ActiveContext.PushTop(deck2.ResetLocation(slot) ? "SUCCESS" : "ERROR");
			}
			else if (base.Operands[0].Value.Equals("GET-INFO"))
			{
				if (GetLocation(result, out var deck3, out var slot2))
				{
					Deck deck4 = DecksManager.Instance.GetDeck(deck3);
					int emptyStuckCount = deck4.GetEmptyStuckCount(slot2);
					ExecutionEngine.Instance.ActiveContext.PushTop(emptyStuckCount);
					string merchDescription = deck4.GetMerchDescription(slot2);
					ExecutionEngine.Instance.ActiveContext.PushTop(merchDescription);
					DateTime? returnTime = deck4.GetReturnTime(slot2);
					ExecutionEngine.Instance.ActiveContext.PushTop(GetDateRepresentation(returnTime));
					string idForSlot = deck4.GetIdForSlot(slot2);
					ExecutionEngine.Instance.ActiveContext.PushTop(idForSlot);
					ExecutionEngine.Instance.ActiveContext.PushTop("SUCCESS");
				}
				else
				{
					ExecutionEngine.Instance.ActiveContext.PushTop("ERROR");
				}
			}
			else if (base.Operands[0].Value.Equals("GETRETURNTIME"))
			{
				if (GetLocation(result, out var deck5, out var slot3))
				{
					Deck deck6 = DecksManager.Instance.GetDeck(deck5);
					DateTime? returnTime2 = deck6.GetReturnTime(slot3);
					ExecutionEngine.Instance.ActiveContext.PushTop(GetDateRepresentation(returnTime2));
				}
				else
				{
					ExecutionEngine.Instance.ActiveContext.PushTop("ERROR");
				}
			}
			else if (base.Operands[0].Value.Equals("MARKRETURNTIME"))
			{
				SetReturnTime(result, DateTime.Now, leaveResult: true);
			}
			else if (base.Operands[0].Value.Equals("AT-DRUM-LOC"))
			{
				if (ControllerService.Instance.Location.IsValid)
				{
					ExecutionEngine.Instance.ActiveContext.PushTop(true);
				}
				else
				{
					ExecutionEngine.Instance.ActiveContext.PushTop(false);
				}
			}
			else if (base.Operands[0].Value.Equals("SETINVENTORY"))
			{
				KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("ID");
				string keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair, result.Errors);
				if (result.Errors.Count > 0)
				{
					ExecutionEngine.Instance.ActiveContext.PushTop("FAILURE");
					return;
				}
				bool flag = SetBarcode(result, keyValuePairValue);
				ExecutionEngine.Instance.ActiveContext.PushTop(flag ? "SUCCESS" : "FAILURE");
			}
			else if (base.Operands[0].Value.Equals("SETID"))
			{
				if (base.Operands[1].Type != TokenType.StringLiteral)
				{
					AddError("E999", "SETID expects a string literal.", result.Errors);
				}
				else if (!SetBarcode(result, base.Operands[1].Value))
				{
					ExecutionEngine.Instance.ActiveContext.PushTop("ERROR");
				}
			}
			else if (base.Operands[0].Value.Equals("RESETRT"))
			{
				if (base.Operands[1].Type != TokenType.StringLiteral)
				{
					AddError("E999", "Unspecified context exit; check the logs.", result.Errors);
					return;
				}
				string value = base.Operands[1].Value;
				if (!DateTime.TryParse(value, out var result2))
				{
					LogHelper.Instance.Log("Location RESETRT Instruction - Unable to parse date/time information.", LogEntryType.Error);
					AddError("E999", "Unspecified context exit; check the logs.", result.Errors);
				}
				else
				{
					SetReturnTime(result, result2, leaveResult: true);
				}
			}
			else if (base.Operands[0].Value.Equals("TIMESTAMP"))
			{
				SetReturnTime(result, DateTime.Now, leaveResult: false);
			}
			else if (base.Operands[0].Value.Equals("SETRETURNTIME"))
			{
				if (base.Operands[1].Type != TokenType.Symbol)
				{
					LogHelper.Instance.Log("Location SETRETURNTIME - Operand expected to be a symbol", LogEntryType.Error);
					AddError("E999", "Unspecified context exit; check the logs.", result.Errors);
				}
				string text = ExecutionEngine.Instance.ActiveContext.GetSymbolValue(base.Operands[1].Value, result.Errors) as string;
				if (!"NONE".Equals(text))
				{
					if (!DateTime.TryParse(text, out var result3))
					{
						LogHelper.Instance.Log("Location SETRETURNTIME Instruction - Unable to parse date/time information.", LogEntryType.Error);
						AddError("E999", "Unspecified context exit; check the logs.", result.Errors);
					}
					else
					{
						SetReturnTime(result, result3, leaveResult: true);
					}
				}
			}
			else if (base.Operands[0].Value.Equals("IS-EXCLUDED"))
			{
				if (GetLocation(result, out var location2))
				{
					ExecutionEngine.Instance.ActiveContext.PushTop(location2.IsExcluded);
					ExecutionEngine.Instance.ActiveContext.PushTop("SUCCESS");
				}
				else
				{
					ExecutionEngine.Instance.ActiveContext.PushTop("LocationOutOfRange".ToUpper());
				}
			}
			else if (base.Operands[0].Value.Equals("SLOTTYPE"))
			{
				if (!GetLocation(result, out var location3))
				{
					ExecutionEngine.Instance.ActiveContext.PushTop("UNKNOWN");
				}
				else
				{
					ExecutionEngine.Instance.ActiveContext.PushTop(location3.IsWide ? "WIDE" : "NOTWIDE");
				}
			}
			else
			{
				result.Errors.Add(Error.NewError("E001", "Unrecognized LOC form.", $"The operand {base.Operands[0].Value} isn't recognized."));
			}
		}
		else if (ControllerService.Instance.IsAtVendLocation)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop("VENDDOOR");
			ExecutionEngine.Instance.ActiveContext.PushTop("VENDDOOR");
		}
		else if (!ControllerService.Instance.Location.IsValid)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop("UNDEFINED");
			ExecutionEngine.Instance.ActiveContext.PushTop("UNDEFINED");
		}
		else
		{
			ExecutionEngine.Instance.ActiveContext.PushTop(ControllerService.Instance.Location.Slot);
			ExecutionEngine.Instance.ActiveContext.PushTop(ControllerService.Instance.Location.Deck);
		}
	}

	protected override bool IsOperandRecognized(string op)
	{
		return IsRecognizedOperand(op);
	}

	private void SetReturnTime(ExecutionResult result, DateTime? newTime, bool leaveResult)
	{
		if (!GetLocation(result, out var deck, out var slot))
		{
			LogHelper.Instance.Log("Unable to get deck and slot information.", LogEntryType.Error);
			ExecutionEngine.Instance.ActiveContext.PushTop("ERROR");
			return;
		}
		Deck deck2 = DecksManager.Instance.GetDeck(deck);
		bool flag = deck2.UpdateReturnTime(slot, newTime);
		if (leaveResult)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop(flag);
		}
	}

	private bool SetBarcode(ExecutionResult result, string id)
	{
		if (!GetLocation(result, out var deck, out var slot))
		{
			LogHelper.Instance.Log("Unable to get deck and slot information.", LogEntryType.Error);
			return false;
		}
		Deck deck2 = DecksManager.Instance.GetDeck(deck);
		deck2.SetSlotMatrix(slot, id);
		return true;
	}

	private bool IsRecognizedOperand(string op)
	{
		switch (op.ToUpper())
		{
		case "SLOTTYPE":
		case "GET-EXCLUDED":
		case "IS-EXCLUDED":
		case "CLEARRETURNTIME":
		case "MARKRETURNTIME":
		case "GETRETURNTIME":
		case "SETRETURNTIME":
		case "GET-INFO":
		case "RESETRT":
		case "SETID":
		case "SYNC":
		case "MOVE-GET":
		case "MOVE-PUT":
		case "SETINVENTORY":
		case "RESET":
		case "TIMESTAMP":
			return true;
		default:
			return false;
		}
	}

	private static string GetNextPearson()
	{
		switch (PearsonCount)
		{
		case 0:
			PearsonCount++;
			return "APPLE STORE";
		case 1:
			PearsonCount++;
			return "CRAWLSPACE";
		case 2:
			PearsonCount = 0;
			return "PICKING UP CANS IN ROAD.";
		default:
			return "HOW DO I KNOW?";
		}
	}
}
