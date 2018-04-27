using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "browse_view")]
public class BrowseViewDefinition : BaseViewDefinition, IViewDefinition
{
	public BrowseViewDefinition()
	{
		base.ViewName = "browse_view";
		base.ActiveFlag = true;
		base.ControlName = "BrowseView";
		base.OnEnter = delegate
		{
			BrowseViewController.ConfigureBrowseView();
		};
		base.OnLeave = delegate
		{
			BrowseViewController.CleanupBrowseView();
		};
	}
}
