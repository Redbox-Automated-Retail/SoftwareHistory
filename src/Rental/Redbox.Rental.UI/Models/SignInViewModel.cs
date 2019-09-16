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

	public string SignInOrLabelText { get; set; }

	public string PhoneAndPinButtonText { get; set; }

	public string SignInPointOnePartOneText { get; set; }

	public string SignInPointOnePartTwoText { get; set; }

	public string SignInPointOnePartThreeText { get; set; }

	public string SignInPointTwoText { get; set; }

	public string SignInPointThreeText { get; set; }

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
		OnPhoneAndPinButtonClicked?.Invoke();
	}

	public void ProcessOnCancelButtonClicked()
	{
		OnCancelButtonClicked?.Invoke();
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
