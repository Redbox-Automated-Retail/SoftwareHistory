using System.Collections.Generic;
using Redbox.TicketServices.Proxy.ComponentModel;

namespace Redbox.TicketServices.Proxy;

internal class TicketDataTable : ITicketDataTable
{
	public string TableName { get; set; }

	public IEnumerable<string> ColumnNames { get; set; }

	public Dictionary<string, object[]> Data { get; set; }

	public List<object[]> DataList { get; set; }

	public void Add(ITicketDataTable ticketDataTable)
	{
		foreach (KeyValuePair<string, object[]> datum in ticketDataTable.Data)
		{
			if (!Data.ContainsKey(datum.Key))
			{
				Data[datum.Key] = datum.Value;
				DataList.Add(datum.Value);
			}
		}
	}
}
