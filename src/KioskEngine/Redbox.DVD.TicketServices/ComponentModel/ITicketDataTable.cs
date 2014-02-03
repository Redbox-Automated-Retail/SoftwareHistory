using System.Collections.Generic;

namespace Redbox.TicketServices.Proxy.ComponentModel;

internal interface ITicketDataTable
{
	string TableName { get; set; }

	IEnumerable<string> ColumnNames { get; set; }

	Dictionary<string, object[]> Data { get; set; }

	List<object[]> DataList { get; set; }

	void Add(ITicketDataTable ticketDataTable);
}
