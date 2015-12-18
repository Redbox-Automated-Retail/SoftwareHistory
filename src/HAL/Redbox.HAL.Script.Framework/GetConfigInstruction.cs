using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class GetConfigInstruction : Instruction
{
	public override string Mnemonic => "GETCFG";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count < 2 || operandTokens[0].Type != TokenType.StringLiteral)
		{
			result.AddInvalidOperandError("Expected a string literal operand.");
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
		object[] args = ConvertSymbolsToParameterArray(result.Errors, context);
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
			case ConfigType.Camera:
				obj = BarcodeConfiguration.Instance.GetPropertyByName(text);
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
			context.PushTop(obj);
		}
	}
}
