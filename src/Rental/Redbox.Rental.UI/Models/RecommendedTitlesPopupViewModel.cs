using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class RecommendedTitlesPopupViewModel : DependencyObject
{
	public Action OnCancelButtonClicked;

	public Func<ISpeechControl> OnGetSpeechControl;

	public static readonly DependencyProperty OfferTextVisibilityProperty = DependencyProperty.Register("OfferTextVisibility", typeof(Visibility), typeof(RecommendedTitlesPopupViewModel), new FrameworkPropertyMetadata(Visibility.Hidden)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty BrowseProductControlModelProperty = DependencyProperty.Register("BrowseProductControlModel", typeof(BrowseControlModel), typeof(RecommendedTitlesPopupViewModel), new FrameworkPropertyMetadata(null));

	public string HeaderText { get; set; }

	public string OfferText { get; set; }

	public string CancelButtonText { get; set; }

	public Visibility OfferTextVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(OfferTextVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(OfferTextVisibilityProperty, value);
			});
		}
	}

	public BrowseControlModel BrowseProductControlModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BrowseControlModel)GetValue(BrowseProductControlModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseProductControlModelProperty, value);
			});
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
