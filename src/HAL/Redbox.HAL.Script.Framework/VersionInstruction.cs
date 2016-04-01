using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class VersionInstruction : Instruction
{
	public override string Mnemonic => "VERSION";

	protected override ExpectedOperands Expected => ExpectedOperands.Optional;

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count > 1)
		{
			result.AddInvalidOperandError("The VERSION instruction supports an optional operand indicating the target board.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		if (base.Operands.Count == 1)
		{
			Token token = base.Operands[0];
			ControlBoards controlBoards = Enum<ControlBoards>.ParseIgnoringCase(token.Value, ControlBoards.None);
			if (controlBoards == ControlBoards.None)
			{
				result.Errors.Add(Error.NewError("E001", "Unrecognized board option.", $"The option {token.Value} is not a board."));
				return;
			}
			string boardVersion = service.GetBoardVersion(controlBoards);
			context.PushTop(boardVersion);
			return;
		}
		string[] boardVersions = service.GetBoardVersions();
		string[] array = boardVersions;
		foreach (string instance in array)
		{
			context.PushTop(instance);
		}
		context.PushTop("REVISION A");
	}
}
