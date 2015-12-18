using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class LocInstruction : Instruction
{
	private static class Keys
	{
		internal const string ClearReturnTime = "CLEARRETURNTIME";

		internal const string IncrementStuck = "INC-STUCK";

		internal const string ClearStuck = "CLEAR-STUCK";

		internal const string Reset = "RESET";

		internal const string GetInfo = "GET-INFO";

		internal const string GetReturnTime = "GETRETURNTIME";

		internal const string MarkReturnTime = "MARKRETURNTIME";

		internal const string AtDrumLocation = "AT-DRUM-LOC";

		internal const string SetInventory = "SETINVENTORY";

		internal const string ResetReturnTime = "RESETRT";

		internal const string Timestamp = "TIMESTAMP";

		internal const string SetReturnTime = "SETRETURNTIME";

		internal const string IsExcluded = "IS-EXCLUDED";

		internal const string SlotType = "SLOTTYPE";

		internal static bool Recognized(string s)
		{
			if (string.Compare(s, "CLEARRETURNTIME", ignoreCase: true) != 0 && string.Compare(s, "INC-STUCK", ignoreCase: true) != 0 && string.Compare(s, "CLEAR-STUCK", ignoreCase: true) != 0 && string.Compare(s, "RESET", ignoreCase: true) != 0 && string.Compare(s, "GET-INFO", ignoreCase: true) != 0 && string.Compare(s, "GETRETURNTIME", ignoreCase: true) != 0 && string.Compare(s, "MARKRETURNTIME", ignoreCase: true) != 0 && string.Compare(s, "AT-DRUM-LOC", ignoreCase: true) != 0 && string.Compare(s, "SETINVENTORY", ignoreCase: true) != 0 && string.Compare(s, "RESETRT", ignoreCase: true) != 0 && string.Compare(s, "TIMESTAMP", ignoreCase: true) != 0 && string.Compare(s, "SETRETURNTIME", ignoreCase: true) != 0 && string.Compare(s, "IS-EXCLUDED", ignoreCase: true) != 0)
			{
				return string.Compare(s, "SLOTTYPE", ignoreCase: true) == 0;
			}
			return true;
		}
	}

	public override string Mnemonic => "LOC";

	protected override ExpectedOperands Expected => ExpectedOperands.Optional;

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (!Keys.Recognized(operandTokens[0].Value))
		{
			result.AddError(ExecutionErrors.InvalidOperand, $"The '{operandTokens[0].Value}' operand is not recognized.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		if (base.Operands.Count >= 1)
		{
			Location location;
			if (base.Operands[0].Value.Equals("CLEARRETURNTIME"))
			{
				SetReturnTime(result, null, leaveResult: true, context);
			}
			else if (base.Operands[0].Value.Equals("INC-STUCK"))
			{
				if (!GetLocation(result, context, out location))
				{
					context.PushTop("ERROR");
				}
				else
				{
					context.PushTop(location.IncrementEmptyStuck() ? "SUCCESS" : "ERROR");
				}
			}
			else if (base.Operands[0].Value.Equals("CLEAR-STUCK"))
			{
				if (!GetLocation(result, context, out location))
				{
					context.PushTop("ERROR");
				}
				else
				{
					context.PushTop(location.ResetEmptyStuck() ? "SUCCESS" : "ERROR");
				}
			}
			else if (base.Operands[0].Value.Equals("RESET"))
			{
				GetLocation(result, context, out var location2);
				context.PushTop(location2.Reset() ? "SUCCESS" : "ERROR");
			}
			else if (base.Operands[0].Value.Equals("GET-INFO"))
			{
				if (GetLocation(result, context, out var location3))
				{
					int stuckCount = location3.StuckCount;
					context.PushTop(stuckCount);
					string instance = location3.Flags.ToString();
					context.PushTop(instance);
					DateTime? returnTime = location3.ReturnTime;
					context.PushTop(GetDateRepresentation(returnTime));
					string matrix = location3.Matrix;
					context.PushTop(matrix);
					context.PushTop("SUCCESS");
				}
				else
				{
					context.PushTop("ERROR");
				}
			}
			else if (base.Operands[0].Value.Equals("GETRETURNTIME"))
			{
				if (GetLocation(result, context, out var location4))
				{
					context.PushTop(GetDateRepresentation(location4.ReturnTime));
				}
				else
				{
					context.PushTop("ERROR");
				}
			}
			else if (base.Operands[0].Value.Equals("MARKRETURNTIME"))
			{
				SetReturnTime(result, DateTime.Now, leaveResult: true, context);
			}
			else if (base.Operands[0].Value.Equals("AT-DRUM-LOC"))
			{
				if (ControllerService.Instance.Location.IsValid)
				{
					context.PushTop(true);
				}
				else
				{
					context.PushTop(false);
				}
			}
			else if (base.Operands[0].Value.Equals("SETINVENTORY"))
			{
				KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("ID");
				string keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair, result.Errors, context);
				if (result.Errors.Count > 0)
				{
					context.PushTop("FAILURE");
					return;
				}
				if (!GetLocation(result, context, out var deck, out var slot))
				{
					LogHelper.Instance.Log("Unable to get deck and slot information.", LogEntryType.Error);
					context.PushTop("FAILURE");
				}
				Deck deck2 = DecksManager.Instance.GetDeck(deck);
				deck2.SetSlotMatrix(slot, keyValuePairValue);
				context.PushTop("SUCCESS");
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
					SetReturnTime(result, result2, leaveResult: true, context);
				}
			}
			else if (base.Operands[0].Value.Equals("TIMESTAMP"))
			{
				SetReturnTime(result, DateTime.Now, leaveResult: false, context);
			}
			else if (base.Operands[0].Value.Equals("SETRETURNTIME"))
			{
				if (base.Operands[1].Type != TokenType.Symbol)
				{
					LogHelper.Instance.Log("Location SETRETURNTIME - Operand expected to be a symbol", LogEntryType.Error);
					AddError("E999", "Unspecified context exit; check the logs.", result.Errors);
				}
				string text = context.GetSymbolValue(base.Operands[1].Value, result.Errors) as string;
				if (!"NONE".Equals(text))
				{
					if (!DateTime.TryParse(text, out var result3))
					{
						LogHelper.Instance.Log("Location SETRETURNTIME Instruction - Unable to parse date/time information.", LogEntryType.Error);
						AddError("E999", "Unspecified context exit; check the logs.", result.Errors);
					}
					else
					{
						SetReturnTime(result, result3, leaveResult: true, context);
					}
				}
			}
			else if (base.Operands[0].Value.Equals("IS-EXCLUDED"))
			{
				if (GetLocation(result, context, out var location5))
				{
					context.PushTop(location5.IsExcluded);
					context.PushTop("SUCCESS");
				}
				else
				{
					context.PushTop("LocationOutOfRange".ToUpper());
				}
			}
			else if (base.Operands[0].Value.Equals("SLOTTYPE"))
			{
				if (!GetLocation(result, context, out var location6))
				{
					context.PushTop("UNKNOWN");
				}
				else
				{
					context.PushTop(location6.IsWide ? "WIDE" : "NOTWIDE");
				}
			}
			else
			{
				result.Errors.Add(Error.NewError("E001", "Unrecognized LOC form.", $"The operand {base.Operands[0].Value} isn't recognized."));
			}
		}
		else if (ControllerService.Instance.IsAtVendLocation)
		{
			context.PushTop("VENDDOOR");
			context.PushTop("VENDDOOR");
		}
		else if (!ControllerService.Instance.Location.IsValid)
		{
			context.PushTop("UNDEFINED");
			context.PushTop("UNDEFINED");
		}
		else
		{
			context.PushTop(ControllerService.Instance.Location.Slot);
			context.PushTop(ControllerService.Instance.Location.Deck);
		}
	}

	protected override bool IsOperandRecognized(string op)
	{
		return Keys.Recognized(op);
	}

	private void SetReturnTime(ExecutionResult result, DateTime? newTime, bool leaveResult, ExecutionContext context)
	{
		if (!GetLocation(result, context, out var location))
		{
			LogHelper.Instance.Log("Unable to get deck and slot information.", LogEntryType.Error);
			context.PushTop("ERROR");
			return;
		}
		bool flag = location.UpdateReturnTime(newTime);
		if (leaveResult)
		{
			context.PushTop(flag);
		}
	}
}
