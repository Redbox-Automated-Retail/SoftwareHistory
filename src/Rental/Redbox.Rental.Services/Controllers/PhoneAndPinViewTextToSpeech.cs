using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class PhoneAndPinViewTextToSpeech
{
	private class TTSConstants
	{
		public const string CheckForPhoneAndPin = "CheckForPhoneAndPin";

		public const string HasNoDigitWithInstructions = "HasNoDigitWithInstructions";

		public const string HasDigitWithInstructions = "HasDigitWithInstructions";

		public const string HasDigitWithoutInstructions = "HasDigitWithoutInstructions";
	}

	public static ISpeechControl GetSpeechControl(Func<PhoneAndPinViewModel> getPhoneAndPinViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_phone_and_pin_view";
		AddSpeechPartCheckForPhoneAndPin(result, getPhoneAndPinViewModelFunction);
		AddSpeechPartHasNoDigitWithInstructions(result, getPhoneAndPinViewModelFunction);
		AddSpeechPartHasDigitWithInstructions(result, getPhoneAndPinViewModelFunction);
		AddSpeechPartHasDigitWithoutInstructions(result, getPhoneAndPinViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartCheckForPhoneAndPin(ISpeechControl speechControl, Func<PhoneAndPinViewModel> getPhoneAndPinViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "CheckForPhoneAndPin",
			Sequence = 0,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 0,
			EndPause = 0
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			PhoneAndPinViewModel phoneAndPinViewModel = getPhoneAndPinViewModelFunction();
			if (phoneAndPinViewModel != null)
			{
				string text = ((phoneAndPinViewModel.PhoneDisplayText != Resources.phone_and_pin_default_phone_display && phoneAndPinViewModel.PhoneDisplayText.Length > 0 && phoneAndPinViewModel.PinDisplayText != Resources.phone_and_pin_default_pin_display && phoneAndPinViewModel.PinDisplayText.Length > 0) ? "HasDigitWithInstructions" : "HasNoDigitWithInstructions");
				speechPart.RegularExpression = new RegularExpression
				{
					Success = text,
					Failure = text
				};
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartHasNoDigitWithInstructions(ISpeechControl speechControl, Func<PhoneAndPinViewModel> getPhoneAndPinViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "HasNoDigitWithInstructions",
			Sequence = 1,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 500,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			PhoneAndPinViewModel phoneAndPinViewModel = getPhoneAndPinViewModelFunction();
			if (phoneAndPinViewModel != null)
			{
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "PLUS",
					Action = delegate
					{
						if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Phone)
						{
							phoneAndPinViewModel.ProcessOnDisplayClicked(PhoneAndPinViewModel.DisplayType.Pin);
						}
						else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Pin)
						{
							phoneAndPinViewModel.ProcessOnDisplayClicked(PhoneAndPinViewModel.DisplayType.Phone);
						}
					}
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						phoneAndPinViewModel.ProcessOnStartButtonClicked();
					},
					Text = "CANCEL"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "1",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("1");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "One"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "2",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("2");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Two"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "3",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("3");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Three"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "4",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("4");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Four"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "5",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("5");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Five"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "6",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("6");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Six"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "7",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("7");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Seven"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "8",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("8");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Eight"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "9",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("9");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Nine"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "0",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("0");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Zero"
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Please enter your phone number and pin with the help of the keypad."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "The keypad is located below the screen."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "The numbers are arranged in a standard telephone keypad pattern."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Pressing plus switches between phone and pin."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Pressing cancel will go back without signing in."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartHasDigitWithInstructions(ISpeechControl speechControl, Func<PhoneAndPinViewModel> getPhoneAndPinViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "HasDigitWithInstructions",
			Sequence = 2,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 500,
			EndPause = 1000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			PhoneAndPinViewModel phoneAndPinViewModel = getPhoneAndPinViewModelFunction();
			if (phoneAndPinViewModel != null)
			{
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "PLUS",
					Action = delegate
					{
						if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Phone)
						{
							phoneAndPinViewModel.ProcessOnDisplayClicked(PhoneAndPinViewModel.DisplayType.Pin);
						}
						else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Pin)
						{
							phoneAndPinViewModel.ProcessOnDisplayClicked(PhoneAndPinViewModel.DisplayType.Phone);
						}
					}
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "ENTER",
					Action = delegate
					{
						if (phoneAndPinViewModel.IsSignInButtonEnabled)
						{
							phoneAndPinViewModel.ProcessOnSignInButtonClicked();
						}
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "ENTER"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						phoneAndPinViewModel.ProcessOnStartButtonClicked();
					},
					Text = "CANCEL"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "MINUS",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnBackButtonClicked();
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "BACK"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CLEAR",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnClearAllButtonClicked();
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "CLEAR ALL"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "1",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("1");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "One"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "2",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("2");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Two"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "3",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("3");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Three"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "4",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("4");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Four"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "5",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("5");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Five"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "6",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("6");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Six"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "7",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("7");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Seven"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "8",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("8");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Eight"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "9",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("9");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Nine"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "0",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("0");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Zero"
				});
				if (phoneAndPinViewModel.PhoneDisplayText != Resources.phone_and_pin_default_phone_display && phoneAndPinViewModel.PhoneDisplayText.Length > 0)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = $"You have entered {phoneAndPinViewModel.TTSPhoneDisplayText} as your phone number."
					});
				}
				if (phoneAndPinViewModel.PinDisplayText != Resources.phone_and_pin_default_pin_display && phoneAndPinViewModel.PinDisplayText.Length > 0)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = $"You have entered {phoneAndPinViewModel.TTSPinDisplayText} as your pin."
					});
				}
				if (phoneAndPinViewModel.IsSignInButtonEnabled)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = "If these are correct, press the enter key."
					});
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = "Pressing clear clears all the digits from the current field."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To remove a single digit, press the minus key, which acts as the backspace key."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Pressing plus switches between phone and pin."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "Pressing cancel will go back without signing in."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartHasDigitWithoutInstructions(ISpeechControl speechControl, Func<PhoneAndPinViewModel> getPhoneAndPinViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "HasDigitWithoutInstructions",
			Sequence = 3,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 1000,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			PhoneAndPinViewModel phoneAndPinViewModel = getPhoneAndPinViewModelFunction();
			if (phoneAndPinViewModel != null)
			{
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "PLUS",
					Action = delegate
					{
						if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Phone)
						{
							phoneAndPinViewModel.ProcessOnDisplayClicked(PhoneAndPinViewModel.DisplayType.Pin);
						}
						else if (phoneAndPinViewModel.DisplayTypeFocus == PhoneAndPinViewModel.DisplayType.Pin)
						{
							phoneAndPinViewModel.ProcessOnDisplayClicked(PhoneAndPinViewModel.DisplayType.Phone);
						}
					}
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "ENTER",
					Action = delegate
					{
						if (phoneAndPinViewModel.IsSignInButtonEnabled)
						{
							phoneAndPinViewModel.ProcessOnSignInButtonClicked();
						}
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithInstructions",
					Text = "ENTER"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						phoneAndPinViewModel.ProcessOnStartButtonClicked();
					},
					Text = "CANCEL"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "MINUS",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnBackButtonClicked();
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "BACK"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CLEAR",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnClearAllButtonClicked();
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "CLEAR ALL"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "1",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("1");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "One"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "2",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("2");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Two"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "3",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("3");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Three"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "4",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("4");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Four"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "5",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("5");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Five"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "6",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("6");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Six"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "7",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("7");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Seven"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "8",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("8");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Eight"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "9",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("9");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Nine"
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "0",
					Action = delegate
					{
						phoneAndPinViewModel.KeypadModel.ProcessOnNumberButtonClicked("0");
					},
					Function = "TTSSkipToSpeechPart",
					Value = "HasDigitWithoutInstructions",
					Text = "Zero"
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}
}
