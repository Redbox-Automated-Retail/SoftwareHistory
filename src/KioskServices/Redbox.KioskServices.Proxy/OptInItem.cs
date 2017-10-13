using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy;

public class OptInItem : IOptInItem
{
	public long ItemId { get; set; }

	public Item_Type Item_Type { get; set; }

	public OptInMethodType OptInMethodType { get; set; }

	public string OptInMethodData { get; set; }

	public string TemplateName { get; set; }

	public string CampaignName { get; set; }
}
