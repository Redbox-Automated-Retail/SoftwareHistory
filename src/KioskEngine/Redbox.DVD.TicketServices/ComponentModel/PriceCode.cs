namespace Redbox.TicketServices.Proxy.ComponentModel;

internal class PriceCode
{
	public long ProductionId { get; set; }

	public int PriceCodeId { get; set; }

	public string PriceCodeShortNote { get; set; }

	public decimal? MinSupplierPricePerTicket { get; set; }

	public decimal? MaxSupplierPricePerTicket { get; set; }
}
