using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ReturnViewController
{
	public static void ConfigureReturnView(Action goBackAction)
	{
		IActor actor = ActorFunctions.GetActor("ReturnViewUserControl");
		if (actor != null && actor.WPFFrameworkElement is ReturnViewUserControl returnViewUserControl)
		{
			returnViewUserControl.OnGoBack += goBackAction;
			string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", "back_button");
			string dictionaryStringValue2 = BundleFunctions.GetDictionaryStringValue("strings", "disc_return_message");
			returnViewUserControl.BackButtonText = dictionaryStringValue;
			returnViewUserControl.MessageText = dictionaryStringValue2;
		}
	}

	public static void SetBackButtonIsVisible(bool isEnabled)
	{
		IActor actor = ActorFunctions.GetActor("ReturnViewUserControl");
		if (actor != null && actor.WPFFrameworkElement is ReturnViewUserControl returnViewUserControl)
		{
			returnViewUserControl.BackButtonVisibility = ((!isEnabled) ? Visibility.Hidden : Visibility.Visible);
		}
	}
}
