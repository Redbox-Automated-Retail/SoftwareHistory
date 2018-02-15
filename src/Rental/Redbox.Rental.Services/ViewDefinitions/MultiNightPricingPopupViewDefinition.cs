using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "multi_night_pricing_popup_view")]
public class MultiNightPricingPopupViewDefinition : BaseViewDefinition, IViewDefinition
{
	public MultiNightPricingPopupViewDefinition()
	{
		base.ViewName = "multi_night_pricing_popup_view";
		base.ActiveFlag = true;
		base.ControlName = "MultiNightPricingPopupView";
		base.Clear = false;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			MultiNightPricingPopupViewController.ConfigureMultiNightPricingPopupView(viewFrameInstance?.Parameter as ITitleProduct);
		};
		base.OnLeave = delegate
		{
			MultiNightPricingPopupViewController.LeaveMultiNightPricingPopupView();
		};
	}
}
