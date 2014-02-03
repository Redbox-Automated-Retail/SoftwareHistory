using System.Collections.Generic;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.ComponentModel;

internal interface IFileSetService
{
	bool RebootRequired { get; }

	ErrorList ProcessPollReply(List<PollReply> pollReplyList);

	void MarkRebootRequired();
}
