using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class ShoppingCartViewModel : DependencyObject
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public static readonly DependencyProperty PayButtonTextProperty = DependencyProperty.Register("PayButtonText", typeof(string), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AcceptAndPayTextProperty = DependencyProperty.Register("AcceptAndPayText", typeof(string), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty PricingModelProperty = DependencyProperty.Register("PricingModel", typeof(PricingModel), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty PromoCodeButtonEnabledProperty = DependencyProperty.Register("PromoCodeButtonEnabled", typeof(bool), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(true));

	public static readonly DependencyProperty PayButtonEnabledProperty = DependencyProperty.Register("PayButtonEnabled", typeof(bool), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty ADASpacerVisibilityProperty = DependencyProperty.Register("ADASpacerVisibility", typeof(Visibility), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty AddMovieButtonVisibilityProperty = DependencyProperty.Register("AddMovieButtonVisibility", typeof(Visibility), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty AddGameButtonVisibilityProperty = DependencyProperty.Register("AddGameButtonVisibility", typeof(Visibility), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty UpdateBagButtonVisibilityProperty = DependencyProperty.Register("UpdateBagButtonVisibility", typeof(Visibility), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty DisplayProductModelsProperty = DependencyProperty.Register("DisplayProductModels", typeof(List<DisplayCheckoutProductModel>), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty SignInButtonVisibilityProperty = DependencyProperty.Register("SignInButtonVisibility", typeof(Visibility), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty UsePointsButtonEnabledProperty = DependencyProperty.Register("UsePointsButtonEnabled", typeof(bool), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty UsePointsButtonVisibilityProperty = DependencyProperty.Register("UsePointsButtonVisibility", typeof(Visibility), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty LoyaltySystemErrorTextProperty = DependencyProperty.Register("LoyaltySystemErrorText", typeof(string), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ServiceFeeTextVisibilityProperty = DependencyProperty.Register("ServiceFeeTextVisibility", typeof(Visibility), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty MDVFootnoteVisibilityProperty = DependencyProperty.Register("MDVFootnoteVisibility", typeof(Visibility), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty PointsEstimateTextProperty = DependencyProperty.Register("PointsEstimateText", typeof(string), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty PointsEstimateVisibilityProperty = DependencyProperty.Register("PointsEstimateVisibility", typeof(Visibility), typeof(ShoppingCartViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public string LabelTop { get; set; }

	public string PriceHeaderText { get; set; }

	public string BackButtonText { get; set; }

	public string SignInButtonText { get; set; }

	public string UsePointsButtonText { get; set; }

	public string DontMissYourPerksText { get; set; }

	public string PassiveSaleNote { get; set; }

	public string MDVFootnote { get; set; }

	public string ServiceFeeText { get; set; }

	public string TermsAndPrivacyButtonText { get; set; }

	public string PrivacyText { get; set; }

	public string PromoCodeButtonText { get; set; }

	public string AddMovieButtonText { get; set; }

	public string AddGameButtonText { get; set; }

	public string UpdateBagButtonText { get; set; }

	public string PointsEstimateIntroText { get; set; }

	public string PointsEstimateText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PointsEstimateTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PointsEstimateTextProperty, value);
			});
		}
	}

	public string PointsEstimateEndText { get; set; }

	public Visibility PointsEstimateVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(PointsEstimateVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PointsEstimateVisibilityProperty, value);
			});
		}
	}

	public string LoyaltySystemErrorText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(LoyaltySystemErrorTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(LoyaltySystemErrorTextProperty, value);
			});
		}
	}

	public string PayButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PayButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PayButtonTextProperty, value);
			});
		}
	}

	public bool PayButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(PayButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PayButtonEnabledProperty, value);
			});
		}
	}

	public string AcceptAndPayText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AcceptAndPayTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AcceptAndPayTextProperty, value);
			});
		}
	}

	public PricingModel PricingModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (PricingModel)GetValue(PricingModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PricingModelProperty, value);
			});
		}
	}

	public Visibility ADASpacerVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ADASpacerVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ADASpacerVisibilityProperty, value);
			});
		}
	}

	public Visibility AddMovieButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AddMovieButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddMovieButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility AddGameButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AddGameButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AddGameButtonVisibilityProperty, value);
			});
		}
	}

	public bool PromoCodeButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(PromoCodeButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PromoCodeButtonEnabledProperty, value);
			});
		}
	}

	public List<DisplayCheckoutProductModel> DisplayProductModels
	{
		get
		{
			return base.Dispatcher.Invoke(() => (List<DisplayCheckoutProductModel>)GetValue(DisplayProductModelsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DisplayProductModelsProperty, value);
			});
		}
	}

	public Visibility UpdateBagButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(UpdateBagButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(UpdateBagButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility SignInButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(SignInButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SignInButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility UsePointsButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(UsePointsButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(UsePointsButtonVisibilityProperty, value);
			});
		}
	}

	public bool UsePointsButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(UsePointsButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(UsePointsButtonEnabledProperty, value);
			});
		}
	}

	public Visibility ServiceFeeTextVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(ServiceFeeTextVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ServiceFeeTextVisibilityProperty, value);
			});
		}
	}

	public Visibility MDVFootnoteVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(MDVFootnoteVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MDVFootnoteVisibilityProperty, value);
			});
		}
	}

	public event Action OnBackButtonClicked;

	public event Action OnTermsAndPrivacyButtonClicked;

	public event Action OnPromoCodeButtonClicked;

	public event Action OnPayButtonClicked;

	public event Action OnAddMovieButtonClicked;

	public event Action OnAddGameButtonClicked;

	public event Action OnUpdateBagButtonClicked;

	public event Action OnSignInButtonClicked;

	public event Action OnUsePointsButtonClicked;

	public event Action OnLoaded;

	public event Action<DisplayProductModel, object> OnCancelBrowseItemModel;

	public ShoppingCartViewModel()
	{
		PricingModel = new PricingModel
		{
			SubtotalLineVisibility = Visibility.Visible,
			AddedDiscsLineVisibility = Visibility.Collapsed,
			ReservedDiscsLineVisibility = Visibility.Collapsed,
			PricingLineMargin = new Thickness(0.0, 7.0, 0.0, 7.0)
		};
	}

	public void ProcessOnCancelBrowseItemModel(DisplayProductModel displayProductModel, object parameter)
	{
		if (OnCancelBrowseItemModel != null)
		{
			OnCancelBrowseItemModel(displayProductModel, parameter);
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

	public void ProcessOnSignInButtonClicked()
	{
		if (OnSignInButtonClicked != null)
		{
			OnSignInButtonClicked();
		}
	}

	public void ProcessOnBackButtonClicked()
	{
		if (OnBackButtonClicked != null)
		{
			OnBackButtonClicked();
		}
	}

	public void ProcessOnTermsAndPrivacyButtonClicked()
	{
		if (OnTermsAndPrivacyButtonClicked != null)
		{
			OnTermsAndPrivacyButtonClicked();
		}
	}

	public void ProcessOnPromoCodeButtonClicked()
	{
		if (OnPromoCodeButtonClicked != null)
		{
			OnPromoCodeButtonClicked();
		}
	}

	public void ProcessOnPayButtonClicked()
	{
		if (OnPayButtonClicked != null)
		{
			OnPayButtonClicked();
		}
	}

	public void ProcessOnAddMovieButtonClicked()
	{
		if (OnAddMovieButtonClicked != null)
		{
			OnAddMovieButtonClicked();
		}
	}

	public void ProcessOnAddGameButtonClicked()
	{
		if (OnAddGameButtonClicked != null)
		{
			OnAddGameButtonClicked();
		}
	}

	public void ProcessOnUpdateBagButtonClicked()
	{
		OnUpdateBagButtonClicked?.Invoke();
	}

	public void ProcessOnUsePointsButtonClicked()
	{
		OnUsePointsButtonClicked?.Invoke();
	}

	public void ProcessOnLoaded()
	{
		OnLoaded?.Invoke();
	}
}
