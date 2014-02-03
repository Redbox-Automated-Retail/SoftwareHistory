using System.Collections.Generic;

namespace Redbox.TicketServices.Proxy.ComponentModel;

internal interface IExcludeProductions
{
	List<long> GetList();

	void RemoveIfNotIn(List<long> list);

	void RemoveAll();

	void Remove(long production_id);

	void Add(long production_id);
}
