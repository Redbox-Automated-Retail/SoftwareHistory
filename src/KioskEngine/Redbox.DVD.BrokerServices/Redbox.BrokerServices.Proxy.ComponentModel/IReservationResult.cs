using System.Collections.Generic;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

internal interface IReservationResult
{
	bool Success { get; set; }

	string ErrorMessage { get; set; }

	List<IReservedItem> Items { get; set; }
}
