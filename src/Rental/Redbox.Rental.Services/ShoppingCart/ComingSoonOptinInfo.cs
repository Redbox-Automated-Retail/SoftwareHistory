using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.ShoppingCart;

public class ComingSoonOptinInfo : IComingSoonOptinInfo
{
	private List<IOptInItem> _optInItems = new List<IOptInItem>();

	public string EmailAddress { get; set; }

	public List<IOptInItem> OptInItems => _optInItems;

	public void AddProduct(long productId)
	{
		IOptInItem optInItem = ServiceLocator.Instance.GetService<IPromotionServices>()?.NewOptInItem();
		if (optInItem != null)
		{
			optInItem.ItemId = productId;
			optInItem.Item_Type = Item_Type.KioskRentalTitleID;
			optInItem.OptInMethodType = OptInMethodType.EmailAddress;
			optInItem.OptInMethodData = EmailAddress;
			optInItem.TemplateName = "EmailTemplate";
			optInItem.CampaignName = "ComingSoonRentalCampaign";
			OptInItems.Add(optInItem);
		}
	}

	public void RemoveProduct(long productId)
	{
		IOptInItem optInItem = OptInItems?.FirstOrDefault((IOptInItem x) => x.ItemId == productId);
		if (optInItem != null)
		{
			OptInItems.Remove(optInItem);
		}
	}
}
