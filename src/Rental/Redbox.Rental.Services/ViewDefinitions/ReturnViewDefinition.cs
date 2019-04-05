using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "return_view")]
public class ReturnViewDefinition : BaseViewDefinition, IViewDefinition
{
	public ReturnViewDefinition()
	{
		base.ViewName = "return_view";
		base.ActiveFlag = true;
		base.ControlName = "ReturnView";
	}
}
