using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class VersionInstruction : Instruction
{
	public override string Mnemonic => "VERSION";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count <= 1)
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
		else
		{
			AddError("C002", "The VERSION instruction supports an optional operand indicating the target board.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		if (base.Operands.Count == 1)
		{
			Token token = base.Operands[0];
			KnownBoards knownBoards;
			try
			{
				knownBoards = Enum<KnownBoards>.ParseIgnoringCase(token.Value, KnownBoards.None);
			}
			catch (Exception)
			{
				result.Errors.Add(Error.NewError("E001", "Unrecognized board option.", $"The option {token.Value} is not a board."));
				return;
			}
			if (knownBoards == KnownBoards.None)
			{
				result.Errors.Add(Error.NewError("E001", "Unrecognized board option.", $"The option {token.Value} is not a board."));
				return;
			}
			string boardVersion = SerialBoard.Instance.GetBoardVersion(knownBoards);
			ExecutionEngine.Instance.ActiveContext.PushTop(boardVersion);
			return;
		}
		string machineRevision = ControllerService.Instance.GetMachineRevision(out var versions);
		string[] array = versions;
		foreach (string text in array)
		{
			if (text != null)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(text.Trim());
			}
		}
		ExecutionEngine.Instance.ActiveContext.PushTop($"REVISION {machineRevision}");
	}
}
