using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class TransferInstruction : Instruction
{
	protected static class KeyNames
	{
		public const string GetPeek = "GET-PEEK";

		public const string PutPeek = "PUT-PEEK";

		public const string SourceDeck = "SRC-DECK";

		public const string SourceSlot = "SRC-SLOT";

		public const string DestDeck = "DEST-DECK";

		public const string DestSlot = "DEST-SLOT";

		public const string HomeOnFailure = "HOMEONFAILURE";

		public const string ID = "ID";
	}

	public override string Mnemonic => "XFER";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count > 0 && tokensAfterMnemonic.Count <= 7)
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
		else
		{
			AddError("C002", "Expected key value pair SRC-DECK=y, SRC-SLOT=x, DEST-DECK=y, DEST-SLOT=x. The values of x and y may be numeric literals or symbols.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		if (PickerBoard.Instance.IsFull())
		{
			ExecutionEngine.Instance.ActiveContext.PushTop("PICKERFULL");
			LogHelper.Instance.Log("XFER: the picker is full - bail.");
			return;
		}
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("SRC-DECK");
		int? keyValuePairValue = GetKeyValuePairValue<int?>(keyValuePair, result.Errors);
		if (!keyValuePairValue.HasValue)
		{
			AddError("E004", "Source deck or source slot are undefined, reissue the XFER instruction specifying both source deck and slot values.", result.Errors);
			return;
		}
		int value = keyValuePairValue.Value;
		KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair("SRC-SLOT");
		int? keyValuePairValue2 = GetKeyValuePairValue<int?>(keyValuePair2, result.Errors);
		if (!keyValuePairValue2.HasValue)
		{
			AddError("E004", "Source deck or source slot are undefined, reissue the XFER instruction specifying both source deck and slot values.", result.Errors);
			return;
		}
		int value2 = keyValuePairValue2.Value;
		KeyValuePair keyValuePair3 = base.Operands.GetKeyValuePair("DEST-DECK");
		int? keyValuePairValue3 = GetKeyValuePairValue<int?>(keyValuePair3, result.Errors);
		KeyValuePair keyValuePair4 = base.Operands.GetKeyValuePair("DEST-SLOT");
		int? keyValuePairValue4 = GetKeyValuePairValue<int?>(keyValuePair4, result.Errors);
		if (result.Errors.Count > 0)
		{
			return;
		}
		if (!keyValuePairValue3.HasValue || !keyValuePairValue4.HasValue)
		{
			AddError("E004", "Expected key value pair SRC-DECK=y, SRC-SLOT=x, DEST-DECK=y, DEST-SLOT=x. The values of x and y may be numeric literals or symbols.", result.Errors);
			return;
		}
		Location location = new Location(value, value2);
		Location location2 = new Location(keyValuePairValue3.Value, keyValuePairValue4.Value);
		LogHelper.Instance.Log("XFER: {0} to {1}", location.ToString(), location2.ToString());
		ApplicationLog appLog = ExecutionEngine.Instance.ActiveContext.AppLog;
		TransferResult transferResult = ControllerService.Instance.Transfer(location, location2, appLog);
		if (transferResult.Transferred)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop("SUCCESS");
		}
		else
		{
			ExecutionEngine.Instance.ActiveContext.PushTop(transferResult.FailedError.ToUpper());
		}
	}
}
