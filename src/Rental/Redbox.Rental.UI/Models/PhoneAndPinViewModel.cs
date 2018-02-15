using System;
using System.Windows;
using System.Windows.Media;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.Models;

public class PhoneAndPinViewModel : DependencyObject
{
	public enum DisplayType
	{
		Phone,
		Pin
	}

	public Action<DisplayType> OnDisplayClicked;

	public Action OnStartButtonClicked;

	public Action OnSignInButtonClicked;

	public Func<ISpeechControl> OnGetSpeechControl;

	private string _pinDisplayText;

	public static readonly DependencyProperty KeypadModelProperty = DependencyProperty.Register("KeypadModel", typeof(KeypadModel), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty PhoneDisplayBorderBrushProperty = DependencyProperty.Register("PhoneDisplayBorderBrush", typeof(Brush), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty PhoneDisplayBorderShadowDepthProperty = DependencyProperty.Register("PhoneDisplayBorderShadowDepth", typeof(double), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(0.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty PhoneDisplayBorderColorProperty = DependencyProperty.Register("PhoneDisplayBorderColor", typeof(Color), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(Colors.Gray)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty PhoneDisplayTextProperty = DependencyProperty.Register("PhoneDisplayText", typeof(string), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty PinDisplayBorderBrushProperty = DependencyProperty.Register("PinDisplayBorderBrush", typeof(Brush), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty PinDisplayBorderShadowDepthProperty = DependencyProperty.Register("PinDisplayBorderShadowDepth", typeof(double), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(0.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty PinDisplayBorderColorProperty = DependencyProperty.Register("PinDisplayBorderColor", typeof(Color), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(Colors.Gray)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty SecurePinDisplayTextProperty = DependencyProperty.Register("SecurePinDisplayText", typeof(string), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ErrorMessageTextProperty = DependencyProperty.Register("ErrorMessageText", typeof(string), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(null)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ADAPhoneButtonVisibilityProperty = DependencyProperty.Register("ADAPhoneButtonVisibility", typeof(Visibility), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ADAPinButtonVisibilityProperty = DependencyProperty.Register("ADAPinButtonVisibility", typeof(Visibility), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty IsSignInButtonEnabledProperty = DependencyProperty.Register("IsSignInButtonEnabled", typeof(bool), typeof(PhoneAndPinViewModel), new FrameworkPropertyMetadata(false)
	{
		AffectsRender = true
	});

	public PhoneAndPinInfo PhoneAndPinInfo { get; set; }

	public DisplayType DisplayTypeFocus { get; set; }

	public int PhoneRequiredLength { get; set; }

	public int PinRequiredLength { get; set; }

	public string HeaderLabelText { get; set; }

	public string ADAPhoneButtonText { get; set; }

	public string ADAPinButtonText { get; set; }

	public string StartButtonText { get; set; }

	public string SignInButtonText { get; set; }

	public KeypadModel KeypadModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (KeypadModel)GetValue(KeypadModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(KeypadModelProperty, value);
			});
		}
	}

	public Brush PhoneDisplayBorderBrush
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(PhoneDisplayBorderBrushProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PhoneDisplayBorderBrushProperty, value);
			});
		}
	}

	public double PhoneDisplayBorderShadowDepth
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(PhoneDisplayBorderShadowDepthProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PhoneDisplayBorderShadowDepthProperty, value);
			});
		}
	}

	public Color PhoneDisplayBorderColor
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Color)GetValue(PhoneDisplayBorderColorProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PhoneDisplayBorderColorProperty, value);
			});
		}
	}

	public string PhoneDisplayText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PhoneDisplayTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PhoneDisplayTextProperty, value);
			});
		}
	}

	public Brush PinDisplayBorderBrush
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(PinDisplayBorderBrushProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PinDisplayBorderBrushProperty, value);
			});
		}
	}

	public double PinDisplayBorderShadowDepth
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(PinDisplayBorderShadowDepthProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PinDisplayBorderShadowDepthProperty, value);
			});
		}
	}

	public Color PinDisplayBorderColor
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Color)GetValue(PinDisplayBorderColorProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PinDisplayBorderColorProperty, value);
			});
		}
	}

	public string PinDisplayText
	{
		get
		{
			return _pinDisplayText;
		}
		set
		{
			_pinDisplayText = value;
			SecurePinDisplayText = ((value == Resources.phone_and_pin_default_pin_display) ? value : new string('*', value.Length));
		}
	}

	public string SecurePinDisplayText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SecurePinDisplayTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SecurePinDisplayTextProperty, value);
			});
		}
	}

	public string TTSPhoneDisplayText
	{
		get
		{
			string text = "";
			string phoneDisplayText = PhoneDisplayText;
			for (int i = 0; i < phoneDisplayText.Length; i++)
			{
				text = text + phoneDisplayText[i] + " ";
			}
			return text;
		}
	}

	public string TTSPinDisplayText
	{
		get
		{
			string text = "";
			string pinDisplayText = PinDisplayText;
			for (int i = 0; i < pinDisplayText.Length; i++)
			{
				text = text + pinDisplayText[i] + " ";
			}
			return text;
		}
	}

	public string ErrorMessageText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ErrorMessageTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ErrorMessageTextProperty, value);
			});
		}
	}

	public Visibility ADAPhoneButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ADAPhoneButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ADAPhoneButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility ADAPinButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ADAPinButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ADAPinButtonVisibilityProperty, value);
			});
		}
	}

	public bool IsSignInButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsSignInButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsSignInButtonEnabledProperty, value);
			});
		}
	}

	public void ProcessOnDisplayClicked(DisplayType displayType)
	{
		if (OnDisplayClicked != null)
		{
			OnDisplayClicked(displayType);
		}
	}

	public void ProcessOnStartButtonClicked()
	{
		if (OnStartButtonClicked != null)
		{
			OnStartButtonClicked();
		}
	}

	public void ProcessOnSignInButtonClicked()
	{
		if (OnSignInButtonClicked != null)
		{
			OnSignInButtonClicked();
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

	public void SetDefaultDisplays()
	{
		DisplayTypeFocus = DisplayType.Phone;
		PhoneDisplayBorderBrush = new SolidColorBrush(Color.FromRgb(120, 45, 141));
		PhoneDisplayBorderShadowDepth = 2.0;
		PhoneDisplayBorderColor = Color.FromRgb(120, 45, 141);
		PinDisplayBorderBrush = new SolidColorBrush(Color.FromRgb(165, 165, 165));
		PinDisplayBorderShadowDepth = 4.0;
		PinDisplayBorderColor = Color.FromRgb(225, 225, 225);
		PhoneDisplayText = Resources.phone_and_pin_default_phone_display;
		PinDisplayText = Resources.phone_and_pin_default_pin_display;
		IsSignInButtonEnabled = false;
	}
}
