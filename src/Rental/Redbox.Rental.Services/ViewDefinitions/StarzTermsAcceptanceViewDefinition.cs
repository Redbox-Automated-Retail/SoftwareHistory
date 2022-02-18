using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "starz_terms_acceptance_view")]
public class StarzTermsAcceptanceViewDefinition : BaseViewDefinition, IViewDefinition
{
	public StarzTermsAcceptanceViewDefinition()
	{
		base.ViewName = "starz_terms_acceptance_view";
		base.ActiveFlag = true;
		base.ControlName = "StarzTermsAcceptanceView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			StarzTermsAcceptanceViewController.ConfigureStarzTermsAcceptanceView(viewFrameInstance.Parameter as StarzTermsAcceptanceViewParameters);
		};
	}
}
