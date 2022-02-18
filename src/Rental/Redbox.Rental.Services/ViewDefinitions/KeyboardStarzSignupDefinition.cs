using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "keyboard_starz_signup_view")]
public class KeyboardStarzSignupDefinition : BaseViewDefinition, IViewDefinition
{
	public KeyboardStarzSignupDefinition()
	{
		base.ViewName = "keyboard_starz_signup_view";
		base.ActiveFlag = true;
		base.ControlName = "KeyboardStarzSignupView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			KeyboardViewController.ConfigureView(viewFrameInstance?.Parameter);
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			KeyboardViewController.ClosingView(viewFrameInstance?.Parameter);
		};
	}
}
