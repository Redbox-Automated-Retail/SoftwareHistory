using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "review_popup_view")]
public class ReviewPopupViewDefinition : BaseViewDefinition, IViewDefinition
{
	public ReviewPopupViewDefinition()
	{
		base.ViewName = "review_popup_view";
		base.ActiveFlag = true;
		base.ControlName = "ReviewPopupView";
		base.Clear = false;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			ReviewPopupViewController.ConfigureReviewPopupView(viewFrameInstance?.Parameter as ReviewPopupInfo);
		};
		base.OnLeave = delegate
		{
			ReviewPopupViewController.LeaveReviewPopupView();
		};
	}
}
