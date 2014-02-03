using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class GetInstruction : Instruction
{
	protected static class KeyNames
	{
		public const string Peek = "PEEK";

		public const string HomeOnFailure = "HOMEONFAILURE";

		public const string Center = "CENTER";
	}

	public override string Mnemonic => "GET";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count > 0 && tokensAfterMnemonic[0].IsKeyValuePair)
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("PEEK");
		if (keyValuePair != null)
		{
			bool.TryParse(keyValuePair.Value, out var _);
		}
		bool flag = false;
		KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair("CENTER");
		if (keyValuePair2 != null && bool.TryParse(keyValuePair2.Value, out var result3))
		{
			flag = result3;
		}
		bool testOnEmpty = false;
		KeyValuePair keyValuePair3 = base.Operands.GetKeyValuePair("TESTONEMPTY");
		if (keyValuePair3 != null && bool.TryParse(keyValuePair3.Value, out var result4))
		{
			testOnEmpty = result4;
		}
		bool flag2 = false;
		KeyValuePair keyValuePair4 = base.Operands.GetKeyValuePair("GET-RT");
		if (keyValuePair4 != null && bool.TryParse(keyValuePair4.Value, out var result5))
		{
			flag2 = result5;
		}
		if (ExecutionEngine.Instance.ActiveContext.IsImmediate && ControllerConfiguration.Instance.Mode == ControllerMode.Normal)
		{
			testOnEmpty = true;
		}
		GetResult getResult = ControllerService.Instance.Get(testOnEmpty, ExecutionEngine.Instance.ActiveContext.AppLog);
		if (getResult.Success && flag)
		{
			ControllerService.Instance.CenterItem(CenterDiskMethod.VendDoorAndBack);
		}
		if (flag2)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop(GetDateRepresentation(getResult.ReturnTime));
		}
		ExecutionEngine.Instance.ActiveContext.PushTop(getResult.Previous);
		ExecutionEngine.Instance.ActiveContext.PushTop(getResult.ToString().ToUpper());
	}
}
