using System;

namespace Redbox.Rental.UI.Views;

public class ActionCallbackInfo
{
	public Action CancelAction { get; set; }

	public Action<object> CallbackAction { get; set; }
}
