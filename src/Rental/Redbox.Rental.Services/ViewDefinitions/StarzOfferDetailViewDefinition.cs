using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "starz_offer_detail_view")]
public class StarzOfferDetailViewDefinition : BaseViewDefinition, IViewDefinition
{
	public StarzOfferDetailViewDefinition()
	{
		base.ViewName = "starz_offer_detail_view";
		base.ActiveFlag = true;
		base.ControlName = "StarzOfferDetailView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			StarzOfferDetailViewController.ConfigureView(viewFrameInstance.Parameter as IStarzSubscriptionProduct);
		};
		base.OnLeave = delegate
		{
			StarzOfferDetailViewController.ClosingView();
		};
	}
}
