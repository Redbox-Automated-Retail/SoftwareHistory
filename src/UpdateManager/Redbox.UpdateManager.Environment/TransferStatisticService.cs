using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Client;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.Environment;

public class TransferStatisticService : IPollRequestReply
{
	public static TransferStatisticService Instance => Singleton<TransferStatisticService>.Instance;

	public ErrorList GetPollRequest(out List<PollRequest> pollRequestList)
	{
		pollRequestList = new List<PollRequest>();
		ErrorList errorList = new ErrorList();
		try
		{
			IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
			for (TransferStatisticReport transferStatisticReport = service.Peek<TransferStatisticReport>("statistic"); transferStatisticReport != null; transferStatisticReport = service.Peek<TransferStatisticReport>("statistic"))
			{
				PollRequest pollRequest = new PollRequest();
				pollRequest.PollRequestType = PollRequestType.TransferStatisticReport;
				pollRequest.Data = ObjectExtensions.ToJson(transferStatisticReport);
				PollRequest item = pollRequest;
				pollRequestList.Add(item);
				LogHelper.Instance.Log("Enqueued transfer statistics for revision: {0}", transferStatisticReport.ChangeSet);
				service.Dequeue("statistic");
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("StatusMessageService", "Unhandled exception occurred in GetPollRequest.", e));
		}
		return errorList;
	}

	public ErrorList ProcessPollReply(List<PollReply> pollReplyList)
	{
		return new ErrorList();
	}

	private TransferStatisticService()
	{
	}
}
