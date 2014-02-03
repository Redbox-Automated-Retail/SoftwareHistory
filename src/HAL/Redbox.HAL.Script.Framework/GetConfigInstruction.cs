using System;
using Redbox.HAL.Camera.Framework;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class GetConfigInstruction : Instruction
{
	public override string Mnemonic => "GETCFG";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count > 0)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.StringLiteral)
			{
				AddError("C002", "Expected a string literal.", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		ConfigType configType = ConfigType.Controller;
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("TYPE");
		if (keyValuePair != null)
		{
			configType = Enum<ConfigType>.ParseIgnoringCase(keyValuePair.Value, ConfigType.Controller);
		}
		object[] args = ConvertSymbolsToParameterArray(result.Errors);
		if (result.Errors.Count > 0)
		{
			return;
		}
		string text = string.Format(base.Operands[0].Value, args);
		object obj = string.Empty;
		try
		{
			switch (configType)
			{
			case ConfigType.Barcode:
				obj = BarcodeConfiguration.Instance.GetPropertyByName(text);
				break;
			case ConfigType.Camera:
				obj = CameraConfiguration.Instance.GetPropertyByName(text);
				break;
			case ConfigType.Controller:
				obj = ControllerConfiguration.Instance.GetPropertyByName(text);
				if (text.Equals("QlmDeck.Number", StringComparison.CurrentCultureIgnoreCase) && obj == null)
				{
					obj = -1;
				}
				break;
			}
		}
		catch (ArgumentException ex)
		{
			AddError("E002", ex.ToString(), result.Errors);
			return;
		}
		if (obj != null)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop(obj);
		}
	}
}
