using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "OptInThankYouView")]
public partial class OptInThankYouViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	public OptInThankYouViewUserControl()
	{
		InitializeComponent();
	}
}
