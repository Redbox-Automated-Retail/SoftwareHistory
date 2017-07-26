using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ComingSoonViewController
{
	public static void ConfigureComingSoonView(List<IComingSoonTitle> comingSoonTitles)
	{
		IActor actor = ActorFunctions.GetActor("ComingSoonViewUserControl");
		if (actor == null || !(actor.WPFFrameworkElement is ComingSoonViewUserControl comingSoonViewUserControl))
		{
			return;
		}
		if (BundleFunctions.GetCulture() == "es-MX")
		{
			comingSoonViewUserControl.ComingSoonTextStyle = "font_target_larger";
			comingSoonViewUserControl.RentalsAreOnTheirWayStyle = "font_target_large_overlays";
		}
		string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", "coming_soon_coming_soon");
		string dictionaryStringValue2 = BundleFunctions.GetDictionaryStringValue("strings", "rentals_coming_soon");
		comingSoonViewUserControl.ComingSoonText = dictionaryStringValue;
		comingSoonViewUserControl.RentalsAreOnTheirWayText = dictionaryStringValue2;
		foreach (IComingSoonTitle comingSoonTitle in comingSoonTitles)
		{
			comingSoonViewUserControl.AddComingSoonTitle(comingSoonTitle);
		}
	}
}
