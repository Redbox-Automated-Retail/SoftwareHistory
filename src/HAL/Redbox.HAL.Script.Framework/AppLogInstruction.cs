using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class AppLogInstruction : Instruction
{
	private enum OperandArgs
	{
		Operation,
		Name,
		Subdirectory,
		FlushOnWrite
	}

	public override string Mnemonic => "APPLOG";

	protected override bool IsOperandRecognized(string operand)
	{
		return operand.Equals("CONFIGURE");
	}

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count > 0)
		{
			if (tokensAfterMnemonic[0].Value.Equals("CONFIGURE"))
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
			else if (tokensAfterMnemonic[0].Type != TokenType.StringLiteral && tokensAfterMnemonic[0].Type != TokenType.Symbol && tokensAfterMnemonic[0].Type != TokenType.ConstSymbol)
			{
				result.Errors.Add(Error.NewError("C002", "Expected a string literal and any number of optional substitution parameters.", "Correct the statement and recompile."));
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		if (base.Operands[0].Value.Equals("CONFIGURE"))
		{
			Configure(result);
			return;
		}
		string text = base.Operands[0].Value;
		if (base.Operands[0].Type == TokenType.Symbol || base.Operands[0].Type == TokenType.ConstSymbol)
		{
			object symbolValue = ExecutionEngine.Instance.ActiveContext.GetSymbolValue(base.Operands[0].Value, result.Errors);
			if (symbolValue == null || result.Errors.Count > 0)
			{
				return;
			}
			text = symbolValue as string;
			if (text == null)
			{
				result.Errors.Add(Error.NewError("E001", "Incorrect type argument.", "The symbol specified for APPLOG is not a string type."));
				return;
			}
		}
		object[] args = ConvertSymbolsToParameterArray(result.Errors);
		if (result.Errors.Count <= 0)
		{
			string msg = string.Format(text, args);
			ExecutionEngine.Instance.ActiveContext.AppLog?.Write(msg);
		}
	}

	private void Configure(ExecutionResult result)
	{
		ConvertSymbolsToParameterArray(result.Errors);
		if (result.Errors.Count > 0)
		{
			return;
		}
		bool logAppends = false;
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair(OperandArgs.Name.ToString().ToUpper());
		if (keyValuePair != null)
		{
			_ = keyValuePair.Value;
			if (keyValuePair.Type == TokenType.ConstSymbol || keyValuePair.Type == TokenType.Symbol)
			{
				_ = (string)ExecutionEngine.Instance.ActiveContext.GetSymbolValue(keyValuePair.Value, result.Errors);
				if (result.Errors.ContainsError())
				{
					return;
				}
			}
			logAppends = true;
		}
		bool flushOnWrite = true;
		KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair(OperandArgs.FlushOnWrite.ToString().ToUpper());
		if (keyValuePair2 != null && bool.TryParse(keyValuePair2.Value, out var result2))
		{
			flushOnWrite = result2;
		}
		string subDirectory = string.Empty;
		KeyValuePair keyValuePair3 = base.Operands.GetKeyValuePair(OperandArgs.Subdirectory.ToString().ToUpper());
		if (keyValuePair3 != null)
		{
			string text = keyValuePair3.Value;
			if (keyValuePair3.Type == TokenType.ConstSymbol || keyValuePair3.Type == TokenType.Symbol)
			{
				text = (string)ExecutionEngine.Instance.ActiveContext.GetSymbolValue(keyValuePair3.Value, result.Errors);
				if (result.Errors.ContainsError())
				{
					return;
				}
			}
			subDirectory = text;
		}
		ApplicationLog.ConfigureLog(ExecutionEngine.Instance.ActiveContext, logAppends, subDirectory, flushOnWrite, null);
	}
}
