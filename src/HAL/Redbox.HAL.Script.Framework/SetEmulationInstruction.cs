using System;
using Redbox.HAL.Camera.Framework;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class SetEmulationInstruction : Instruction
{
	public override string Mnemonic => "SETEMU";

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
		object[] value = ConvertSymbolsToParameterArray(result.Errors);
		if (result.Errors.Count <= 0)
		{
			string value2 = base.Operands[0].Value;
			switch (configType)
			{
			case ConfigType.Barcode:
				BarcodeEmulationState.Instance.SetPropertyByName(value2, value);
				break;
			case ConfigType.Camera:
				CameraEmulationState.Instance.SetPropertyByName(value2, value);
				break;
			case ConfigType.Controller:
				ControllerEmulationState.Instance.SetPropertyByName(value2, value);
				break;
			}
		}
	}
}
