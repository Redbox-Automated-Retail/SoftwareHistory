using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class SetConfigInstruction : Instruction
{
	public override string Mnemonic => "SETCFG";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens[0].Type != TokenType.StringLiteral)
		{
			result.AddInvalidOperandError("Expected a string literal.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		ConfigType configType = ConfigType.Controller;
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("TYPE");
		if (keyValuePair != null)
		{
			configType = Enum<ConfigType>.ParseIgnoringCase(keyValuePair.Value, ConfigType.Controller);
		}
		object[] value = ConvertSymbolsToParameterArray(result.Errors, context);
		if (result.Errors.Count <= 0)
		{
			string value2 = base.Operands[0].Value;
			switch (configType)
			{
			case ConfigType.Camera:
				BarcodeConfiguration.Instance.SetPropertyByName(value2, value);
				break;
			case ConfigType.Controller:
				ControllerConfiguration.Instance.SetPropertyByName(value2, value);
				break;
			}
		}
	}
}
