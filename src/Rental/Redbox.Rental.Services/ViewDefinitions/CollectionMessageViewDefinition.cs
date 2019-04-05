using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "collection_message_view")]
public class CollectionMessageViewDefinition : BaseViewDefinition, IViewDefinition
{
	public CollectionMessageViewDefinition()
	{
		base.ViewName = "collection_message_view";
		base.ActiveFlag = true;
		base.ControlName = "CollectionMessageView";
		base.Clear = false;
	}
}
