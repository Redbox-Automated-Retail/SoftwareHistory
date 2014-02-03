using System;
using System.Text;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class MoveInstruction : Instruction
{
	protected static class KeyNames
	{
		public const string Deck = "DECK";

		public const string Slot = "SLOT";

		public const string Mode = "MODE";

		public const string Location = "LOCATION";

		public const string Password = "PASSWORD";

		public const string CCW = "CCW";

		public const string CW = "CW";

		public const string ResetOnCommunicationFailure = "RESETONCOMMERROR";

		public const string SlotOffset = "SLOT-OFFSET";
	}

	public override string Mnemonic => "MOVE";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count > 0 && tokensAfterMnemonic.Count <= 4)
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
		else
		{
			AddError("C002", "Expected key value pair DECK=y, SLOT=x, MODE=m.  The values of x and y may be numeric literals or symbols.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
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
			int keyValuePairValue = GetKeyValuePairValue<int>(keyValuePair2, result.Errors);
			if (result.Errors.Count > 0)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(MotionResult.Error.ToString().ToUpper());
				return;
			}
			if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(MotionResult.Error.ToString().ToUpper());
				return;
			}
			if (!IsLocationValid())
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(MotionResult.Error.ToString().ToUpper());
				return;
			}
			int slot = ControllerService.Instance.Location.Slot;
			Deck deck = DecksManager.Instance.GetDeck(ControllerService.Instance.Location.Deck);
			int num = slot - keyValuePairValue;
			if (num <= 0)
			{
				num = deck.NumberOfSlots - Math.Abs(num);
			}
			MotionResult motionResult = ControllerService.Instance.MoveTo(deck.Number, num, MoveMode.None, ExecutionEngine.Instance.ActiveContext.AppLog);
			string text = $"MOVE Deck = {deck.Number} Slot = {num} returned {motionResult.ToString().ToUpper()}";
			ExecutionEngine.Instance.ActiveContext.AppLog.Write(text);
			LogHelper.Instance.Log(text, LogEntryType.Info);
			if (motionResult == MotionResult.Success)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(keyValuePairValue);
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(motionResult.ToString().ToUpper());
			return;
		}
		KeyValuePair keyValuePair3 = base.Operands.GetKeyValuePair("LOCATION");
		string keyValuePairValue2 = GetKeyValuePairValue<string>(keyValuePair3, result.Errors);
		if (!string.IsNullOrEmpty(keyValuePairValue2))
		{
			if (ExecutionEngine.Instance.ActiveContext.IsImmediate)
			{
				KeyValuePair keyValuePair4 = base.Operands.GetKeyValuePair("PASSWORD");
				string keyValuePairValue3 = GetKeyValuePairValue<string>(keyValuePair4, result.Errors);
				if (string.IsNullOrEmpty(keyValuePairValue3))
				{
					AddError("E004", "No password was specified to move to location.", result.Errors);
					return;
				}
				SpecialLocation specialLocation = SpecialLocationLocatorService.Instance.FindByName(keyValuePairValue2);
				if (specialLocation == null)
				{
					AddError("E004", "The specified location was not found.", result.Errors);
					return;
				}
				string text2 = StringExtensions.EncryptToBase64(keyValuePairValue3);
				if (!(text2 == specialLocation.EncryptedPassword))
				{
					AddError("E004", "An invalid password was specified to move to location.", result.Errors);
					return;
				}
			}
			MotionResult motionResult2 = ControllerService.Instance.MoveToLocation(keyValuePairValue2, mode, ExecutionEngine.Instance.ActiveContext.AppLog);
			string text3 = motionResult2.ToString().ToUpper();
			if (motionResult2 != MotionResult.Success)
			{
				string form = $"{Mnemonic} LOCATION=\"{keyValuePairValue2}\" returned {text3}";
				LogInstructionError(form, text3);
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(text3);
			return;
		}
		if (!GetLocation(result, out var location))
		{
			AddError("E004", "Deck or slot are undefined, and no location specified; reissue the MOVE instruction specifying both deck and slot values or the location.", result.Errors);
			return;
		}
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		if (activeContext.IsImmediate && ControllerService.Instance.IsLocationSecure(location))
		{
			string instance = MotionResult.InvalidMoveToSecureLocation.ToString().ToUpper();
			ExecutionEngine.Instance.ActiveContext.PushTop(instance);
			return;
		}
		KeyValuePair keyValuePair5 = base.Operands.GetKeyValuePair("RESETONCOMMERROR");
		if (keyValuePair5 != null)
		{
			new ErrorList();
			bool.TryParse(keyValuePair5.Value, out var _);
		}
		if (IsNextMoveSimulationTimeout("SimMoveTimeout"))
		{
			PerfFunctions.Wait(5000);
			activeContext.PushTop(MotionResult.Timeout.ToString().ToUpper());
		}
		else
		{
			MotionResult motionResult3 = ControllerService.Instance.MoveTo(location, mode, ExecutionEngine.Instance.ActiveContext.AppLog);
			activeContext.PushTop(motionResult3.ToString().ToUpper());
		}
	}

	private bool IsLocationValid()
	{
		return ControllerService.Instance.Location.IsValid;
	}

	private bool IsNextMoveSimulationTimeout(string kvp)
	{
		string secretValue = RegistryOptions.Instance.GetSecretValue<string>("SOFTWARE\\Redbox\\HAL", kvp);
		if (string.IsNullOrEmpty(secretValue))
		{
			return false;
		}
		string[] array = secretValue.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 1; i < array.Length; i++)
			{
				stringBuilder.Append(array[i] + ";");
			}
			RegistryOptions.Instance.SetSecretValue("SOFTWARE\\Redbox\\HAL", kvp, stringBuilder.ToString());
		}
		return array[0] == "T";
	}
}
