using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Marketing;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class BrowseViewController
{
	private static RentalShoppingCartChanged ShoppingCartChangeEventHandler;

	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance.BrowseState;

	private static IViewService ViewService => ServiceLocator.Instance.GetService<IViewService>();

	private static IIdleTimerService IdleTimerService => ServiceLocator.Instance.GetService<IIdleTimerService>();

	private static ITimerService TimerService => ServiceLocator.Instance.GetService<ITimerService>();

	private static IRentalShoppingCartService RentalShoppingCartService => ServiceLocator.Instance.GetService<IRentalShoppingCartService>();

	private static bool IsShoppingCartFull => RentalShoppingCartService?.CurrentRentalShoppingCart?.IsFull ?? false;

	private static bool InTitleMarketingMode => RentalShoppingCartService?.CurrentRentalShoppingCart?.InCartTitleMarketing ?? false;

	private static ISessionAnalyticsService SessionAnalyticsService => ServiceLocator.Instance.GetService<ISessionAnalyticsService>();

	private static bool IsADAMode => ApplicationState.Instance?.IsADAAccessible ?? false;

	private static bool IsForSaleButtonEnabled
	{
		get
		{
			if (BrowseState.TitleFamily == TitleFamily.Movies)
			{
				return ServiceLocator.Instance.GetService<IStoreManager>()?.CanSellUsedMovies ?? false;
			}
			if (BrowseState.TitleFamily == TitleFamily.Games)
			{
				return ServiceLocator.Instance.GetService<IStoreManager>()?.CanSellUsedGames ?? false;
			}
			return false;
		}
	}

	private static bool IsSpanish => BundleFunctions.GetCulture() == "es-MX";

	public static void CacheProductImages()
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		List<IKioskProduct> products = null;
		service?.GetProducts(BrowseViewFilter.All, TitleFamily.All, TitleType.All, BrowseSort.MerchandiseDate, null, null, null, isADAView: false, out products);
		foreach (IKioskProduct item in products)
		{
			OrchestrationService.GetProductBitmapImage(item);
		}
		service?.GetProducts(BrowseViewFilter.ComingSoon, TitleFamily.All, TitleType.All, BrowseSort.MerchandiseDate, null, null, null, isADAView: false, out products);
		foreach (IKioskProduct item2 in products)
		{
			OrchestrationService.GetProductBitmapImage(item2);
		}
	}

	public static void ConfigureBrowseView()
	{
		FrameworkElement viewUserControl = GetViewUserControl();
		if (viewUserControl != null)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			BrowseViewModel browseViewModel = new BrowseViewModel();
			ConfigureBrowseControl(browseViewModel);
			LogHelper.Instance.Log($"ConfigureBrowseView ConfigureBrowseControl stopwatch: {stopwatch.Elapsed}");
			ConfigureMiniCartControl(browseViewModel);
			ConfigureShoppingCartChangeHandler();
			ConfigureTopMenu(browseViewModel);
			ConfigureBrowseFilterBar(viewUserControl, browseViewModel);
			ConfigureCheckOutBar(browseViewModel);
			ConfigureTTS(browseViewModel);
			viewUserControl.DataContext = browseViewModel;
			CommandManager.InvalidateRequerySuggested();
			(IdleTimerService?.GetIdleTimer("idle_timer"))?.Start();
			stopwatch.Stop();
			LogHelper.Instance.Log($"ConfigureBrowseView stopwatch: {stopwatch.Elapsed}");
		}
	}

	private static FrameworkElement GetViewUserControl()
	{
		IViewFrameInstance obj = ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame();
		return ServiceLocator.Instance.GetService<IRenderingService>().GetScene("Default")?.GetWPFGridChild(obj?.Id);
	}

	private static void ConfigureShoppingCartChangeHandler()
	{
		if (ShoppingCartChangeEventHandler == null)
		{
			ShoppingCartChangeEventHandler = CurrentRentalShoppingCart_OnChange;
			RentalShoppingCartService.CurrentRentalShoppingCart.OnChange += ShoppingCartChangeEventHandler;
		}
	}

	public static void CleanupBrowseView()
	{
		IRentalShoppingCart rentalShoppingCart = RentalShoppingCartService?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null && ShoppingCartChangeEventHandler != null)
		{
			try
			{
				rentalShoppingCart.OnChange -= ShoppingCartChangeEventHandler;
			}
			catch
			{
			}
			ShoppingCartChangeEventHandler = null;
		}
	}

	private static void ConfigureTopMenu()
	{
		ConfigureTopMenu(GetBrowseViewModelFromView());
	}

	private static void ConfigureTopMenu(BrowseViewModel browseViewModel)
	{
		TopMenuModel topMenuModel = new TopMenuModel();
		if (!ApplicationState.Instance.IsADAAccessible && !InTitleMarketingMode)
		{
			CreateMenuButtons(topMenuModel);
		}
		topMenuModel.MenuBackgroundStyle = WPFFunctions.GetStyle(GetViewUserControl(), "browse_top_menu_background_collection_style");
		topMenuModel.MenuBackgroundText = null;
		if (IsADAMode || InTitleMarketingMode)
		{
			if (IsADAMode)
			{
				topMenuModel.MenuBackgroundText = ((BrowseState.TitleFamily == TitleFamily.Games) ? Resources.browse_family_games : Resources.browse_family_movies);
				topMenuModel.MenuTextStyle = WPFFunctions.GetStyle(GetViewUserControl(), "browse_top_menu_text_ada_style");
			}
			topMenuModel.MenuBackgroundStyle = WPFFunctions.GetStyle(GetViewUserControl(), "browse_top_menu_background_ada_style");
		}
		else if (BrowseState.BrowseCollectionProduct != null)
		{
			topMenuModel.MenuBackgroundText = BrowseState.BrowseCollectionProduct.ProductName;
			topMenuModel.MenuTextStyle = WPFFunctions.GetStyle(GetViewUserControl(), "browse_top_menu_text_collection_style");
		}
		topMenuModel.OnBrowseMenuButtonClicked += HandleBrowseMenuButtonClick;
		browseViewModel.BrowseTopMenuModel = topMenuModel;
	}

	private static void HandleBrowseMenuButtonClick(BrowseMenuButton browseMenuButton)
	{
		if (browseMenuButton == null)
		{
			return;
		}
		BrowseState.SelectedAlphaGroupName = null;
		bool browseCollections = BrowseState.BrowseCollections;
		BrowseState.BrowseCollections = false;
		BrowseProductFamilyMenuButton browseProductFamilyMenuButton = browseMenuButton as BrowseProductFamilyMenuButton;
		if (browseMenuButton is BrowseGenreMenuButton browseGenreMenuButton)
		{
			bool flag = false;
			if (browseCollections)
			{
				flag = true;
				BrowseState.UnwindBrowseStateSetStack();
				UpdateDisplayProductModels(GetBrowseViewModelFromView().BrowseProductControlModel.BrowseItems, isADAMiniCart: false);
				if (BrowseState.TitleFamily == TitleFamily.Games)
				{
					BrowseState.SetDefaultBrowseStateSet();
				}
			}
			BrowseState.Genre = browseGenreMenuButton.FilterValue;
			BrowseState.PageNumber = 1;
			OrchestrationService.AnalyticsAddViewEvent("filterChange", $"browse-filter-genre,{browseGenreMenuButton.Text}");
			ConfigureBrowseControl();
			if (flag)
			{
				ConfigureBrowseFilterBar();
			}
			if (IsADAMode)
			{
				ConfigureMiniCartControl();
			}
		}
		else if (browseProductFamilyMenuButton != null)
		{
			OrchestrationService.AnalyticsAddViewEvent("filterChange", $"browse-filter,{browseProductFamilyMenuButton.Text}");
			BrowseState.TitleFamily = browseProductFamilyMenuButton.FilterValue;
			BrowseState.TitleType = TitleType.All;
			BrowseState.BrowseCollectionProduct = null;
			BrowseState.BrowseCollections = false;
			BrowseState.PageNumber = 1;
			ServiceLocator.Instance.GetService<IMacroService>();
			BrowseState.Genre = null;
			if (BrowseState.TitleFamily == TitleFamily.Games)
			{
				if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Bluray || BrowseState.BrowseSelectedSort == BrowseSelectedSort.DigitalCode || BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha || BrowseState.BrowseSelectedSort == BrowseSelectedSort.Dvd || BrowseState.BrowseSelectedSort == BrowseSelectedSort._4KUHD)
				{
					BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
					BrowseState.BrowseMode = BrowseViewFilter.All;
				}
				else if (BrowseState.BrowseMode == BrowseViewFilter.ComingSoon)
				{
					BrowseState.BrowseSelectedSort = BrowseSelectedSort.ComingSoon;
				}
				else if (BrowseState.BrowseMode == BrowseViewFilter.ForSale)
				{
					if (OrchestrationService.GetCanSellUsedGames())
					{
						BrowseState.BrowseSelectedSort = BrowseSelectedSort.ForSale;
						BrowseState.BrowseMode = BrowseViewFilter.ForSale;
					}
					else
					{
						BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
						BrowseState.BrowseMode = BrowseViewFilter.All;
					}
				}
				else
				{
					BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
					BrowseState.BrowseMode = BrowseViewFilter.All;
				}
			}
			else if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Xbox360 || BrowseState.BrowseSelectedSort == BrowseSelectedSort.XboxOne || BrowseState.BrowseSelectedSort == BrowseSelectedSort.Ps2 || BrowseState.BrowseSelectedSort == BrowseSelectedSort.Ps3 || BrowseState.BrowseSelectedSort == BrowseSelectedSort.Ps4 || BrowseState.BrowseSelectedSort == BrowseSelectedSort.NintendoDS || BrowseState.BrowseSelectedSort == BrowseSelectedSort.Wii || BrowseState.BrowseSelectedSort == BrowseSelectedSort.WiiU || BrowseState.BrowseSelectedSort == BrowseSelectedSort.NintendoSwitch)
			{
				BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
			}
			else if (BrowseState.BrowseMode == BrowseViewFilter.ForSale)
			{
				if (OrchestrationService.GetCanSellUsedMovies())
				{
					BrowseState.BrowseSelectedSort = BrowseSelectedSort.ForSale;
					BrowseState.BrowseMode = BrowseViewFilter.ForSale;
				}
				else
				{
					BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
					BrowseState.BrowseMode = BrowseViewFilter.All;
				}
			}
			ConfigureBrowseView();
		}
		else if (browseMenuButton is BrowseCollectionsMenuButton browseCollectionsMenuButton)
		{
			BrowseState.PushCollectionsBrowseStateSet();
			BrowseState.CollectionsPopupShown = true;
			OrchestrationService.AnalyticsAddViewEvent("filterChange", $"browse-filter-genre,{browseCollectionsMenuButton.Text}");
			ConfigureBrowseView();
		}
		BrowseViewModel browseViewModelFromView = GetBrowseViewModelFromView();
		if (browseViewModelFromView?.BrowseTopMenuModel?.MenuButtons == null)
		{
			return;
		}
		foreach (BrowseMenuButton menuButton in browseViewModelFromView.BrowseTopMenuModel.MenuButtons)
		{
			if (menuButton is BrowseProductFamilyMenuButton)
			{
				menuButton.IsSelected = false;
				continue;
			}
			if (menuButton is BrowseCollectionsMenuButton)
			{
				menuButton.IsSelected = BrowseState.BrowseCollections;
				continue;
			}
			BrowseGenreMenuButton browseGenreMenuButton2 = menuButton as BrowseGenreMenuButton;
			menuButton.IsSelected = browseMenuButton == menuButton || (browseProductFamilyMenuButton != null && browseProductFamilyMenuButton.FilterValue == TitleFamily.Movies && browseGenreMenuButton2 != null && !browseGenreMenuButton2.FilterValue.HasValue) || (browseProductFamilyMenuButton != null && browseProductFamilyMenuButton.FilterValue == TitleFamily.Games && browseGenreMenuButton2 != null && !browseGenreMenuButton2.FilterValue.HasValue);
		}
	}

	private static BrowseViewModel GetBrowseViewModelFromView()
	{
		FrameworkElement browseViewUserControl = GetViewUserControl();
		if (browseViewUserControl != null)
		{
			return browseViewUserControl.Dispatcher.Invoke(() => browseViewUserControl?.DataContext as BrowseViewModel);
		}
		return null;
	}

	private static void CreateMenuButtons(TopMenuModel topMenuModel)
	{
		if (BrowseState.BrowseCollections || BrowseState.BrowseCollectionProduct == null)
		{
			TitleFamily titleFamily = BrowseState.TitleFamily;
			bool highlightSelectedGenre = !BrowseState.BrowseCollections;
			if (titleFamily == TitleFamily.Movies)
			{
				BrowseMenuButton browseMenuButton = null;
				browseMenuButton = ((!OrchestrationService.RentalIsCollectionsEnabled()) ? ((BrowseMenuButton)CreateGenreMenuButton(Genres.Top20Movies, highlightSelectedGenre)) : ((BrowseMenuButton)new BrowseCollectionsMenuButton
				{
					Text = Resources.Collections,
					IsSelected = BrowseState.BrowseCollections
				}));
				topMenuModel.MenuButtons.AddRange(new List<BrowseMenuButton>
				{
					CreateGenreMenuButton(null, highlightSelectedGenre),
					browseMenuButton,
					CreateGenreMenuButton(Genres.ActionAndAdventure, highlightSelectedGenre),
					CreateGenreMenuButton(Genres.Drama, highlightSelectedGenre),
					CreateGenreMenuButton(Genres.Horror, highlightSelectedGenre),
					CreateGenreMenuButton(Genres.Comedy, highlightSelectedGenre),
					CreateGenreMenuButton(Genres.FamilyMovies, highlightSelectedGenre),
					CreateProductFamilyMenuButton(TitleFamily.Games, ArrowDirection.Right)
				});
			}
			else
			{
				topMenuModel.MenuButtons.AddRange(new List<BrowseMenuButton>
				{
					CreateGenreMenuButton(null, highlightSelectedGenre),
					CreateGenreMenuButton(Genres.ActionGames, highlightSelectedGenre),
					CreateGenreMenuButton(Genres.FamilyGames, highlightSelectedGenre),
					CreateGenreMenuButton(Genres.Fighting, highlightSelectedGenre),
					CreateGenreMenuButton(Genres.MusicAndParty, highlightSelectedGenre),
					CreateGenreMenuButton(Genres.Shooter, highlightSelectedGenre),
					CreateGenreMenuButton(Genres.Sports, highlightSelectedGenre),
					CreateProductFamilyMenuButton(TitleFamily.Movies, ArrowDirection.Right)
				});
			}
		}
		else if (BrowseState.BrowseCollectionProduct != null)
		{
			topMenuModel.MenuButtons.AddRange(new List<BrowseMenuButton>
			{
				CreateProductFamilyMenuButton(TitleFamily.Movies, ArrowDirection.Left, 0),
				CreateProductFamilyMenuButton(TitleFamily.Games, ArrowDirection.Right, 880)
			});
		}
	}

	private static BrowseProductFamilyMenuButton CreateProductFamilyMenuButton(TitleFamily productFamily, ArrowDirection arrowDirection)
	{
		return CreateProductFamilyMenuButton(productFamily, arrowDirection, -1);
	}

	private static BrowseProductFamilyMenuButton CreateProductFamilyMenuButton(TitleFamily productFamily, ArrowDirection arrowDirection, int x)
	{
		string text = ((productFamily == TitleFamily.Movies) ? Resources.To_Movies : Resources.To_Games);
		BrowseProductFamilyMenuButton browseProductFamilyMenuButton = new BrowseProductFamilyMenuButton
		{
			Text = text,
			FilterValue = productFamily,
			ArrowDirection = arrowDirection,
			IsButtonEnabled = true
		};
		if (x > -1)
		{
			browseProductFamilyMenuButton.X = x;
			browseProductFamilyMenuButton.FixedPosition = true;
		}
		return browseProductFamilyMenuButton;
	}

	private static BrowseGenreMenuButton CreateGenreMenuButton(Genres? genre, bool highlightSelectedGenre)
	{
		Genres? genre2 = BrowseState.Genre;
		BrowseGenreMenuButton obj = new BrowseGenreMenuButton
		{
			FilterValue = genre,
			IsSelected = (highlightSelectedGenre && genre2 == genre)
		};
		string name = (genre.HasValue ? $"genre_{(int)genre.Value}" : ((BrowseState.TitleFamily == TitleFamily.Movies) ? "Movies_ALL" : "Games_ALL"));
		obj.Text = Resources.ResourceManager.GetString(name);
		return obj;
	}

	private static void ConfigureTTS(BrowseViewModel browseViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement viewUserControl = GetViewUserControl();
		if (service != null && service.TTSEnabled && viewUserControl != null)
		{
			browseViewModel.OnGetSpeechControl = () => BrowseViewTextToSpeech.GetSpeechControl(GetBrowseViewModelFromView);
		}
	}

	private static void ConfigureCheckOutBar(BrowseViewModel browseViewModel)
	{
		browseViewModel.OnCheckOutButtonClicked = delegate
		{
			BrowseState.UnwindBrowseStateSetStack();
			BrowseState.SetDefaultBrowseStateSet();
			ShoppingCartViewFunctions.ViewShoppingCart();
		};
		browseViewModel.OnToggleTitleFamilyButtonClicked = delegate
		{
			BrowseState.TitleFamily = ((BrowseState.TitleFamily != TitleFamily.Movies) ? TitleFamily.Movies : TitleFamily.Games);
			BrowseState.TitleType = TitleType.All;
			BrowseState.BrowseMode = BrowseViewFilter.All;
			BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
			BrowseState.Genre = null;
			BrowseState.PageNumber = 1;
			ConfigureBrowseView();
		};
		browseViewModel.OnSignInButtonClicked = delegate
		{
			SessionAnalyticsService?.AddViewEvent("buttonPress", "signin-browse");
			OrchestrationService.ViewShow("sign_in_view");
		};
		browseViewModel.OnSignOutButtonClicked = delegate
		{
			SessionAnalyticsService?.AddViewEvent("buttonPress", "signout");
			PersonalizationFunctions.ShowLogoutDialog();
		};
		browseViewModel.OnMyPerksButtonClicked = HandleMyPerksButtonClick;
		string checkOutButtonText = null;
		Style style = null;
		int num = RentalShoppingCartService?.CurrentRentalShoppingCart?.Count ?? 0;
		if (ShowNoThanks())
		{
			style = WPFFunctions.GetStyle(GetViewUserControl(), "classic_gray_button_style");
			checkOutButtonText = Resources.no_thanks_button;
		}
		else
		{
			style = WPFFunctions.GetStyle(GetViewUserControl(), "warmpurple_white_border_button_with_dropshadow_style");
			string check_out_button_text = Resources.check_out_button_text;
			if (check_out_button_text.Contains("{0}"))
			{
				checkOutButtonText = string.Format(check_out_button_text, num);
			}
		}
		browseViewModel.IsADAAccessible = ApplicationState.Instance.IsADAAccessible;
		browseViewModel.CheckOutButtonText = checkOutButtonText;
		browseViewModel.CheckOutButtonStyle = style;
		browseViewModel.SwitchProductFamilyButtonVisibility = ((!ApplicationState.Instance.IsADAAccessible) ? Visibility.Collapsed : Visibility.Visible);
		browseViewModel.SwitchProductFamilyButtonText = ((BrowseState.TitleFamily == TitleFamily.Movies) ? Resources.Games : Resources.Movies);
		browseViewModel.AddToADACartLabelText = Resources.add_to_cart_ada_label;
		browseViewModel.AddToADACartLabelVisibility = ((!IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
		browseViewModel.DontMissYourPerksText = Resources.perks_dont_miss;
		browseViewModel.SignInButtonText = Resources.sign_in;
		browseViewModel.SignInButtonVisibility = ((!PersonalizationFunctions.IsSignInEnabled) ? Visibility.Collapsed : Visibility.Visible);
		browseViewModel.SignOutButtonText = Resources.sign_out_button;
		browseViewModel.SignOutButtonVisibility = ((!PersonalizationFunctions.IsSignedIn) ? Visibility.Collapsed : Visibility.Visible);
		browseViewModel.MyPerksButtonText = Resources.my_perks;
		browseViewModel.MyPerksButtonVisibility = ((!PersonalizationFunctions.IsLoyaltySignedIn) ? Visibility.Collapsed : Visibility.Visible);
	}

	private static void HandleMyPerksButtonClick()
	{
		SessionAnalyticsService?.AddViewEvent("buttonPress", "myperks");
		PersonalizationFunctions.ShowMemberPerks();
	}

	private static bool ShowNoThanks()
	{
		bool result = false;
		if (RentalShoppingCartService != null)
		{
			IRentalShoppingCart currentRentalShoppingCart = RentalShoppingCartService.CurrentRentalShoppingCart;
			if (currentRentalShoppingCart != null)
			{
				result = currentRentalShoppingCart.InCartPromoOfferMade && !PromotionService.Instance.IsFreeInCartOfferRunning && !PromotionService.Instance.IsRentTwoInCartOfferRunning && !currentRentalShoppingCart.InCartPromoOfferCancel && !currentRentalShoppingCart.Items.Any((IRentalShoppingCartItem x) => x.Sequence == RentalShoppingCartItemSequence.post_offer);
			}
		}
		return result;
	}

	private static void ConfigureBrowseFilterBar()
	{
		BrowseViewModel browseViewModelFromView = GetBrowseViewModelFromView();
		ConfigureBrowseFilterBar(GetViewUserControl(), browseViewModelFromView);
	}

	private static void ConfigureBrowseFilterBar(FrameworkElement browseViewUserControl, BrowseViewModel browseViewModel)
	{
		FilterBarModel filterBarModel = new FilterBarModel();
		filterBarModel.OnStartOverButtonClicked += delegate
		{
			OrchestrationService.AnalyticsAddViewEvent("buttonPress", "startOver");
			ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession(null);
		};
		filterBarModel.OnBackButtonClicked += delegate
		{
			SessionAnalyticsService?.AddViewEvent("buttonPress", "back_button");
			BrowseState.PopBrowseStateSet();
			if (BrowseState.IsNonBrowseView)
			{
				BrowseState.PopBrowseStateSet();
				IViewService service = ServiceLocator.Instance.GetService<IViewService>();
				service.GoBackSafe("back_buton_press", popOnly: false);
				service.Show();
			}
			else
			{
				if (!BrowseState.BrowseCollections)
				{
					UpdateDisplayProductModels(browseViewModel.BrowseProductControlModel.BrowseItems, isADAMiniCart: false);
				}
				ConfigureBrowseControl();
				ConfigureTopMenu(browseViewModel);
				ConfigureBrowseFilterBar();
				CommandManager.InvalidateRequerySuggested();
			}
		};
		filterBarModel.OnMoreCollectionsButtonClicked += delegate
		{
			BrowseState.PushCollectionsBrowseStateSet();
			SessionAnalyticsService?.AddViewEvent("filterChange", "browse-filter-genre,MORE_COLLECTIONS");
			ConfigureBrowseControl();
			ConfigureTopMenu();
			ConfigureBrowseFilterBar();
			CommandManager.InvalidateRequerySuggested();
		};
		filterBarModel.OnHomeButtonClicked += delegate
		{
			OrchestrationService.AnalyticsAddViewEvent("buttonPress", "Home");
			OrchestrationService.UnwindToSafe("start_view", discard: true, "Home button pressed");
		};
		filterBarModel.OnFormatFilterButtonClicked += delegate
		{
			OrchestrationService.AnalyticsAddViewEvent("buttonPress", "format");
			OrchestrationService.ViewShow("format_filter_popup_view");
		};
		filterBarModel.HomeButtonText = Resources.home_button;
		filterBarModel.HomeButtonVisibility = ((!PersonalizationFunctions.IsSignedIn || BrowseState.BrowseCollections || BrowseState.BrowseCollectionProduct != null || InTitleMarketingMode) ? Visibility.Collapsed : Visibility.Visible);
		filterBarModel.StartOverButtonVisibility = ((PersonalizationFunctions.IsSignedIn || BrowseState.BrowseCollections || BrowseState.BrowseCollectionProduct != null || InTitleMarketingMode) ? Visibility.Collapsed : Visibility.Visible);
		filterBarModel.StartOverButtonText = Resources.start_over_button;
		filterBarModel.StartOverButtonStyle = WPFFunctions.GetStyle(browseViewUserControl, OrchestrationService.IsCultureSpanish() ? "spanish_start_over_button_style" : "english_start_over_button_style");
		filterBarModel.BackButtonVisibility = ((!BrowseState.BrowseCollections && BrowseState.BrowseCollectionProduct == null) ? Visibility.Collapsed : Visibility.Visible);
		filterBarModel.BackButtonText = Resources.back_button;
		filterBarModel.MoreCollectionsButtonVisibility = ((BrowseState.BrowseCollectionProduct == null) ? Visibility.Collapsed : Visibility.Visible);
		filterBarModel.MoreCollectionsButtonText = Resources.more_collections;
		filterBarModel.FilterBarTextVisibility = ((BrowseState.BrowseCollectionProduct == null) ? Visibility.Collapsed : Visibility.Visible);
		filterBarModel.FilterBarText = BrowseState.BrowseCollectionProduct?.LegalInfo;
		filterBarModel.FormatFilterButtonVisibility = ((!ShouldShowFormatFilterButton()) ? Visibility.Collapsed : Visibility.Visible);
		filterBarModel.FormatFilterButtonStrings = new Dictionary<TitleType, string>();
		filterBarModel.FormatFilterButtonStrings.Add(TitleType.All, Resources.format_button);
		filterBarModel.FormatFilterButtonStrings.Add(TitleType.DVD, Resources.dvd);
		filterBarModel.FormatFilterButtonStrings.Add(TitleType.Bluray, Resources.blu_ray);
		filterBarModel.FormatFilterButtonStrings.Add(TitleType.DigitalCode, Resources.digital_code);
		filterBarModel.FormatFilterButtonStrings.Add(TitleType._4KUHD, Resources.four_k_uhd);
		filterBarModel.FormatFilterButtonSelectedStyle = WPFFunctions.GetStyle(browseViewUserControl, "classic_purple_radio_button_style");
		filterBarModel.FormatFilterButtonUnselectedStyle = WPFFunctions.GetStyle(browseViewUserControl, "classic_rubine_radio_button_style");
		filterBarModel.SelectedFormat = BrowseState.TitleType;
		IConfiguration instance = Redbox.Rental.Services.Configuration.Configuration.Instance;
		List<BrowseFilter> filters = null;
		if (!BrowseState.BrowseCollections && BrowseState.BrowseCollectionProduct == null && !InTitleMarketingMode)
		{
			filters = CreateBrowseFilters(instance);
		}
		filterBarModel.Filters = filters;
		filterBarModel.FilterMargin = new Thickness((IsADAMode || BrowseState.TitleFamily == TitleFamily.Games) ? 8 : 16, 0.0, 0.0, 0.0);
		filterBarModel.OnBrowseFilterButtonClicked += delegate(BrowseFilter browseFilter)
		{
			foreach (BrowseFilter filter in filterBarModel.Filters)
			{
				filter.IsSelected = browseFilter == filter;
			}
			HandleFilterButtonClick(browseFilter.ButtonType);
			if (BrowseState.TitleType == TitleType.All)
			{
				filterBarModel.SelectedFormat = TitleType.All;
			}
		};
		browseViewModel.BrowseFilterBarModel = filterBarModel;
	}

	private static bool ShouldShowFormatFilterButton()
	{
		if (BrowseState.TitleFamily == TitleFamily.Movies && !BrowseState.BrowseCollections && BrowseState.BrowseCollectionProduct == null)
		{
			return !InTitleMarketingMode;
		}
		return false;
	}

	internal static void HandleFilterButtonClick(BrowseFilterType browseFilterType)
	{
		if (IsADAMode)
		{
			BrowseState.Genre = null;
		}
		switch (browseFilterType)
		{
		case BrowseFilterType.AllNewest:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.Newest, TitleType.All, "browse-filter-family,Newest", 1);
			break;
		case BrowseFilterType.BluRay:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.Bluray, TitleType.Bluray, "browse-filter-family,Blu-ray", 1);
			break;
		case BrowseFilterType.Dvd:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.Dvd, TitleType.DVD, "browse-filter-family,DVD", 1);
			break;
		case BrowseFilterType._4KUHD:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort._4KUHD, TitleType._4KUHD, "browse-filter-family,4K-UHD", 1);
			break;
		case BrowseFilterType.ForSale:
			SetBrowseFilters(BrowseViewFilter.ForSale, BrowseSelectedSort.ForSale, TitleType.All, "browse-filter-family,for-sale", 1);
			break;
		case BrowseFilterType.ComingSoon:
			SetBrowseFilters(BrowseViewFilter.ComingSoon, BrowseSelectedSort.ComingSoon, TitleType.All, "browse-filter-family,Coming-soon", 1);
			break;
		case BrowseFilterType.AtoZ:
			BrowseState.SelectedAlphaGroupName = null;
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.Alpha, TitleType.All, "browse-filter-family,alpha", 1);
			break;
		case BrowseFilterType.DigitalCode:
			SetBrowseFilters(BrowseViewFilter.ForSale, BrowseSelectedSort.DigitalCode, TitleType.DigitalCode, "browse-filter-family,Digital Code", 1);
			break;
		case BrowseFilterType.XboxOne:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.XboxOne, TitleType.XboxOne, "browse-filter-family,Xbox One", 1);
			break;
		case BrowseFilterType.Xbox360:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.Xbox360, TitleType.Xbox360, "browse-filter-family,Xbox 360", 1);
			break;
		case BrowseFilterType.PS3:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.Ps3, TitleType.PS3, "browse-filter-family,PS3", 1);
			break;
		case BrowseFilterType.PS4:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.Ps4, TitleType.PS4, "browse-filter-family,PS4", 1);
			break;
		case BrowseFilterType.Wii:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.Wii, TitleType.Wii, "browse-filter-family,Wii", 1);
			break;
		case BrowseFilterType.WiiU:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.WiiU, TitleType.WiiU, "browse-filter-family,Wii U", 1);
			break;
		case BrowseFilterType.NintendoSwitch:
			SetBrowseFilters(BrowseViewFilter.All, BrowseSelectedSort.NintendoSwitch, TitleType.NintendoSwitch, "browse-filter-family,Nintendo Switch", 1);
			break;
		case BrowseFilterType.Top20:
			BrowseState.Genre = Genres.Top20Movies;
			SetBrowseFilters(GetGenreBrowseMode(), BrowseSelectedSort.Top20, BrowseState.TitleType, "browse-filter-family,Top 20", 1);
			break;
		case BrowseFilterType.Action:
			BrowseState.Genre = Genres.ActionAndAdventure;
			SetBrowseFilters(GetGenreBrowseMode(), BrowseSelectedSort.Action, BrowseState.TitleType, "browse-filter-family,Action", 1);
			break;
		case BrowseFilterType.Drama:
			BrowseState.Genre = Genres.Drama;
			SetBrowseFilters(GetGenreBrowseMode(), BrowseSelectedSort.Drama, BrowseState.TitleType, "browse-filter-family,Drama", 1);
			break;
		case BrowseFilterType.Horror:
			BrowseState.Genre = Genres.Horror;
			SetBrowseFilters(GetGenreBrowseMode(), BrowseSelectedSort.Horror, BrowseState.TitleType, "browse-filter-family,Horror", 1);
			break;
		case BrowseFilterType.Comedy:
			BrowseState.Genre = Genres.Comedy;
			SetBrowseFilters(GetGenreBrowseMode(), BrowseSelectedSort.Comedy, BrowseState.TitleType, "browse-filter-family,Comedy", 1);
			break;
		case BrowseFilterType.Family:
			BrowseState.Genre = Genres.FamilyMovies;
			SetBrowseFilters(GetGenreBrowseMode(), BrowseSelectedSort.Family, BrowseState.TitleType, "browse-filter-family,Family", 1);
			break;
		}
	}

	private static BrowseViewFilter GetGenreBrowseMode()
	{
		if (BrowseState.TitleType != TitleType.DigitalCode)
		{
			return BrowseViewFilter.All;
		}
		return BrowseViewFilter.ForSale;
	}

	internal static void SetBrowseFilters(BrowseViewFilter browseMode, BrowseSelectedSort browseSelectedSort, TitleType titleType, string analyticAttributeValue, int? pageNumber)
	{
		BrowseState.BrowseMode = browseMode;
		BrowseState.BrowseSelectedSort = browseSelectedSort;
		BrowseState.TitleType = titleType;
		if (pageNumber.HasValue)
		{
			BrowseState.PageNumber = pageNumber.Value;
		}
		if (!string.IsNullOrEmpty(analyticAttributeValue))
		{
			ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
			IShoppingSessionService service2 = ServiceLocator.Instance.GetService<IShoppingSessionService>();
			if (service != null && service2 != null)
			{
				string bufferedSessionID = service2.GetCurrentSession().Id.ToString();
				service.AddNode("viewEvents", "viewEvent", bufferedSessionID);
				service.AddAttribute("viewEvent", "subItemType", "filterChange", bufferedSessionID);
				service.AddAttribute("viewEvent", "subItemValue", analyticAttributeValue, bufferedSessionID);
			}
		}
		ConfigureBrowseControl();
		ConfigureMiniCartControl();
	}

	private static void ConfigureBrowseControl()
	{
		ConfigureBrowseControl(GetBrowseViewModelFromView());
	}

	private static void ConfigureBrowseControl(BrowseViewModel browseViewModel)
	{
		if (browseViewModel == null)
		{
			return;
		}
		browseViewModel.BackgroundImageVisibility = Visibility.Collapsed;
		BrowseControlModel browseControlModel = new BrowseControlModel();
		browseControlModel.SetPageNumber(Math.Max(BrowseState.PageNumber, 1), PageChangeSource.None);
		IBrowseState browseState = BrowseState;
		browseControlModel.NoResultsText = ((browseState != null && browseState.TitleFamily == TitleFamily.Movies) ? Resources.filter_movies_empty : Resources.filter_games_empty);
		browseControlModel.ShowNoResultsMessage = true;
		browseControlModel.BorderShadowVisibility = Visibility.Collapsed;
		DisplayGroupSelectionModel displayGroupSelectionModel = new DisplayGroupSelectionModel();
		displayGroupSelectionModel.OnGroupSelectionItemClicked = BrowseViewModel_OnGroupSelectionItemClicked;
		browseControlModel.ShowBorderItems = Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectedSort != BrowseSelectedSort.Alpha;
		browseControlModel.OnNotifyBrowseItemsShown += BrowseProductControl_OnNotifyBrowseItemsShown;
		browseControlModel.OnBrowseItemModelSelected += BrowseProductControl_OnDisplayProductModelSelected;
		browseControlModel.OnAddBrowseItemModel += BrowseProductControl_OnAddDisplayProductModel;
		browseControlModel.OnPageChanged += delegate(int prevPageNumber, int newPageNumber, PageChangeSource pageChangeSource)
		{
			SessionAnalyticsService?.AddViewEvent("pageChange", (newPageNumber < prevPageNumber) ? "browse-page-back" : "browse-page-next");
			bool num2 = newPageNumber != prevPageNumber;
			BrowseState.PageNumber = newPageNumber;
			if (IsADAMode)
			{
				browseViewModel.MiniCartControlModel.SetPageNumber(newPageNumber, PageChangeSource.None);
			}
			if (num2 && pageChangeSource == PageChangeSource.PaddleButton)
			{
				IncrementPaddleClickCount();
			}
		};
		if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha)
		{
			browseControlModel.LayoutDirection = BrowseLayoutDirection.TopToBottomThenLeftToRight;
			browseControlModel.BrowseItemMargin = new Thickness(12.0, 1.0, 10.0, 0.0);
			browseControlModel.ShowBorderItems = false;
			displayGroupSelectionModel.BrowseGroupSelectionVisibility = Visibility.Visible;
			browseControlModel.PaddleOverlapWidth = 0.0;
			browseControlModel.PageNumberVisibility = Visibility.Collapsed;
			if (ApplicationState.Instance.IsADAAccessible)
			{
				browseControlModel.RowCount = 5;
				browseControlModel.ColumnCount = 2;
				browseControlModel.Margin = new Thickness(0.0, 0.0, 0.0, 40.0);
				displayGroupSelectionModel.BrowseGroupSelectionMargin = new Thickness(35.0, 522.0, 35.0, 0.0);
			}
			else
			{
				browseControlModel.RowCount = 6;
				browseControlModel.ColumnCount = 3;
				browseControlModel.Margin = new Thickness(0.0, 55.0, 0.0, 0.0);
				displayGroupSelectionModel.BrowseGroupSelectionMargin = new Thickness(38.0, 84.0, 38.0, 0.0);
			}
		}
		else
		{
			displayGroupSelectionModel.BrowseGroupSelectionVisibility = Visibility.Collapsed;
			if (BrowseState.BrowseCollections)
			{
				browseControlModel.RowCount = 2;
				browseControlModel.ColumnCount = 3;
				browseControlModel.BrowseItemMargin = new Thickness(8.0, 20.0, 8.0, 3.0);
			}
			else
			{
				browseControlModel.PaddleOverlapWidth = 24.0;
				browseControlModel.BrowseItemMargin = new Thickness(6.0, 36.0, 6.0, 0.0);
				browseControlModel.RowCount = 2;
				browseControlModel.ColumnCount = 5;
			}
		}
		if (BrowseState.BrowseCollections)
		{
			browseControlModel.BrowseItems = CreateBrowseItemsFromCollections();
		}
		else if (BrowseState.BrowseCollectionProduct != null)
		{
			CollectionProduct collectionProduct = BrowseState.BrowseCollectionProduct as CollectionProduct;
			List<IKioskProduct> list = null;
			if (collectionProduct != null)
			{
				IRentalShoppingCart rentalShoppingCart = RentalShoppingCartService?.CurrentRentalShoppingCart;
				List<long> include = null;
				List<long> exclude = null;
				if (rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel)
				{
					IPromotion promotion = PromotionService.Instance.GetPromotion();
					if (promotion?.CampaignTitles != null && promotion?.CampaignTitles?.Titles != null)
					{
						List<long> list2 = new List<long>();
						list2.AddRange(promotion.CampaignTitles.Titles);
						if (promotion.CampaignTitles.Include)
						{
							include = list2;
						}
						else
						{
							exclude = list2;
						}
					}
				}
				list = collectionProduct.GetInCartAvailableKioskProducts(TitleFamily.All, TitleType.All, include, exclude);
			}
			List<IKioskProduct> kioskProducts = list;
			IBrowseState browseState2 = BrowseState;
			browseControlModel.BrowseItems = CreateBrowseItemsFromKioskProducts(kioskProducts, browseState2 != null && browseState2.BrowseSelectedSort == BrowseSelectedSort.Alpha, isADAMiniCart: false, browseControlModel.VisibleItemCount, browseControlModel.PageNumber);
		}
		else
		{
			ApplicationState.Instance.BrowseProducts = GetBrowseKioskProducts();
			List<IKioskProduct> browseProducts = ApplicationState.Instance.BrowseProducts;
			IBrowseState browseState3 = BrowseState;
			browseControlModel.BrowseItems = CreateBrowseItemsFromKioskProducts(browseProducts, browseState3 != null && browseState3.BrowseSelectedSort == BrowseSelectedSort.Alpha, isADAMiniCart: false, browseControlModel.VisibleItemCount, browseControlModel.PageNumber);
		}
		if (BrowseState != null)
		{
			BrowseState.BrowseItemModels = browseControlModel.BrowseItems;
		}
		for (int num = 0; num < browseControlModel.RowCount * browseControlModel.ColumnCount * 3; num++)
		{
			if (BrowseState.BrowseCollections)
			{
				browseControlModel.BrowseControls.Add(new DisplayCollectionUserControl
				{
					Name = $"product_{num + 1}"
				});
				continue;
			}
			if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha)
			{
				browseControlModel.BrowseControls.Add(new DisplayProductAToZUserControl
				{
					Name = $"product_{num + 1}"
				});
				continue;
			}
			browseControlModel.BrowseControls.Add(new DisplayProductBrowseUserControl
			{
				Name = $"product_{num + 1}",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			});
		}
		if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha)
		{
			displayGroupSelectionModel.GroupSelectionItems = CreateAToZGroups(browseControlModel.BrowseItems);
		}
		if (browseControlModel.BrowseItems != null && browseControlModel.BrowseItems.Count == 0)
		{
			browseControlModel.PageNumberVisibility = Visibility.Collapsed;
		}
		browseViewModel.BrowseGroupSelection = displayGroupSelectionModel;
		browseViewModel.BrowseProductControlModel = browseControlModel;
	}

	private static void BrowseProductControl_OnNotifyBrowseItemsShown(List<IBrowseItemModel> browseItemModels, object parameter)
	{
		List<IKioskProduct> list = new List<IKioskProduct>();
		List<IBrowseItemModel> list2 = new List<IBrowseItemModel>();
		if (browseItemModels != null)
		{
			foreach (IBrowseItemModel browseItemModel in browseItemModels)
			{
				if (browseItemModel?.Data is IKioskProduct item)
				{
					list.Add(item);
				}
				if (!browseItemModel.IsBorderItem)
				{
					list2.Add(browseItemModel);
				}
			}
		}
		if (!AreProductListsIdentical(list, BrowseState.BrowseProductsShown))
		{
			if (list.Count > 0)
			{
				SessionAnalyticsService?.AddViewEvent("productsShown", list.Count.ToString());
			}
			BrowseState.BrowseProductsShown = list;
			BrowseState.TTSBrowseItemModelsShown = list2;
		}
	}

	private static bool AreProductListsIdentical(List<IKioskProduct> shownKioskProducts, List<IKioskProduct> browseProductsShown)
	{
		bool flag = shownKioskProducts?.Count == browseProductsShown?.Count;
		if (flag)
		{
			for (int i = 0; i < shownKioskProducts.Count; i++)
			{
				flag = shownKioskProducts[i] is ITitleProduct && browseProductsShown[i] is ITitleProduct && (shownKioskProducts[i] as ITitleProduct)?.ProductId == (browseProductsShown[i] as ITitleProduct).ProductId;
				if (!flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	private static void IncrementPaddleClickCount()
	{
		IMarketingConfigurationManager service = ServiceLocator.Instance.GetService<IMarketingConfigurationManager>();
		if (service != null && service.CollectionsPopUpPaddleClicks > 0)
		{
			BrowseState.PaddleClickCount++;
			if (BrowseState.PaddleClickCount >= service.CollectionsPopUpPaddleClicks)
			{
				BrowseState.PaddleClickCount = 0;
				CollectionMessageViewController.CheckCollectionsPopup("interaction");
			}
		}
	}

	private static void BrowseViewModel_OnGroupSelectionItemClicked(DisplayGroupSelectionItemModel displayGroupSelectionItemModel)
	{
		BrowseViewModel browseViewModelFromView = GetBrowseViewModelFromView();
		if (browseViewModelFromView?.BrowseGroupSelection?.GroupSelectionItems == null || displayGroupSelectionItemModel?.Name == null)
		{
			return;
		}
		BrowseState.SelectedAlphaGroupName = displayGroupSelectionItemModel.Name;
		foreach (DisplayGroupSelectionItemModel groupSelectionItem in browseViewModelFromView.BrowseGroupSelection.GroupSelectionItems)
		{
			groupSelectionItem.IsSelected = groupSelectionItem.IsEnabled && groupSelectionItem.Name == displayGroupSelectionItemModel?.Name;
		}
		foreach (IBrowseItemModel item2 in browseViewModelFromView.BrowseProductControlModel.BrowseItems.Where((IBrowseItemModel x) => x is DisplayGroupModel))
		{
			if (item2 is DisplayGroupModel displayGroupModel)
			{
				displayGroupModel.IsSelected = displayGroupModel.Name == displayGroupSelectionItemModel.Name;
			}
		}
		DisplayGroupModel item = browseViewModelFromView?.BrowseProductControlModel?.BrowseItems.FirstOrDefault((IBrowseItemModel x) => x is DisplayGroupModel && (x as DisplayGroupModel).Name == displayGroupSelectionItemModel.Name) as DisplayGroupModel;
		int num = browseViewModelFromView?.BrowseProductControlModel?.BrowseItems?.IndexOf(item) ?? (-1);
		if (num > -1)
		{
			browseViewModelFromView.BrowseProductControlModel.SetPageNumber(browseViewModelFromView?.BrowseProductControlModel.GetPageOfBrowseItemIndex(num) ?? 1, PageChangeSource.GroupSelection);
		}
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service == null || !service.TTSEnabled)
		{
			return;
		}
		IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
		if (service2 != null)
		{
			string value = null;
			DisplayGroupSelectionItemModel displayGroupSelectionItemModel2 = displayGroupSelectionItemModel;
			if (displayGroupSelectionItemModel2 == null || displayGroupSelectionItemModel2.IsEnabled)
			{
				value = $"There are no titles starting with the letter {displayGroupSelectionItemModel?.Name}.";
			}
			service2["TTS:SelectedAlphaGroupEmptyMessage"] = value;
		}
	}

	private static List<DisplayGroupSelectionItemModel> CreateAToZGroups(List<IBrowseItemModel> browseItems)
	{
		List<DisplayGroupSelectionItemModel> list = new List<DisplayGroupSelectionItemModel>();
		List<string> list2 = new List<string> { "#" };
		for (int i = 65; i < 91; i++)
		{
			list2.Add(((char)i).ToString());
		}
		LinearGradientBrush resource = GetResource<LinearGradientBrush>(GetViewUserControl(), "group_selection_item_unselected_background");
		LinearGradientBrush resource2 = GetResource<LinearGradientBrush>(GetViewUserControl(), "group_selection_item_selected_background");
		foreach (string eachGroupName in list2)
		{
			bool isEnabled = browseItems.Any((IBrowseItemModel x) => x is DisplayGroupModel && ((DisplayGroupModel)x).Name == eachGroupName);
			list.Add(new DisplayGroupSelectionItemModel
			{
				Name = eachGroupName,
				EnableTextStyle = WPFFunctions.GetStyle(GetViewUserControl(), "group_selection_item_enabled"),
				DisabledTextStyle = WPFFunctions.GetStyle(GetViewUserControl(), "group_selection_item_disabled"),
				SelectedBackgroundBrush = resource2,
				UnselectedBackgroundBrusch = resource,
				IsEnabled = isEnabled,
				IsSelected = (eachGroupName == BrowseState.SelectedAlphaGroupName)
			});
		}
		return list;
	}

	private static void BrowseProductControl_OnDisplayProductModelSelected(IBrowseItemModel browseItemModel, object parameter)
	{
		if (!(browseItemModel.Data is IKioskProduct kioskProduct))
		{
			return;
		}
		BrowseState.BrowseSelectionSelectedProduct = kioskProduct;
		switch (kioskProduct.KioskProductType)
		{
		case KioskProductType.Title:
		case KioskProductType.MDV:
		case KioskProductType.TitleRollup:
			if (kioskProduct is ITitleProduct)
			{
				IViewService service = ServiceLocator.Instance.GetService<IViewService>();
				if (BrowseState != null && service != null)
				{
					BrowseState.ProductDetailShownFromView = service.Peek();
				}
				PrepareToLeaveBrowseView();
				ServiceLocator.Instance.GetService<ISessionAnalyticsService>()?.AddViewEvent("context", ("productDisplayed," + ((ITitleProduct)kioskProduct)?.ProductId) ?? string.Empty);
				OrchestrationService.ViewShow("product_detail_view");
			}
			break;
		case KioskProductType.Collection:
			ShowCollection(kioskProduct as CollectionProduct);
			break;
		case KioskProductType.None:
			break;
		}
	}

	private static void ShowCollection(CollectionProduct collectionProduct)
	{
		if (collectionProduct != null)
		{
			BrowseState.PushCollectionBrowseStateSet(collectionProduct);
			BrowseState.CollectionsPopupShown = true;
			SessionAnalyticsService?.AddViewEvent("filterChange", $"browse-filter-genre,{collectionProduct.ProductName}");
			ConfigureBrowseControl();
			ConfigureTopMenu();
			ConfigureBrowseFilterBar();
		}
	}

	private static void PrepareToLeaveBrowseView()
	{
		GetBrowseViewModelFromView();
	}

	private static void BrowseProductControl_OnAddDisplayProductModel(IBrowseItemModel browseItemModel, object parameter)
	{
		if (parameter?.ToString() == BrowseFunctions.Constants.ComingSoon)
		{
			ProcessComingSoonOptin(browseItemModel);
		}
		else if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha && !IsADAMode)
		{
			BrowseProductControl_OnDisplayProductModelSelected(browseItemModel, parameter);
		}
		else
		{
			if (!(browseItemModel.Data is IKioskProduct kioskProduct) || GetBrowseViewModelFromView() == null)
			{
				return;
			}
			switch (kioskProduct.KioskProductType)
			{
			case KioskProductType.Title:
			case KioskProductType.MDV:
			{
				ITitleProduct titleProduct = kioskProduct as ITitleProduct;
				if ((titleProduct?.ProductId ?? 0) == 1139)
				{
					BrowseFunctions.AddProductToCart(titleProduct, isPurchase: true);
					break;
				}
				IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
				bool flag = rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel;
				if (titleProduct.HasMultiNightPrice && !BrowseFunctions.IsPurchase && !flag)
				{
					OrchestrationService.ViewShow("multi_night_pricing_popup_view", titleProduct);
				}
				else
				{
					BrowseFunctions.AddProductToCart(titleProduct, BrowseFunctions.IsPurchase);
				}
				break;
			}
			case KioskProductType.TitleRollup:
			{
				SessionAnalyticsService?.AddViewEvent("buttonPress", "shopping-cart-modal-add-item");
				TitleRollupProduct titleRollupProduct = kioskProduct as TitleRollupProduct;
				if ((titleRollupProduct?.ProductId ?? 0) == 1139)
				{
					BrowseFunctions.AddProductToCart(titleRollupProduct, isPurchase: true);
				}
				else
				{
					OrchestrationService.ViewShow(BrowseFunctions.IsPurchase ? "purchase_popup_view" : "rental_popup_view", new RentalFormatSelectionViewParameter(titleRollupProduct, BrowseFunctions.IsPurchase));
				}
				break;
			}
			case KioskProductType.Collection:
				OrchestrationService.StarteUserInteraction();
				SessionAnalyticsService?.AddViewEvent("buttonPress", "add_to_cart");
				break;
			case KioskProductType.None:
				break;
			}
		}
	}

	private static void ProcessComingSoonOptin(IBrowseItemModel browseItemModel)
	{
		IKioskProduct kioskProduct = browseItemModel.Data as IKioskProduct;
		ITitleProduct titleProduct = kioskProduct as ITitleProduct;
		IComingSoonOptinInfo comingSoonOptInInfo = RentalShoppingCartService?.CurrentRentalShoppingCart?.ComingSoonOptInInfo;
		if (titleProduct == null || comingSoonOptInInfo == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(comingSoonOptInInfo.EmailAddress))
		{
			if (comingSoonOptInInfo.OptInItems?.FirstOrDefault((IOptInItem x) => x.ItemId == titleProduct.ProductId) == null)
			{
				comingSoonOptInInfo.AddProduct(titleProduct.ProductId);
			}
			else
			{
				comingSoonOptInInfo.RemoveProduct(titleProduct.ProductId);
			}
			if (browseItemModel is DisplayProductModel browseItemModel2)
			{
				ConfigureDisplayProductModel(kioskProduct, browseItemModel2, isADAMiniCart: false, GetDisplayProductModelConstants());
			}
			return;
		}
		OrchestrationService.ViewShow("keyboard_email_coming_soon_view", new KeyboardInfo
		{
			MaxLength = 64,
			KeyboardText = comingSoonOptInInfo.EmailAddress,
			CancelAction = delegate
			{
				ViewService?.GoBack("go back", popOnly: false);
			},
			IsValidEntry = ValidationServices.IsValidEmailAddress,
			ContinueAction = delegate(string keyboardText)
			{
				if (comingSoonOptInInfo != null && ViewService != null)
				{
					comingSoonOptInInfo.EmailAddress = keyboardText;
					comingSoonOptInInfo.AddProduct(titleProduct.ProductId);
					ViewService.UnwindTo("browse_view", discard: true, "KeyboardView: KeyboardMode.EMAIL_COMING_SOON  - NEXT button");
				}
			}
		});
	}

	private static void ConfigureMiniCartControl()
	{
		ConfigureMiniCartControl(GetBrowseViewModelFromView());
	}

	private static void ConfigureMiniCartControl(BrowseViewModel browseViewModel)
	{
		if (browseViewModel != null)
		{
			BrowseControlModel browseControlModel = new BrowseControlModel();
			browseControlModel.OnAddBrowseItemModel += BrowseProductControl_OnAddDisplayProductModel;
			browseControlModel.BackgroundColor = Colors.Transparent;
			browseControlModel.ShowBorderItems = false;
			browseControlModel.PageNumberVisibility = Visibility.Collapsed;
			browseControlModel.BorderShadowVisibility = Visibility.Collapsed;
			browseControlModel.Margin = (IsADAMode ? new Thickness(0.0, 0.0, 0.0, 20.0) : new Thickness(0.0, 0.0, 0.0, 0.0));
			browseControlModel.BrowseItemMargin = new Thickness(0.0, 12.0, 0.0, 0.0);
			browseControlModel.RowCount = ((!ApplicationState.Instance.IsADAAccessible) ? 1 : 2);
			browseControlModel.ColumnCount = 5;
			int num = (ApplicationState.Instance.IsADAAccessible ? 10 : 5);
			for (int i = 0; i < num; i++)
			{
				browseControlModel.BrowseControls.Add(new DisplayProductWithCommandsUserControl
				{
					Name = $"minicart_product_{i + 1}",
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				});
			}
			browseControlModel.OnCancelBrowseItemModel += MiniCartControl_OnCancelDisplayProductModel;
			if (ApplicationState.Instance.IsADAAccessible)
			{
				List<IKioskProduct> browseProducts = ApplicationState.Instance.BrowseProducts;
				IBrowseState browseState = BrowseState;
				browseControlModel.BrowseItems = CreateBrowseItemsFromKioskProducts(browseProducts, browseState != null && browseState.BrowseSelectedSort == BrowseSelectedSort.Alpha, isADAMiniCart: true, RentalShoppingCartService?.CurrentRentalShoppingCart?.MaximumCapacity ?? 0, 1);
				browseControlModel.SetPageNumber(BrowseState.PageNumber, PageChangeSource.None);
			}
			else
			{
				browseControlModel.BrowseItems = CreateBrowseItemsFromCartItems();
			}
			browseViewModel.MiniCartControlModel = browseControlModel;
			browseViewModel.MiniCartVisibility = Visibility.Visible;
			browseViewModel.EmptyCartSpacesVisibility = (IsADAMode ? Visibility.Collapsed : Visibility.Visible);
			browseViewModel.EmptyCartAddItemMessageVisibility = ((IsADAMode || (RentalShoppingCartService?.CurrentRentalShoppingCart?.Count ?? 0) != 0) ? Visibility.Collapsed : Visibility.Visible);
			browseViewModel.AddToCartLabel1Text = Resources.add_to_cart_label1;
			browseViewModel.AddToCartLabel2Text = Resources.add_to_cart_label2;
		}
	}

	private static void MiniCartControl_OnCancelDisplayProductModel(IBrowseItemModel browseItemModel, object parameter)
	{
		if (browseItemModel == null)
		{
			return;
		}
		IRentalShoppingCartItem rentalShoppingCartItem = browseItemModel.Data as IRentalShoppingCartItem;
		IRentalShoppingCart rentalShoppingCart = RentalShoppingCartService?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null && rentalShoppingCartItem != null)
		{
			IRentalShoppingCartItem rentalShoppingCartItem2 = rentalShoppingCart.Items.FirstOrDefault((IRentalShoppingCartItem x) => x.ProductId == rentalShoppingCartItem.ProductId);
			if (rentalShoppingCartItem2 != null)
			{
				SessionAnalyticsService?.AddViewEvent("buttonPress", "shopping-cart-remove-item");
				rentalShoppingCart.RemoveItem(rentalShoppingCartItem2, userRemoved: true);
			}
		}
	}

	private static void CurrentRentalShoppingCart_OnChange(IRentalShoppingCart rentalShoppingCart)
	{
		BrowseViewModel browseViewModelFromView = GetBrowseViewModelFromView();
		if (browseViewModelFromView == null)
		{
			return;
		}
		if (!IsADAMode)
		{
			ConfigureMiniCartControl(browseViewModelFromView);
		}
		ConfigureCheckOutBar(browseViewModelFromView);
		if (browseViewModelFromView?.BrowseProductControlModel?.BrowseItems != null)
		{
			UpdateDisplayProductModels(browseViewModelFromView.BrowseProductControlModel.BrowseItems, isADAMiniCart: false);
			if (IsADAMode)
			{
				UpdateDisplayProductModels(browseViewModelFromView.MiniCartControlModel.BrowseItems, isADAMiniCart: true);
			}
		}
	}

	private static void UpdateDisplayProductModels(List<IBrowseItemModel> displayProductModels, bool isADAMiniCart)
	{
		DisplayProductModelConstants displayProductModelConstants = GetDisplayProductModelConstants();
		foreach (IBrowseItemModel displayProductModel2 in displayProductModels)
		{
			if (displayProductModel2 is DisplayProductModel displayProductModel)
			{
				ConfigureDisplayProductModel(displayProductModel.Data as IKioskProduct, displayProductModel, isADAMiniCart, displayProductModelConstants);
			}
		}
	}

	private static List<IBrowseItemModel> CreateBrowseItemsFromCartItems()
	{
		List<IBrowseItemModel> list = new List<IBrowseItemModel>();
		IRentalShoppingCart rentalShoppingCart = RentalShoppingCartService?.CurrentRentalShoppingCart;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (rentalShoppingCart != null && service != null)
		{
			foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
			{
				IKioskProduct kioskProduct = null;
				service.GetProduct(item.ProductId, excludeTitleRollup: true, out kioskProduct);
				if (kioskProduct != null)
				{
					DisplayProductModel displayProductModel = new DisplayProductModel
					{
						Name = kioskProduct.ProductName,
						Data = item,
						Image = OrchestrationService.GetProductBitmapImage(kioskProduct)
					};
					displayProductModel.ConfigureForMiniCart();
					displayProductModel.MiniCartCancelButtonVisibility = ((InTitleMarketingMode && item.Sequence == RentalShoppingCartItemSequence.pre_offer) ? Visibility.Collapsed : Visibility.Visible);
					bool flag = item.Action == RentalShoppingCartItemAction.purchase;
					displayProductModel.Banner = (flag ? BannerCollection.Instance.FirstOrDefault((Banner x) => x.BannerType == BannerType.Purchase) : null);
					displayProductModel.BannerVisibility = ((displayProductModel.Banner == null) ? Visibility.Collapsed : Visibility.Visible);
					displayProductModel.BannerText = ((flag && displayProductModel.Banner != null) ? Resources.ResourceManager.GetString(displayProductModel.Banner.TextResourceName) : null);
					displayProductModel.BannerBackgroundBrush = ((flag && displayProductModel.Banner != null) ? WPFFunctions.GetBrush(GetViewUserControl(), displayProductModel.Banner.BannerStyleName) : null);
					list.Add(displayProductModel);
				}
			}
		}
		return list;
	}

	private static List<IBrowseItemModel> CreateBrowseItemsFromCollections()
	{
		List<IBrowseItemModel> list = new List<IBrowseItemModel>();
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (service != null)
		{
			List<IKioskProduct> products = null;
			service.GetCollectionProducts(out products);
			foreach (IKioskProduct item in products)
			{
				if (item is CollectionProduct collectionProduct && collectionProduct.AvailableKioskProducts.Count > 0)
				{
					DisplayCollectionModel displayCollectionModel = BrowseFunctions.CreateDisplayCollectionModelFromCollectionProduct(collectionProduct);
					if (displayCollectionModel != null)
					{
						list.Add(displayCollectionModel);
					}
				}
			}
		}
		return list;
	}

	private static DisplayProductModelConstants GetDisplayProductModelConstants()
	{
		IMarketingConfigurationManager service = ServiceLocator.Instance.GetService<IMarketingConfigurationManager>();
		DisplayProductModelConstants obj = new DisplayProductModelConstants
		{
			StarringLabel = Resources.starring_label,
			GenresLabel = Resources.genres_label,
			OneNightPriceLabel = Resources.one_night_price_label,
			OneNightReturnTimeLabel = Resources.return_time_label,
			MultiNightPriceLabel = Resources.multi_night_price_label,
			MultiNightReturnTimeLabel = Resources.return_time_label,
			AdImage = GetAdImage()
		};
		obj.AdHeight = ((obj.AdImage != null) ? 110 : 20);
		obj.MainHeight = ((obj.AdImage != null) ? 500 : 610);
		obj.ShowCollections = service?.ShowCollections ?? false;
		obj.CollectionTextLine1 = Resources.title_detail_collection_text;
		obj.BuyText = Resources.title_detail_buy;
		obj.TitleDetailFormatButtonTextValues = new TitleRollupUtilities.TitleDetailFormatRentalButtonTextValues();
		obj.Add = Resources.add;
		obj.ComingSoonLabel = Resources.coming_label;
		obj.OutOfStockLine1Text = Resources.out_of_stock_line1;
		obj.OutOfStockLine2Text = Resources.out_of_stock_line2;
		obj.OutOfStockLine3Text = Resources.out_of_stock_line3;
		obj.Movies = Resources.Movies;
		obj.Games = Resources.Games;
		obj.FontMuseo20Style = WPFFunctions.GetStyle(GetViewUserControl(), "font_museo_slab_900_20");
		obj.FontMuseo16Style = WPFFunctions.GetStyle(GetViewUserControl(), "font_museo_slab_900_16");
		obj.PriceFor = Resources.price_for;
		obj.Night = Resources.night;
		obj.PricingService = ServiceLocator.Instance.GetService<IPricingService>();
		obj.RentalShoppingCartService = RentalShoppingCartService;
		IRentalShoppingCart rentalShoppingCart = RentalShoppingCartService?.CurrentRentalShoppingCart;
		obj.IsInCartBrowse = rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel;
		return obj;
	}

	private static List<IBrowseItemModel> CreateBrowseItemsFromKioskProducts(List<IKioskProduct> kioskProducts, bool createAlphaGroups, bool isADAMiniCart, int visibleItemCount, int pageNumber)
	{
		DisplayGroupModel displayGroupModel = null;
		List<IBrowseItemModel> result = new List<IBrowseItemModel>();
		if (kioskProducts != null)
		{
			DisplayProductModelConstants displayProductModelConstants = GetDisplayProductModelConstants();
			int num = (pageNumber - 1) * visibleItemCount;
			int num2 = num + visibleItemCount;
			int num3 = 0;
			foreach (IKioskProduct kioskProduct in kioskProducts)
			{
				ITitleProduct titleProduct = kioskProduct as ITitleProduct;
				DisplayProductModel displayProductModel = new DisplayProductModel
				{
					Name = kioskProduct.ProductName,
					Data = kioskProduct,
					Image = OrchestrationService.GetProductBitmapImage(kioskProduct),
					LockAspectRatio = true,
					Header = titleProduct.ProductName,
					DisplayProductModelConstants = displayProductModelConstants
				};
				if (num3 >= num && num3 < num2)
				{
					ConfigureDisplayProductModel(kioskProduct, displayProductModel, isADAMiniCart, displayProductModelConstants);
				}
				num3++;
				if (createAlphaGroups)
				{
					string alphaGroupCharacter = GetAlphaGroupCharacter(kioskProduct as ITitleProduct);
					if (displayGroupModel == null || displayGroupModel.Name != alphaGroupCharacter)
					{
						displayGroupModel = new DisplayGroupModel
						{
							Name = alphaGroupCharacter,
							SeparatorLineStyle = WPFFunctions.GetStyle(GetViewUserControl(), "separator_line_for_group"),
							UnselectedTitleStyle = WPFFunctions.GetStyle(GetViewUserControl(), "browse_group_header"),
							SelectedTitleStyle = WPFFunctions.GetStyle(GetViewUserControl(), "browse_group_header_selected"),
							IsSelected = (alphaGroupCharacter == BrowseState.SelectedAlphaGroupName)
						};
						result.Add(displayGroupModel);
					}
					displayProductModel.SeparatorLineStyle = WPFFunctions.GetStyle(GetViewUserControl(), "separator_line_for_product");
					displayProductModel.TitleStyle = WPFFunctions.GetStyle(GetViewUserControl(), "browse_product_title");
				}
				result.Add(displayProductModel);
			}
			TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			Task.Factory.StartNew(delegate
			{
				foreach (IBrowseItemModel item in result)
				{
					IKioskProduct eachKioskProduct = item.Data as IKioskProduct;
					DisplayProductModel browseItemModel = item as DisplayProductModel;
					ConfigureDisplayProductModel(eachKioskProduct, browseItemModel, isADAMiniCart, displayProductModelConstants);
				}
			}, CancellationToken.None, TaskCreationOptions.None, scheduler);
		}
		return result;
	}

	private static BitmapImage GetAdImage()
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		IMarketingAdService service2 = ServiceLocator.Instance.GetService<IMarketingAdService>();
		Image image = null;
		if (service2 != null)
		{
			MarketingAd screenAd = service2.GetScreenAd(MarketingAdLocation.TitleDetail, createImpression: true);
			if (screenAd.Visible)
			{
				image = service?.GetBitmap(screenAd.ImageName);
				if (image != null)
				{
					return ServiceLocator.Instance.GetService<IBitmapCacheService>()?.ToBitmapImage(image);
				}
			}
		}
		return null;
	}

	private static string GetAlphaGroupCharacter(ITitleProduct browseItem)
	{
		char c = ((browseItem != null && browseItem.SortName != null) ? browseItem.SortName.ToUpper()[0] : ' ');
		string result = "#";
		if (c >= 'A' && c <= 'Z')
		{
			result = c.ToString();
		}
		return result;
	}

	private static void ConfigureDisplayProductModel(IKioskProduct eachKioskProduct, DisplayProductModel browseItemModel, bool isADAMiniCart, DisplayProductModelConstants displayProductModelConstants)
	{
		if (browseItemModel == null || eachKioskProduct == null)
		{
			return;
		}
		ITitleProduct titleProduct = eachKioskProduct as ITitleProduct;
		bool isEmptyCase = titleProduct?.IsEmptyCase ?? false;
		BannerType bannerType = BannerType.None;
		browseItemModel.ComingSoonButtonVisibility = Visibility.Collapsed;
		if (BrowseState.BrowseSelectedSort != BrowseSelectedSort.Alpha)
		{
			ITitleProduct titleProduct2 = titleProduct;
			if (titleProduct2 != null && titleProduct2.ComingSoon)
			{
				bannerType = BannerType.ComingSoon;
				string text = null;
				if ((RentalShoppingCartService?.CurrentRentalShoppingCart?.ComingSoonOptInInfo?.OptInItems)?.FirstOrDefault((IOptInItem x) => x.ItemId == (titleProduct?.ProductId ?? 0)) != null)
				{
					text = $"{Resources.email_optin_reminder_set_button_text}";
					browseItemModel.ComingSoonButtonStyle = WPFFunctions.GetStyle(GetViewUserControl(), "classic_green_button_style");
					browseItemModel.ComingSoonButtonCheckMarkVisibility = Visibility.Visible;
				}
				else
				{
					text = $"{Resources.email_optin_button_text}";
					browseItemModel.ComingSoonButtonStyle = WPFFunctions.GetStyle(GetViewUserControl(), "rubine_white_border_button_style");
					browseItemModel.ComingSoonButtonCheckMarkVisibility = Visibility.Collapsed;
				}
				browseItemModel.ComingSoonButtonText = text;
				browseItemModel.ComingSoonButtonVisibility = Visibility.Visible;
			}
			else
			{
				bannerType = BrowseFunctions.GetProductBannerType(titleProduct, BrowseState.TitleType != TitleType.All);
			}
		}
		bool flag = IsProductSelected(titleProduct);
		IBrowseState browseState = BrowseState;
		if (browseState == null || browseState.BrowseMode != BrowseViewFilter.ComingSoon)
		{
			IRentalShoppingCartService rentalShoppingCartService = RentalShoppingCartService;
			if ((((rentalShoppingCartService != null && rentalShoppingCartService.CurrentRentalShoppingCartIsFull) || !titleProduct.IsAvailable) | flag) || !titleProduct.InStock)
			{
				IBrowseState browseState2 = BrowseState;
				if (browseState2 != null && browseState2.BrowseSelectedSort == BrowseSelectedSort.Alpha)
				{
					browseItemModel.ImageOpacity = 0.2;
				}
				else
				{
					browseItemModel.ImageOpacity = 0.4;
				}
				goto IL_01cb;
			}
		}
		browseItemModel.ImageOpacity = 1.0;
		goto IL_01cb;
		IL_0470:
		int addButtonBarVisibility;
		browseItemModel.AddButtonBarVisibility = (Visibility)addButtonBarVisibility;
		ITitleProduct titleProduct3 = titleProduct;
		browseItemModel.OutOfStockVisibility = ((titleProduct3 == null || titleProduct3.InStock || titleProduct.ComingSoon) ? Visibility.Collapsed : Visibility.Visible);
		ITitleProduct titleProduct4 = titleProduct;
		browseItemModel.ComingSoonVisibility = ((titleProduct4 == null || !titleProduct4.ComingSoon) ? Visibility.Collapsed : Visibility.Visible);
		return;
		IL_01cb:
		BrowseFunctions.ConfigureBanner(browseItemModel, titleProduct, isADAMiniCart, bannerType, GetViewUserControl());
		ProductFlags browseProductFlag = GetBrowseProductFlag(titleProduct, isEmptyCase, flag);
		BrowseFunctions.ConfigureProductFlag(browseItemModel, browseProductFlag, isADAMiniCart, GetViewUserControl());
		int num;
		if (!IsShoppingCartFull)
		{
			ITitleProduct titleProduct5 = titleProduct;
			if (titleProduct5 != null && titleProduct5.IsAvailable && !flag)
			{
				num = ((titleProduct?.InStock ?? false) ? 1 : 0);
				goto IL_022e;
			}
		}
		num = 0;
		goto IL_022e;
		IL_022e:
		bool flag2 = (byte)num != 0;
		browseItemModel.CanAdd = flag2 || BrowseState.BrowseMode == BrowseViewFilter.ComingSoon;
		Visibility visibility = ((!flag2 && !IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.CornerAddButtonVisibility = ((!isADAMiniCart) ? visibility : Visibility.Collapsed);
		browseItemModel.RightAddButtonVisibility = ((visibility == Visibility.Collapsed) ? Visibility.Hidden : visibility);
		browseItemModel.ADAMiniCartAddButtonVisibility = ((!isADAMiniCart) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.AddButtonPlusSignVisibility = (IsADAMode ? Visibility.Hidden : Visibility.Visible);
		browseItemModel.AddButtonTextVisibility = ((!IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.AddButtonStyle = (IsADAMode ? WPFFunctions.GetStyle(GetViewUserControl(), "rubine_white_border_button_style") : WPFFunctions.GetStyle(GetViewUserControl(), "classic_rubine_button_style"));
		if (isADAMiniCart)
		{
			browseItemModel.ConfigureForADAMiniCart();
		}
		else if (BrowseState.BrowseSelectedSort == BrowseSelectedSort.Alpha)
		{
			browseItemModel.ConfigureForAToZBrowse();
		}
		else
		{
			browseItemModel.ConfigureForBrowse();
		}
		bool flag3 = titleProduct is TitleRollupProduct && titleProduct.KioskProductType == KioskProductType.TitleRollup;
		Visibility showBuy = ((IsShoppingCartFull || (!titleProduct.IsSellable && !titleProduct.IsEmptyCase) || !(flag3 ? (!((TitleRollupProduct)titleProduct).KioskProducts.All((ITitleProduct x) => RentalShoppingCartService.CurrentRentalShoppingCart.HasItem(x.ProductId))) : (!RentalShoppingCartService.CurrentRentalShoppingCart.HasItem(titleProduct.ProductId)))) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.ShowBuy = showBuy;
		browseItemModel.ButtonData.Clear();
		if (!titleProduct.IsEmptyCase)
		{
			if (flag3)
			{
				foreach (ITitleProduct item in ((TitleRollupProduct)titleProduct).KioskProducts.OrderBy((ITitleProduct p) => p.TitleType))
				{
					AddFormatButtonData(browseItemModel, item, flag3, displayProductModelConstants);
				}
			}
			else
			{
				AddFormatButtonData(browseItemModel, titleProduct, flag3, displayProductModelConstants);
			}
		}
		if (!IsShoppingCartFull)
		{
			ITitleProduct titleProduct6 = titleProduct;
			if (titleProduct6 != null && !titleProduct6.ComingSoon)
			{
				ITitleProduct titleProduct7 = titleProduct;
				if (titleProduct7 != null && titleProduct7.InStock)
				{
					addButtonBarVisibility = 0;
					goto IL_0470;
				}
			}
		}
		addButtonBarVisibility = 2;
		goto IL_0470;
	}

	private static void AddFormatButtonData(DisplayProductModel browseItemModel, ITitleProduct product, bool isTitleRollup, DisplayProductModelConstants displayProductModelConstants)
	{
		TitleRollupFormatButtonProperties rollupFormatButtonProperties = TitleRollupUtilities.GetRollupFormatButtonProperties(product, isPurchase: false, displayProductModelConstants.TitleDetailFormatButtonTextValues);
		if (rollupFormatButtonProperties.Visibility == Visibility.Visible)
		{
			TilteDetailRentalFormatButtonData tilteDetailRentalFormatButtonData = new TilteDetailRentalFormatButtonData(rollupFormatButtonProperties.ButtonText, rollupFormatButtonProperties.TitleType, rollupFormatButtonProperties.ButtonTextLine2, rollupFormatButtonProperties.IsEnabled);
			if (!product.IsMovie || product.IsEmptyCase)
			{
				tilteDetailRentalFormatButtonData.Line1Text = displayProductModelConstants.Add;
			}
			browseItemModel.ButtonData.Add(tilteDetailRentalFormatButtonData);
		}
	}

	private static bool IsProductSelected(ITitleProduct titleProduct)
	{
		bool flag = true;
		if (titleProduct != null)
		{
			if (titleProduct is TitleRollupProduct titleRollupProduct)
			{
				foreach (ITitleProduct kioskProduct in titleRollupProduct.KioskProducts)
				{
					if ((BrowseState.BrowseMode == BrowseViewFilter.ForSale && kioskProduct.IsSellable) || (BrowseState.BrowseMode != BrowseViewFilter.ForSale && kioskProduct.IsRentable))
					{
						flag = flag && (!kioskProduct.InStock || (RentalShoppingCartService?.CurrentRentalShoppingCart.HasItem(kioskProduct.ProductId) ?? true));
					}
				}
			}
			else
			{
				flag = RentalShoppingCartService?.CurrentRentalShoppingCart.HasItem(titleProduct.ProductId) ?? true;
			}
		}
		return flag;
	}

	private static ProductFlags GetBrowseProductFlag(ITitleProduct titleProduct, bool isEmptyCase, bool isProductSelected)
	{
		ProductFlags result = ProductFlags.None;
		if (titleProduct != null && !IsShoppingCartFull && !InTitleMarketingMode)
		{
			if (isEmptyCase && !titleProduct.InStock)
			{
				result = ProductFlags.OutOfStock;
			}
			else if (!titleProduct.IsAvailable)
			{
				result = ProductFlags.ComingSoon;
			}
			else if (BrowseState.BrowseMode != BrowseViewFilter.ForSale && titleProduct.ShowOutOfStock)
			{
				result = ProductFlags.OutOfStock;
			}
			else if (!isProductSelected)
			{
				result = BrowseFunctions.GetProductFlag(titleProduct);
			}
		}
		return result;
	}

	private static List<IKioskProduct> GetBrowseKioskProducts()
	{
		IBrowseState browseState = Redbox.Rental.Services.Session.Session.Instance.BrowseState;
		LogHelper.Instance.Log("RentalGetProducts by browseMode: {0} family: {1} familyType: {2} selectedSort: {3} selectedGenere: {4}, isADAView: {5} ", browseState.BrowseMode, browseState.TitleFamily, browseState.TitleType, browseState.BrowseSelectedSort, browseState.Genre, ApplicationState.Instance.IsADAAccessible);
		try
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			BrowseSort browseSort = browseState.BrowseSelectedSort.ConvertToBrowseSort(browseState.BrowseMode);
			List<long> include = new List<long>();
			List<long> exclude = new List<long>();
			IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
			if (rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel)
			{
				include = PromotionService.Instance.GetInCartIncludeTitleList();
				exclude = PromotionService.Instance.GetInCartExcludeTitleList();
			}
			ErrorList products = service.GetProducts(browseState.BrowseMode, browseState.TitleFamily, browseState.TitleType, browseSort, (long?)browseState.Genre, include, exclude, ApplicationState.Instance.IsADAAccessible, out var products2);
			if (products.Count > 0)
			{
				products.ToLogHelper();
				return null;
			}
			if (products2 == null)
			{
				LogHelper.Instance.Log("Error: RentalDataFunctions.RentalGetProducts return null");
				return null;
			}
			LogHelper.Instance.Log("RentalGetProducts by browseViewfilter count: {0}", products2.Count);
			return products2;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.RentalGetProducts", e);
			return null;
		}
	}

	private static T GetResource<T>(FrameworkElement frameworkElement, string resourceName)
	{
		T result = default(T);
		if (!string.IsNullOrEmpty(resourceName))
		{
			object obj = frameworkElement?.FindResource(resourceName);
			if (obj != null && obj is T result2)
			{
				return result2;
			}
		}
		return result;
	}

	private static List<BrowseFilter> CreateBrowseFilters(IConfiguration configuration)
	{
		List<BrowseFilter> list = new List<BrowseFilter>();
		IBrowseState browseState = BrowseState;
		int height = 32;
		int width = (IsADAMode ? 77 : 96);
		if (browseState.TitleFamily == TitleFamily.Movies)
		{
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.AtoZ,
				Name = "abc_button",
				Height = height,
				Width = width,
				IsSelected = (browseState.BrowseSelectedSort == BrowseSelectedSort.Alpha)
			});
		}
		if (BrowseState.TitleFamily == TitleFamily.Games)
		{
			if (configuration.EnabledGameTypes[TitleType.PS4] && configuration.GameHasContentTypes[TitleType.PS4])
			{
				list.Add(new BrowseFilter
				{
					ButtonType = BrowseFilterType.PS4,
					Name = "ps4_button",
					Height = height,
					Width = 76,
					IsSelected = (browseState.BrowseSelectedSort == BrowseSelectedSort.Ps4)
				});
			}
			if (configuration.EnabledGameTypes[TitleType.XboxOne] && configuration.GameHasContentTypes[TitleType.XboxOne])
			{
				list.Add(new BrowseFilter
				{
					ButtonType = BrowseFilterType.XboxOne,
					Name = "xbox_one_button",
					Height = height,
					Width = 96,
					IsSelected = (browseState.BrowseSelectedSort == BrowseSelectedSort.XboxOne)
				});
			}
			if (configuration.EnabledGameTypes[TitleType.NintendoSwitch] && configuration.GameHasContentTypes[TitleType.NintendoSwitch])
			{
				list.Add(new BrowseFilter
				{
					ButtonType = BrowseFilterType.NintendoSwitch,
					Name = "nintendo_switch_button",
					Height = height,
					Width = 136,
					IsSelected = (browseState.BrowseSelectedSort == BrowseSelectedSort.NintendoSwitch)
				});
			}
			if (configuration.EnabledGameTypes[TitleType.Xbox360] && configuration.GameHasContentTypes[TitleType.Xbox360])
			{
				list.Add(new BrowseFilter
				{
					ButtonType = BrowseFilterType.Xbox360,
					Name = "xbox_360_button",
					Height = height,
					Width = 96,
					IsSelected = (browseState.BrowseSelectedSort == BrowseSelectedSort.Xbox360)
				});
			}
		}
		if (IsForSaleButtonEnabled)
		{
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.ForSale,
				Name = "for_sale_button",
				Height = height,
				Width = width,
				IsSelected = (browseState.BrowseSelectedSort == BrowseSelectedSort.ForSale)
			});
		}
		if (IsADAMode && BrowseState.TitleFamily == TitleFamily.Movies)
		{
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.Top20,
				Name = "top_20_filter_button",
				Height = height,
				Width = (IsSpanish ? 107 : 77),
				IsSelected = IsGenreButtonSelected(BrowseSelectedSort.Top20, Genres.Top20Movies)
			});
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.Action,
				Name = "action_filter_button",
				Height = height,
				Width = 77,
				IsSelected = IsGenreButtonSelected(BrowseSelectedSort.Action, Genres.ActionAndAdventure)
			});
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.Drama,
				Name = "drama_filter_button",
				Height = height,
				Width = 77,
				IsSelected = IsGenreButtonSelected(BrowseSelectedSort.Drama, Genres.Drama)
			});
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.Horror,
				Name = "horror_filter_button",
				Height = height,
				Width = 77,
				IsSelected = IsGenreButtonSelected(BrowseSelectedSort.Horror, Genres.Horror)
			});
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.Comedy,
				Name = "comedy_filter_button",
				Height = height,
				Width = 77,
				IsSelected = IsGenreButtonSelected(BrowseSelectedSort.Comedy, Genres.Comedy)
			});
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.Family,
				Name = "family_filter_button",
				Height = height,
				Width = 77,
				IsSelected = IsGenreButtonSelected(BrowseSelectedSort.Family, Genres.FamilyMovies)
			});
		}
		if (!IsADAMode)
		{
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.ComingSoon,
				Name = "coming_soon_button",
				Height = height,
				Width = 112,
				IsSelected = (browseState.BrowseSelectedSort == BrowseSelectedSort.ComingSoon)
			});
		}
		foreach (BrowseFilter item in list)
		{
			item.Text = Resources.ResourceManager.GetString(item.Name);
		}
		return list;
	}

	private static bool IsGenreButtonSelected(BrowseSelectedSort genreSort, Genres genre)
	{
		if (BrowseState.BrowseSelectedSort != genreSort)
		{
			if (BrowseState.BrowseSelectedSort != BrowseSelectedSort.Alpha && BrowseState.BrowseSelectedSort != BrowseSelectedSort.ForSale && IsADAMode)
			{
				return BrowseState.Genre == genre;
			}
			return false;
		}
		return true;
	}
}
