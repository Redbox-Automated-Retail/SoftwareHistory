namespace Redbox.KioskServices.Proxy.Requests;

public class BlurayUpsell
{
	public int ProductGroupId { get; set; }

	public string Offer { get; set; }

	public long TitleId { get; set; }
}
