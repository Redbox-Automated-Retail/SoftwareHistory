using System.Collections.Generic;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.ComponentModel;

internal interface IPollRequestReply
{
	ErrorList GetPollRequest(out List<PollRequest> pollRequestList);

	ErrorList ProcessPollReply(List<PollReply> pollReplyList);
}
