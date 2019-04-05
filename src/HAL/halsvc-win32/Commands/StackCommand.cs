using System.ComponentModel;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Attributes;
using Redbox.HAL.IPC.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Win32.Commands;

[Command("stack")]
public sealed class StackCommand
{
	[Usage("STACK show job: job-id")]
	[Description("Dumps a diagnostic list of the stack contents.")]
	[CommandForm(Name = "show")]
	public void Show(CommandContext context, [CommandKeyValue(KeyName = "job", IsRequired = true)] string jobId)
	{
		IExecutionContext job = CommandHelper.GetJob(jobId, context.Errors);
		if (job != null)
		{
			int position = job.StackDepth + job.ResultCount;
			job.ForeachStackItem(delegate(object o)
			{
				context.Messages.Add($"{position--}: {o}");
			});
			job.ForeachResult(delegate(string r)
			{
				context.Messages.Add($"{position--}: {r}");
			});
		}
	}

	[Usage("STACK clear job: job-id")]
	[Description("Clears the execution engine's data stack.")]
	[CommandForm(Name = "clear")]
	public void Clear(CommandContext context, [CommandKeyValue(KeyName = "job", IsRequired = true)] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors)?.ClearStack();
	}

	[Description("Removes only entries on the stack that were added using the RESULT instruction.")]
	[Usage("STACK remove-results job: job-id")]
	[CommandForm(Name = "remove-results")]
	public void RemoveResults(CommandContext context, [CommandKeyValue(KeyName = "job", IsRequired = true)] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors)?.ClearResults();
	}

	[Description("Pops any value at the specified end of the data stack for the designated job.  Values are popped from the top of the stack by default.")]
	[CommandForm(Name = "pop")]
	[Usage("STACK pop job: job-id value: value [end: bottom|top]")]
	public void Pop(CommandContext context, [CommandKeyValue(KeyName = "job", IsRequired = true)] string jobId, StackEnd? end)
	{
		IExecutionContext job = CommandHelper.GetJob(jobId, context.Errors);
		if (job != null)
		{
			context.Messages.Add(job.Pop(end ?? StackEnd.Top).ToString());
		}
	}

	[Usage("STACK push job: job-id [end: bottom|top]")]
	[Description("Pushes value onto specified end of stack for the designated job.")]
	[CommandForm(Name = "push")]
	public void Push(CommandContext context, [CommandKeyValue(KeyName = "job", IsRequired = true)] string jobId, [CommandKeyValue(IsRequired = true)] string value, StackEnd? end)
	{
		IExecutionContext job = CommandHelper.GetJob(jobId, context.Errors);
		if (job != null)
		{
			IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
			service.PushValue(job, value, end ?? StackEnd.Top, context.Errors);
		}
	}
}
