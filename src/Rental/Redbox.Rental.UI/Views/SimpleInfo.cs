using System;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public class SimpleInfo
{
	public Action CancelAction { get; set; }

	public Action ContinueAction { get; set; }

	public SimpleModel Model { get; set; }
}
