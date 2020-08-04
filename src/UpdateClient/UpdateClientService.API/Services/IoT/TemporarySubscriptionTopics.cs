using System;
using System.Collections.Generic;

namespace UpdateClientService.API.Services.IoT;

public class TemporarySubscriptionTopics
{
	public List<string> Topics { get; set; }

	public DateTime ExpireTime { get; set; }
}
