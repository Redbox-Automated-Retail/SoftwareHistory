using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class SignInViewModel : DependencyObject
{
	public Action OnEmailAndPasswordButtonClicked;

	public Action OnPhoneAndPinButtonClicked;

	public Action OnCancelButtonClicked;

	public Func<ISpeechControl> OnGetSpeechControl;

	public string HeaderLabelText { get; set; }

	public string EmailAndPasswordButtonText { get; set; }

	public string PhoneAndPinButtonText { get; set; }

	public string CancelButtonText { get; set; }

	public void ProcessOnEmailAndPasswordButtonClicked()
	{
		if (OnEmailAndPasswordButtonClicked != null)
		{
			OnEmailAndPasswordButtonClicked();
		}
	}

	public void ProcessOnPhoneAndPinButtonClicked()
	{
		if (OnPhoneAndPinButtonClicked != null)
		{
			OnPhoneAndPinButtonClicked();
		}
	}

	public void ProcessOnCancelButtonClicked()
	{
		if (OnCancelButtonClicked != null)
		{
			OnCancelButtonClicked();
		}
	}

	public ISpeechControl ProcessGetSpeechControl()
	{
		ISpeechControl result = null;
		if (OnGetSpeechControl != null)
		{
			result = OnGetSpeechControl();
		}
		return result;
	}
}
