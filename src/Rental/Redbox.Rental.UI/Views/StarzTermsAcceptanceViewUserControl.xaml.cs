using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "StarzTermsAcceptanceView")]
public partial class StarzTermsAcceptanceViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private StarzTermsAcceptanceViewModel StarzTermsAcceptanceViewModel => base.DataContext as StarzTermsAcceptanceViewModel;

	public StarzTermsAcceptanceViewUserControl()
	{
		InitializeComponent();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return StarzTermsAcceptanceViewModel?.ProcessGetSpeechControl();
	}
}
