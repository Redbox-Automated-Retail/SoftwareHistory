using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Model.Ads;

namespace Redbox.Rental.UI.Views;

public class SwipeViewModel : DependencyObject
{
	public Action OnBackButtonClicked;

	public Action OnTimeout;

	public Func<ISpeechControl> OnGetSpeechControl;

	public static readonly DependencyProperty BackButtonVisibilityProperty = DependencyProperty.Register("BackButtonVisibility", typeof(Visibility), typeof(SwipeViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty ImageStackPanelVisibilityProperty = DependencyProperty.Register("ImageStackPanelVisibility", typeof(Visibility), typeof(SwipeViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty ApplePayImageVisibilityProperty = DependencyProperty.Register("ApplePayImageVisibility", typeof(Visibility), typeof(SwipeViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty GooglePayImageVisibilityProperty = DependencyProperty.Register("GooglePayImageVisibility", typeof(Visibility), typeof(SwipeViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public string TitleText { get; set; }

	public string MessageText { get; set; }

	public string BackButtonText { get; set; }

	public string ChipNotEnabledText { get; set; }

	public ICardReadAttempt CardReadAttempt { get; set; }

	public BitmapImage AdImage { get; set; }

	public IAdImpression AdImpression { get; set; }

	public Visibility BackButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BackButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BackButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility ImageStackPanelVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ImageStackPanelVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ImageStackPanelVisibilityProperty, value);
			});
		}
	}

	public Visibility ApplePayImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ApplePayImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ApplePayImageVisibilityProperty, value);
			});
		}
	}

	public Visibility GooglePayImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(GooglePayImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(GooglePayImageVisibilityProperty, value);
			});
		}
	}

	public void ProcessOnBackButtonClicked()
	{
		OnBackButtonClicked?.Invoke();
	}

	public void ProcessOnTimeout()
	{
		OnTimeout?.Invoke();
	}
}
