using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "join_perks_promo_view")]
public class JoinPerksPromoDefinition : BaseViewDefinition, IViewDefinition
{
	public JoinPerksPromoDefinition()
	{
		base.ViewName = "join_perks_promo_view";
		base.ActiveFlag = true;
		base.ControlName = "JoinPerksPromoView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			SimpleViewController.ConfigurePromoView(viewFrameInstance?.Parameter as SimpleInfo);
		};
	}
}
