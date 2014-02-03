using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.ServiceProxies;

public class RepositoryServiceProxy : IRepositoryService, IDisposable
{
	private string m_url;

	public static RepositoryServiceProxy Instance => Singleton<RepositoryServiceProxy>.Instance;

	public void Initialize(string url)
	{
		m_url = url;
		ServiceLocator.Instance.AddService(typeof(IRepositoryService), this);
	}

	public void Dispose()
	{
	}

	public void TrimRepository(string name)
	{
		string command = $"repository trim-repository name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ActivateTo(string name, string hash, out bool deferredMove)
	{
		string command = $"repository activate-to name: '{name}' hash: '{hash}'";
		deferredMove = false;
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (clientCommandResult.Success)
		{
			deferredMove = StringExtensions.ToObject<bool>(clientCommandResult.CommandMessages[0]);
		}
		return ConvertToComponentErrors(clientCommandResult.Errors);
	}

	public string FormatStagedFileName(string file)
	{
		string command = $"repository format-stage-file-name file: '{StringExtensions.Escaped(file)}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		return StringExtensions.ToObject<string>(clientCommandResult.CommandMessages[0]);
	}

	public void UnpackChangeSet(string name, Guid id, string archive)
	{
		string command = string.Format("repository unpack-change-set name: '{2}' id: '{0}' archive: '{1}'", id, StringExtensions.Escaped(archive), name);
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
	}

	public bool ContainsRepository(string name)
	{
		string command = $"repository contains-repository name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		return clientCommandResult.Success && StringExtensions.ToObject<bool>(clientCommandResult.CommandMessages[0]);
	}

	public void AddRepository(string name)
	{
		string command = $"repository add-repository name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
	}

	public void AddRevisions(string name, List<string> revisions)
	{
		string command = $"repository add-revisions name: '{name}' revisions: '{StringExtensions.EscapedJson(ObjectExtensions.ToJson(revisions))}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
	}

	public void SetLabel(string name, string revision, string label)
	{
		if (string.IsNullOrEmpty(label))
		{
			return;
		}
		string command = $"repository set-label name: '{name}' revision: '{revision}' label: '{label}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
	}

	public void RemoveRevisions(string name, List<string> revisions)
	{
		string command = $"repository remove-revisions name: '{name}' revisions: '{StringExtensions.EscapedJson(ObjectExtensions.ToJson(revisions))}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
	}

	public void AddDelta(string name, List<DeltaItem> deltaItemList)
	{
		throw new ApplicationException("RepositoryServiceProxy.AddDelta is not implemented.");
	}

	public void AddDelta(string name, string revision, string file, bool isSeed, bool isPlaceHolder, string contentHash, string versionHash)
	{
		string command = $"repository add-delta name: '{name}' revision: '{revision}' file: '{StringExtensions.Escaped(file)}' isSeed: {isSeed} isPlaceHolder: {isPlaceHolder} contentHash: '{contentHash}' versionHash: '{versionHash}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
	}

	public string GetLabel(string name, string revision)
	{
		string command = $"repository get-label name: '{name}' revision: '{revision}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		return StringExtensions.ToObject<string>(clientCommandResult.CommandMessages[0]);
	}

	public List<string> GetAllRepositories()
	{
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString("repository list-repositories");
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		return StringExtensions.ToObject<List<string>>(clientCommandResult.CommandMessages[0]);
	}

	public List<IRevLog> GetUnfinishedChanges(string name)
	{
		string command = $"repository list-unfinished-changes name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		List<IRevLog> list = new List<IRevLog>();
		StringExtensions.ToObject<List<RevLog>>(clientCommandResult.CommandMessages[0]).ForEach(list.Add);
		return list;
	}

	public List<IRevLog> GetPendingChanges(string name)
	{
		string command = $"repository list-pending-changes name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		List<IRevLog> list = new List<IRevLog>();
		StringExtensions.ToObject<List<RevLog>>(clientCommandResult.CommandMessages[0]).ForEach(list.Add);
		return list;
	}

	public List<IRevLog> GetPendingActivations(string name)
	{
		string command = $"repository list-pending-activations name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		List<IRevLog> list = new List<IRevLog>();
		StringExtensions.ToObject<List<RevLog>>(clientCommandResult.CommandMessages[0]).ForEach(list.Add);
		return list;
	}

	public List<IRevLog> GetAllChanges(string name)
	{
		string command = $"repository list-all-changes name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		List<IRevLog> list = new List<IRevLog>();
		StringExtensions.ToObject<List<RevLog>>(clientCommandResult.CommandMessages[0]).ForEach(list.Add);
		return list;
	}

	public List<IRevLog> GetActivatedChanges(string name)
	{
		string command = $"repository list-activated-changes name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		List<IRevLog> list = new List<IRevLog>();
		StringExtensions.ToObject<List<RevLog>>(clientCommandResult.CommandMessages[0]).ForEach(list.Add);
		return list;
	}

	public string GetHeadRevision(string name)
	{
		string command = $"repository get-head-hash name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		return StringExtensions.ToObject<string>(clientCommandResult.CommandMessages[0]);
	}

	public string GetActiveRevision(string name)
	{
		string command = $"repository get-active-revision name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		return StringExtensions.ToObject<string>(clientCommandResult.CommandMessages[0]);
	}

	public string GetStagedRevision(string name)
	{
		string command = $"repository get-staged-revision name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		return StringExtensions.ToObject<string>(clientCommandResult.CommandMessages[0]);
	}

	public string GetActiveLabel(string name)
	{
		string command = $"repository get-active-label name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		return StringExtensions.ToObject<string>(clientCommandResult.CommandMessages[0]);
	}

	public List<IRevLog> GetAppliedChanges(string name)
	{
		string command = $"repository list-applied-changes name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		return StringExtensions.ToObject<List<IRevLog>>(clientCommandResult.CommandMessages[0]);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList UpdateTo(string name, string targetHash)
	{
		string command = $"repository update-to name: '{name}' hash: '{targetHash}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		return ConvertToComponentErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList RebuildTo(string name, string targetHash)
	{
		string command = $"repository rebuild-to name: '{name}' hash: '{targetHash}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		return ConvertToComponentErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList RebuildToHead(string name)
	{
		string command = $"repository rebuild-to-head name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		return ConvertToComponentErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ActivateToHead(string name, out bool deferredMove)
	{
		deferredMove = false;
		string command = $"repository activate-to-head name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (clientCommandResult.Success)
		{
			deferredMove = StringExtensions.ToObject<bool>(clientCommandResult.CommandMessages[0]);
		}
		return ConvertToComponentErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList UpdateToHead(string name)
	{
		string command = $"repository update-to-head name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		return ConvertToComponentErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList Verify(string name)
	{
		string command = $"repository verify name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		return ConvertToComponentErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList Repair(string name, out bool deferredMove)
	{
		deferredMove = false;
		string command = $"repository repair name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (clientCommandResult.Success)
		{
			deferredMove = StringExtensions.ToObject<bool>(clientCommandResult.CommandMessages[0]);
		}
		return ConvertToComponentErrors(clientCommandResult.Errors);
	}

	public void Reset(string name, out bool defferedDelete)
	{
		defferedDelete = false;
		string command = $"repository reset name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		defferedDelete = StringExtensions.ToObject<bool>(clientCommandResult.CommandMessages[0]);
	}

	public void Reset(out bool defferedDelete)
	{
		defferedDelete = false;
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString("repository reset-all");
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		defferedDelete = StringExtensions.ToObject<bool>(clientCommandResult.CommandMessages[0]);
	}

	public bool Subscribed(string name)
	{
		string command = $"repository subscribed name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		return clientCommandResult.Success && StringExtensions.ToObject<bool>(clientCommandResult.CommandMessages[0]);
	}

	public void EndableSubscription(string name)
	{
		string command = $"repository enable-subscription name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
	}

	public void DisableSubscription(string name)
	{
		string command = $"repository disable-subscription name: '{name}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
	}

	private RepositoryServiceProxy()
	{
	}

	private static Redbox.UpdateManager.ComponentModel.ErrorList ConvertToComponentErrors(List<Redbox.IPC.Framework.Error> ipcErrors)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errors = new Redbox.UpdateManager.ComponentModel.ErrorList();
		ipcErrors.ForEach(delegate(Redbox.IPC.Framework.Error e)
		{
			errors.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details));
		});
		return errors;
	}
}
