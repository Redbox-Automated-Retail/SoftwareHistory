using System;
using System.Collections.Generic;
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

namespace Redbox.Rental.Services.Controllers;

public class BrowseViewController
{
	private class DisplayProductModelConstants
	{
		public string StarringLabel { get; set; }

		public string GenresLabel { get; set; }

		public string ADay { get; set; }

		public string PriceLabel { get; set; }

		public string ReturnTime { get; set; }

		public string ReturnTimeLabel { get; set; }

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

		public Style FontTargetSmallMediumStyle { get; set; }

		public Style FontTargetMediumSmallStyle { get; set; }
	}

	private static RentalShoppingCartChanged ShoppingCartChangeEventHandler;

	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance.BrowseState;

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
			(IdleTimerService?.GetIdleTimer(BrowseFunctions.Constants.IdleTimerName))?.Start();
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
				topMenuModel.MenuBackgroundText = BundleFunctions.GetDictionaryStringValue("strings", (BrowseState.TitleFamily == TitleFamily.Games) ? "browse_family_games" : "browse_family_movies");
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
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			BrowseState.Genre = null;
			if (BrowseState.TitleFamily == TitleFamily.Games)
			{
				service["Browse:ProductFamily"] = BundleFunctions.GetDictionaryStringValue("strings", "browse_family_games");
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
			else
			{
				service["Browse:ProductFamily"] = BundleFunctions.GetDictionaryStringValue("strings", "browse_family_movies");
				if (BrowseState.BrowseSelectedSort == "Xbox 360" || BrowseState.BrowseSelectedSort == "Xbox One" || BrowseState.BrowseSelectedSort == "PS2" || BrowseState.BrowseSelectedSort == "PS3" || BrowseState.BrowseSelectedSort == "PS4" || BrowseState.BrowseSelectedSort == "NintendoDS" || BrowseState.BrowseSelectedSort == "Wii" || BrowseState.BrowseSelectedSort == "Wii U" || BrowseState.BrowseSelectedSort == "Nintendo Switch" || BrowseState.BrowseSelectedSort == null)
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
					Text = BundleFunctions.GetDictionaryStringValue("strings", "Collections"),
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
		string key = ((productFamily == TitleFamily.Movies) ? "To_Movies" : "To_Games");
		BrowseProductFamilyMenuButton browseProductFamilyMenuButton = new BrowseProductFamilyMenuButton
		{
			Text = BundleFunctions.GetDictionaryStringValue("strings", key),
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
		string key = (genre.HasValue ? $"genre_{(int)genre.Value}" : ((BrowseState.TitleFamily == TitleFamily.Movies) ? "Movies_ALL" : "Games_ALL"));
		obj.Text = BundleFunctions.GetDictionaryStringValue("strings", key);
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
			OrchestrationService.ExecuteScript("view_shopping_cart");
		};
		browseViewModel.OnToggleTitleFamilyButtonClicked = delegate
		{
			BrowseState.TitleFamily = ((BrowseState.TitleFamily != TitleFamily.Movies) ? TitleFamily.Movies : TitleFamily.Games);
			BrowseState.TitleType = TitleType.All;
			BrowseState.BrowseSelectedSort = "Newest";
			BrowseState.Genre = null;
			BrowseState.PageNumber = 1;
			ConfigureBrowseView();
		};
		string checkOutButtonText = null;
		Style style = null;
		int num = RentalShoppingCartService?.CurrentRentalShoppingCart?.Count ?? 0;
		if (ShowNoThanks())
		{
			style = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "classic_gray_button_style");
			checkOutButtonText = BundleFunctions.GetDictionaryStringValue("strings", "no_thanks_button");
		}
		else
		{
			style = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "classic_orange_button_style");
			string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", "check_out_button_text");
			if (dictionaryStringValue.Contains("{0}"))
			{
				checkOutButtonText = string.Format(dictionaryStringValue, num);
			}
		}
		browseViewModel.IsADAAccessible = ApplicationState.Instance.IsADAAccessible;
		browseViewModel.CheckOutButtonText = checkOutButtonText;
		browseViewModel.CheckOutButtonStyle = style;
		browseViewModel.SwitchProductFamilyButtonVisibility = ((!ApplicationState.Instance.IsADAAccessible) ? Visibility.Collapsed : Visibility.Visible);
		browseViewModel.SwitchProductFamilyButtonText = BundleFunctions.GetDictionaryStringValue("strings", (BrowseState.TitleFamily == TitleFamily.Movies) ? "To_Games" : "To_Movies");
		browseViewModel.AddToCartLabelVisibility = ((num != 0 || ApplicationState.Instance.IsADAAccessible) ? Visibility.Collapsed : Visibility.Visible);
		browseViewModel.AddToCartLabel1Text = BundleFunctions.GetDictionaryStringValue("strings", "add_to_cart_label1");
		browseViewModel.AddToCartLabel2Text = BundleFunctions.GetDictionaryStringValue("strings", "add_to_cart_label2");
		browseViewModel.AddToADACartLabelText = BundleFunctions.GetDictionaryStringValue("strings", "add_to_cart_ada_label");
		browseViewModel.AddToADACartLabelVisibility = ((!IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
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
		filterBarModel.StartOverButtonVisibility = ((BrowseState.BrowseCollections || BrowseState.BrowseCollectionProduct != null || InTitleMarketingMode) ? Visibility.Collapsed : Visibility.Visible);
		filterBarModel.StartOverButtonText = BundleFunctions.GetDictionaryStringValue("strings", "start_over_button");
		filterBarModel.StartOverButtonStyle = WPFFunctions.GetStyle(browseViewUserControl, OrchestrationService.IsCultureSpanish() ? "spanish_start_over_button_style" : "english_start_over_button_style");
		filterBarModel.BackButtonVisibility = ((!BrowseState.BrowseCollections && BrowseState.BrowseCollectionProduct == null) ? Visibility.Collapsed : Visibility.Visible);
		filterBarModel.BackButtonText = BundleFunctions.GetDictionaryStringValue("strings", "back_button");
		filterBarModel.MoreCollectionsButtonVisibility = ((BrowseState.BrowseCollectionProduct == null) ? Visibility.Collapsed : Visibility.Visible);
		filterBarModel.MoreCollectionsButtonText = BundleFunctions.GetDictionaryStringValue("strings", "more_collections");
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
		browseControlModel.NoResultsText = BundleFunctions.GetDictionaryStringValue("strings", (browseState != null && browseState.TitleFamily == TitleFamily.Movies) ? "filter_movies_empty" : "filter_games_empty");
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
				browseProductControl.BrowseItemMargin = new Thickness(8.0, 36.0, 8.0, 0.0);
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
					Promotion promotion = PromotionService.Instance.GetPromotion();
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
			browseProductControl.BrowseControls.Add(new DisplayProductUserControl
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
			BrowseFunctions.AddProductToCart(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.Bluray), BrowseFunctions.IsPurchase);
		}
	}

	private static void TitleRollupOverlayModel_OnTitleRollupOverlayAddDVD(IBrowseItemModel browseItemModel, object parameter)
	{
		if (browseItemModel?.Data is TitleRollupProduct titleRollupProduct)
		{
			BrowseFunctions.AddProductToCart(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.DVD), BrowseFunctions.IsPurchase);
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
				BrowseFunctions.AddProductToCart(kioskProduct as ITitleProduct, BrowseFunctions.IsPurchase);
				break;
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
					BrowseFunctions.AddProductToCart(titleRollupProduct, BrowseFunctions.IsPurchase);
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
		IComingSoonOptinInfo comingSoonOptinInfo = RentalShoppingCartService?.CurrentRentalShoppingCart?.ComingSoonOptInInfo;
		if (titleProduct == null || comingSoonOptinInfo == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(comingSoonOptinInfo.EmailAddress))
		{
			if (comingSoonOptinInfo.OptInItems?.FirstOrDefault((IOptInItem x) => x.ItemId == titleProduct.ProductId) == null)
			{
				comingSoonOptinInfo.AddProduct(titleProduct.ProductId);
			}
			else
			{
				comingSoonOptinInfo.RemoveProduct(titleProduct.ProductId);
			}
			if (browseItemModel is DisplayProductModel browseItemModel2)
			{
				ConfigureDisplayProductModel(kioskProduct, browseItemModel2, isADAMiniCart: false, GetDisplayProductModelConstants());
			}
		}
		else
		{
			OrchestrationService.InvokeGetEmailAddressAndRegisterEmailReminderCallback(titleProduct.ProductId);
		}
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
				browseControlModel.BrowseControls.Add(new DisplayProductUserControl
				{
					Name = $"minicart_product_{i + 1}",
					ShowVacantItemBackground = !ApplicationState.Instance.IsADAAccessible,
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
					displayProductModel.CancelButtonVisibility = ((InTitleMarketingMode && item.Sequence == RentalShoppingCartItemSequence.pre_offer) ? Visibility.Collapsed : Visibility.Visible);
					bool flag = item.Action == RentalShoppingCartItemAction.purchase;
					displayProductModel.Banner = (flag ? BannerCollection.Instance.FirstOrDefault((Banner x) => x.BannerType == BannerType.Purchase) : null);
					displayProductModel.BannerVisibility = ((displayProductModel.Banner == null) ? Visibility.Collapsed : Visibility.Visible);
					displayProductModel.BannerText = ((flag && displayProductModel.Banner != null) ? BundleFunctions.GetDictionaryStringValue("strings", displayProductModel.Banner.TextKey) : null);
					displayProductModel.BannerBackgroundBrush = ((flag && displayProductModel.Banner != null) ? GetBrush(GetBrowseViewUserControl(), displayProductModel.Banner.BannerStyleName) : null);
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
			StarringLabel = BundleFunctions.GetDictionaryStringValue("strings", "starring_label"),
			GenresLabel = BundleFunctions.GetDictionaryStringValue("strings", "genres_label"),
			ADay = BundleFunctions.GetDictionaryStringValue("strings", "price_a_day"),
			PriceLabel = BundleFunctions.GetDictionaryStringValue("strings", "price_tag"),
			ReturnTime = ReturnTime(),
			ReturnTimeLabel = BundleFunctions.GetDictionaryStringValue("strings", "return_time_label"),
			AdImage = GetAdImage()
		};
		obj.AdHeight = ((obj.AdImage != null) ? 115 : 20);
		obj.MainHeight = ((obj.AdImage != null) ? 490 : 590);
		obj.ShowCollections = service?.ShowCollections ?? false;
		obj.CollectionTextLine1 = BundleFunctions.GetDictionaryStringValue("strings", "title_detail_collection_text");
		obj.BuyText = BundleFunctions.GetDictionaryStringValue("strings", "title_detail_buy");
		obj.TitleRollupOverlayButtonTextValues = new TitleRollupUtilities.TitleRollupOverlayButtonTextValues();
		obj.Add = BundleFunctions.GetDictionaryStringValue("strings", "add");
		obj.ComingSoonLabel = BundleFunctions.GetDictionaryStringValue("strings", "coming_label");
		obj.OutOfStockLine1Text = BundleFunctions.GetDictionaryStringValue("strings", "out_of_stock_line1");
		obj.OutOfStockLine2Text = BundleFunctions.GetDictionaryStringValue("strings", "out_of_stock_line2");
		obj.OutOfStockLine3Text = BundleFunctions.GetDictionaryStringValue("strings", "out_of_stock_line3");
		obj.Movies = BundleFunctions.GetDictionaryStringValue("strings", "Movies");
		obj.Games = BundleFunctions.GetDictionaryStringValue("strings", "Games");
		obj.FontTargetMediumSmallStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "font_target_medium_small");
		obj.FontTargetSmallMediumStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "font_target_small_medium");
		return obj;
	}

	private static List<IBrowseItemModel> CreateBrowseItemsFromKioskProducts(List<IKioskProduct> kioskProducts, bool createAlphaGroups, bool isADAMiniCart)
	{
		DisplayGroupModel displayGroupModel = null;
		List<IBrowseItemModel> list = new List<IBrowseItemModel>();
		if (kioskProducts != null)
		{
			DisplayProductModelConstants displayProductModelConstants = GetDisplayProductModelConstants();
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

	private static string PriceADay(IKioskProduct product, DisplayProductModelConstants displayProductModelConstants)
	{
		string result = string.Empty;
		IPricingService pricingService = ServiceLocator.Instance.GetService<IPricingService>();
		if (product != null && pricingService != null)
		{
			string aDay = displayProductModelConstants.ADay;
			TitleRollupProduct titleRollup = product as TitleRollupProduct;
			List<ITitleProduct> list = titleRollup?.KioskProducts;
			if (list == null && product is ITitleProduct)
			{
				list = new List<ITitleProduct> { (ITitleProduct)product };
			}
			List<string> prices = new List<string>();
			list?.ForEach(delegate(ITitleProduct x)
			{
				IPricingRecord productPricingRecord = pricingService.GetProductPricingRecord(x.ProductId);
				if (productPricingRecord != null)
				{
					string text = ((titleRollup != null && TitleTypeProperties.Properties.ContainsKey(x.TitleType)) ? TitleTypeProperties.Properties[x.TitleType].Name : string.Empty);
					if (((ITitleProduct)product).IsEmptyCase)
					{
						prices.Add("$" + pricingService.GetEmptyCasePrice());
					}
					else
					{
						prices.Add("$" + productPricingRecord.InitialNight + " " + aDay + " " + text);
					}
				}
			});
			result = string.Join(", ", prices);
		}
		return result;
	}

	public static string ReturnTime()
	{
		_ = string.Empty;
		DateTime dateTime = DateTime.Now.AddDays(1.0);
		string text = dateTime.DayOfWeek.ToString();
		if (OrchestrationService.IsCultureSpanish())
		{
			text = BundleFunctions.GetDictionaryStringValue("strings", text);
		}
		return $"9pm {text}, {dateTime.ToShortDateString()}";
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
			format = value2?.Name;
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
				ratingDescription = value.Name;
			}
		}
		else
		{
			ratingVisibility = Visibility.Collapsed;
		}
		Visibility showCollections = ((titleProduct == null || titleProduct.ComingSoon || !displayProductModelConstants.ShowCollections || ((!(titleProduct?.Collections?.Any())) ?? true)) ? Visibility.Collapsed : Visibility.Visible);
		bool flag = titleProduct is TitleRollupProduct && titleProduct.KioskProductType == KioskProductType.TitleRollup;
		IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
		decimal num = (titleProduct.IsEmptyCase ? service.GetEmptyCasePrice() : ((titleProduct.TitleFamily == TitleFamily.Movies) ? titleProduct.MoviePurchasePrice : titleProduct.GamePurchasePrice));
		string format2 = ((num - decimal.Round(num) != 0m) ? "${0:F2}" : "${0:F0}");
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
		browseItemModel.PriceADay = PriceADay(titleProduct, displayProductModelConstants);
		browseItemModel.PriceLabel = displayProductModelConstants.PriceLabel;
		browseItemModel.RatingLabel = ratingLabel;
		browseItemModel.RatingDescription = ratingDescription;
		browseItemModel.RatingVisibility = ratingVisibility;
		browseItemModel.ReturnTime = displayProductModelConstants.ReturnTime;
		browseItemModel.ReturnTimeLabel = displayProductModelConstants.ReturnTimeLabel;
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
					goto IL_03ae;
				}
			}
		}
		releaseYearVisibility = 2;
		goto IL_03ae;
		IL_03ae:
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
		browseItemModel.BuyButtonHeight = ((flag && !titleProduct.IsEmptyCase) ? 88 : 94);
		browseItemModel.BuyButtonTextStyle = ((!flag) ? displayProductModelConstants.FontTargetSmallMediumStyle : (OrchestrationService.IsCultureSpanish() ? displayProductModelConstants.FontTargetSmallMediumStyle : displayProductModelConstants.FontTargetMediumSmallStyle));
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
		bool flag = titleProduct?.IsEmptyCase ?? false;
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
					text = string.Format("{0}", BundleFunctions.GetDictionaryStringValue("strings", "email_optin_reminder_set_button_text"));
					browseItemModel.ComingSoonButtonStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "classic_green_button_style");
					browseItemModel.ComingSoonButtonCheckMarkVisibility = Visibility.Visible;
				}
				else
				{
					text = string.Format("{0}", BundleFunctions.GetDictionaryStringValue("strings", "email_optin_button_text"));
					browseItemModel.ComingSoonButtonStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), "rubine_white_border_button_style");
					browseItemModel.ComingSoonButtonCheckMarkVisibility = Visibility.Collapsed;
				}
				browseItemModel.ComingSoonButtonText = text;
				browseItemModel.ComingSoonButtonVisibility = Visibility.Visible;
			}
			else if (titleProduct.TitleType == TitleType.Bluray)
			{
				bannerType = BannerType.Bluray;
			}
			else if (titleProduct.TitleType == TitleType.DigitalCode)
			{
				bannerType = BannerType.DigitalCode;
			}
		}
		bool flag2 = IsProductSelected(titleProduct);
		IBrowseState browseState = BrowseState;
		if (browseState == null || browseState.BrowseMode != BrowseViewFilter.ComingSoon)
		{
			IRentalShoppingCartService rentalShoppingCartService = RentalShoppingCartService;
			if ((rentalShoppingCartService != null && rentalShoppingCartService.CurrentRentalShoppingCartIsFull) || !titleProduct.IsAvailable || (flag2 | flag) || !titleProduct.InStock)
			{
				if (BrowseState?.BrowseSelectedSort == "alpha")
				{
					browseItemModel.ImageOpacity = 0.2;
				}
				else
				{
					browseItemModel.ImageOpacity = 0.4;
				}
				goto IL_0203;
			}
		}
		browseItemModel.ImageOpacity = 1.0;
		goto IL_0203;
		IL_039f:
		int num;
		bool flag3 = (byte)num != 0;
		browseItemModel.CanAdd = flag3 || BrowseState.BrowseMode == BrowseViewFilter.ComingSoon;
		Visibility visibility = ((!flag3 && !IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.CornerAddButtonVisibility = ((!isADAMiniCart) ? visibility : Visibility.Collapsed);
		browseItemModel.RightAddButtonVisibility = ((visibility == Visibility.Collapsed) ? Visibility.Hidden : visibility);
		browseItemModel.ADAMiniCartAddButtonVisibility = ((!isADAMiniCart) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.AddButtonPlusSignVisibility = (IsADAMode ? Visibility.Collapsed : Visibility.Visible);
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
		bool flag4 = titleProduct is TitleRollupProduct && titleProduct.KioskProductType == KioskProductType.TitleRollup;
		Visibility showBuy = ((IsShoppingCartFull || (!titleProduct.IsSellable && !titleProduct.IsEmptyCase) || !(flag4 ? (!((TitleRollupProduct)titleProduct).KioskProducts.All((ITitleProduct x) => RentalShoppingCartService.CurrentRentalShoppingCart.HasItem(x.ProductId))) : (!RentalShoppingCartService.CurrentRentalShoppingCart.HasItem(titleProduct.ProductId)))) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.ShowBuy = showBuy;
		IKioskProduct kioskProduct2;
		if (!flag4)
		{
			IKioskProduct kioskProduct = ((titleProduct.TitleType != TitleType.Bluray) ? titleProduct : null);
			kioskProduct2 = kioskProduct;
		}
		else
		{
			kioskProduct2 = ((TitleRollupProduct)titleProduct).PrimaryProduct;
		}
		ITitleProduct titleProduct3 = (ITitleProduct)kioskProduct2;
		ITitleProduct titleProduct4 = (flag4 ? ((TitleRollupProduct)titleProduct).KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.Bluray) : ((titleProduct.TitleType == TitleType.Bluray) ? titleProduct : null));
		if (titleProduct3 != null && titleProduct4 != null && titleProduct3.ProductId == titleProduct4.ProductId)
		{
			titleProduct3 = null;
		}
		TitleRollupOverlayButtonProperties titleRollupOverlayButtonProperties = ((titleProduct3 != null && !titleProduct3.IsEmptyCase) ? TitleRollupUtilities.GetTitleRollupOverlayButtonProperties(titleProduct3, isPurchase: false, displayProductModelConstants.TitleRollupOverlayButtonTextValues) : null);
		TitleRollupOverlayButtonProperties titleRollupOverlayButtonProperties2 = ((titleProduct4 != null && !titleProduct4.IsEmptyCase) ? TitleRollupUtilities.GetTitleRollupOverlayButtonProperties(titleProduct4, isPurchase: false, displayProductModelConstants.TitleRollupOverlayButtonTextValues) : null);
		browseItemModel.ShowPrimary = titleRollupOverlayButtonProperties?.Visibility ?? Visibility.Collapsed;
		browseItemModel.ShowBluRay = titleRollupOverlayButtonProperties2?.Visibility ?? Visibility.Collapsed;
		browseItemModel.PrimaryEnabled = titleRollupOverlayButtonProperties?.Enabled ?? false;
		browseItemModel.BluRayEnabled = titleRollupOverlayButtonProperties2?.Enabled ?? false;
		string text2 = ((!((titleProduct?.IsMovie ?? false) & flag4) || titleProduct.IsEmptyCase) ? displayProductModelConstants.Add : (titleRollupOverlayButtonProperties?.ButtonText ?? string.Empty));
		bool flag5 = text2 != null && text2.Split('\n').Count() > 1;
		string primaryText = (flag5 ? text2.Split('\n')[0] : text2);
		string primaryLine2Text = (flag5 ? text2.Split('\n')[1] : text2);
		bool flag6 = (titleRollupOverlayButtonProperties2?.ButtonText?.Split('\n').Count() ?? 0) > 1;
		string bluRayText = (flag6 ? titleRollupOverlayButtonProperties2.ButtonText.Split('\n')[0] : titleRollupOverlayButtonProperties2?.ButtonText);
		string bluRayLine2Text = (flag6 ? titleRollupOverlayButtonProperties2.ButtonText.Split('\n')[1] : titleRollupOverlayButtonProperties2?.ButtonText);
		browseItemModel.PrimaryOutOfStockVisibility = ((!flag5) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.BluRayOutOfStockVisibility = ((!flag6) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.PrimaryText = primaryText;
		browseItemModel.PrimaryLine2Text = primaryLine2Text;
		browseItemModel.BluRayText = bluRayText;
		browseItemModel.BluRayLine2Text = bluRayLine2Text;
		int addButtonBarVisibility;
		if (!IsShoppingCartFull)
		{
			ITitleProduct titleProduct5 = titleProduct;
			if (titleProduct5 != null && !titleProduct5.ComingSoon)
			{
				ITitleProduct titleProduct6 = titleProduct;
				if (titleProduct6 != null && titleProduct6.InStock)
				{
					addButtonBarVisibility = 0;
					goto IL_0826;
				}
			}
		}
		addButtonBarVisibility = 2;
		goto IL_0826;
		IL_0826:
		browseItemModel.AddButtonBarVisibility = (Visibility)addButtonBarVisibility;
		ITitleProduct titleProduct7 = titleProduct;
		browseItemModel.OutOfStockVisibility = ((titleProduct7 == null || titleProduct7.InStock || titleProduct.ComingSoon) ? Visibility.Collapsed : Visibility.Visible);
		ITitleProduct titleProduct8 = titleProduct;
		browseItemModel.ComingSoonVisibility = ((titleProduct8 == null || !titleProduct8.ComingSoon) ? Visibility.Collapsed : Visibility.Visible);
		return;
		IL_0203:
		browseItemModel.BannerVisibility = ((isADAMiniCart || bannerType == BannerType.None) ? Visibility.Collapsed : Visibility.Visible);
		if (bannerType != BannerType.None)
		{
			Banner banner = (browseItemModel.Banner = BannerCollection.Instance.Find((Banner x) => x.BannerType == bannerType));
			if (banner.BannerType == BannerType.ComingSoon)
			{
				if (titleProduct != null)
				{
					browseItemModel.BannerText = string.Format("{1} {0:M/d}", titleProduct.MerchandiseDate, BundleFunctions.GetDictionaryStringValue("strings", banner.TextKey));
				}
			}
			else
			{
				browseItemModel.BannerText = BundleFunctions.GetDictionaryStringValue("strings", banner.TextKey);
			}
			browseItemModel.BannerBackgroundBrush = GetBrush(GetBrowseViewUserControl(), banner.BannerStyleName);
		}
		ProductFlags productFlags = GetProductFlag(titleProduct, flag, flag2);
		ProductFlag productFlag = ProductFlagCollection.Instance.FirstOrDefault((ProductFlag x) => x.Flag == productFlags);
		browseItemModel.FlagVisibility = ((isADAMiniCart || productFlag == null || productFlag.Flag == ProductFlags.None) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.FlagStyle = WPFFunctions.GetStyle(GetBrowseViewUserControl(), productFlag?.FlagStyleName);
		browseItemModel.FlagText = BundleFunctions.GetDictionaryStringValue("strings", productFlag?.TextKeyName);
		if (browseItemModel.FlagVisibility == Visibility.Visible)
		{
			browseItemModel.FlagMargin = new Thickness(0.0, 45.0, 0.0, 0.0);
		}
		if (!IsShoppingCartFull)
		{
			ITitleProduct titleProduct9 = titleProduct;
			if (titleProduct9 != null && titleProduct9.IsAvailable && (!flag2 | flag))
			{
				num = ((titleProduct?.InStock ?? false) ? 1 : 0);
				goto IL_039f;
			}
		}
		num = 0;
		goto IL_039f;
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

	private static ProductFlags GetProductFlag(ITitleProduct titleProduct, bool isEmptyCase, bool isProductSelected)
	{
		ProductFlags result = ProductFlags.None;
		if (titleProduct != null)
		{
			double totalDays = (DateTime.Now - titleProduct.MerchandiseDate).TotalDays;
			_ = (DateTime.Now - titleProduct.SortDate).TotalDays;
			bool flag = ServiceLocator.Instance.GetService<IDataService>()?.GetIsProductThinned(titleProduct.ProductId) ?? false;
			if (!IsShoppingCartFull && !InTitleMarketingMode)
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
					if (titleProduct.Genres.Contains(1002L))
					{
						result = ProductFlags.AwardWinner;
					}
					else if (totalDays <= 14.0 && titleProduct.Genres.Contains(1025L) && Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily != TitleFamily.Games)
					{
						result = ProductFlags.BackAgain;
					}
					else if (totalDays <= 14.0 && titleProduct.Genres.Contains(1095L) && Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily != TitleFamily.Movies)
					{
						result = ProductFlags.Classics;
					}
					else if (totalDays <= 28.0)
					{
						result = ProductFlags.New;
					}
					else if ((totalDays >= 105.0 && Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily != TitleFamily.Games) & flag)
					{
						result = ProductFlags.LastChance;
					}
				}
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
			if (rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel && !PromotionService.Instance.IsFreeInCartOfferRunning)
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

	private static System.Windows.Media.Brush GetBrush(FrameworkElement frameworkElement, string brushName)
	{
		System.Windows.Media.Brush result = null;
		object obj = frameworkElement.FindResource(brushName);
		if (obj != null)
		{
			result = obj as System.Windows.Media.Brush;
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
			item.Text = BundleFunctions.GetDictionaryStringValue("strings", item.Name);
		}
		return list;
	}
}
