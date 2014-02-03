using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Command;

[Command("repository")]
internal class RepositoryCommand
{
	[CommandForm(Name = "trim-repository")]
	[Usage("repository trim-repository name:")]
	public void TrimRepository(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		service.TrimRepository(name);
	}

	[CommandForm(Name = "disable-subscription")]
	[Usage("repository disable-subscription name:")]
	public void DisableSuscription(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		service.DisableSubscription(name);
	}

	[CommandForm(Name = "enable-subscription")]
	[Usage("repository enable-subscription name:")]
	public void EnableSuscription(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		service.EndableSubscription(name);
	}

	[Usage("repository subscribed name:")]
	[CommandForm(Name = "subscribed")]
	public void Subscribed(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		bool flag = service.Subscribed(name);
		context.Messages.Add(ObjectExtensions.ToJson(flag));
	}

	[CommandForm(Name = "activate-to")]
	[Usage("repository activate-to name: hash:")]
	public void ActivateTo(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string hash)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = service.ActivateTo(name, hash, out var deferredMove);
		if (errorList.ContainsError())
		{
			context.Errors.AddRange(errorList.ToIPCErrors());
		}
		else
		{
			context.Messages.Add(ObjectExtensions.ToJson(deferredMove));
		}
	}

	[Usage("repository format-stage-file name:")]
	[CommandForm(Name = "format-stage-file-name")]
	public void FormatStageFileName(CommandContext context, [CommandKeyValue(IsRequired = true)] string file)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		string instance = service.FormatStagedFileName(file.Replace("\\\\", "\\"));
		context.Messages.Add(ObjectExtensions.ToJson(instance));
	}

	[CommandForm(Name = "unpack-change-set")]
	[Usage("repository unpack-change-set name: id: archive:")]
	public void UnpackChangeset(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string id, [CommandKeyValue(IsRequired = true)] string archive)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		service.UnpackChangeSet(name, new Guid(id), archive.Replace("\\\\", "\\"));
	}

	[Usage("repository contains-repository name:")]
	[CommandForm(Name = "contains-repository")]
	public void ContainsRepository(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		bool flag = service.ContainsRepository(name);
		context.Messages.Add(ObjectExtensions.ToJson(flag));
	}

	[CommandForm(Name = "add-repository")]
	[Usage("repository add-repository name:")]
	public void AddRepository(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		service.AddRepository(name);
	}

	[Usage("repository add-revisions name: revisions:")]
	[CommandForm(Name = "add-revisions")]
	public void AddRevisions(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string revisions)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		service.AddRevisions(name, StringExtensions.ToObject<List<string>>(revisions));
	}

	[CommandForm(Name = "remove-revisions")]
	[Usage("repository remove-revisions name: revisions:")]
	public void RemoveRepository(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string revisions)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		service.RemoveRevisions(name, StringExtensions.ToObject<List<string>>(revisions));
	}

	[CommandForm(Name = "add-delta")]
	[Usage("repository add-delta name: revision: file: isSeed: isPlaceHolder: contentHash: versionHash:")]
	public void AddDelta(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string revision, [CommandKeyValue(IsRequired = true)] string file, [CommandKeyValue(IsRequired = true)] bool isSeed, [CommandKeyValue(IsRequired = true)] bool isPlaceHolder, [CommandKeyValue(IsRequired = true)] string contentHash, [CommandKeyValue(IsRequired = true)] string versionHash)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		service.AddDelta(name, revision, file, isSeed, isPlaceHolder, contentHash, versionHash);
	}

	[Usage("repository list-repositories name:")]
	[CommandForm(Name = "list-repositories")]
	public void ListRepositories(CommandContext context)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		List<string> allRepositories = service.GetAllRepositories();
		context.Messages.Add(ObjectExtensions.ToJson(allRepositories));
	}

	[CommandForm(Name = "list-unfinished-changes")]
	[Usage("repository list-unfinished-changes name:")]
	public void ListUnfinishedChanges(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		List<IRevLog> unfinishedChanges = service.GetUnfinishedChanges(name);
		context.Messages.Add(ObjectExtensions.ToJson(unfinishedChanges));
	}

	[CommandForm(Name = "list-pending-changes")]
	[Usage("repository list-pending-changes name:")]
	public void ListPendingChanges(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		List<IRevLog> pendingChanges = service.GetPendingChanges(name);
		context.Messages.Add(ObjectExtensions.ToJson(pendingChanges));
	}

	[CommandForm(Name = "list-pending-activations")]
	[Usage("repository list-pending-activations name:")]
	public void ListPendingActivations(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		List<IRevLog> pendingActivations = service.GetPendingActivations(name);
		context.Messages.Add(ObjectExtensions.ToJson(pendingActivations));
	}

	[Usage("repository list-all-changes name:")]
	[CommandForm(Name = "list-all-changes")]
	public void ListAllChanges(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		List<IRevLog> allChanges = service.GetAllChanges(name);
		context.Messages.Add(ObjectExtensions.ToJson(allChanges));
	}

	[Usage("repository list-activated-changes name:")]
	[CommandForm(Name = "list-activated-changes")]
	public void ListActivatedChanges(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		List<IRevLog> activatedChanges = service.GetActivatedChanges(name);
		context.Messages.Add(ObjectExtensions.ToJson(activatedChanges));
	}

	[Usage("repository get-head-hash name:")]
	[CommandForm(Name = "get-head-hash")]
	public void GetHeadRevision(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		string headRevision = service.GetHeadRevision(name);
		context.Messages.Add(ObjectExtensions.ToJson(headRevision));
	}

	[Usage("repository get-active-revision name:")]
	[CommandForm(Name = "get-active-revision")]
	public void GetAcitveRevision(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		string activeRevision = service.GetActiveRevision(name);
		context.Messages.Add(ObjectExtensions.ToJson(activeRevision));
	}

	[CommandForm(Name = "get-active-label")]
	[Usage("repository get-active-label name:")]
	public void GetAcitveLabel(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		string activeLabel = service.GetActiveLabel(name);
		context.Messages.Add(ObjectExtensions.ToJson(activeLabel));
	}

	[CommandForm(Name = "get-staged-revision")]
	[Usage("repository get-staged-revision name:")]
	public void GetStagedRevision(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		string stagedRevision = service.GetStagedRevision(name);
		context.Messages.Add(ObjectExtensions.ToJson(stagedRevision));
	}

	[CommandForm(Name = "list-applied-changes")]
	[Usage("repository list-applied-changes name:")]
	public void ListAppliedChanges(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		List<IRevLog> appliedChanges = service.GetAppliedChanges(name);
		context.Messages.Add(ObjectExtensions.ToJson(appliedChanges));
	}

	[CommandForm(Name = "update-to")]
	[Usage("repository update-to name: hash:")]
	public void UpdateTo(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string hash)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = service.UpdateTo(name, hash);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				context.Errors.Add(e.IsWarning ? Redbox.IPC.Framework.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.IPC.Framework.Error.NewError(e.Code, e.Description, e.Details));
			});
		}
	}

	[Usage("repository rebuild-to name: hash:")]
	[CommandForm(Name = "rebuild-to")]
	public void RebuildToHash(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string hash)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = service.RebuildTo(name, hash);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				context.Errors.Add(e.IsWarning ? Redbox.IPC.Framework.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.IPC.Framework.Error.NewError(e.Code, e.Description, e.Details));
			});
		}
	}

	[CommandForm(Name = "update-to-head")]
	[Usage("repository update-to-head name:")]
	public void UpdateTo(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = service.UpdateToHead(name);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				context.Errors.Add(e.IsWarning ? Redbox.IPC.Framework.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.IPC.Framework.Error.NewError(e.Code, e.Description, e.Details));
			});
		}
	}

	[CommandForm(Name = "rebuild-to-head")]
	[Usage("repository rebuild-to-head name:")]
	public void RebuildTo(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = service.RebuildToHead(name);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				context.Errors.Add(e.IsWarning ? Redbox.IPC.Framework.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.IPC.Framework.Error.NewError(e.Code, e.Description, e.Details));
			});
		}
	}

	[CommandForm(Name = "activate-to-head")]
	[Usage("repository activate-to-head name:")]
	public void ActivateToHead(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = service.ActivateToHead(name, out var deferredMove);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				context.Errors.Add(e.IsWarning ? Redbox.IPC.Framework.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.IPC.Framework.Error.NewError(e.Code, e.Description, e.Details));
			});
		}
		else
		{
			context.Messages.Add(ObjectExtensions.ToJson(deferredMove));
		}
	}

	[Usage("repository verify name:")]
	[CommandForm(Name = "verify")]
	public void Verify(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = service.Verify(name);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Redbox.UpdateManager.ComponentModel.Error e)
			{
				context.Errors.Add(e.IsWarning ? Redbox.IPC.Framework.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.IPC.Framework.Error.NewError(e.Code, e.Description, e.Details));
			});
		}
	}

	[CommandForm(Name = "repair")]
	[Usage("repository repair name:")]
	public void Repair(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = service.Repair(name, out var deferredMove);
		if (errorList.ContainsError())
		{
			context.Messages.Add(ObjectExtensions.ToJson(deferredMove));
		}
	}

	[CommandForm(Name = "reset")]
	[Usage("repository reset name:")]
	public void Reset(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		service.Reset(name, out var defferedDelete);
		context.Messages.Add(ObjectExtensions.ToJson(defferedDelete));
	}

	[Usage("repository reset-all")]
	[CommandForm(Name = "reset-all")]
	public void ResetAll(CommandContext context)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		service.Reset(out var _);
	}

	[CommandForm(Name = "set-label")]
	[Usage("repository set-label name: revision: label:")]
	public void SetLabel(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string revision, [CommandKeyValue(IsRequired = true)] string label)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		service.SetLabel(name, revision, label);
	}

	[Usage("repository get-label name: revision:")]
	[CommandForm(Name = "get-label")]
	public void GetLabel(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true)] string revision)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		context.Messages.Add(ObjectExtensions.ToJson(service.GetLabel(name, revision)));
	}
}
