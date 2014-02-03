namespace Redbox.TicketServices.Proxy.ComponentModel;

internal class Event
{
	public long EventId { get; set; }

	public string EventName { get; set; }

	public string ShortNote { get; set; }

	public string LongNote { get; set; }

	public int PrimaryCategoryID { get; set; }

	public decimal? MaxSupplierPrice { get; set; }

	public decimal? MinSupplierPrice { get; set; }

	public string SortName { get; set; }

	public int Featured { get; set; }
}
