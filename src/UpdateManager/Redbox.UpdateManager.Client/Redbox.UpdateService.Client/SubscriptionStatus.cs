using System;

namespace Redbox.UpdateService.Client;

internal class SubscriptionStatus
{
	public DateTime ModifiedOn { get; set; }

	public SubscriptionState State { get; set; }
}
