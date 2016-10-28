using System;
using System.Collections.Generic;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

public interface IReservation
{
	string CardId { get; }

	DateTime TransactionDate { get; }

	long ReferenceNumber { get; }

	IDictionary<string, ICollection<string>> Barcodes { get; }
}
