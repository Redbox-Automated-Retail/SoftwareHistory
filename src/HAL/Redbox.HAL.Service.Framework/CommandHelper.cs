using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework;
using Redbox.HAL.Script.Framework;

namespace Redbox.HAL.Service.Framework;

public static class CommandHelper
{
	public static void FormatJob(ExecutionContext context, CommandContext result)
	{
		result.Messages.Add(string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}", context.ID, context.Label ?? "(not labeled)", context.ProgramName, context.Priority, context.StartTime.HasValue ? context.StartTime.ToString() : "N/A", context.Status, context.Result.ExecutionTime.HasValue ? context.Result.ExecutionTime.ToString() : "Not Started", context.ConnectionState, context.EnableDebugging ? "1" : "0", context.TraceExecution ? "1" : "0"));
	}

	public static ExecutionContext GetJob(string jobId, ErrorList errors, bool required)
	{
		if (string.IsNullOrEmpty(jobId))
		{
			if (required)
			{
				errors.Add(Error.NewError("S001", "The job: parameter is required.", "Correct your command and resubmit."));
			}
		}
		else
		{
			ExecutionContext job = ExecutionEngine.Instance.GetJob(jobId);
			if (job != null)
			{
				return job;
			}
			errors.Add(Error.NewError("S001", $"The job for job ID '{jobId}' does not exist.", "Correct your command and resubmit."));
		}
		return null;
	}

	public static ProgramInstruction GetProgram(ExecutionContext context, string programName, ErrorList errors)
	{
		ProgramInstruction program = context.GetProgram(programName);
		if (program == null)
		{
			errors.Add(Error.NewError("S001", $"The program '{programName}' does not exist in the program cache for job '{context.ID}'.", "Correct your command and resubmit."));
		}
		return program;
	}
}
