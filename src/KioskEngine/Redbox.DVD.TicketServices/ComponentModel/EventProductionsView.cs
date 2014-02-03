using System.Collections.Generic;

namespace Redbox.TicketServices.Proxy.ComponentModel;

internal class EventProductionsView
{
	public Event Event { get; set; }

	public List<Production> Productions { get; set; }
}
