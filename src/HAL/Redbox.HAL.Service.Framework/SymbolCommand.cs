using System.ComponentModel;
using Redbox.HAL.Configuration;
using Redbox.HAL.IPC.Framework;
using Redbox.HAL.Script.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Framework;

[Command("symbol")]
public sealed class SymbolCommand
{
	[Usage("SYMBOL show job: job-id")]
	[Description("Dumps a diagnostic list of the symbol list contents.")]
	[CommandForm(Name = "show")]
	public void Show(CommandContext context, [CommandKeyValue(KeyName = "job", IsRequired = true)] string jobId)
	{
		ExecutionContext job = CommandHelper.GetJob(jobId, context.Errors, required: true);
		if (job == null)
		{
			return;
		}
		foreach (string symbolName in job.GetSymbolNames())
		{
			if (!symbolName.StartsWith("__"))
			{
				context.Messages.Add($"{symbolName}: {job.GetSymbolValue(symbolName, context.Errors)}");
			}
		}
	}

	[Description("Clears the execution engine's symbol list.")]
	[CommandForm(Name = "clear")]
	[Usage("SYMBOL clear job: job-id")]
	public void Clear(CommandContext context, [CommandKeyValue(KeyName = "job", IsRequired = true)] string jobId)
	{
		ExecutionContext job = CommandHelper.GetJob(jobId, context.Errors, required: true);
		if (job != null)
		{
			job.ClearSymbolTable();
			job.SaveSymbolTable();
		}
	}
}
