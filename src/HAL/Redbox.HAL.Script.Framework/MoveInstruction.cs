using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class MoveInstruction : Instruction
{
	private static class KeyNames
	{
		public const string Deck = "DECK";

		public const string Slot = "SLOT";

		public const string Mode = "MODE";

		public const string Location = "LOCATION";

		public const string Password = "PASSWORD";

		public const string SlotOffset = "SLOT-OFFSET";
	}

	public override string Mnemonic => "MOVE";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count == 0 || operandTokens.Count > 4)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected key value pair DECK=y, SLOT=x, MODE=m.  The values of x and y may be numeric literals or symbols.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		MoveMode mode = MoveMode.None;
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("MODE");
		if (keyValuePair != null)
		{
			mode = Enum<MoveMode>.ParseIgnoringCase(keyValuePair.Value, MoveMode.None);
		}
		KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair("SLOT-OFFSET");
		if (base.Operands.Count == 1 && keyValuePair2 != null)
		{
			int keyValuePairValue = GetKeyValuePairValue<int>(keyValuePair2, result.Errors, context);
			if (result.Errors.Count > 0)
			{
				context.PushTop(MotionResult.Error.ToString().ToUpper());
				return;
			}
			if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode)
			{
				context.PushTop(MotionResult.Error.ToString().ToUpper());
				return;
			}
			if (!IsLocationValid())
			{
				context.PushTop(MotionResult.Error.ToString().ToUpper());
				return;
			}
			int slot = ControllerService.Instance.Location.Slot;
			Deck deck = DecksManager.Instance.GetDeck(ControllerService.Instance.Location.Deck);
			int num = slot - keyValuePairValue;
			if (num <= 0)
			{
				num = deck.NumberOfSlots - Math.Abs(num);
			}
			MotionResult motionResult = ControllerService.Instance.MoveTo(deck.Number, num, MoveMode.None, context.AppLog);
			string text = $"MOVE Deck = {deck.Number} Slot = {num} returned {motionResult.ToString().ToUpper()}";
			context.AppLog.Write(text);
			LogHelper.Instance.Log(text, LogEntryType.Info);
			if (motionResult == MotionResult.Success)
			{
				context.PushTop(keyValuePairValue);
			}
			context.PushTop(motionResult.ToString().ToUpper());
		}
		else
		{
			KeyValuePair keyValuePair3 = base.Operands.GetKeyValuePair("LOCATION");
			string keyValuePairValue2 = GetKeyValuePairValue<string>(keyValuePair3, result.Errors, context);
			Location location;
			if (!string.IsNullOrEmpty(keyValuePairValue2))
			{
				AddError("E004", "The LOCATION operand is no longer supported.", result.Errors);
			}
			else if (!GetLocation(result, context, out location))
			{
				AddError("E004", "Deck or slot are undefined, and no location specified; reissue the MOVE instruction specifying both deck and slot values or the location.", result.Errors);
			}
			else if (context.IsImmediate && location.IsSecure)
			{
				string instance = MotionResult.InvalidMoveToSecureLocation.ToString().ToUpper();
				context.PushTop(instance);
			}
			else
			{
				MotionResult motionResult2 = ControllerService.Instance.MoveTo(location, mode, context.AppLog);
				context.PushTop(motionResult2.ToString().ToUpper());
			}
		}
	}

	private bool IsLocationValid()
	{
		return ControllerService.Instance.Location.IsValid;
	}
}
