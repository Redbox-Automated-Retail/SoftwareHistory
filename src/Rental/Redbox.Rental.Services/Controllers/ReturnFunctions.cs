using System;
using Redbox.Core;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services.Controllers;

public class ReturnFunctions
{
	public static void ShowMachineFullWarning()
	{
		Action action = delegate
		{
			ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession("machine full return");
		};
		MessagePopupViewController.ShowMachineFullWarning(action, action);
	}
}
