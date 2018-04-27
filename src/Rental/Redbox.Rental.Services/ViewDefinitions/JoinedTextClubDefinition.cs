using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "joind_text_club_view")]
public class JoinedTextClubDefinition : BaseViewDefinition, IViewDefinition
{
	public JoinedTextClubDefinition()
	{
		base.ViewName = "joind_text_club_view";
		base.ActiveFlag = true;
		base.ControlName = "JoinedTextClubView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			SimpleViewController.ConfigureTextView(viewFrameInstance?.Parameter as SimpleInfo);
		};
	}
}
