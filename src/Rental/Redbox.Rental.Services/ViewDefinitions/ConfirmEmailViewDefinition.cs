using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "confirm_email_view")]
public class ConfirmEmailViewDefinition : BaseViewDefinition, IViewDefinition
{
	public ConfirmEmailViewDefinition()
	{
		base.ViewName = "confirm_email_view";
		base.ActiveFlag = true;
		base.ControlName = "ConfirmEmailView";
		base.OnEnter = delegate
		{
			ConfirmEmailViewController.ConfigureView();
		};
	}
}
