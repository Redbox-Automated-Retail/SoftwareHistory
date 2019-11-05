using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "optin_thank_you_view")]
public class OptInThankYouViewDefinition : BaseViewDefinition, IViewDefinition
{
	public OptInThankYouViewDefinition()
	{
		base.ViewName = "optin_thank_you_view";
		base.ControlName = "OptInThankYouView";
		base.OnEnter = delegate
		{
			OptInThankYouViewController.ConfigureView();
		};
		base.OnLeave = delegate
		{
			OptInThankYouViewController.LeaveView();
		};
	}
}
