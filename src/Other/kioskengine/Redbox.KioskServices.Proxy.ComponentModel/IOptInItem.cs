namespace Redbox.KioskServices.Proxy.ComponentModel;

public interface IOptInItem
{
	long ItemId { get; set; }

	Item_Type Item_Type { get; set; }

	OptInMethodType OptInMethodType { get; set; }

	string OptInMethodData { get; set; }

	string TemplateName { get; set; }

	string CampaignName { get; set; }
}
