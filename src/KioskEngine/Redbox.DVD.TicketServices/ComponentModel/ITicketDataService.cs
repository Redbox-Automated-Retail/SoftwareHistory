using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.TicketServices.Proxy.ComponentModel;

internal interface ITicketDataService
{
	bool SellTickets { get; }

	bool IsSellTicketsOn { get; }

	ErrorList Initialize();

	ErrorList Start();

	ErrorList Stop();

	ErrorList GetTableData(string tableName, out ITicketDataTable data);

	ErrorList GetImageName(int ownerTypeId, int subTypeId, long ownerId, out string imageName);

	ErrorList GetImage(string imageName, out byte[] data);

	bool RefreshIfNeeded();

	bool RefreshViews(string browseFilter, string sort);

	ErrorList GetBrowseEvents(out List<long> events);

	ErrorList GetCarouselEvents(out List<long> events);
}
