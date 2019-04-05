using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

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
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("TYPE");
		object[] value = OperandsHelper.ConvertSymbolsToParameterArray(base.Operands, result.Errors, context);
		if (result.Errors.Count <= 0)
		{
			string value2 = base.Operands[0].Value;
			IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
			service.SetPropertyByName(keyValuePair.Value, value2, value);
		}
	}
}
