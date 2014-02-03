using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class SyncInstruction : Instruction
{
	public override string Mnemonic => "SYNC";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		base.Operands.AddRange(tokensAfterMnemonic);
	}

	protected override void OnExecute(ExecutionResult result)
	{
		if (!GetLocation(result, out var location))
		{
			return;
		}
		SyncType type = SyncType.Standard;
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("TYPE");
		if (keyValuePair != null)
		{
			try
			{
				type = (SyncType)Enum.Parse(typeof(SyncType), keyValuePair.Value, ignoreCase: true);
			}
			catch
			{
				result.Errors.Add(Error.NewError("E777", "Unrecognized sync type.", $"Unrecognized sync type {keyValuePair}"));
				return;
			}
		}
		SyncResult syncResult = SyncHelper.Instance.SyncSlot(result, location, type);
		ExecutionEngine.Instance.ActiveContext.PushTop(syncResult.ToString().ToUpper());
	}
}
