using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
	private class DisplayProductModelConstants
	{
		public string StarringLabel { get; set; }

		public string GenresLabel { get; set; }

		public string OneNightPriceLabel { get; set; }

		public string OneNightReturnTimeLabel { get; set; }

		public string MultiNightPriceLabel { get; set; }

		public string MultiNightReturnTimeLabel { get; set; }

		public BitmapImage AdImage { get; set; }

		public int AdHeight { get; set; }

		public int MainHeight { get; set; }

		public bool ShowCollections { get; set; }

		public string CollectionTextLine1 { get; set; }

		public string BuyText { get; set; }

		public TitleRollupUtilities.TitleRollupOverlayButtonTextValues TitleRollupOverlayButtonTextValues { get; set; }

		public string Add { get; set; }

		public string ComingSoonLabel { get; set; }

		public string OutOfStockLine1Text { get; set; }

		public string OutOfStockLine2Text { get; set; }

		public string OutOfStockLine3Text { get; set; }

		public string Movies { get; set; }

		public string Games { get; set; }

		public Style FontMuseo16Style { get; set; }

		public Style FontMuseo20Style { get; set; }

		public string PriceFor { get; set; }

		public string Night { get; set; }

		public IPricingService PricingService { get; set; }

		public bool IsInCartBrowse { get; set; }
	}

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

	private static bool IsDigitalCodeButtonEnabled
	{
		get
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			List<IKioskProduct> products = null;
			service?.GetProducts(BrowseViewFilter.ForSale, TitleFamily.Movies, TitleType.DigitalCode, BrowseSort.MerchandiseDate, null, null, null, isADAView: false, out products);
			if (products != null)
			{
				return products.Count > 0;
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
		FrameworkElement browseViewUserControl = GetBrowseViewUserControl();
		if (browseViewUserControl != null)
		{
			BrowseViewModel browseViewModel = new BrowseViewModel();
			ConfigureBrowseControl(browseViewModel);
			ConfigureMiniCartControl(browseViewModel);
			ConfigureShoppingCartChangeHandler();
			ConfigureTopMenu(browseViewModel);
			ConfigureBrowseFilterBar(browseViewUserControl, browseViewModel);
			ConfigureCheckOutBar(browseViewModel);
			ConfigureTTS(browseViewModel);
			browseViewUserControl.DataContext = browseViewModel;
			CommandManager.InvalidateRequerySuggested();
			(IdleTimerService?.GetIdleTimer("idle_timer"))?.Start();
		}
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
		topMenuModel.MenuBackgroundStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "browse_top_menu_background_collection_style");
		topMenuModel.MenuBackgroundText = null;
		if (IsADAMode || InTitleMarketingMode)
		{
			if (IsADAMode)
			{
				topMenuModel.MenuBackgroundText = ((BrowseState.TitleFamily == TitleFamily.Games) ? Resources.browse_family_games : Resources.browse_family_movies);
				topMenuModel.MenuTextStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "browse_top_menu_text_ada_style");
			}
			topMenuModel.MenuBackgroundStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "browse_top_menu_background_ada_style");
		}
		else if (BrowseState.BrowseCollectionProduct != null)
		{
			topMenuModel.MenuBackgroundText = BrowseState.BrowseCollectionProduct.ProductName;
			topMenuModel.MenuTextStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "browse_top_menu_text_collection_style");
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
				if (BrowseState.BrowseSelectedSort == "Blu-ray" || BrowseState.BrowseSelectedSort == "Digital Code" || BrowseState.BrowseSelectedSort == "alpha")
				{
					BrowseState.BrowseSelectedSort = "Newest";
					BrowseState.BrowseMode = BrowseViewFilter.All;
				}
				else if (BrowseState.BrowseMode == BrowseViewFilter.ComingSoon)
				{
					BrowseState.BrowseSelectedSort = "coming-soon";
				}
				else if (BrowseState.BrowseMode == BrowseViewFilter.ForSale)
				{
					if (OrchestrationService.GetCanSellUsedGames())
					{
						BrowseState.BrowseSelectedSort = "for-sale";
						BrowseState.BrowseMode = BrowseViewFilter.ForSale;
					}
					else
					{
						BrowseState.BrowseSelectedSort = "Newest";
						BrowseState.BrowseMode = BrowseViewFilter.All;
					}
				}
				else
				{
					BrowseState.BrowseSelectedSort = "Newest";
					BrowseState.BrowseMode = BrowseViewFilter.All;
				}
			}
			else if (BrowseState.BrowseSelectedSort == "Xbox 360" || BrowseState.BrowseSelectedSort == "Xbox One" || BrowseState.BrowseSelectedSort == "PS2" || BrowseState.BrowseSelectedSort == "PS3" || BrowseState.BrowseSelectedSort == "PS4" || BrowseState.BrowseSelectedSort == "NintendoDS" || BrowseState.BrowseSelectedSort == "Wii" || BrowseState.BrowseSelectedSort == "Wii U" || BrowseState.BrowseSelectedSort == "Nintendo Switch" || BrowseState.BrowseSelectedSort == null)
			{
				BrowseState.BrowseSelectedSort = "Newest";
			}
			else if (BrowseState.BrowseMode == BrowseViewFilter.ForSale)
			{
				if (OrchestrationService.GetCanSellUsedMovies())
				{
					BrowseState.BrowseSelectedSort = "for-sale";
					BrowseState.BrowseMode = BrowseViewFilter.ForSale;
				}
				else
				{
					BrowseState.BrowseSelectedSort = "Newest";
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
		FrameworkElement browseViewUserControl = GetBrowseViewUserControl();
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
		FrameworkElement browseViewUserControl = GetBrowseViewUserControl();
		if (service != null && service.TTSEnabled && browseViewUserControl != null)
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
			BrowseState.BrowseSelectedSort = "Newest";
			BrowseState.Genre = null;
			BrowseState.PageNumber = 1;
			ConfigureBrowseView();
		};
		browseViewModel.OnSignInButtonClicked = delegate
		{
			SessionAnalyticsService?.AddViewEvent("buttonPress", "signin");
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
			style = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "classic_gray_button_style");
			checkOutButtonText = Resources.no_thanks_button;
		}
		else
		{
			style = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "warmpurple_white_border_button_with_dropshadow_style");
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

	private static FrameworkElement GetBrowseViewUserControl()
	{
		FrameworkElement result = null;
		IActor actor = ActorFunctions.GetActor("BrowseViewUserControl");
		if (actor != null)
		{
			result = actor.WPFFrameworkElement;
		}
		return result;
	}

	private static void ConfigureBrowseFilterBar()
	{
		BrowseViewModel browseViewModelFromView = GetBrowseViewModelFromView();
		ConfigureBrowseFilterBar(GetBrowseViewUserControl(), browseViewModelFromView);
	}

	private static void ConfigureBrowseFilterBar(FrameworkElement browseViewUserControl, BrowseViewModel browseViewModel)
	{
		FilterBarModel filterBarModel = new FilterBarModel();
		filterBarModel.OnStartOverButtonClicked += delegate
		{
			OrchestrationService.ExecuteScript("start_over");
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
			ApplicationState.Instance.KeepSessionOnStartViewEnter = true;
			OrchestrationService.ExecuteScript("start_over");
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
			HandleFilterButtonClick(browseFilter);
		};
		browseViewModel.BrowseFilterBarModel = filterBarModel;
	}

	internal static void HandleFilterButtonClick(BrowseFilter browseFilterButton)
	{
		if (IsADAMode)
		{
			BrowseState.Genre = null;
		}
		switch (browseFilterButton.ButtonType)
		{
		case BrowseFilterType.AllNewest:
			SetBrowseFilters(BrowseViewFilter.All, "Newest", TitleType.All, "browse-filter-family,Newest", 1);
			break;
		case BrowseFilterType.BluRay:
			SetBrowseFilters(BrowseViewFilter.All, "Blu-ray", TitleType.Bluray, "browse-filter-family,Blu-ray", 1);
			break;
		case BrowseFilterType.ForSale:
			SetBrowseFilters(BrowseViewFilter.ForSale, "for-sale", TitleType.All, "browse-filter-family,for-sale", 1);
			break;
		case BrowseFilterType.ComingSoon:
			SetBrowseFilters(BrowseViewFilter.ComingSoon, "coming-soon", TitleType.All, "browse-filter-family,Coming-soon", 1);
			break;
		case BrowseFilterType.AtoZ:
			BrowseState.SelectedAlphaGroupName = null;
			SetBrowseFilters(BrowseViewFilter.All, "alpha", TitleType.All, "browse-filter-family,alpha", 1);
			break;
		case BrowseFilterType.DigitalCode:
			SetBrowseFilters(BrowseViewFilter.ForSale, "Digital Code", TitleType.DigitalCode, "browse-filter-family,Digital Code", 1);
			break;
		case BrowseFilterType.XboxOne:
			SetBrowseFilters(BrowseViewFilter.All, "Xbox One", TitleType.XboxOne, "browse-filter-family,Xbox One", 1);
			break;
		case BrowseFilterType.Xbox360:
			SetBrowseFilters(BrowseViewFilter.All, "Xbox 360", TitleType.Xbox360, "browse-filter-family,Xbox 360", 1);
			break;
		case BrowseFilterType.PS3:
			SetBrowseFilters(BrowseViewFilter.All, "PS3", TitleType.PS3, "browse-filter-family,PS3", 1);
			break;
		case BrowseFilterType.PS4:
			SetBrowseFilters(BrowseViewFilter.All, "PS4", TitleType.PS4, "browse-filter-family,PS4", 1);
			break;
		case BrowseFilterType.Wii:
			SetBrowseFilters(BrowseViewFilter.All, "Wii", TitleType.Wii, "browse-filter-family,Wii", 1);
			break;
		case BrowseFilterType.WiiU:
			SetBrowseFilters(BrowseViewFilter.All, "Wii U", TitleType.WiiU, "browse-filter-family,Wii U", 1);
			break;
		case BrowseFilterType.NintendoSwitch:
			SetBrowseFilters(BrowseViewFilter.All, "Nintendo Switch", TitleType.NintendoSwitch, "browse-filter-family,Nintendo Switch", 1);
			break;
		case BrowseFilterType.Top20:
			BrowseState.Genre = Genres.Top20Movies;
			SetBrowseFilters(BrowseViewFilter.All, "Top 20", TitleType.All, "browse-filter-family,Top 20", 1);
			break;
		case BrowseFilterType.Action:
			BrowseState.Genre = Genres.ActionAndAdventure;
			SetBrowseFilters(BrowseViewFilter.All, "Action", TitleType.All, "browse-filter-family,Action", 1);
			break;
		case BrowseFilterType.Drama:
			BrowseState.Genre = Genres.Drama;
			SetBrowseFilters(BrowseViewFilter.All, "Drama", TitleType.All, "browse-filter-family,Drama", 1);
			break;
		case BrowseFilterType.Horror:
			BrowseState.Genre = Genres.Horror;
			SetBrowseFilters(BrowseViewFilter.All, "Horror", TitleType.All, "browse-filter-family,Horror", 1);
			break;
		case BrowseFilterType.Comedy:
			BrowseState.Genre = Genres.Comedy;
			SetBrowseFilters(BrowseViewFilter.All, "Comedy", TitleType.All, "browse-filter-family,Comedy", 1);
			break;
		case BrowseFilterType.Family:
			BrowseState.Genre = Genres.FamilyMovies;
			SetBrowseFilters(BrowseViewFilter.All, "Family", TitleType.All, "browse-filter-family,Family", 1);
			break;
		}
	}

	internal static void SetBrowseFilters(BrowseViewFilter browseMode, string browseSelectedSort, TitleType titleType, string analyticAttributeValue, int? pageNumber)
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
		BrowseControlModel browseProductControl = new BrowseControlModel();
		browseProductControl.SetPageNumber(Math.Max(BrowseState.PageNumber, 1), PageChangeSource.None);
		BrowseControlModel browseControlModel = browseProductControl;
		IBrowseState browseState = BrowseState;
		browseControlModel.NoResultsText = ((browseState != null && browseState.TitleFamily == TitleFamily.Movies) ? Resources.filter_movies_empty : Resources.filter_games_empty);
		browseProductControl.ShowNoResultsMessage = true;
		browseProductControl.BorderShadowVisibility = Visibility.Collapsed;
		DisplayGroupSelectionModel displayGroupSelectionModel = new DisplayGroupSelectionModel();
		displayGroupSelectionModel.OnGroupSelectionItemClicked = BrowseViewModel_OnGroupSelectionItemClicked;
		browseProductControl.ShowBorderItems = Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectedSort != "alpha";
		browseProductControl.OnNotifyBrowseItemsShown += BrowseProductControl_OnNotifyBrowseItemsShown;
		browseProductControl.OnBrowseItemModelSelected += BrowseProductControl_OnDisplayProductModelSelected;
		browseProductControl.OnAddBrowseItemModel += BrowseProductControl_OnAddDisplayProductModel;
		browseProductControl.TitleRollupOverlayModel.OnTitleRollupOverlayCancel += TitleRollupOverlayModel_OnTitleRollupOverlayCancel;
		browseProductControl.TitleRollupOverlayModel.OnTitleRollupOverlayAddDVD += TitleRollupOverlayModel_OnTitleRollupOverlayAddDVD;
		browseProductControl.TitleRollupOverlayModel.OnTitleRollupOverlayAddBluray += TitleRollupOverlayModel_OnTitleRollupOverlayAddBluray;
		browseProductControl.TitleRollupOverlayModel.OnTitleRollupOverlayAddDigitalCode += TitleRollupOverlayModel_OnTitleRollupOverlayAddDigitalCode;
		browseProductControl.OnPageChanged += delegate(int prevPageNumber, int newPageNumber, PageChangeSource pageChangeSource)
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
				browseProductControl.TitleRollupOverlayModel.Visibility = Visibility.Collapsed;
			}
		};
		if (BrowseState.BrowseSelectedSort == "alpha")
		{
			browseProductControl.LayoutDirection = BrowseLayoutDirection.TopToBottomThenLeftToRight;
			browseProductControl.BrowseItemMargin = new Thickness(12.0, 1.0, 10.0, 0.0);
			browseProductControl.ShowBorderItems = false;
			displayGroupSelectionModel.BrowseGroupSelectionVisibility = Visibility.Visible;
			browseProductControl.PaddleOverlapWidth = 0.0;
			browseProductControl.PageNumberVisibility = Visibility.Collapsed;
			if (ApplicationState.Instance.IsADAAccessible)
			{
				browseProductControl.RowCount = 5;
				browseProductControl.ColumnCount = 2;
				browseProductControl.Margin = new Thickness(0.0, 0.0, 0.0, 40.0);
				displayGroupSelectionModel.BrowseGroupSelectionMargin = new Thickness(35.0, 522.0, 35.0, 0.0);
			}
			else
			{
				browseProductControl.RowCount = 6;
				browseProductControl.ColumnCount = 3;
				browseProductControl.Margin = new Thickness(0.0, 55.0, 0.0, 0.0);
				displayGroupSelectionModel.BrowseGroupSelectionMargin = new Thickness(38.0, 84.0, 38.0, 0.0);
			}
		}
		else
		{
			displayGroupSelectionModel.BrowseGroupSelectionVisibility = Visibility.Collapsed;
			if (BrowseState.BrowseCollections)
			{
				browseProductControl.RowCount = 2;
				browseProductControl.ColumnCount = 3;
				browseProductControl.BrowseItemMargin = new Thickness(8.0, 20.0, 8.0, 3.0);
			}
			else
			{
				browseProductControl.PaddleOverlapWidth = 24.0;
				browseProductControl.BrowseItemMargin = new Thickness(6.0, 36.0, 6.0, 0.0);
				browseProductControl.RowCount = 2;
				browseProductControl.ColumnCount = 5;
			}
		}
		if (BrowseState.BrowseCollections)
		{
			browseProductControl.BrowseItems = CreateBrowseItemsFromCollections();
		}
		else if (BrowseState.BrowseCollectionProduct != null)
		{
			CollectionProduct collectionProduct = BrowseState.BrowseCollectionProduct as CollectionProduct;
			List<IKioskProduct> kioskProducts = null;
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
						List<long> list = new List<long>();
						list.AddRange(promotion.CampaignTitles.Titles);
						if (promotion.CampaignTitles.Include)
						{
							include = list;
						}
						else
						{
							exclude = list;
						}
					}
				}
				kioskProducts = collectionProduct.GetInCartAvailableKioskProducts(TitleFamily.All, TitleType.All, include, exclude);
			}
			browseProductControl.BrowseItems = CreateBrowseItemsFromKioskProducts(kioskProducts, BrowseState?.BrowseSelectedSort == "alpha", isADAMiniCart: false);
		}
		else
		{
			ApplicationState.Instance.BrowseProducts = GetBrowseKioskProducts();
			browseProductControl.BrowseItems = CreateBrowseItemsFromKioskProducts(ApplicationState.Instance.BrowseProducts, BrowseState?.BrowseSelectedSort == "alpha", isADAMiniCart: false);
		}
		if (BrowseState != null)
		{
			BrowseState.BrowseItemModels = browseProductControl.BrowseItems;
		}
		for (int num = 0; num < browseProductControl.RowCount * browseProductControl.ColumnCount * 3; num++)
		{
			if (BrowseState.BrowseCollections)
			{
				browseProductControl.BrowseControls.Add(new DisplayCollectionUserControl
				{
					Name = $"product_{num + 1}"
				});
				continue;
			}
			if (BrowseState.BrowseSelectedSort == "alpha")
			{
				browseProductControl.BrowseControls.Add(new DisplayProductAToZUserControl
				{
					Name = $"product_{num + 1}"
				});
				continue;
			}
			browseProductControl.BrowseControls.Add(new DisplayProductWithCommandsUserControl
			{
				Name = $"product_{num + 1}",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			});
		}
		if (BrowseState.BrowseSelectedSort == "alpha")
		{
			displayGroupSelectionModel.GroupSelectionItems = CreateAToZGroups(browseProductControl.BrowseItems);
		}
		if (browseProductControl.BrowseItems != null && browseProductControl.BrowseItems.Count == 0)
		{
			browseProductControl.PageNumberVisibility = Visibility.Collapsed;
		}
		browseViewModel.BrowseGroupSelection = displayGroupSelectionModel;
		browseViewModel.BrowseProductControlModel = browseProductControl;
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
		if (list.Count > 0)
		{
			SessionAnalyticsService?.AddViewEvent("productsShown", list.Count.ToString());
		}
		BrowseState.BrowseProductsShown = list;
		BrowseState.TTSBrowseItemModelsShown = list2;
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
		LinearGradientBrush resource = GetResource<LinearGradientBrush>(GetBrowseViewUserControl(), "group_selection_item_unselected_background");
		LinearGradientBrush resource2 = GetResource<LinearGradientBrush>(GetBrowseViewUserControl(), "group_selection_item_selected_background");
		foreach (string eachGroupName in list2)
		{
			bool isEnabled = browseItems.Any((IBrowseItemModel x) => x is DisplayGroupModel && ((DisplayGroupModel)x).Name == eachGroupName);
			list.Add(new DisplayGroupSelectionItemModel
			{
				Name = eachGroupName,
				EnableTextStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "group_selection_item_enabled"),
				DisabledTextStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "group_selection_item_disabled"),
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
		BrowseViewModel browseViewModelFromView = GetBrowseViewModelFromView();
		if (browseViewModelFromView?.BrowseProductControlModel?.TitleRollupOverlayModel != null)
		{
			browseViewModelFromView.BrowseProductControlModel.TitleRollupOverlayModel.Visibility = Visibility.Collapsed;
		}
	}

	private static void TitleRollupOverlayModel_OnTitleRollupOverlayAddDigitalCode(IBrowseItemModel browseItemModel, object parameter)
	{
		if (browseItemModel?.Data is TitleRollupProduct titleRollupProduct)
		{
			BrowseFunctions.AddProductToCart(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.DigitalCode), isPurchase: true);
		}
	}

	private static void TitleRollupOverlayModel_OnTitleRollupOverlayAddBluray(IBrowseItemModel browseItemModel, object parameter)
	{
		if (browseItemModel?.Data is TitleRollupProduct titleRollupProduct)
		{
			ITitleProduct titleProduct = titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.Bluray);
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
		}
	}

	private static void TitleRollupOverlayModel_OnTitleRollupOverlayAddDVD(IBrowseItemModel browseItemModel, object parameter)
	{
		if (browseItemModel?.Data is TitleRollupProduct titleRollupProduct)
		{
			ITitleProduct titleProduct = titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.DVD);
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
		}
	}

	private static void TitleRollupOverlayModel_OnTitleRollupOverlayCancel(IBrowseItemModel browseItemModel, object parameter)
	{
		string subItemValue = "shopping-cart-cancel-add-item";
		if (BrowseFunctions.IsPurchase)
		{
			subItemValue = "buy-cancel-add-item";
		}
		SessionAnalyticsService?.AddViewEvent("buttonPress", subItemValue);
	}

	private static void BrowseProductControl_OnAddDisplayProductModel(IBrowseItemModel browseItemModel, object parameter)
	{
		if (parameter?.ToString() == BrowseFunctions.Constants.ComingSoon)
		{
			ProcessComingSoonOptin(browseItemModel);
		}
		else if (BrowseState.BrowseSelectedSort == "alpha" && !IsADAMode)
		{
			BrowseProductControl_OnDisplayProductModelSelected(browseItemModel, parameter);
		}
		else
		{
			if (!(browseItemModel.Data is IKioskProduct kioskProduct))
			{
				return;
			}
			BrowseViewModel browseViewModelFromView = GetBrowseViewModelFromView();
			if (browseViewModelFromView == null)
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
				if (browseViewModelFromView?.BrowseProductControlModel?.TitleRollupOverlayModel != null)
				{
					browseViewModelFromView.BrowseProductControlModel.TitleRollupOverlayModel.Visibility = Visibility.Collapsed;
				}
				SessionAnalyticsService?.AddViewEvent("buttonPress", "shopping-cart-modal-add-item");
				TitleRollupProduct titleRollupProduct = kioskProduct as TitleRollupProduct;
				if ((titleRollupProduct?.ProductId ?? 0) == 1139)
				{
					BrowseFunctions.AddProductToCart(titleRollupProduct, isPurchase: true);
				}
				else
				{
					TitleRollupUtilities.ConfigureTitleRollupOverlay(browseItemModel, browseViewModelFromView.BrowseProductControlModel.TitleRollupOverlayModel, showDigitalCodeInfo: false, BrowseFunctions.IsPurchase);
				}
				break;
			}
			case KioskProductType.Collection:
				OrchestrationService.CreateStartViewAnalytics(AnalyticsSessionType.BrowseVend);
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
				browseControlModel.BrowseItems = CreateBrowseItemsFromKioskProducts(ApplicationState.Instance.BrowseProducts, BrowseState?.BrowseSelectedSort == "alpha", isADAMiniCart: true);
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
		browseViewModelFromView.BrowseProductControlModel.TitleRollupOverlayModel.Visibility = Visibility.Collapsed;
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
					displayProductModel.BannerText = ((flag && displayProductModel.Banner != null) ? Resources.ResourceManager.GetString(displayProductModel.Banner.TextKey) : null);
					displayProductModel.BannerBackgroundBrush = ((flag && displayProductModel.Banner != null) ? WPFFunctions.GetBrush(GetBrowseViewUserControl(), displayProductModel.Banner.BannerStyleName) : null);
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
		obj.MainHeight = ((obj.AdImage != null) ? 470 : 560);
		obj.ShowCollections = service?.ShowCollections ?? false;
		obj.CollectionTextLine1 = Resources.title_detail_collection_text;
		obj.BuyText = Resources.title_detail_buy;
		obj.TitleRollupOverlayButtonTextValues = new TitleRollupUtilities.TitleRollupOverlayButtonTextValues();
		obj.Add = Resources.add;
		obj.ComingSoonLabel = Resources.coming_label;
		obj.OutOfStockLine1Text = Resources.out_of_stock_line1;
		obj.OutOfStockLine2Text = Resources.out_of_stock_line2;
		obj.OutOfStockLine3Text = Resources.out_of_stock_line3;
		obj.Movies = Resources.Movies;
		obj.Games = Resources.Games;
		obj.FontMuseo20Style = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "font_museo_slab_900_20");
		obj.FontMuseo16Style = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "font_museo_slab_900_16");
		obj.PriceFor = Resources.price_for;
		obj.Night = Resources.night;
		obj.PricingService = ServiceLocator.Instance.GetService<IPricingService>();
		IRentalShoppingCart rentalShoppingCart = RentalShoppingCartService?.CurrentRentalShoppingCart;
		obj.IsInCartBrowse = rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel;
		return obj;
	}

	private static List<IBrowseItemModel> CreateBrowseItemsFromKioskProducts(List<IKioskProduct> kioskProducts, bool createAlphaGroups, bool isADAMiniCart)
	{
		DisplayGroupModel displayGroupModel = null;
		List<IBrowseItemModel> list = new List<IBrowseItemModel>();
		if (kioskProducts != null)
		{
			DisplayProductModelConstants displayProductModelConstants = GetDisplayProductModelConstants();
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			foreach (IKioskProduct kioskProduct in kioskProducts)
			{
				DisplayProductModel displayProductModel = new DisplayProductModel
				{
					Name = kioskProduct.ProductName,
					Data = kioskProduct,
					Image = OrchestrationService.GetProductBitmapImage(kioskProduct),
					LockAspectRatio = true
				};
				InitializeDisplayProductModel(displayProductModel, displayProductModelConstants);
				ConfigureDisplayProductModel(kioskProduct, displayProductModel, isADAMiniCart, displayProductModelConstants);
				if (createAlphaGroups)
				{
					string alphaGroupCharacter = GetAlphaGroupCharacter(kioskProduct as ITitleProduct);
					if (displayGroupModel == null || displayGroupModel.Name != alphaGroupCharacter)
					{
						displayGroupModel = new DisplayGroupModel
						{
							Name = alphaGroupCharacter,
							SeparatorLineStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "separator_line_for_group"),
							UnselectedTitleStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "browse_group_header"),
							SelectedTitleStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "browse_group_header_selected"),
							IsSelected = (alphaGroupCharacter == BrowseState.SelectedAlphaGroupName)
						};
						list.Add(displayGroupModel);
					}
					displayProductModel.SeparatorLineStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "separator_line_for_product");
					displayProductModel.TitleStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "browse_product_title");
				}
				list.Add(displayProductModel);
			}
			stopwatch.Stop();
			LogHelper.Instance.Log($"BrowseViewController Create Browse Items stopwatch: {stopwatch.Elapsed}");
		}
		return list;
	}

	private static string GetGenreText(List<long> genres)
	{
		List<string> genreStrings = new List<string>();
		genres?.ForEach(delegate(long x)
		{
			GenresProperties.Properties.TryGetValue((Genres)x, out var value);
			if (value != null)
			{
				genreStrings.Add(value.Description);
			}
		});
		return string.Join(", ", genreStrings);
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

	private static string GetPriceText(IKioskProduct kioskProduct, int pricingRecordIndex, DisplayProductModelConstants displayProductModelConstants)
	{
		List<string> priceText = new List<string>();
		((kioskProduct is TitleRollupProduct) ? ((TitleRollupProduct)kioskProduct).KioskProducts : ((kioskProduct is ITitleProduct) ? new List<ITitleProduct> { (ITitleProduct)kioskProduct } : null))?.ForEach(delegate(ITitleProduct tp)
		{
			IList<IPricingRecord> list = displayProductModelConstants.PricingService?.GetProductPricingRecords(tp.ProductId);
			if (tp.TitleType != TitleType.DigitalCode)
			{
				IPricingRecord pricingRecord = ((list != null && list.Count > pricingRecordIndex) ? list[pricingRecordIndex] : null);
				if (pricingRecord != null)
				{
					string arg = ((TitleTypeProperties.Properties.ContainsKey(tp.TitleType) && TitleTypeProperties.Properties[tp.TitleType].GetName(Resources.ResourceManager) != null) ? $" {displayProductModelConstants.PriceFor} {TitleTypeProperties.Properties[tp.TitleType].GetName(Resources.ResourceManager)}" : string.Empty);
					string arg2 = (((pricingRecord.InitialDays ?? 1) > 1) ? string.Format(" {0} {1} {2}{3}", displayProductModelConstants.PriceFor, pricingRecord.InitialDays ?? 1, displayProductModelConstants.Night, (pricingRecord.InitialDays > 1) ? "s" : string.Empty) : string.Empty);
					priceText.Add($"{pricingRecord.InitialNight:C}{arg}{arg2}");
				}
			}
		});
		return string.Join("\n", priceText);
	}

	private static string GetReturnTimeText(IKioskProduct kioskProduct, int pricingRecordIndex, DisplayProductModelConstants displayProductModelConstants)
	{
		List<string> returnTimeText = new List<string>();
		((kioskProduct is TitleRollupProduct) ? ((TitleRollupProduct)kioskProduct).KioskProducts : ((kioskProduct is ITitleProduct) ? new List<ITitleProduct> { (ITitleProduct)kioskProduct } : null))?.ForEach(delegate(ITitleProduct tp)
		{
			if (tp.TitleType != TitleType.DigitalCode)
			{
				IList<IPricingRecord> list = displayProductModelConstants.PricingService?.GetProductPricingRecords(tp.ProductId);
				IPricingRecord pricingRecord = ((list != null && list.Count > pricingRecordIndex) ? list[pricingRecordIndex] : null);
				if (pricingRecord != null)
				{
					DateTime dateTime = DateTime.Now.AddDays(pricingRecord.InitialDays ?? 1);
					string arg = Resources.ResourceManager.GetString(dateTime.DayOfWeek.ToString());
					string arg2 = ((pricingRecordIndex > 0 && TitleTypeProperties.Properties.ContainsKey(tp.TitleType) && TitleTypeProperties.Properties[tp.TitleType].GetName(Resources.ResourceManager) != null) ? $" {displayProductModelConstants.PriceFor} {TitleTypeProperties.Properties[tp.TitleType].GetName(Resources.ResourceManager)}" : string.Empty);
					returnTimeText.Add($"9pm {arg}, {dateTime.ToShortDateString()}{arg2}");
				}
			}
		});
		if (pricingRecordIndex <= 0)
		{
			if (returnTimeText.Count <= 0)
			{
				return string.Empty;
			}
			return returnTimeText[0];
		}
		return string.Join("\n", returnTimeText);
	}

	private static string GetHotDealText(IKioskProduct kioskProduct)
	{
		List<string> hotDealText = new List<string>();
		((kioskProduct is TitleRollupProduct) ? ((TitleRollupProduct)kioskProduct).KioskProducts : ((kioskProduct is ITitleProduct) ? new List<ITitleProduct> { (ITitleProduct)kioskProduct } : null))?.ForEach(delegate(ITitleProduct tp)
		{
			if (tp.TitleType != TitleType.DigitalCode)
			{
				(ServiceLocator.Instance.GetService<IPricingService>()?.GetProductPricingRecords(tp.ProductId))?.ForEach(delegate(IPricingRecord pr)
				{
					if (pr.IsHotDeal)
					{
						string arg = ((TitleTypeProperties.Properties.ContainsKey(tp.TitleType) && TitleTypeProperties.Properties[tp.TitleType].GetName(Resources.ResourceManager) != null) ? $" {TitleTypeProperties.Properties[tp.TitleType].GetName(Resources.ResourceManager)}" : string.Empty);
						hotDealText.Add($"{pr.ExtraNight:C}{arg}");
					}
				});
			}
		});
		return string.Format(Resources.hot_deal_text, string.Join(", ", hotDealText));
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

	private static void InitializeDisplayProductModel(DisplayProductModel browseItemModel, DisplayProductModelConstants displayProductModelConstants)
	{
		ITitleProduct titleProduct = browseItemModel?.Data as ITitleProduct;
		RatingsProperty value = null;
		RatingsProperties.Properties.TryGetValue((Ratings)(titleProduct?.RatingId ?? 0), out value);
		TitleTypeProperty value2 = null;
		string format = null;
		if (titleProduct != null)
		{
			_ = titleProduct.TitleType;
			TitleTypeProperties.Properties.TryGetValue(titleProduct.TitleType, out value2);
			format = value2?.GetName(Resources.ResourceManager);
		}
		string ratingLabel = string.Empty;
		string ratingDescription = string.Empty;
		Visibility ratingVisibility = Visibility.Visible;
		if (value != null && !titleProduct.IsEmptyCase)
		{
			if (titleProduct.TitleFamily == TitleFamily.Games)
			{
				ratingLabel = "ESRB Rating: ";
				ratingDescription = $"{value.Name} - {value.Description}";
			}
			else if (titleProduct.Genres.Contains(1017L))
			{
				ratingLabel = "TV Rating: ";
				ratingDescription = value.Name;
			}
			else
			{
				ratingLabel = "MPAA Rating: ";
				ratingDescription = ((value.Code == Ratings.NR.ToString()) ? "Not rated by the MPAA" : value.Name);
			}
		}
		else
		{
			ratingVisibility = Visibility.Collapsed;
		}
		Visibility showCollections = ((titleProduct == null || titleProduct.ComingSoon || !displayProductModelConstants.ShowCollections || ((!(titleProduct?.Collections?.Any())) ?? true)) ? Visibility.Collapsed : Visibility.Visible);
		bool flag = titleProduct is TitleRollupProduct && titleProduct.KioskProductType == KioskProductType.TitleRollup;
		IList<IPricingRecord> list = displayProductModelConstants.PricingService?.GetProductPricingRecords(titleProduct.ProductId);
		bool flag2 = list != null && list.Count > 1;
		decimal num = (titleProduct.IsEmptyCase ? displayProductModelConstants.PricingService.GetEmptyCasePrice() : titleProduct.PurchasePrice);
		string format2 = ((num - decimal.Round(num) != 0m) ? "${0:F2}" : "${0:F0}");
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		bool flag3 = rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel;
		browseItemModel.Header = titleProduct.ProductName;
		browseItemModel.Description = titleProduct.ProductDescription;
		browseItemModel.Rating = value?.Name;
		browseItemModel.Format = format;
		browseItemModel.RunningTime = titleProduct?.RunningTime;
		browseItemModel.ReleaseYear = titleProduct?.NationalStreetDate?.Year.ToString();
		browseItemModel.CCSupported = ((titleProduct == null || titleProduct.IsEmptyCase || titleProduct == null || !titleProduct.IsMovie || titleProduct == null || !titleProduct.ClosedCaptioned) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.Starring = string.Join(", ", titleProduct?.Starring ?? new List<string>());
		browseItemModel.StarringLabel = displayProductModelConstants?.StarringLabel;
		browseItemModel.Genres = GetGenreText(titleProduct?.Genres);
		browseItemModel.GenresLabel = displayProductModelConstants?.GenresLabel;
		browseItemModel.DirectedBy = string.Join(", ", titleProduct?.Directors ?? new List<string>());
		browseItemModel.OneNightPrice = GetPriceText(titleProduct, 0, displayProductModelConstants);
		browseItemModel.OneNightPriceLabel = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? displayProductModelConstants.OneNightPriceLabel : null);
		browseItemModel.MultiNightPrice = GetPriceText(titleProduct, 1, displayProductModelConstants);
		browseItemModel.MultiNightPriceLabel = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? displayProductModelConstants.MultiNightPriceLabel : null);
		browseItemModel.MultiNightPriceVisibility = ((!flag2 || flag3 || titleProduct.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.RatingLabel = ratingLabel;
		browseItemModel.RatingDescription = ratingDescription;
		browseItemModel.RatingVisibility = ratingVisibility;
		browseItemModel.OneNightReturnTime = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? GetReturnTimeText(titleProduct, 0, displayProductModelConstants) : null);
		browseItemModel.OneNightReturnTimeLabel = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? displayProductModelConstants.OneNightReturnTimeLabel : null);
		browseItemModel.MultiNightReturnTime = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? GetReturnTimeText(titleProduct, 1, displayProductModelConstants) : null);
		browseItemModel.MultiNightReturnTimeLabel = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? displayProductModelConstants.MultiNightReturnTimeLabel : null);
		browseItemModel.MultiNightReturnTimeVisibility = ((!flag2 || flag3 || titleProduct.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.HotDealText = GetHotDealText(titleProduct);
		browseItemModel.HotDealTextVisibility = ((!titleProduct.HasHotDeal) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.RunningTimeVisibility = ((titleProduct == null || titleProduct.IsEmptyCase || titleProduct == null || !titleProduct.IsMovie || string.IsNullOrEmpty(titleProduct?.RunningTime)) ? Visibility.Collapsed : Visibility.Visible);
		int releaseYearVisibility;
		if (titleProduct != null && !titleProduct.IsEmptyCase && titleProduct != null)
		{
			DateTime? nationalStreetDate = titleProduct.NationalStreetDate;
			if (nationalStreetDate.HasValue)
			{
				_ = nationalStreetDate.GetValueOrDefault().Year;
				if (true)
				{
					releaseYearVisibility = 0;
					goto IL_0522;
				}
			}
		}
		releaseYearVisibility = 2;
		goto IL_0522;
		IL_0522:
		browseItemModel.ReleaseYearVisibility = (Visibility)releaseYearVisibility;
		browseItemModel.FormatVisibility = ((titleProduct == null || titleProduct.IsEmptyCase || titleProduct == null || !titleProduct.IsGame) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.AdImageVisibility = ((displayProductModelConstants.AdImage == null) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.AdImage = displayProductModelConstants.AdImage;
		browseItemModel.AdHeight = displayProductModelConstants.AdHeight;
		browseItemModel.MainHeight = displayProductModelConstants.MainHeight;
		browseItemModel.NotEmptyCaseVisibility = ((titleProduct != null && titleProduct.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.SubPlatformImage = ((titleProduct != null && titleProduct.IsGame) ? OrchestrationService.GetSubPlatformBitmapImage(titleProduct) : null);
		browseItemModel.SubPlatformVisibility = ((titleProduct == null || !titleProduct.IsGame || titleProduct == null || !titleProduct.SubPlatformId.HasValue) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.ShowCollections = showCollections;
		browseItemModel.CollectionsEnabled = titleProduct?.Collections?.Any() ?? false;
		browseItemModel.CollectionTextLine1 = displayProductModelConstants.CollectionTextLine1;
		browseItemModel.BuyText = displayProductModelConstants.BuyText;
		browseItemModel.BuyButtonTextStyle = (flag ? displayProductModelConstants.FontMuseo20Style : displayProductModelConstants.FontMuseo16Style);
		browseItemModel.BuyPriceText = string.Format(format2, num);
		browseItemModel.BuyPriceVisibility = ((flag && !titleProduct.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.StarringVisibility = ((titleProduct == null || !titleProduct.IsMovie || titleProduct == null || titleProduct.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.ComingSoonLabelText = displayProductModelConstants.ComingSoonLabel + " " + ((titleProduct != null && titleProduct.ReleaseDate.HasValue) ? titleProduct.ReleaseDate.Value.ToString("MM/dd") : string.Empty);
		browseItemModel.OutOfStockLine1Text = displayProductModelConstants.OutOfStockLine1Text;
		browseItemModel.OutOfStockLine2Text = string.Format(displayProductModelConstants.OutOfStockLine2Text, (titleProduct != null && titleProduct.IsMovie) ? displayProductModelConstants.Movies : displayProductModelConstants.Games);
		browseItemModel.OutOfStockLine3Text = displayProductModelConstants.OutOfStockLine3Text;
	}

	private static void ConfigureDisplayProductModel(IKioskProduct eachKioskProduct, DisplayProductModel browseItemModel, bool isADAMiniCart, DisplayProductModelConstants displayProductModelConstants)
	{
		ITitleProduct titleProduct = eachKioskProduct as ITitleProduct;
		bool isEmptyCase = titleProduct?.IsEmptyCase ?? false;
		BannerType bannerType = BannerType.None;
		browseItemModel.ComingSoonButtonVisibility = Visibility.Collapsed;
		if (BrowseState.BrowseSelectedSort != "alpha")
		{
			ITitleProduct titleProduct2 = titleProduct;
			if (titleProduct2 != null && titleProduct2.ComingSoon)
			{
				bannerType = BannerType.ComingSoon;
				string text = null;
				if ((RentalShoppingCartService?.CurrentRentalShoppingCart?.ComingSoonOptInInfo?.OptInItems)?.FirstOrDefault((IOptInItem x) => x.ItemId == (titleProduct?.ProductId ?? 0)) != null)
				{
					text = $"{Resources.email_optin_reminder_set_button_text}";
					browseItemModel.ComingSoonButtonStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "classic_green_button_style");
					browseItemModel.ComingSoonButtonCheckMarkVisibility = Visibility.Visible;
				}
				else
				{
					text = $"{Resources.email_optin_button_text}";
					browseItemModel.ComingSoonButtonStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "rubine_white_border_button_style");
					browseItemModel.ComingSoonButtonCheckMarkVisibility = Visibility.Collapsed;
				}
				browseItemModel.ComingSoonButtonText = text;
				browseItemModel.ComingSoonButtonVisibility = Visibility.Visible;
			}
			else
			{
				bannerType = BrowseFunctions.GetProductBannerType(titleProduct);
			}
		}
		bool flag = IsProductSelected(titleProduct);
		IBrowseState browseState = BrowseState;
		if (browseState == null || browseState.BrowseMode != BrowseViewFilter.ComingSoon)
		{
			IRentalShoppingCartService rentalShoppingCartService = RentalShoppingCartService;
			if ((((rentalShoppingCartService != null && rentalShoppingCartService.CurrentRentalShoppingCartIsFull) || !titleProduct.IsAvailable) | flag) || !titleProduct.InStock)
			{
				if (BrowseState?.BrowseSelectedSort == "alpha")
				{
					browseItemModel.ImageOpacity = 0.2;
				}
				else
				{
					browseItemModel.ImageOpacity = 0.4;
				}
				goto IL_01c7;
			}
		}
		browseItemModel.ImageOpacity = 1.0;
		goto IL_01c7;
		IL_01c7:
		BrowseFunctions.ConfigureBanner(browseItemModel, titleProduct, isADAMiniCart, bannerType, GetBrowseViewUserControl());
		ProductFlags browseProductFlag = GetBrowseProductFlag(titleProduct, isEmptyCase, flag);
		BrowseFunctions.ConfigureProductFlag(browseItemModel, browseProductFlag, isADAMiniCart, GetBrowseViewUserControl());
		int num;
		if (!IsShoppingCartFull)
		{
			ITitleProduct titleProduct3 = titleProduct;
			if (titleProduct3 != null && titleProduct3.IsAvailable && !flag)
			{
				num = ((titleProduct?.InStock ?? false) ? 1 : 0);
				goto IL_022a;
			}
		}
		num = 0;
		goto IL_022a;
		IL_022a:
		bool flag2 = (byte)num != 0;
		browseItemModel.CanAdd = flag2 || BrowseState.BrowseMode == BrowseViewFilter.ComingSoon;
		Visibility visibility = ((!flag2 && !IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.CornerAddButtonVisibility = ((!isADAMiniCart) ? visibility : Visibility.Collapsed);
		browseItemModel.RightAddButtonVisibility = ((visibility == Visibility.Collapsed) ? Visibility.Hidden : visibility);
		browseItemModel.ADAMiniCartAddButtonVisibility = ((!isADAMiniCart) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.AddButtonPlusSignVisibility = (IsADAMode ? Visibility.Hidden : Visibility.Visible);
		browseItemModel.AddButtonTextVisibility = ((!IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.AddButtonStyle = (IsADAMode ? WPFFunctions.GetStyle(GetBrowseViewUserControl(), "rubine_white_border_button_style") : WPFFunctions.GetStyle(GetBrowseViewUserControl(), "classic_rubine_button_style"));
		if (isADAMiniCart)
		{
			browseItemModel.ConfigureForADAMiniCart();
		}
		else if (BrowseState.BrowseSelectedSort == "alpha")
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
		IKioskProduct kioskProduct2;
		if (!flag3)
		{
			IKioskProduct kioskProduct = ((titleProduct.TitleType != TitleType.Bluray) ? titleProduct : null);
			kioskProduct2 = kioskProduct;
		}
		else
		{
			kioskProduct2 = ((TitleRollupProduct)titleProduct).PrimaryProduct;
		}
		ITitleProduct titleProduct4 = (ITitleProduct)kioskProduct2;
		ITitleProduct titleProduct5 = (flag3 ? ((TitleRollupProduct)titleProduct).KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.Bluray) : ((titleProduct.TitleType == TitleType.Bluray) ? titleProduct : null));
		if (titleProduct4 != null && titleProduct5 != null && titleProduct4.ProductId == titleProduct5.ProductId)
		{
			titleProduct4 = null;
		}
		TitleRollupOverlayButtonProperties titleRollupOverlayButtonProperties = ((titleProduct4 != null && !titleProduct4.IsEmptyCase) ? TitleRollupUtilities.GetTitleRollupOverlayButtonProperties(titleProduct4, isPurchase: false, displayProductModelConstants.TitleRollupOverlayButtonTextValues) : null);
		TitleRollupOverlayButtonProperties titleRollupOverlayButtonProperties2 = ((titleProduct5 != null && !titleProduct5.IsEmptyCase) ? TitleRollupUtilities.GetTitleRollupOverlayButtonProperties(titleProduct5, isPurchase: false, displayProductModelConstants.TitleRollupOverlayButtonTextValues) : null);
		browseItemModel.ShowPrimary = titleRollupOverlayButtonProperties?.Visibility ?? Visibility.Collapsed;
		browseItemModel.ShowBluRay = titleRollupOverlayButtonProperties2?.Visibility ?? Visibility.Collapsed;
		browseItemModel.PrimaryEnabled = titleRollupOverlayButtonProperties?.Enabled ?? false;
		browseItemModel.BluRayEnabled = titleRollupOverlayButtonProperties2?.Enabled ?? false;
		string text2 = ((!((titleProduct?.IsMovie ?? false) & flag3) || titleProduct.IsEmptyCase) ? displayProductModelConstants.Add : (titleRollupOverlayButtonProperties?.ButtonText ?? string.Empty));
		bool flag4 = text2 != null && text2.Split('\n').Count() > 1;
		string primaryText = (flag4 ? text2.Split('\n')[0] : text2);
		string primaryLine2Text = (flag4 ? text2.Split('\n')[1] : text2);
		bool flag5 = (titleRollupOverlayButtonProperties2?.ButtonText?.Split('\n').Count() ?? 0) > 1;
		string bluRayText = (flag5 ? titleRollupOverlayButtonProperties2.ButtonText.Split('\n')[0] : titleRollupOverlayButtonProperties2?.ButtonText);
		string bluRayLine2Text = (flag5 ? titleRollupOverlayButtonProperties2.ButtonText.Split('\n')[1] : titleRollupOverlayButtonProperties2?.ButtonText);
		browseItemModel.PrimaryOutOfStockVisibility = ((!flag4) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.BluRayOutOfStockVisibility = ((!flag5) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.PrimaryText = primaryText;
		browseItemModel.PrimaryLine2Text = primaryLine2Text;
		browseItemModel.BluRayText = bluRayText;
		browseItemModel.BluRayLine2Text = bluRayLine2Text;
		int addButtonBarVisibility;
		if (!IsShoppingCartFull)
		{
			ITitleProduct titleProduct6 = titleProduct;
			if (titleProduct6 != null && !titleProduct6.ComingSoon)
			{
				ITitleProduct titleProduct7 = titleProduct;
				if (titleProduct7 != null && titleProduct7.InStock)
				{
					addButtonBarVisibility = 0;
					goto IL_06b1;
				}
			}
		}
		addButtonBarVisibility = 2;
		goto IL_06b1;
		IL_06b1:
		browseItemModel.AddButtonBarVisibility = (Visibility)addButtonBarVisibility;
		ITitleProduct titleProduct8 = titleProduct;
		browseItemModel.OutOfStockVisibility = ((titleProduct8 == null || titleProduct8.InStock || titleProduct.ComingSoon) ? Visibility.Collapsed : Visibility.Visible);
		ITitleProduct titleProduct9 = titleProduct;
		browseItemModel.ComingSoonVisibility = ((titleProduct9 == null || !titleProduct9.ComingSoon) ? Visibility.Collapsed : Visibility.Visible);
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
				ButtonType = BrowseFilterType.BluRay,
				Name = "blu_ray_button",
				Height = height,
				Width = width,
				IsSelected = (browseState.BrowseSelectedSort == "Blu-ray")
			});
			if (IsDigitalCodeButtonEnabled && !IsADAMode)
			{
				list.Add(new BrowseFilter
				{
					ButtonType = BrowseFilterType.DigitalCode,
					Name = "digital_code_button",
					Height = height,
					Width = 112,
					IsSelected = (browseState.BrowseSelectedSort == "Digital Code")
				});
			}
		}
		list.Add(new BrowseFilter
		{
			ButtonType = BrowseFilterType.AllNewest,
			Name = "newest_button",
			Height = height,
			Width = width,
			IsSelected = (browseState.BrowseSelectedSort == "Newest")
		});
		if (browseState.TitleFamily == TitleFamily.Movies)
		{
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.AtoZ,
				Name = "abc_button",
				Height = height,
				Width = width,
				IsSelected = (browseState.BrowseSelectedSort == "alpha")
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
					IsSelected = (browseState.BrowseSelectedSort == "PS4")
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
					IsSelected = (browseState.BrowseSelectedSort == "Xbox One")
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
					IsSelected = (browseState.BrowseSelectedSort == "Nintendo Switch")
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
					IsSelected = (browseState.BrowseSelectedSort == "Xbox 360")
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
				IsSelected = (browseState.BrowseSelectedSort == "for-sale")
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
				IsSelected = (browseState.BrowseSelectedSort == "Top 20" || (browseState.BrowseSelectedSort == "Newest" && IsADAMode && browseState.Genre == Genres.Top20Movies))
			});
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.Action,
				Name = "action_filter_button",
				Height = height,
				Width = 77,
				IsSelected = (browseState.BrowseSelectedSort == "Action" || (browseState.BrowseSelectedSort == "Newest" && IsADAMode && browseState.Genre == Genres.ActionAndAdventure))
			});
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.Drama,
				Name = "drama_filter_button",
				Height = height,
				Width = 77,
				IsSelected = (browseState.BrowseSelectedSort == "Drama" || (browseState.BrowseSelectedSort == "Newest" && IsADAMode && browseState.Genre == Genres.Drama))
			});
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.Horror,
				Name = "horror_filter_button",
				Height = height,
				Width = 77,
				IsSelected = (browseState.BrowseSelectedSort == "Horror" || (browseState.BrowseSelectedSort == "Newest" && IsADAMode && browseState.Genre == Genres.Horror))
			});
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.Comedy,
				Name = "comedy_filter_button",
				Height = height,
				Width = 77,
				IsSelected = (browseState.BrowseSelectedSort == "Comedy" || (browseState.BrowseSelectedSort == "Newest" && IsADAMode && browseState.Genre == Genres.Comedy))
			});
			list.Add(new BrowseFilter
			{
				ButtonType = BrowseFilterType.Family,
				Name = "family_filter_button",
				Height = height,
				Width = 77,
				IsSelected = (browseState.BrowseSelectedSort == "Family" || (browseState.BrowseSelectedSort == "Newest" && IsADAMode && browseState.Genre == Genres.FamilyMovies))
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
				IsSelected = (browseState.BrowseSelectedSort == "coming-soon")
			});
		}
		foreach (BrowseFilter item in list)
		{
			item.Text = Resources.ResourceManager.GetString(item.Name);
		}
		return list;
	}
}
