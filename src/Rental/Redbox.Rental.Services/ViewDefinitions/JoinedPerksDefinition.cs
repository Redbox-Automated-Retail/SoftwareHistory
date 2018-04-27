using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "joined_perks_view")]
public class JoinedPerksDefinition : BaseViewDefinition, IViewDefinition
{
	public JoinedPerksDefinition()
	{
		base.ViewName = "joined_perks_view";
		base.ActiveFlag = true;
		base.ControlName = "JoinedPerksView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			SimpleViewController.ConfigurePerksView(viewFrameInstance?.Parameter as SimpleInfo);
		};
	}
}
