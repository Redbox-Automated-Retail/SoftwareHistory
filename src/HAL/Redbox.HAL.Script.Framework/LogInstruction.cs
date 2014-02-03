using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class LogInstruction : Instruction
{
	protected static class KeyNames
	{
		public const string EntryType = "ENTRYTYPE";
	}

	public override string Mnemonic => "LOG";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count > 0)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.StringLiteral && tokensAfterMnemonic[0].Type != TokenType.Symbol)
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
		if (base.Operands.Count > 0)
		{
			object[] args = ConvertSymbolsToParameterArray(result.Errors);
			if (result.Errors.Count > 0)
			{
				return;
			}
			LogEntryType type = LogEntryType.Info;
			KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("ENTRYTYPE");
			if (keyValuePair != null)
			{
				type = Enum<LogEntryType>.ParseIgnoringCase(keyValuePair.Value, LogEntryType.Info);
			}
			ILogger logger = ExecutionEngine.Instance.Logger;
			if (logger == null)
			{
				return;
			}
			string text = base.Operands[0].Value;
			if (base.Operands[0].Type == TokenType.Symbol)
			{
				object symbolValue = ExecutionEngine.Instance.ActiveContext.GetSymbolValue(base.Operands[0].Value, result.Errors);
				if (symbolValue == null || result.Errors.Count > 0)
				{
					return;
				}
				text = symbolValue as string;
				if (text == null)
				{
					result.Errors.Add(Error.NewError("E001", "Incorrect type argument.", "The symbol specified for LOG is not a string type."));
					return;
				}
			}
			string toPrint = string.Format(text, args);
			logger.Log(GetFormattedMessage(toPrint), type);
		}
		else if (ExecutionEngine.Instance.ActiveContext.StackDepth > 0)
		{
			object obj = ExecutionEngine.Instance.ActiveContext.PopTop();
			if (obj != null)
			{
				ExecutionEngine.Instance.Logger?.Log(GetFormattedMessage(obj.ToString()), LogEntryType.Info);
			}
		}
	}

	private string GetFormattedMessage(string toPrint)
	{
		return $"[{ExecutionEngine.Instance.ActiveContext.ProgramName}, {ExecutionEngine.Instance.ActiveContext.ID}]: {toPrint}";
	}
}
