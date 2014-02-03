namespace Redbox.HAL.Script.Framework;

public class CompiledProgram
{
	public ProgramInstruction ParseTree { get; set; }

	public ExecutionResult CompilerResult { get; set; }

	public override string ToString()
	{
		if (CompilerResult.Errors.ErrorCount == 0 && CompilerResult.Errors.WarningCount == 0)
		{
			return "Cached";
		}
		return $"Errors: {CompilerResult.Errors.ErrorCount}, Warnings: {CompilerResult.Errors.WarningCount}";
	}
}
