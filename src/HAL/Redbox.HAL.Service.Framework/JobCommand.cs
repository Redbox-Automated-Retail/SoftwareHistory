using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Configuration;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework;
using Redbox.HAL.Script.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Framework;

[Command("job")]
public sealed class JobCommand
{
	[Usage("JOB get job: job-id")]
	[CommandForm(Name = "get")]
	public void Get(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		ExecutionContext job = CommandHelper.GetJob(jobId, context.Errors, required: true);
		if (job != null)
		{
			CommandHelper.FormatJob(job, context);
		}
	}

	[Usage("JOB list")]
	[CommandForm(Name = "list")]
	public void List(CommandContext context)
	{
		List<ExecutionContext> list = ExecutionEngine.Instance.SafeGetJobList();
		using (new DisposeableList<ExecutionContext>(list))
		{
			list.Sort((ExecutionContext x, ExecutionContext y) => x.Priority.CompareTo(y.Priority));
			foreach (ExecutionContext item in list)
			{
				CommandHelper.FormatJob(item, context);
			}
		}
	}

	[Usage("JOB set-label job: job-id value: label")]
	[CommandForm(Name = "set-label")]
	public void SetLabel(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId, string value)
	{
		ExecutionContext job = CommandHelper.GetJob(jobId, context.Errors, required: true);
		if (job != null)
		{
			job.Label = value;
			job.Save();
		}
	}

	[Usage("JOB get-errors job: job-id")]
	[CommandForm(Name = "get-errors")]
	public void GetErrors(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		ExecutionContext job = CommandHelper.GetJob(jobId, context.Errors, required: true);
		if (job != null)
		{
			ProtocolHelper.FormatErrors(job.Result.Errors, context);
		}
	}

	[Usage("JOB get-messages job: job-id")]
	[CommandForm(Name = "get-messages")]
	public void GetMessages(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		ExecutionContext job = CommandHelper.GetJob(jobId, context.Errors, required: true);
		if (job != null)
		{
			ProtocolHelper.FormatEventMessages(job.EnumerateMessages(), context);
		}
	}

	[CommandForm(Name = "get-results")]
	[Usage("JOB get-results job: job-id")]
	public void GetResults(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		ExecutionContext job = CommandHelper.GetJob(jobId, context.Errors, required: true);
		if (job == null)
		{
			return;
		}
		using (new ResultsReader(job))
		{
			IEnumerable<string> messages = job.EnumerateResults();
			ProtocolHelper.FormatEventMessages(messages, context);
		}
	}

	[Usage("JOB disconnect job: job-id")]
	[CommandForm(Name = "disconnect")]
	public void Disconnect(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		ExecutionContext job = CommandHelper.GetJob(jobId, context.Errors, required: true);
		if (job != null)
		{
			DisconnectJob(job);
		}
	}

	[CommandForm(Name = "connect")]
	[Usage("JOB connect job: job-id")]
	public void Connect(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		ExecutionContext executionContext = CommandHelper.GetJob(jobId, context.Errors, required: true);
		if (executionContext != null)
		{
			executionContext.MessageSink = context.MessageSink;
			executionContext.ConnectionState = ConnectionState.Connected;
			executionContext.Save();
			context.Session.Disconnect += delegate
			{
				DisconnectJob(executionContext);
			};
		}
	}

	[Usage("JOB pend job: job-id")]
	[CommandForm(Name = "pend")]
	public void Pend(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors, required: true)?.Pend();
	}

	[Usage("JOB signal job: job-id value: signalValue")]
	[CommandForm(Name = "signal")]
	public void Signal(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId, string value)
	{
		CommandHelper.GetJob(jobId, context.Errors, required: true)?.Signal(value);
	}

	[CommandForm(Name = "trash")]
	[Usage("JOB trash job: job-id")]
	public void Trash(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors, required: true)?.Trash();
	}

	[Usage("JOB resume job: job-id")]
	[CommandForm(Name = "resume")]
	public void Resume(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors, required: true)?.Resume();
	}

	[Usage("JOB suspend job: job-id")]
	[CommandForm(Name = "suspend")]
	public void Suspend(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors, required: true)?.Suspend();
	}

	[CommandForm(Name = "suspend-all")]
	[Usage("JOB suspend-all")]
	public void SuspendAll(CommandContext context)
	{
		context.Errors.Add(Error.NewError("C999", "Unsupported Client API", "The SuspendAll client API is no longer supported."));
	}

	[Usage("JOB resume-all")]
	[CommandForm(Name = "resume-all")]
	public void ResumeAll(CommandContext context)
	{
		context.Errors.Add(Error.NewError("C999", "Unsupported Client API", "The ResumeAll client API is no longer supported."));
	}

	[CommandForm(Name = "terminate")]
	[Usage("JOB terminate job: job-id")]
	public void Terminate(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors, required: true)?.Terminate();
	}

	[CommandForm(Name = "set-priority")]
	[Usage("JOB set-priority job: job-id value: priority")]
	public void SetPriority(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId, ProgramPriority value)
	{
		ExecutionContext job = CommandHelper.GetJob(jobId, context.Errors, required: true);
		if (job != null)
		{
			job.Priority = value;
			job.Save();
		}
	}

	[Usage("JOB set-start-time job: job-id value: startTime")]
	[CommandForm(Name = "set-start-time")]
	public void SetStartTime(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId, DateTime value)
	{
		ExecutionContext job = CommandHelper.GetJob(jobId, context.Errors, required: true);
		if (job != null)
		{
			job.StartTime = value;
			job.Save();
		}
	}

	[CommandForm(Name = "collect-garbage")]
	[Usage("JOB collect-garbage force: true|false")]
	public void CollectGarbage(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "force")] bool force)
	{
		ExecutionEngine.Instance.CleanupJobs(force);
	}

	[Usage("JOB scheduler-status")]
	[CommandForm(Name = "scheduler-status")]
	public void GetSchedulerStatus(CommandContext context)
	{
		context.Messages.Add((ExecutionEngine.Instance.ActiveContext != null) ? ExecutionEngine.Instance.ActiveContext.ID : "idle");
	}

	[Usage("JOB wait-for-completion job: job-id")]
	[CommandForm(Name = "wait-for-completion")]
	public void WaitForCompletion(CommandContext context, [CommandKeyValue(IsRequired = true, KeyName = "job")] string jobId)
	{
		CommandHelper.GetJob(jobId, context.Errors, required: true)?.WaitForCompletion();
	}

	[Usage("JOB schedule name: program-name [debugging: flag] [priority: priority] [startTime: startTime] [label: label] [resume-now] [no-duplicates]")]
	[CommandForm(Name = "schedule")]
	public void Schedule(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, bool? debugging, ProgramPriority? priority, DateTime? startTime, string label)
	{
		bool flag = context.Parameters.ContainsKey("resume-now");
		if (context.Parameters.ContainsKey("no-duplicates") && label != null)
		{
			ExecutionContext[] jobByLabel = ExecutionEngine.Instance.GetJobByLabel(label);
			if (jobByLabel != null && jobByLabel.Length > 0)
			{
				context.Errors.Add(Error.NewWarning("S998", $"A job with the label '{label}' already exists and the no-duplicates flag was specified.", "Issue the JOB schedule command again passing in a non-duplicate label: or remove the no-duplicates parameter."));
				if (flag)
				{
					ExecutionContext[] jobByLabel2 = ExecutionEngine.Instance.GetJobByLabel(label);
					foreach (ExecutionContext executionContext in jobByLabel2)
					{
						executionContext.Resurrect();
					}
				}
				return;
			}
		}
		ExecutionContext executionContext2 = ScheduleJob(name, label, startTime, priority ?? ProgramPriority.Normal, debugging ?? false);
		context.Errors.AddRange(executionContext2.Result.Errors);
		if (!context.Errors.ContainsError() && flag)
		{
			executionContext2.Resume();
		}
		CommandHelper.FormatJob(executionContext2, context);
	}

	[Usage("JOB execute-immediate statement: statement-line")]
	[CommandForm(Name = "execute-immediate")]
	public void ExecuteImmediate([CommandKeyValue(IsRequired = true)] CommandContext context, string statement)
	{
		if (ExecutionEngine.Instance.IsImmediateJobPending())
		{
			context.Errors.Add(Error.NewError("S001", "An immediate command is already scheduled to execute.  Only one immediate job can be scheduled at a time.", "Use the JOB wait-for-completion command and then resubmit the immediate job."));
			return;
		}
		using MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(statement));
		ExecutionResult executionResult = ExecutionEngine.Instance.Compile(stream, "$$$immediate$$$");
		if (executionResult.Errors.ContainsError())
		{
			context.Errors.AddRange(executionResult.Errors);
			return;
		}
		ExecutionContext executionContext = ExecutionEngine.Instance.ExecuteImmediate();
		executionContext.WaitForCompletion();
		CommandHelper.FormatJob(executionContext, context);
		context.Errors.AddRange(executionContext.Result.Errors);
	}

	[CommandForm(Name = "execute-immediate-base64")]
	[Usage("JOB execute-immediate-base64 statement: base64-encoded-statement")]
	public void ExecuteImmediateBase64(CommandContext context, [CommandKeyValue(IsRequired = true, BinaryEncoding = BinaryEncoding.Base64)] byte[] statement)
	{
		if (ExecutionEngine.Instance.IsImmediateJobPending())
		{
			context.Errors.Add(Error.NewError("S001", "An immediate command is already scheduled to execute.  Only one immediate job can be scheduled at a time.", "Use the JOB wait-for-completion command and then resubmit the immediate job."));
			return;
		}
		using MemoryStream stream = new MemoryStream(statement);
		ExecutionResult executionResult = ExecutionEngine.Instance.Compile(stream, "$$$immediate$$$");
		if (executionResult.Errors.ContainsError())
		{
			context.Errors.AddRange(executionResult.Errors);
			return;
		}
		ExecutionContext executionContext = ExecutionEngine.Instance.ExecuteImmediate();
		executionContext.WaitForCompletion();
		CommandHelper.FormatJob(executionContext, context);
		context.Errors.AddRange(executionContext.Result.Errors);
	}

	[Usage("JOB init-status")]
	[CommandForm(Name = "init-status")]
	public void StartupInitSucceeded(CommandContext context)
	{
		context.Messages.Add(ExecutionEngine.Instance.GetInitJobStatus());
	}

	[Usage("JOB quick-return-status")]
	[CommandForm(Name = "quick-return-status")]
	public void QuickReturnIsRunning(CommandContext context)
	{
		string text = (QuickReturnDevice.Instance.IsConfigured ? "ACTIVE" : "INACTIVE");
		LogHelper.Instance.Log($"JOB quick-return-status status: {text}");
		context.Messages.Add(text);
	}

	private static ExecutionContext ScheduleJob(string programName, string label, DateTime? startTime, ProgramPriority priority, bool enableDebugging)
	{
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob(programName, label, startTime, priority);
		executionContext.EnableDebugging = enableDebugging;
		return executionContext;
	}

	private static void DisconnectJob(ExecutionContext executionContext)
	{
		if (executionContext != null)
		{
			executionContext.MessageSink = null;
			executionContext.ConnectionState = ConnectionState.Disconnected;
			executionContext.Save();
		}
	}
}
