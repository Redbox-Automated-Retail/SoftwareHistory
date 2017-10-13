using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views;

public class CollectionMessageViewFunctions
{
	[KernelFunction(Name = "kernel.check_collections_popup")]
	internal static void CheckCollectionsPopup(string reason)
	{
		CollectionMessageViewController.CheckCollectionsPopup(reason);
	}
}
