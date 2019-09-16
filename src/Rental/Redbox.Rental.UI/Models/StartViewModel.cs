using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model;

namespace Redbox.Rental.UI.Models;

public class StartViewModel : DependencyObject
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public static readonly DependencyProperty CarouselModelProperty = DependencyProperty.Register("CarouselModel", typeof(CarouselModel), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty CarouselTestModelProperty = DependencyProperty.Register("CarouselTestModel", typeof(CarouselTestModel), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BannerImageProperty = DependencyProperty.Register("BannerImage", typeof(BitmapImage), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AllMoviesButtonTextLine1Property = DependencyProperty.Register("AllMoviesButtonTextLine1", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AllMoviesButtonTextLine2Property = DependencyProperty.Register("AllMoviesButtonTextLine2", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AllMoviesButtonTextLine2StyleProperty = DependencyProperty.Register("AllMoviesButtonTextLine2Style", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty RentOrBuyGamesButtonVisibilityProperty = DependencyProperty.Register("RentOrBuyGamesButtonVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty TVButtonTextLine1Property = DependencyProperty.Register("TVButtonTextLine1", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty TVButtonTextLine2Property = DependencyProperty.Register("TVButtonTextLine2", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty GamesButtonTextLine1Property = DependencyProperty.Register("GamesButtonTextLine1", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty GamesButtonTextLine2Property = DependencyProperty.Register("GamesButtonTextLine2", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty PickUpButtonTextProperty = DependencyProperty.Register("PickUpButtonText", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ReturnButtonTextProperty = DependencyProperty.Register("ReturnButtonText", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ToggleLanguageButtonTextProperty = DependencyProperty.Register("ToggleLanguageButtonText", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ToggleSignInButtonTextProperty = DependencyProperty.Register("ToggleSignInButtonText", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BurnInVisibilityProperty = DependencyProperty.Register("BurnInVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty BurnInViewLabelTextProperty = DependencyProperty.Register("BurnInViewLabelText", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AdaTurnedOnLabelTextProperty = DependencyProperty.Register("AdaTurnedOnLabelText", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AdaTurnedOnLabelVisibilityProperty = DependencyProperty.Register("AdaTurnedOnLabelVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty CarouselGridRowHeightProperty = DependencyProperty.Register("CarouselGridRowHeight", typeof(int), typeof(StartViewModel), new FrameworkPropertyMetadata(438));

	public static readonly DependencyProperty BannerVisibilityProperty = DependencyProperty.Register("BannerVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty LoadingTitlesImageVisibilityProperty = DependencyProperty.Register("LoadingTitlesImageVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty SignInVisibilityProperty = DependencyProperty.Register("SignInVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty SignInButtonEnabledProperty = DependencyProperty.Register("SignInButtonEnabled", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty BuyMoviesOrGamesButtonTextLine1Property = DependencyProperty.Register("BuyMoviesOrGamesButtonTextLine1", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyMoviesOrGamesButtonTextLine1StyleProperty = DependencyProperty.Register("BuyMoviesOrGamesButtonTextLine1Style", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyMoviesOrGamesButtonTextLine2Property = DependencyProperty.Register("BuyMoviesOrGamesButtonTextLine2", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyMoviesOrGamesButtonTextLine2StyleProperty = DependencyProperty.Register("BuyMoviesOrGamesButtonTextLine2Style", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyMoviesOrGamesButtonVisibilityProperty = DependencyProperty.Register("BuyMoviesOrGamesButtonVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty RentTVButtonVisibilityProperty = DependencyProperty.Register("RentTVButtonVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty BuyMoviesButtonWidthProperty = DependencyProperty.Register("BuyMoviesButtonWidth", typeof(int), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyMoviesButtonTextMarginProperty = DependencyProperty.Register("BuyMoviesButtonTextMargin", typeof(Thickness), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyGamesButtonTextLine1Property = DependencyProperty.Register("BuyGamesButtonTextLine1", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyGamesButtonTextLine1StyleProperty = DependencyProperty.Register("BuyGamesButtonTextLine1Style", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyGamesButtonTextLine2Property = DependencyProperty.Register("BuyGamesButtonTextLine2", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyGamesButtonTextLine2StyleProperty = DependencyProperty.Register("BuyGamesButtonTextLine2Style", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyGamesButtonVisibilityProperty = DependencyProperty.Register("BuyGamesButtonVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty RentGamesEnabledProperty = DependencyProperty.Register("RentGamesButtonEnabled", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty RentTVEnabledProperty = DependencyProperty.Register("RentTVButtonEnabled", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty BuyGamesEnabledProperty = DependencyProperty.Register("BuyGamesButtonEnabled", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty BuyMoviesEnabledProperty = DependencyProperty.Register("BuyMoviesButtonEnabled", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty ReturnButtonEnabledProperty = DependencyProperty.Register("ReturnButtonEnabled", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(true));

	public static readonly DependencyProperty UseClassicStartViewProperty = DependencyProperty.Register("UseClassicStartView", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty UseTestStartViewProperty = DependencyProperty.Register("UseTestStartView", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty UseTestScreenSaverProperty = DependencyProperty.Register("UseTestScreenSaver", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty IsReturnOnlyModeProperty = DependencyProperty.Register("IsReturnOnlyMode", typeof(bool), typeof(StartViewModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty AllMoviesButtonHeightProperty = DependencyProperty.Register("AllMoviesButtonHeight", typeof(int), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AllMoviesButtonCornerRadiusProperty = DependencyProperty.Register("AllMoviesButtonCornerRadius", typeof(int), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty GamesButtonHeightProperty = DependencyProperty.Register("GamesButtonHeight", typeof(int), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty GamesButtonCornerRadiusProperty = DependencyProperty.Register("GamesButtonCornerRadius", typeof(int), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	private string _abTestVersion = "A";

	public bool BurnInModeEnabled { get; set; }

	public bool UseClassicStartView
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(UseClassicStartViewProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(UseClassicStartViewProperty, value);
			});
		}
	}

	public bool UseTestStartView
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(UseTestStartViewProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(UseTestStartViewProperty, value);
			});
		}
	}

	public bool UseTestScreenSaver
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(UseTestScreenSaverProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(UseTestScreenSaverProperty, value);
			});
		}
	}

	public bool IsReturnOnlyMode
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsReturnOnlyModeProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsReturnOnlyModeProperty, value);
			});
		}
	}

	public string AllMoviesButtonTextLine1
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AllMoviesButtonTextLine1Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AllMoviesButtonTextLine1Property, value);
			});
		}
	}

	public string AllMoviesButtonTextLine2
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AllMoviesButtonTextLine2Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AllMoviesButtonTextLine2Property, value);
			});
		}
	}

	public Style AllMoviesButtonTextLine2Style
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(AllMoviesButtonTextLine2StyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AllMoviesButtonTextLine2StyleProperty, value);
			});
		}
	}

	public Visibility RentOrBuyGamesButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(RentOrBuyGamesButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RentOrBuyGamesButtonVisibilityProperty, value);
			});
		}
	}

	public string TVButtonTextLine1
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(TVButtonTextLine1Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TVButtonTextLine1Property, value);
			});
		}
	}

	public string TVButtonTextLine2
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(TVButtonTextLine2Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TVButtonTextLine2Property, value);
			});
		}
	}

	public string GamesButtonTextLine1
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(GamesButtonTextLine1Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(GamesButtonTextLine1Property, value);
			});
		}
	}

	public string GamesButtonTextLine2
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(GamesButtonTextLine2Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(GamesButtonTextLine2Property, value);
			});
		}
	}

	public string BuyMoviesOrGamesButtonTextLine1
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BuyMoviesOrGamesButtonTextLine1Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyMoviesOrGamesButtonTextLine1Property, value);
			});
		}
	}

	public Style BuyMoviesOrGamesButtonTextLine1Style
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(BuyMoviesOrGamesButtonTextLine1StyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyMoviesOrGamesButtonTextLine1StyleProperty, value);
			});
		}
	}

	public string BuyMoviesOrGamesButtonTextLine2
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BuyMoviesOrGamesButtonTextLine2Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyMoviesOrGamesButtonTextLine2Property, value);
			});
		}
	}

	public Style BuyMoviesOrGamesButtonTextLine2Style
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(BuyMoviesOrGamesButtonTextLine2StyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyMoviesOrGamesButtonTextLine2StyleProperty, value);
			});
		}
	}

	public Visibility BuyMoviesOrGamesButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BuyMoviesOrGamesButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyMoviesOrGamesButtonVisibilityProperty, value);
			});
		}
	}

	public Visibility RentTVButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(RentTVButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RentTVButtonVisibilityProperty, value);
			});
		}
	}

	public int BuyMoviesButtonWidth
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(BuyMoviesButtonWidthProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyMoviesButtonWidthProperty, value);
			});
		}
	}

	public Thickness BuyMoviesButtonTextMargin
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Thickness)GetValue(BuyMoviesButtonTextMarginProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyMoviesButtonTextMarginProperty, value);
			});
		}
	}

	public string BuyGamesButtonTextLine1
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BuyGamesButtonTextLine1Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyGamesButtonTextLine1Property, value);
			});
		}
	}

	public Style BuyGamesButtonTextLine1Style
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(BuyGamesButtonTextLine1StyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyGamesButtonTextLine1StyleProperty, value);
			});
		}
	}

	public string BuyGamesButtonTextLine2
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BuyGamesButtonTextLine2Property));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyGamesButtonTextLine2Property, value);
			});
		}
	}

	public Style BuyGamesButtonTextLine2Style
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(BuyGamesButtonTextLine2StyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyGamesButtonTextLine2StyleProperty, value);
			});
		}
	}

	public Visibility BuyGamesButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BuyGamesButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyGamesButtonVisibilityProperty, value);
			});
		}
	}

	public bool RentGamesEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(RentGamesEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RentGamesEnabledProperty, value);
			});
		}
	}

	public bool RentTVEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(RentTVEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(RentTVEnabledProperty, value);
			});
		}
	}

	public bool BuyGamesEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(BuyGamesEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyGamesEnabledProperty, value);
			});
		}
	}

	public bool BuyMoviesEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(BuyMoviesEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BuyMoviesEnabledProperty, value);
			});
		}
	}

	public bool SignInButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(SignInButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SignInButtonEnabledProperty, value);
			});
		}
	}

	public string PickUpButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PickUpButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PickUpButtonTextProperty, value);
			});
		}
	}

	public string ReturnButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ReturnButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ReturnButtonTextProperty, value);
			});
		}
	}

	public string ToggleLanguageButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ToggleLanguageButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ToggleLanguageButtonTextProperty, value);
			});
		}
	}

	public string ToggleSignInButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ToggleSignInButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ToggleSignInButtonTextProperty, value);
			});
		}
	}

	public string BurnInViewLabelText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BurnInViewLabelTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BurnInViewLabelTextProperty, value);
			});
		}
	}

	public Visibility BurnInVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BurnInVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BurnInVisibilityProperty, value);
			});
		}
	}

	public Visibility AdaTurnedOnLabelVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(AdaTurnedOnLabelVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdaTurnedOnLabelVisibilityProperty, value);
			});
		}
	}

	public string AdaTurnedOnLabelText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(AdaTurnedOnLabelTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AdaTurnedOnLabelTextProperty, value);
			});
		}
	}

	public int CarouselGridRowHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(CarouselGridRowHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CarouselGridRowHeightProperty, value);
			});
		}
	}

	public Visibility BannerVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BannerVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BannerVisibilityProperty, value);
			});
		}
	}

	public BitmapImage BannerImage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BitmapImage)GetValue(BannerImageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BannerImageProperty, value);
			});
		}
	}

	public Visibility LoadingTitlesImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(LoadingTitlesImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(LoadingTitlesImageVisibilityProperty, value);
			});
		}
	}

	public Visibility SignInVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(SignInVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SignInVisibilityProperty, value);
			});
		}
	}

	public CarouselModel CarouselModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (CarouselModel)GetValue(CarouselModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CarouselModelProperty, value);
			});
		}
	}

	public CarouselTestModel CarouselTestModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (CarouselTestModel)GetValue(CarouselTestModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CarouselTestModelProperty, value);
			});
		}
	}

	public bool ReturnButtonEnabled
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(ReturnButtonEnabledProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ReturnButtonEnabledProperty, value);
			});
		}
	}

	public int AllMoviesButtonHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(AllMoviesButtonHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AllMoviesButtonHeightProperty, value);
			});
		}
	}

	public int AllMoviesButtonCornerRadius
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(AllMoviesButtonCornerRadiusProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(AllMoviesButtonCornerRadiusProperty, value);
			});
		}
	}

	public int GamesButtonHeight
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(GamesButtonHeightProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(GamesButtonHeightProperty, value);
			});
		}
	}

	public int GamesButtonCornerRadius
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(GamesButtonCornerRadiusProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(GamesButtonCornerRadiusProperty, value);
			});
		}
	}

	public StartViewBannerType BannerType { get; set; }

	public string BannerCollectionId { get; set; }

	public MarketingScreenSaverModel ScreenSaverModel { get; set; }

	public event Action OnRentMoviesButtonClicked;

	public event Action OnRentOrBuyGameButtonClicked;

	public event Action OnRentTVButtonClicked;

	public event Action OnBuyMoviesButtonClicked;

	public event Action OnBuyGamesButtonClicked;

	public event Action OnPickupButtonClicked;

	public event Action OnReturnButtonClicked;

	public event Action OnSignInButtonClicked;

	public event Action OnHelpButtonClicked;

	public event Action OnToggleLanguage;

	public event Action OnToggleADAMode;

	public event Action OnExitBurnInView;

	public event Action OnBannerClicked;

	public void ProcessBannerClicked()
	{
		OnBannerClicked?.Invoke();
	}

	public void ProcessExitBurnInView()
	{
		OnExitBurnInView?.Invoke();
	}

	public void ProcessToggleLanguage()
	{
		OnToggleLanguage?.Invoke();
	}

	public void ProcessToggleADAMode()
	{
		OnToggleADAMode?.Invoke();
	}

	public void ProcessRentMovieButtonClick()
	{
		OnRentMoviesButtonClicked?.Invoke();
	}

	public void ProcessRentOrBuyGameButtonClick()
	{
		OnRentOrBuyGameButtonClicked?.Invoke();
	}

	public void ProcessRentTVButtonClick()
	{
		OnRentTVButtonClicked?.Invoke();
	}

	public void ProcessBuyMoviesButtonClick()
	{
		OnBuyMoviesButtonClicked?.Invoke();
	}

	public void ProcessBuyGamesButtonClick()
	{
		OnBuyGamesButtonClicked?.Invoke();
	}

	public void ProcessPickupButtonClick()
	{
		OnPickupButtonClicked?.Invoke();
	}

	public void ProcessReturnButtonClicked()
	{
		OnReturnButtonClicked?.Invoke();
	}

	public void ProcessHelpButtonClicked()
	{
		OnHelpButtonClicked?.Invoke();
	}

	public void ProcessSignInButtonClicked()
	{
		OnSignInButtonClicked?.Invoke();
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
