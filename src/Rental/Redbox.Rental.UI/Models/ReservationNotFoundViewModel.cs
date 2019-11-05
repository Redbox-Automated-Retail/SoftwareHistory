using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class ReservationNotFoundViewModel
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public Action OnOkButtonClick { get; set; }

	public string TitleText { get; set; }

	public string MessageText { get; set; }

	public string OkButtonText { get; set; }

	public string ImageName { get; set; }

	public bool ActionExecuted { get; set; }

	public void ProcessOkButtonClick()
	{
		OnOkButtonClick?.Invoke();
	}
}
