using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Rental.Model;

namespace Redbox.Rental.UI.Models;

public class StartViewModel : DependencyObject
{
	public static readonly DependencyProperty CarouselModelProperty = DependencyProperty.Register("CarouselModel", typeof(CarouselModel), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BannerImageProperty = DependencyProperty.Register("BannerImage", typeof(BitmapImage), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AllMoviesButtonTextLine1Property = DependencyProperty.Register("AllMoviesButtonTextLine1", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AllMoviesButtonTextLine2Property = DependencyProperty.Register("AllMoviesButtonTextLine2", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AllMoviesButtonTextLine2StyleProperty = DependencyProperty.Register("AllMoviesButtonTextLine2Style", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty RentOrBuyGamesButtonVisibilityProperty = DependencyProperty.Register("RentOrBuyGamesButtonVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty GamesButtonTextLine1Property = DependencyProperty.Register("GamesButtonTextLine1", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty GamesButtonTextLine1StyleProperty = DependencyProperty.Register("GamesButtonTextLine1Style", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty GamesButtonTextLine2Property = DependencyProperty.Register("GamesButtonTextLine2", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty PickUpButtonTextProperty = DependencyProperty.Register("PickUpButtonText", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ReturnButtonTextProperty = DependencyProperty.Register("ReturnButtonText", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ToggleLanguageButtonTextProperty = DependencyProperty.Register("ToggleLanguageButtonText", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BurnInVisibilityProperty = DependencyProperty.Register("BurnInVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty BurnInViewLabelTextProperty = DependencyProperty.Register("BurnInViewLabelText", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AdaTurnedOnLabelTextProperty = DependencyProperty.Register("AdaTurnedOnLabelText", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty AdaTurnedOnLabelVisibilityProperty = DependencyProperty.Register("AdaTurnedOnLabelVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty BannerVisibilityProperty = DependencyProperty.Register("BannerVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty LoadingTitlesImageVisibilityProperty = DependencyProperty.Register("LoadingTitlesImageVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public static readonly DependencyProperty BuyMoviesOrGamesButtonTextLine1Property = DependencyProperty.Register("BuyMoviesOrGamesButtonTextLine1", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyMoviesOrGamesButtonTextLine1StyleProperty = DependencyProperty.Register("BuyMoviesOrGamesButtonTextLine1Style", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyMoviesOrGamesButtonTextLine2Property = DependencyProperty.Register("BuyMoviesOrGamesButtonTextLine2", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyMoviesOrGamesButtonTextLine2StyleProperty = DependencyProperty.Register("BuyMoviesOrGamesButtonTextLine2Style", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyMoviesOrGamesButtonVisibilityProperty = DependencyProperty.Register("BuyMoviesOrGamesButtonVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty BuyMoviesButtonWidthProperty = DependencyProperty.Register("BuyMoviesButtonWidth", typeof(int), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyMoviesButtonTextMarginProperty = DependencyProperty.Register("BuyMoviesButtonTextMargin", typeof(Thickness), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyGamesButtonTextLine1Property = DependencyProperty.Register("BuyGamesButtonTextLine1", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyGamesButtonTextLine1StyleProperty = DependencyProperty.Register("BuyGamesButtonTextLine1Style", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyGamesButtonTextLine2Property = DependencyProperty.Register("BuyGamesButtonTextLine2", typeof(string), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyGamesButtonTextLine2StyleProperty = DependencyProperty.Register("BuyGamesButtonTextLine2Style", typeof(Style), typeof(StartViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BuyGamesButtonVisibilityProperty = DependencyProperty.Register("BuyGamesButtonVisibility", typeof(Visibility), typeof(StartViewModel), new FrameworkPropertyMetadata(Visibility.Visible));

	public bool BurnInModeEnabled { get; set; }

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

	public Style GamesButtonTextLine1Style
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Style)GetValue(GamesButtonTextLine1StyleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(GamesButtonTextLine1StyleProperty, value);
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

	public StartViewBannerType BannerType { get; set; }

	public string BannerCollectionId { get; set; }

	public event Action OnRentMoviesButtonClicked;

	public event Action OnRentOrBuyGameButtonClicked;

	public event Action OnBuyMoviesButtonClicked;

	public event Action OnBuyGamesButtonClicked;

	public event Action OnPickupButtonClicked;

	public event Action OnReturnButtonClicked;

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
}
