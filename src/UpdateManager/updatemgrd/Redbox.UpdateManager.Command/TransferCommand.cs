using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Command;

[Command("transfer")]
internal class TransferCommand
{
	[Usage("transfer cancel-all")]
	[CommandForm(Name = "cancel-all")]
	public void CancelAll(CommandContext context)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		Redbox.IPC.Framework.ErrorList collection = ConvertErrors(service.CancelAll());
		context.Errors.AddRange(collection);
	}

	[Usage("transfer repositories-in-transit")]
	[CommandForm(Name = "repositories-in-transit")]
	public void InTransit(CommandContext context)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		Redbox.IPC.Framework.ErrorList collection = ConvertErrors(service.GetRepositoriesInTransit(out var inTransit));
		context.Errors.AddRange(collection);
		context.Messages.Add(ObjectExtensions.ToJson(new List<string>(inTransit)));
	}

	[Usage("transfer list [allusers:]")]
	[CommandForm(Name = "list")]
	public void InTransit(CommandContext context, bool? allusers)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		context.Errors.AddRange(ConvertErrors(service.GetJobs(out var jobs, allusers.HasValue && allusers.Value)));
		if (!context.Errors.ContainsError())
		{
			context.Messages.Add(ObjectExtensions.ToJson(jobs));
		}
	}

	[CommandForm(Name = "create-download")]
	[Usage("transfer create-download name:")]
	public void CreateDownload(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		context.Errors.AddRange(ConvertErrors(service.CreateDownloadJob(name, out var job)));
		if (!context.Errors.ContainsError())
		{
			context.Messages.Add(ObjectExtensions.ToJson(job));
		}
	}

	[CommandForm(Name = "create-upload")]
	[Usage("transfer create-upload name:")]
	public void CreateUpload(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		context.Errors.AddRange(ConvertErrors(service.CreateUploadJob(name, out var job)));
		if (!context.Errors.ContainsError())
		{
			context.Messages.Add(ObjectExtensions.ToJson(job));
		}
	}

	[CommandForm(Name = "get-job")]
	[Usage("transfer get-job id:")]
	public void GetJob(CommandContext context, [CommandKeyValue(IsRequired = true)] string id)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		context.Errors.AddRange(ConvertErrors(service.GetJob(new Guid(id), out var job)));
		if (!context.Errors.ContainsError())
		{
			context.Messages.Add(ObjectExtensions.ToJson(job));
		}
	}

	[CommandForm(Name = "are-jobs-running")]
	[Usage("transfer are-jobs-running:")]
	public void AreJobsRunning(CommandContext context)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		context.Errors.AddRange(ConvertErrors(service.AreJobsRunning(out var isRunning)));
		if (!context.Errors.ContainsError())
		{
			context.Messages.Add(ObjectExtensions.ToJson(isRunning));
		}
	}

	[CommandForm(Name = "add-item")]
	[Usage("add-item id: url: file:")]
	public void AddItem(CommandContext context, [CommandKeyValue(IsRequired = true)] string id, [CommandKeyValue(IsRequired = true)] string url, [CommandKeyValue(IsRequired = true)] string file)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		context.Errors.AddRange(ConvertErrors(service.GetJob(new Guid(id), out var job)));
		if (!context.Errors.ContainsError())
		{
			context.Errors.AddRange(ConvertErrors(job.AddItem(url, file)));
		}
	}

	[Usage("complete id:")]
	[CommandForm(Name = "complete")]
	public void Complete(CommandContext context, [CommandKeyValue(IsRequired = true)] string id)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		context.Errors.AddRange(ConvertErrors(service.GetJob(new Guid(id), out var job)));
		if (!context.Errors.ContainsError())
		{
			context.Errors.AddRange(ConvertErrors(job.Complete()));
		}
	}

	[Usage("resume id:")]
	[CommandForm(Name = "resume")]
	public void Resume(CommandContext context, [CommandKeyValue(IsRequired = true)] string id)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		context.Errors.AddRange(ConvertErrors(service.GetJob(new Guid(id), out var job)));
		if (!context.Errors.ContainsError())
		{
			context.Errors.AddRange(ConvertErrors(job.Resume()));
		}
	}

	[Usage("cancel id:")]
	[CommandForm(Name = "cancel")]
	public void Cancel(CommandContext context, [CommandKeyValue(IsRequired = true)] string id)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		context.Errors.AddRange(ConvertErrors(service.GetJob(new Guid(id), out var job)));
		if (!context.Errors.ContainsError())
		{
			context.Errors.AddRange(ConvertErrors(job.Cancel()));
		}
	}

	[Usage("suspend id:")]
	[CommandForm(Name = "suspend")]
	public void Suspend(CommandContext context, [CommandKeyValue(IsRequired = true)] string id)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		context.Errors.AddRange(ConvertErrors(service.GetJob(new Guid(id), out var job)));
		if (!context.Errors.ContainsError())
		{
			context.Errors.AddRange(ConvertErrors(job.Suspend()));
		}
	}

	[CommandForm(Name = "set-priority")]
	[Usage("set-priority id:")]
	public void SetPriority(CommandContext context, [CommandKeyValue(IsRequired = true)] string id, [CommandKeyValue(IsRequired = true)] TransferJobPriority priority)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		context.Errors.AddRange(ConvertErrors(service.GetJob(new Guid(id), out var job)));
		if (!context.Errors.ContainsError())
		{
			context.Errors.AddRange(ConvertErrors(job.SetPriority(priority)));
		}
	}

	[CommandForm(Name = "take-ownership")]
	[Usage("take-ownership id:")]
	public void TakeOwnership(CommandContext context, [CommandKeyValue(IsRequired = true)] string id)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		context.Errors.AddRange(ConvertErrors(service.GetJob(new Guid(id), out var job)));
		if (!context.Errors.ContainsError())
		{
			context.Errors.AddRange(ConvertErrors(job.TakeOwnership()));
		}
	}

	[CommandForm(Name = "set-no-progress-timeout")]
	[Usage("set-no-progress-timeout id: timeout:")]
	public void SetNoProgressTimeout(CommandContext context, [CommandKeyValue(IsRequired = true)] string id, [CommandKeyValue(IsRequired = true)] uint? timeout)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		context.Errors.AddRange(ConvertErrors(service.GetJob(new Guid(id), out var job)));
		if (!context.Errors.ContainsError())
		{
			context.Errors.AddRange(ConvertErrors(job.SetNoProgressTimeout(timeout.Value)));
		}
	}

	[Usage("set-minimum-retry-delay id: seconds:")]
	[CommandForm(Name = "set-minimum-retry-delay")]
	public void SetMinimumRetryDelay(CommandContext context, [CommandKeyValue(IsRequired = true)] string id, [CommandKeyValue(IsRequired = true)] uint? seconds)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		context.Errors.AddRange(ConvertErrors(service.GetJob(new Guid(id), out var job)));
		if (!context.Errors.ContainsError())
		{
			context.Errors.AddRange(ConvertErrors(job.SetMinimumRetryDelay(seconds.Value)));
		}
	}

	[CommandForm(Name = "get-errors")]
	[Usage("get-errors id:")]
	public void GetErrors(CommandContext context, [CommandKeyValue(IsRequired = true)] string id)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		context.Errors.AddRange(ConvertErrors(service.GetJob(new Guid(id), out var job)));
		if (!context.Errors.ContainsError())
		{
			job.GetErrors(out var errors);
			context.Errors.AddRange(ConvertErrors(errors));
		}
	}

	[Usage("get-items id:")]
	[CommandForm(Name = "get-items")]
	public void GetItems(CommandContext context, [CommandKeyValue(IsRequired = true)] string id)
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		context.Errors.AddRange(ConvertErrors(service.GetJob(new Guid(id), out var job)));
		if (!context.Errors.ContainsError())
		{
			Redbox.UpdateManager.ComponentModel.ErrorList items = job.GetItems(out var items2);
			context.Errors.AddRange(ConvertErrors(items));
			if (!context.Errors.ContainsError())
			{
				context.Messages.Add(ObjectExtensions.ToJson(items2));
			}
		}
	}

	private static Redbox.IPC.Framework.ErrorList ConvertErrors(List<Redbox.UpdateManager.ComponentModel.Error> errors)
	{
		Redbox.IPC.Framework.ErrorList ipc = new Redbox.IPC.Framework.ErrorList();
		errors.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
		{
			ipc.Add(e.IsWarning ? Redbox.IPC.Framework.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.IPC.Framework.Error.NewError(e.Code, e.Description, e.Details));
		});
		return ipc;
	}
}
