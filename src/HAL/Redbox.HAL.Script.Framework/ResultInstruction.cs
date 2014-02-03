using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class ResultInstruction : Instruction
{
	public static class Prefixes
	{
		public const string ResultPrefix = "RESULT";
	}

	protected static class StackEnds
	{
		public const string Bottom = "bottom";

		public const string Top = "top";
	}

	protected static class KeyNames
	{
		public const string End = "END";

		public const string Deck = "DECK";

		public const string Slot = "SLOT";

		public const string Code = "CODE";

		public const string Item = "ITEM";

		public const string Message = "MESSAGE";

		public const string ReturnTime = "RETURNTIME";

		public const string PreviousID = "PREVIOUS";
	}

	public override string Mnemonic => "RESULT";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count >= 2)
		{
			if (tokens.GetKeyValuePair("CODE") == null || tokens.GetKeyValuePair("MESSAGE") == null)
			{
				AddError("C002", $"Expected key value parameters CODE=c and MESSAGE=m. (line {base.LineNumber})", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
		else
		{
			AddError("C002", "Expected the key value parameters CODE=c and MESSAGE=m.  Optional pairs can be: DECK=n, SLOT=n, and END=BOTTOM|TOP.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("END");
		bool flag = keyValuePair != null && string.Compare(keyValuePair.Value, "bottom", ignoreCase: true) == 0;
		string keyValuePairValue = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair("CODE"), result.Errors);
		string keyValuePairValue2 = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair("MESSAGE"), result.Errors);
		int? keyValuePairValue3 = GetKeyValuePairValue<int?>(base.Operands.GetKeyValuePair("DECK"), result.Errors);
		int? keyValuePairValue4 = GetKeyValuePairValue<int?>(base.Operands.GetKeyValuePair("SLOT"), result.Errors);
		string keyValuePairValue5 = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair("ITEM"), result.Errors);
		string keyValuePairValue6 = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair("RETURNTIME"), result.Errors);
		string keyValuePairValue7 = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair("PREVIOUS"), result.Errors);
		if (result.Errors.Count > 0)
		{
			return;
		}
		DateTime? returnTime = null;
		if (!string.IsNullOrEmpty(keyValuePairValue6))
		{
			if (!DateTime.TryParse(keyValuePairValue6, out var result2))
			{
				if (!"NONE".Equals(keyValuePairValue6))
				{
					LogHelper.Instance.Log($"Unable to parse date time {keyValuePairValue6}", LogEntryType.Error);
				}
				returnTime = null;
			}
			else
			{
				returnTime = result2;
			}
		}
		ResultHelper.Instance.CreateResult(flag ? StackEnd.Bottom : StackEnd.Top, keyValuePairValue, keyValuePairValue2, keyValuePairValue3, keyValuePairValue4, keyValuePairValue5, returnTime, keyValuePairValue7);
	}
}
