using System;
using System.Windows;
using System.Windows.Controls;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models;

public class StarzOfferDetailViewModel : DependencyObject
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public static readonly DependencyProperty BackButtonVisibilityProperty = DependencyProperty.Register("BackButtonVisibility", typeof(Visibility), typeof(StarzOfferDetailViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty ScanQRCodeButtonVisibilityProperty = DependencyProperty.Register("ScanQRCodeButtonVisibility", typeof(Visibility), typeof(StarzOfferDetailViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public DynamicRoutedCommand BackButtonCommand { get; set; }

	public DynamicRoutedCommand AcceptButtonCommand { get; set; }

	public DynamicRoutedCommand ScanQRCodeButtonCommand { get; set; }

	public DynamicRoutedCommand TermsButtonCommand { get; set; }

	public UserControl OfferUserControl { get; set; }

	public IStarzSubscriptionProduct StarzSubscriptionProduct { get; set; }

	public string TitleText { get; set; }

	public string TitleClause2Text { get; set; }

	public string TitleClause3Text { get; set; }

	public string SubtitleText { get; set; }

	public string SubtitleAsteriskText { get; set; }

	public string SubtitleClause2Text { get; set; }

	public string Subtitle2Text { get; set; }

	public Visibility Subtitle2Visibility { get; set; }

	public string RegisteredTrademarkSymbol { get; set; }

	public string Bullet1Text1 { get; set; }

	public string Bullet1Text2 { get; set; }

	public string Bullet2Text { get; set; }

	public string Bullet3Text { get; set; }

	public string LeftTitleText { get; set; }

	public string RightTitleText { get; set; }

	public string LeftSubtitleText { get; set; }

	public string RightSubtitleText { get; set; }

	public string LeftSubtitle2Text { get; set; }

	public string RightSubtitle2Text { get; set; }

	public string AcceptButtonText { get; set; }

	public string AcceptButtonLegalText { get; set; }

	public string NotReadyYetText { get; set; }

	public string ScanQRCodeButtonText { get; set; }

	public string TermsButton1Text { get; set; }

	public string TermsButton2Text { get; set; }

	public Visibility TermsButton1Visibility { get; set; }

	public Visibility TermsButton2Visibility { get; set; }

	public string BackButtonText { get; set; }

	public string LegalText { get; set; }

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

	public Visibility ScanQRCodeButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ScanQRCodeButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ScanQRCodeButtonVisibilityProperty, value);
			});
		}
	}

	public ISpeechControl ProcessGetSpeechControl()
	{
		return OnGetSpeechControl?.Invoke();
	}
}
