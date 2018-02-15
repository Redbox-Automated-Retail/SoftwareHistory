using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "prompt_sign_in_view")]
public class PromptSignInDefinition : BaseViewDefinition, IViewDefinition
{
	public PromptSignInDefinition()
	{
		base.ViewName = "prompt_sign_in_view";
		base.ActiveFlag = true;
		base.ControlName = "PromptSignInView";
		base.OnEnter = delegate
		{
			PromptSignInController.ConfigureView();
		};
	}
}
