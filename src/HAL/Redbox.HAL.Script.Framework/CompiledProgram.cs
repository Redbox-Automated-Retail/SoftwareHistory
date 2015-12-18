namespace Redbox.HAL.Script.Framework;

public sealed class CompiledProgram
{
	public readonly ProgramInstruction ParseTree;

	public readonly ExecutionResult CompilerResult;

	public override string ToString()
	{
		if (CompilerResult.Errors.ErrorCount == 0 && CompilerResult.Errors.WarningCount == 0)
		{
			return "Cached";
		}
		return $"Errors: {CompilerResult.Errors.ErrorCount}, Warnings: {CompilerResult.Errors.WarningCount}";
	}

	internal CompiledProgram(ExecutionResult compileResult, ProgramInstruction tree)
	{
		ParseTree = tree;
		CompilerResult = compileResult;
	}
}
