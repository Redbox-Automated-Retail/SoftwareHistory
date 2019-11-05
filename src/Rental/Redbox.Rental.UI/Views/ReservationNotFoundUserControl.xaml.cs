using System.Windows.Input;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "ReservationNotFoundView")]
public partial class ReservationNotFoundUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private ReservationNotFoundViewModel Model => base.DataContext as ReservationNotFoundViewModel;

	public ReservationNotFoundUserControl()
	{
		InitializeComponent();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return Model?.OnGetSpeechControl();
	}

	private void OkCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		Model?.ProcessOkButtonClick();
		HandleWPFHit();
	}
}
