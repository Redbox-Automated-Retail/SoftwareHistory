using System.ComponentModel;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Configuration;
using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework;
using Redbox.HAL.Script.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Framework;

[Command("stack")]
public sealed class StackCommand
{
	[CommandForm(Name = "show")]
	[Description("Dumps a diagnostic list of the stack contents.")]
	[Usage("STACK show job: job-id")]
	public void Show(CommandContext context, [CommandKeyValue(KeyName = "job", IsRequired = true)] string jobId)
	{
		ExecutionContext job = CommandHelper.GetJob(jobId, context.Errors, required: true);
		if (job == null)
		{
			return;
		}
		using (new ResultsReader(job))
		{
			int num = job.StackDepth + job.ResultCount;
			foreach (object item in job.EnumerateStack())
			{
				context.Messages.Add($"{num--}: {item}");
			}
			foreach (string item2 in job.EnumerateResults())
			{
				context.Messages.Add($"{num--}: {item2}");
			}
		}
	}

	[CommandForm(Name = "clear")]
	[Usage("STACK clear job: job-id")]
	[Description("Clears the execution engine's data stack.")]
	public void Clear(CommandContext context, [CommandKeyValue(KeyName = "job", IsRequired = true)] string jobId)
	{
		ExecutionContext job = CommandHelper.GetJob(jobId, context.Errors, required: true);
		if (job != null)
		{
			job.ClearStack();
			job.SaveDataStack();
		}
	}

	[Usage("STACK remove-results job: job-id")]
	[Description("Removes only entries on the stack that were added using the RESULT instruction.")]
	[CommandForm(Name = "remove-results")]
	public void RemoveResults(CommandContext context, [CommandKeyValue(KeyName = "job", IsRequired = true)] string jobId)
	{
		ExecutionContext job = CommandHelper.GetJob(jobId, context.Errors, required: true);
		if (job != null)
		{
			job.RemoveResultsFromStack();
			job.SaveDataStack();
		}
	}

	[Usage("STACK push job: job-id value: value [end: bottom|top]")]
	[CommandForm(Name = "pop")]
	[Description("Pushes value onto specified end of stack for the designated job.")]
	public void Pop(CommandContext context, [CommandKeyValue(KeyName = "job", IsRequired = true)] string jobId, StackEnd? end)
	{
		ExecutionContext job = CommandHelper.GetJob(jobId, context.Errors, required: true);
		if (job != null)
		{
			MessageList messages = context.Messages;
			StackEnd? stackEnd = end;
			messages.Add((stackEnd.GetValueOrDefault() == StackEnd.Bottom && stackEnd.HasValue) ? job.PopBottom().ToString() : job.PopTop().ToString());
			job.SaveDataStack();
		}
	}

	[Usage("STACK pop job: job-id [end: bottom|top]")]
	[Description("Pops any value at the specified end of the data stack for the designated job.  Values are popped from the top of the stack by default.")]
	[CommandForm(Name = "push")]
	public void Push(CommandContext context, [CommandKeyValue(KeyName = "job", IsRequired = true)] string jobId, [CommandKeyValue(IsRequired = true)] string value, StackEnd? end)
	{
		ExecutionContext job = CommandHelper.GetJob(jobId, context.Errors, required: true);
		if (job == null)
		{
			return;
		}
		Tokenizer tokenizer = new Tokenizer(1, string.Format("PUSH " + value));
		tokenizer.Tokenize();
		if (tokenizer.Errors.ContainsError())
		{
			context.Errors.AddRange(tokenizer.Errors);
			return;
		}
		foreach (Token token in tokenizer.Tokens)
		{
			if (token.Type == TokenType.NumericLiteral || token.Type == TokenType.StringLiteral)
			{
				object instance = token.ConvertValue();
				StackEnd? stackEnd = end;
				if (stackEnd.GetValueOrDefault() == StackEnd.Bottom && stackEnd.HasValue)
				{
					job.PushBottom(instance);
				}
				else
				{
					job.PushTop(instance);
				}
			}
		}
		job.SaveDataStack();
	}
}
