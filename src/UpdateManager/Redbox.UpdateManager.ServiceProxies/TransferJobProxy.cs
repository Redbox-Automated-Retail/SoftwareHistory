using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.ServiceProxies;

public class TransferJobProxy : ITransferJob
{
	private string m_url;

	public TransferJobType JobType { get; set; }

	public string Name { get; set; }

	public Guid ID { get; set; }

	public DateTime StartTime { get; set; }

	public DateTime ModifiedTime { get; set; }

	public DateTime? FinishTime { get; set; }

	public string Owner { get; set; }

	public ulong TotalBytesTransfered { get; set; }

	public ulong TotalBytes { get; set; }

	public TransferStatus Status { get; set; }

	public Redbox.UpdateManager.ComponentModel.ErrorList AddItem(string url, string file)
	{
		ClientCommandResult clientCommandResult = ExecuteCommandString($"transfer add-item id: '{ID}' url: '{StringExtensions.Escaped(url)}' file: '{StringExtensions.Escaped(file)}'");
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList Complete()
	{
		ClientCommandResult clientCommandResult = ExecuteCommandString($"transfer complete id: '{ID}'");
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList Cancel()
	{
		ClientCommandResult clientCommandResult = ExecuteCommandString($"transfer cancel id: '{ID}'");
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList Suspend()
	{
		ClientCommandResult clientCommandResult = ExecuteCommandString($"transfer suspend id: '{ID}'");
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList Resume()
	{
		ClientCommandResult clientCommandResult = ExecuteCommandString($"transfer resume id: '{ID}'");
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList SetPriority(TransferJobPriority priority)
	{
		ClientCommandResult clientCommandResult = ExecuteCommandString(string.Format("transfer set-priority id: '{0}' priority: {1}", ID, priority.ToString("G")));
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList SetCallback(ITransferCallbackParameters parameters)
	{
		throw new NotImplementedException();
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList TakeOwnership()
	{
		return new Redbox.UpdateManager.ComponentModel.ErrorList();
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList SetNoProgressTimeout(uint period)
	{
		ClientCommandResult clientCommandResult = ExecuteCommandString($"transfer set-no-progress-timeout id: '{ID}' timeout: {period}");
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList SetMinimumRetryDelay(uint seconds)
	{
		ClientCommandResult clientCommandResult = ExecuteCommandString($"transfer set-minimum-retry-delay id: '{ID}' seconds: {seconds}");
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public void GetErrors(out Redbox.UpdateManager.ComponentModel.ErrorList errors)
	{
		ClientCommandResult clientCommandResult = ExecuteCommandString($"transfer get-errors id: '{ID}'");
		errors = ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList GetItems(out List<ITransferItem> items)
	{
		items = new List<ITransferItem>();
		ClientCommandResult clientCommandResult = ExecuteCommandString($"transfer get-items id: '{ID}'");
		if (clientCommandResult.Success)
		{
			items = StringExtensions.ToObject<List<TransferItem>>(clientCommandResult.CommandMessages[0]).ConvertAll((Converter<TransferItem, ITransferItem>)((TransferItem t) => t));
		}
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public TransferJobProxy()
	{
	}

	internal void SetUrl(string url)
	{
		m_url = url;
	}

	internal TransferJobProxy(Guid id, string url)
	{
		ID = id;
		m_url = url;
	}

	private ClientCommandResult ExecuteCommandString(string command)
	{
		using UpdateManagerService updateManagerService = new UpdateManagerService(m_url);
		return updateManagerService.ExecuteCommandString(command);
	}

	private static Redbox.UpdateManager.ComponentModel.ErrorList ConvertFromIPCErrors(List<Redbox.IPC.Framework.Error> ipc)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errors = new Redbox.UpdateManager.ComponentModel.ErrorList();
		ipc.ForEach(delegate(Redbox.IPC.Framework.Error e)
		{
			errors.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details));
		});
		return errors;
	}
}
