using System;
using Redbox.HAL.Component.Model;
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

		internal const string WithOffset = "WITH-OFFSET";

		public const string Axis = "AXIS";

		public const string Units = "UNITS";
	}

	public override string Mnemonic => "MOVE";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count < 2)
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
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair("SLOT-OFFSET");
		if (base.Operands.Count == 1 && keyValuePair2 != null)
		{
			int keyValuePairValue = GetKeyValuePairValue<int>(keyValuePair2, result.Errors, context);
			if (result.Errors.Count > 0)
			{
				context.PushTop(ErrorCodes.InvalidLocation.ToString().ToUpper());
				return;
			}
			if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode)
			{
				context.PushTop(ErrorCodes.InvalidLocation.ToString().ToUpper());
				return;
			}
			if (service.CurrentLocation == null)
			{
				context.PushTop(ErrorCodes.InvalidLocation.ToString().ToUpper());
				return;
			}
			IDecksService service2 = ServiceLocator.Instance.GetService<IDecksService>();
			int slot = service.CurrentLocation.Slot;
			IDeck deck = service2.GetFrom(service.CurrentLocation);
			int num = slot - keyValuePairValue;
			if (num <= 0)
			{
				num = deck.NumberOfSlots - Math.Abs(num);
			}
			ErrorCodes errorCodes = service.MoveTo(deck.Number, num, MoveMode.None, context.AppLog);
			string text = $"MOVE Deck = {deck.Number} Slot = {num} returned {errorCodes.ToString().ToUpper()}";
			context.AppLog.Write(text);
			LogHelper.Instance.Log(text, LogEntryType.Info);
			if (errorCodes == ErrorCodes.Success)
			{
				context.PushTop(keyValuePairValue);
			}
			context.PushTop(errorCodes.ToString().ToUpper());
			return;
		}
		KeyValuePair keyValuePair3 = base.Operands.GetKeyValuePair("LOCATION");
		string keyValuePairValue2 = GetKeyValuePairValue<string>(keyValuePair3, result.Errors, context);
		if (!string.IsNullOrEmpty(keyValuePairValue2))
		{
			AddError("E004", "The LOCATION operand is no longer supported.", result.Errors);
			return;
		}
		if (!GetLocation(result, context, out var location))
		{
			AddError("E004", "Deck or slot are undefined, and no location specified; reissue the MOVE instruction specifying both deck and slot values or the location.", result.Errors);
			return;
		}
		IDumpbinService service3 = ServiceLocator.Instance.GetService<IDumpbinService>();
		if (context.IsImmediate && service3.IsBin(location))
		{
			string instance = ErrorCodes.InvalidMoveToSecureLocation.ToString().ToUpper();
			context.PushTop(instance);
			return;
		}
		ErrorCodes errorCodes2;
		if (string.Compare(base.Operands[0].Value, "WITH-OFFSET", ignoreCase: true) == 0)
		{
			KeyValuePair keyValuePair4 = base.Operands.GetKeyValuePair("AXIS");
			if (keyValuePair4 == null)
			{
				AddError("E004", "Axis KVP is not specified.", result.Errors);
				return;
			}
			KeyValuePair keyValuePair5 = base.Operands.GetKeyValuePair("UNITS");
			if (keyValuePair5 == null)
			{
				AddError("E004", "Offset units are not specified.", result.Errors);
				return;
			}
			int? keyValuePairValue3 = GetKeyValuePairValue<int?>(keyValuePair5, result.Errors, context);
			if (!keyValuePairValue3.HasValue)
			{
				AddError("E004", "Units conversion error.", result.Errors);
				return;
			}
			OffsetMoveData data = ((Enum<Axis>.ParseIgnoringCase(keyValuePair4.Value, Axis.X) == Axis.X) ? new OffsetMoveData(keyValuePairValue3.Value, 0) : new OffsetMoveData(0, keyValuePairValue3.Value));
			errorCodes2 = service.MoveTo(location, mode, context.AppLog, ref data);
		}
		else
		{
			errorCodes2 = service.MoveTo(location, mode, context.AppLog);
		}
		context.PushTop(errorCodes2.ToString().ToUpper());
	}
}
