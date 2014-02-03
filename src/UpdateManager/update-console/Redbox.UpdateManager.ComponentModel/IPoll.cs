using System.Collections.Generic;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.ComponentModel;

internal interface IPoll
{
	ErrorList Poll(List<PollRequest> pollRequestList, out List<PollReply> pollReplyList);
}
