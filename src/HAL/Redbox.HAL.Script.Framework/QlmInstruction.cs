using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class QlmInstruction : Instruction
{
	protected static class Commands
	{
		public const string Engage = "ENGAGE";

		public const string Disengage = "DISENGAGE";

		public const string Halt = "HALT";

		public const string Status = "STATUS";

		public const string Lift = "LIFT";

		public const string Drop = "DROP";

		public const string EndQlmOp = "ENDQLMOP";

		public const string ClearPicker = "CLEAR-PICKER";

		public const string UnloadDisk = "UNLOAD-DISK";

		public const string Setup = "SETUP";

		public const string ThinDisk = "THIN-DISK";
	}

	public override string Mnemonic => "QLM";

	private static QLM QLM => QLM.Instance;

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count == 1 || tokensAfterMnemonic.Count == 2)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.Symbol || (string.Compare(tokensAfterMnemonic[0].Value, "ENGAGE", ignoreCase: true) != 0 && string.Compare(tokensAfterMnemonic[0].Value, "DISENGAGE", ignoreCase: true) != 0 && string.Compare(tokensAfterMnemonic[0].Value, "LIFT", ignoreCase: true) != 0 && string.Compare(tokensAfterMnemonic[0].Value, "DROP", ignoreCase: true) != 0 && string.Compare(tokensAfterMnemonic[0].Value, "HALT", ignoreCase: true) != 0 && string.Compare(tokensAfterMnemonic[0].Value, "ENDQLMOP", ignoreCase: true) != 0 && string.Compare(tokensAfterMnemonic[0].Value, "CLEAR-PICKER", ignoreCase: true) != 0 && string.Compare(tokensAfterMnemonic[0].Value, "UNLOAD-DISK", ignoreCase: true) != 0 && string.Compare(tokensAfterMnemonic[0].Value, "THIN-DISK", ignoreCase: true) != 0 && string.Compare(tokensAfterMnemonic[0].Value, "SETUP", ignoreCase: true) != 0 && string.Compare(tokensAfterMnemonic[0].Value, "STATUS", ignoreCase: true) != 0))
			{
				AddError("C002", "Expected a symbol: ENGAGE, DISENGAGE, HALT, UP, DOWN or STATUS.", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
		else
		{
			AddError("C002", "Expected a symbol: ENGAGE, DISENGAGE, UP, DOWN or HALT.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		Token token = base.Operands[0];
		if (token.Type != TokenType.Symbol)
		{
			return;
		}
		bool home = true;
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("HOME");
		if (keyValuePair != null && bool.TryParse(keyValuePair.Value, out var result2))
		{
			home = result2;
		}
		if (string.Compare(token.Value, "HALT", ignoreCase: true) == 0)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop(Instruction.ConvertTimeoutBoolToMessage(QLM.HaltOperation()));
		}
		else if (string.Compare(token.Value, "ENGAGE", ignoreCase: true) == 0)
		{
			QlmMotionResult qlmMotionResult = QLM.Engage(home, activeContext.AppLog);
			if (QlmMotionResult.Success == qlmMotionResult)
			{
				MerchandizingHelper.Instance.CorrectMotionControlCommunication();
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(qlmMotionResult.ToString().ToUpper());
		}
		else if (string.Compare(token.Value, "LIFT", ignoreCase: true) == 0)
		{
			QLM.Lift();
		}
		else if (string.Compare(token.Value, "DROP", ignoreCase: true) == 0)
		{
			QLM.Drop();
		}
		else if (string.Compare(token.Value, "DISENGAGE", ignoreCase: true) == 0)
		{
			QlmMotionResult qlmMotionResult2 = QLM.Disengage(home, activeContext.AppLog);
			if (QlmMotionResult.Success == qlmMotionResult2)
			{
				MerchandizingHelper.Instance.CorrectMotionControlCommunication();
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(qlmMotionResult2.ToString().ToUpper());
		}
		else if (string.Compare(token.Value, "STATUS", ignoreCase: true) == 0)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop(QLM.GetStatus().ToString().ToUpper());
		}
		else if (string.Compare(token.Value, "ENDQLMOP", ignoreCase: true) == 0)
		{
			MotionResult failCode = MotionResult.Success;
			KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair("MOVE-EC");
			if (keyValuePair2 != null)
			{
				string keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair2, result.Errors);
				if (result.Errors.Count == 0)
				{
					try
					{
						failCode = (MotionResult)Enum.Parse(typeof(MotionResult), keyValuePairValue, ignoreCase: true);
					}
					catch (ArgumentException)
					{
						failCode = MotionResult.Success;
					}
				}
			}
			if (MerchandizingHelper.Instance.CleanupJob(result, failCode))
			{
				MerchandizingHelper.Instance.CorrectMotionControlCommunication();
			}
		}
		else if (string.Compare(token.Value, "CLEAR-PICKER") == 0)
		{
			UnloadHelper.Instance.CheckAndClearPicker(result);
		}
		else if (string.Compare(token.Value, "SETUP") == 0)
		{
			KeyValuePair keyValuePair3 = base.Operands.GetKeyValuePair("CHECK-SLOTS");
			bool checkSlots = true;
			if (keyValuePair3 != null && bool.TryParse(keyValuePair3.Value, out var result3))
			{
				checkSlots = result3;
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(MerchandizingHelper.Instance.Setup(result, checkSlots));
		}
		else if (string.Compare(token.Value, "UNLOAD-DISK") == 0)
		{
			int num = CurrentSlot(result);
			if (num != 0)
			{
				if (!ControllerService.Instance.EmptyCountExceeds(0))
				{
					ExecutionEngine.Instance.ActiveContext.PushTop(MerchandizeResult.MachineFull.ToString().ToUpper());
				}
				MerchandizeResult merchandizeResult = UnloadHelper.Instance.UnloadDisk(result, QLM.Deck.Number, num);
				string text = merchandizeResult.ToString().ToUpper();
				if (merchandizeResult != MerchandizeResult.Success)
				{
					ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Info, "Unload-Disc returned status {0}", text);
				}
				ExecutionEngine.Instance.ActiveContext.PushTop(text);
			}
		}
		else
		{
			if (string.Compare(token.Value, "THIN-DISK") != 0)
			{
				return;
			}
			string iD = GetID(result);
			if (!string.IsNullOrEmpty(iD))
			{
				MerchandizeResult merchandizeResult2 = ThinHelper.MakeInstance(result).ThinDisk(iD, MerchFlags.None);
				string text2 = merchandizeResult2.ToString().ToUpper();
				if (merchandizeResult2 != MerchandizeResult.Success)
				{
					ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Info, "thin-disk returned status {0}", text2);
				}
				ExecutionEngine.Instance.ActiveContext.PushTop(text2);
			}
		}
	}

	private int CurrentSlot(ExecutionResult result)
	{
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("CURRENT-SLOT");
		if (keyValuePair == null)
		{
			result.Errors.Add(Error.NewError("E099", "Missing operand.", "The CURRENT-SLOT operand is missing."));
			return 0;
		}
		int keyValuePairValue = GetKeyValuePairValue<int>(keyValuePair, result.Errors);
		if (result.Errors.Count != 0)
		{
			return 0;
		}
		return keyValuePairValue;
	}
}
