using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Client;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.Environment;

public class RepositoryPollService : IPollRequestReply
{
	private string m_previousClientRepositoryDataDTOHash = string.Empty;

	private List<ClientRepositoryDataDTO> m_previousClientRepositoryDataDTO = new List<ClientRepositoryDataDTO>();

	private int m_syncId;

	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private int _lockTimeout = 3000;

	public static RepositoryPollService Instance => Singleton<RepositoryPollService>.Instance;

	public ErrorList GetPollRequest(out List<PollRequest> pollRequestList)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			pollRequestList = new List<PollRequest>();
			if (!_lock.TryEnterWriteLock(_lockTimeout))
			{
				errorList.Add(Error.NewError("RepositoryPollService.GetPollRequest", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
				return errorList;
			}
			try
			{
				errorList.AddRange(GetClientRepositoryDataDTO(out var currentClientRepositoryDataDTO));
				string text = RepositoryDiffMerge.Hash(currentClientRepositoryDataDTO);
				if (m_syncId == 0 || string.IsNullOrEmpty(m_previousClientRepositoryDataDTOHash))
				{
					m_syncId = 1;
					RepositoryDiffWrapper repositoryDiffWrapper = new RepositoryDiffWrapper();
					repositoryDiffWrapper.O = text;
					repositoryDiffWrapper.A = currentClientRepositoryDataDTO;
					RepositoryDiffWrapper wrapper = repositoryDiffWrapper;
					PollRequest pollRequest = new PollRequest();
					pollRequest.PollRequestType = PollRequestType.Repositories;
					pollRequest.SyncId = 1;
					pollRequest.Data = RepositoryDiffMerge.Minimize(wrapper);
					PollRequest item = pollRequest;
					pollRequestList.Add(item);
					m_previousClientRepositoryDataDTO = currentClientRepositoryDataDTO;
					m_previousClientRepositoryDataDTOHash = text;
					return errorList;
				}
				if (text != m_previousClientRepositoryDataDTOHash)
				{
					if (!RepositoryDiffMerge.TryDiff(m_previousClientRepositoryDataDTO, currentClientRepositoryDataDTO, verify: true, out var rdw))
					{
						m_syncId = 0;
						errorList.Add(Error.NewWarning("RPS01", "TryDiff failed", "Resetting SyncID and sending up a full sync"));
						return errorList;
					}
					m_syncId++;
					PollRequest pollRequest2 = new PollRequest();
					pollRequest2.PollRequestType = PollRequestType.Repositories;
					pollRequest2.SyncId = m_syncId;
					pollRequest2.Data = RepositoryDiffMerge.Minimize(rdw);
					PollRequest item2 = pollRequest2;
					pollRequestList.Add(item2);
					m_previousClientRepositoryDataDTOHash = text;
					m_previousClientRepositoryDataDTO = currentClientRepositoryDataDTO;
				}
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("RPS002", "There was an unhandled exception in RepositoryPollService.GetPollRequest", e));
			pollRequestList = new List<PollRequest>();
		}
		return errorList;
	}

	public ErrorList ProcessPollReply(List<PollReply> pollReplyList)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("RepositoryService.ProcessPollReply", LogEntryType.Debug);
			if (pollReplyList == null || !pollReplyList.Any())
			{
				LogHelper.Instance.Log("No pollReplyList in RepositoryService.ProcessPollReply", LogEntryType.Debug);
				return errorList;
			}
			LogHelper.Instance.Log("PollReplyList in RepositoryService.ProcessPollReply: " + ObjectExtensions.ToJson(pollReplyList), LogEntryType.Debug);
			if (pollReplyList.Any((PollReply pr) => (pr.PollReplyType == PollReplyType.RepositoryChangeSet && pr.SyncId == 0 && string.IsNullOrEmpty(pr.Data)) || pr.PollReplyType == PollReplyType.UpdateRepositories))
			{
				LogHelper.Instance.Log("Server requested full sync", LogEntryType.Info);
				if (!_lock.TryEnterWriteLock(_lockTimeout))
				{
					errorList.Add(Error.NewError("RepositoryPollService.ProcessPollReply", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
					return errorList;
				}
				try
				{
					m_syncId = 0;
				}
				finally
				{
					_lock.ExitWriteLock();
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("RPS003", "There was an unhandled exception in RepositoryPollService.ProcessPollReply", e));
		}
		return errorList;
	}

	private RepositoryPollService()
	{
	}

	private static IEnumerable<Error> GetClientRepositoryDataDTO(out List<ClientRepositoryDataDTO> currentClientRepositoryDataDTO)
	{
		ErrorList errorList = new ErrorList();
		currentClientRepositoryDataDTO = new List<ClientRepositoryDataDTO>();
		try
		{
			IRepositoryService repositoryService = ServiceLocator.Instance.GetService<IRepositoryService>();
			ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
			List<string> allRepositories = repositoryService.GetAllRepositories();
			currentClientRepositoryDataDTO = allRepositories.Select((string repositoryName) => new ClientRepositoryDataDTO
			{
				R = repositoryName,
				H = repositoryService.GetHeadRevision(repositoryName),
				A = repositoryService.GetActiveRevision(repositoryName),
				S = repositoryService.GetStagedRevision(repositoryName),
				I = "0000000000000000000000000000000000000000"
			}).ToList();
			errorList.AddRange(service.AreJobsRunning(out var isRunning));
			if (isRunning)
			{
				IDataStoreService service2 = ServiceLocator.Instance.GetService<IDataStoreService>();
				errorList.AddRange(service.GetJobs(out var jobs, allUsers: true));
				foreach (ITransferJob item in jobs)
				{
					ChangeSet changeSet = service2.Get<ChangeSet>(item.ID);
					ClientRepositoryDataDTO clientRepositoryDataDTO = currentClientRepositoryDataDTO.FirstOrDefault((ClientRepositoryDataDTO crd) => crd.R == changeSet.Name);
					if (clientRepositoryDataDTO != null)
					{
						clientRepositoryDataDTO.I = changeSet.Head;
					}
				}
			}
			currentClientRepositoryDataDTO.Sort();
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("RPS010", "There was an error in gathering the clientRepositoryInfo", e));
		}
		return errorList;
	}
}
