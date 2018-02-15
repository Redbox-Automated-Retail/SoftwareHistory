using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Marketing;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class StartViewController
{
	private class Constants
	{
		public static string BurnInIdleTimerName = "burn-in-timer";

		public static string BurnInViewDurationTimerName = "burn-in-view-duration-timer";

		public static string StartIdleTimerName = "start-idle-timer";

		public static int StartIdleTimerDuration = 30000;

		public static string AdaIdleTimerName = "ada_idle_timer";

		public static int AdaIdleTimerDuration = 15000;

		public static string MarketingAdTimerName = "marketing_ad_check_for_new_active_campaign";

		public static int MarketingAdTimerDuration = 60000;

		public static int MinimumCarouselItems = 8;

		public static int MaximumCarouselItems = 11;
	}

	private static Action _toggleCultureCallback;

	private static Action _startIdleTimerCallback;

	private static IIdleTimerService IdleTimerService => ServiceLocator.Instance.GetService<IIdleTimerService>();

	private static ITimerService TimerService => ServiceLocator.Instance.GetService<ITimerService>();

	private static IMarketingAdService MarketingAdService => ServiceLocator.Instance.GetService<IMarketingAdService>();

	private static ISessionAnalyticsService SessionAnalyticsService => ServiceLocator.Instance.GetService<ISessionAnalyticsService>();

	public static void ConfigureStartView(Action toggleCultureCallback, Action startIdleTimerCallback)
	{
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectedSort = "Newest";
		FrameworkElement startViewUserControl = GetStartViewUserControl();
		if (startViewUserControl != null)
		{
			_toggleCultureCallback = toggleCultureCallback;
			_startIdleTimerCallback = startIdleTimerCallback;
			StartViewModel startViewModel = new StartViewModel();
			ConfigureButtonsAndText(startViewModel, startViewUserControl);
			startViewModel.OnToggleLanguage += ToggleLanguage;
			startViewModel.OnToggleADAMode += ToggleAdaMode;
			startViewModel.OnRentMoviesButtonClicked += GoToMovieBrowseView;
			startViewModel.OnRentOrBuyGameButtonClicked += GoToGameView;
			startViewModel.OnPickupButtonClicked += GoToPickupReservationSwipeView;
			startViewModel.OnReturnButtonClicked += GoToReturnProductView;
			startViewModel.OnHelpButtonClicked += GoToHelpDocumentView;
			startViewModel.OnBannerClicked += BannerClicked;
			startViewModel.OnBuyMoviesButtonClicked += GoToForSaleMovieBrowseView;
			startViewModel.OnBuyGamesButtonClicked += GoToForSaleGamesBrowseView;
			startViewModel.OnSignInButtonClicked += ProcessSignInSignOutCommand;
			MarketingAdService?.StartSession();
			ConfigureBurnInTimers(startViewModel);
			ConfigureStartViewIdleTimer();
			ConfigureAdaIdleTimer();
			ConfigureBanner(startViewModel);
			ConfigureMarketingAdTimer();
			if (ApplicationState.Instance.IsABEOn)
			{
				TurnAdaOn();
			}
			else
			{
				TurnAdaOff();
			}
			ConfigureCarousel(startViewModel);
			startViewUserControl.DataContext = startViewModel;
			CommandManager.InvalidateRequerySuggested();
		}
	}

	private static FrameworkElement GetStartViewUserControl()
	{
		FrameworkElement result = null;
		IActor actor = ActorFunctions.GetActor("StartViewUserControl");
		if (actor != null)
		{
			result = actor.WPFFrameworkElement;
		}
		return result;
	}

	private static void ConfigureButtonsAndText(StartViewModel startViewModel, FrameworkElement startViewUserControl)
	{
		if (startViewModel == null)
		{
			startViewModel = GetStartViewModelFromView();
		}
		if (startViewUserControl == null)
		{
			startViewUserControl = GetStartViewUserControl();
		}
		if (startViewModel != null && startViewUserControl != null)
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			bool flag = (service?.GetInStockGameCount() ?? 0) > 0;
			SetCultureSpecificText(startViewModel, startViewUserControl, flag);
			IStoreManager service2 = ServiceLocator.Instance.GetService<IStoreManager>();
			bool flag2 = service2.CanSellUsedMovies && service2.PurchasePrice > 0m;
			startViewModel.BuyMoviesOrGamesButtonVisibility = ((!flag2) ? Visibility.Collapsed : Visibility.Visible);
			startViewModel.BuyGamesButtonVisibility = ((!flag2 || !service2.CanSellUsedGames) ? Visibility.Collapsed : Visibility.Visible);
			startViewModel.RentOrBuyGamesButtonVisibility = Visibility.Visible;
			List<IKioskProduct> products = null;
			Redbox.KioskEngine.ComponentModel.ErrorList source = service?.GetProducts(BrowseViewFilter.ForSale, TitleFamily.All, TitleType.All, BrowseSort.MerchandiseDate, null, null, null, isADAView: false, out products);
			bool buyGamesEnabled = !source.Any() && products.Any((IKioskProduct x) => ((ITitleProduct)x).TitleFamily == TitleFamily.Games);
			bool buyMoviesEnabled = !source.Any() && products.Any((IKioskProduct x) => ((ITitleProduct)x).TitleFamily == TitleFamily.Movies);
			startViewModel.RentGamesEnabled = flag;
			startViewModel.BuyGamesEnabled = buyGamesEnabled;
			startViewModel.BuyMoviesEnabled = buyMoviesEnabled;
			startViewModel.BuyMoviesButtonWidth = (service2.CanSellUsedGames ? 135 : 290);
			startViewModel.BuyMoviesButtonTextMargin = (service2.CanSellUsedGames ? new Thickness(15.0) : new Thickness(100.0, 5.0, 0.0, 0.0));
			ServiceLocator.Instance.GetService<IMacroService>()["Start:BuyMoviesOrGames"] = "Movies" + (service2.CanSellUsedGames ? " and Games" : string.Empty);
			IPersonalizationService service3 = ServiceLocator.Instance.GetService<IPersonalizationService>();
			startViewModel.SignInVisibility = ((service3 == null || !service3.IsEarlyIdEnabled) ? Visibility.Collapsed : Visibility.Visible);
			startViewModel.SignInButtonEnabled = service3?.IsEarlyIdEnabled ?? false;
		}
	}

	public static bool IsProductInCarousel(long productId)
	{
		bool result = false;
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView?.CarouselModel?.CarouselDisplayProductModels != null)
		{
			foreach (DisplayProductModel carouselDisplayProductModel in startViewModelFromView.CarouselModel.CarouselDisplayProductModels)
			{
				if (!(carouselDisplayProductModel.Data is IKioskProduct kioskProduct))
				{
					continue;
				}
				switch (kioskProduct.KioskProductType)
				{
				case KioskProductType.Title:
					if ((kioskProduct as ITitleProduct).ProductId == productId)
					{
						result = true;
					}
					break;
				case KioskProductType.MDV:
					if (kioskProduct is MDVProduct mDVProduct && mDVProduct.TitleProducts.Any((ITitleProduct product) => product.ProductId == productId))
					{
						result = true;
					}
					break;
				case KioskProductType.TitleRollup:
					if (kioskProduct is TitleRollupProduct titleRollupProduct && titleRollupProduct.KioskProducts.Any((ITitleProduct product) => product.ProductId == productId))
					{
						result = true;
					}
					break;
				}
			}
		}
		return result;
	}

	public static void ReloadCarousel(bool force)
	{
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null && ((startViewModelFromView.CarouselModel.CarouselDisplayProductModels.Count() < Constants.MaximumCarouselItems) | force))
		{
			LogHelper.Instance.Log("Reloading Carousel");
			ConfigureButtonsAndText(startViewModelFromView, null);
			ConfigureCarousel(GetStartViewModelFromView());
		}
	}

	public static void LeaveStartView()
	{
		(IdleTimerService?.GetIdleTimer(Constants.BurnInIdleTimerName))?.Stop();
		(TimerService?.GetTimer(Constants.BurnInViewDurationTimerName))?.Stop();
		(IdleTimerService?.CreateIdleTimer(Constants.StartIdleTimerName))?.Stop();
		(IdleTimerService?.GetIdleTimer(Constants.AdaIdleTimerName))?.Stop();
	}

	private static void ConfigureMarketingAdTimer()
	{
		(TimerService?.CreateTimer(Constants.MarketingAdTimerName, Constants.MarketingAdTimerDuration, Constants.MarketingAdTimerDuration, delegate
		{
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			if (service.Peek() == service.GetStartOverViewName())
			{
				IMarketingAdService marketingAdService = MarketingAdService;
				if (marketingAdService != null && marketingAdService.CheckForNewActiveCampaign())
				{
					LogHelper.Instance.Log("activating new marketing ads campaign");
					MarketingAdService?.StartSession();
					ConfigureBanner(GetStartViewModelFromView());
				}
			}
		}))?.Start();
	}

	private static void CloseTitleRollupOverlay()
	{
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null)
		{
			CloseTitleRollupOverlay(startViewModelFromView);
		}
	}

	private static void CloseTitleRollupOverlay(StartViewModel startViewModel)
	{
		if (startViewModel.CarouselModel.TitleRollupOverlayModel.Visibility == Visibility.Visible)
		{
			startViewModel.CarouselModel.TitleRollupOverlayModel.Visibility = Visibility.Collapsed;
			startViewModel.CarouselModel.IsAnimationOn = true;
		}
	}

	private static void BannerClicked()
	{
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView == null)
		{
			return;
		}
		CloseTitleRollupOverlay(startViewModelFromView);
		switch (startViewModelFromView.BannerType)
		{
		case StartViewBannerType.bluray:
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.PageNumber = 1;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectedSort = "Blu-ray";
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseMode = BrowseViewFilter.All;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleType = TitleType.Bluray;
			PrepareToLeaveStartView();
			OrchestrationService.ViewShow("browse_view");
			break;
		case StartViewBannerType.game:
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily = TitleFamily.Games;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleType = TitleType.All;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.Genre = null;
			PrepareToLeaveStartView();
			OrchestrationService.ViewShow("browse_view");
			break;
		case StartViewBannerType.collections:
		{
			ICollectionProduct collection = null;
			if (startViewModelFromView.BannerCollectionId != "ALL")
			{
				long result = 0L;
				long.TryParse(startViewModelFromView.BannerCollectionId, out result);
				IDataService service = ServiceLocator.Instance.GetService<IDataService>();
				if (service != null)
				{
					service.GetCollection(result, out collection);
					if (collection == null)
					{
						LogHelper.Instance.Log("collection banner click: invalid collection id {0}.  Showing all collections instead.", startViewModelFromView.BannerCollectionId);
					}
				}
			}
			PrepareToLeaveStartView();
			ShowCollectionBrowseView(collection);
			break;
		}
		case StartViewBannerType.none:
		case StartViewBannerType.ad:
			break;
		}
	}

	private static void ConfigureBanner(StartViewModel startViewModel)
	{
		IMarketingConfigurationManager service = ServiceLocator.Instance.GetService<IMarketingConfigurationManager>();
		bool flag = service?.ShowBlurayBrowse ?? false;
		bool flag2 = service?.ShowGameBanner ?? false;
		IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
		string text = null;
		StartViewBannerType startViewBannerType = StartViewBannerType.none;
		Image image = null;
		BitmapImage bitmapImage = null;
		if (MarketingAdService != null)
		{
			MarketingAd screenAd = MarketingAdService.GetScreenAd(MarketingAdLocation.Welcome, createImpression: true);
			if (screenAd.Visible)
			{
				startViewBannerType = StartViewBannerType.ad;
				image = service2.GetBitmap(screenAd.ImageName);
			}
		}
		if (image == null)
		{
			image = service2.GetBitmap("banner_image");
			if (image != null)
			{
				IResource resource = service2.GetResource("banner_image");
				if (resource != null)
				{
					IAspect aspect = resource.GetAspect("content");
					if (aspect != null)
					{
						text = (aspect.Properties.ContainsKey("collectionid") ? aspect.Properties["collectionid"].ToUpper() : null);
						if (text == string.Empty)
						{
							text = null;
						}
					}
				}
				if (string.IsNullOrEmpty(text) || OrchestrationService.RentalIsCollectionsEnabled())
				{
					startViewBannerType = StartViewBannerType.collections;
				}
			}
		}
		if (startViewBannerType == StartViewBannerType.none)
		{
			if (flag)
			{
				startViewBannerType = StartViewBannerType.bluray;
				image = service2.GetBitmap("image_bluray_banner");
			}
			else if (flag2)
			{
				startViewBannerType = StartViewBannerType.game;
				image = service2.GetBitmap("image_game_banner");
			}
			else
			{
				image = null;
			}
		}
		if (image != null)
		{
			bitmapImage = ServiceLocator.Instance.GetService<IBitmapCacheService>()?.ToBitmapImage(image);
		}
		if (startViewModel != null)
		{
			startViewModel.BannerCollectionId = text;
			startViewModel.BannerType = startViewBannerType;
			startViewModel.BannerImage = bitmapImage;
			startViewModel.BannerVisibility = ((bitmapImage == null) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private static void ConfigureAdaIdleTimer()
	{
		(IdleTimerService?.CreateIdleTimer(Constants.AdaIdleTimerName)).Initialize(Constants.AdaIdleTimerDuration, delegate
		{
			ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			if (service != null && !service.TTSEnabled)
			{
				TurnAdaOff();
				ConfigureCarousel(GetStartViewModelFromView());
			}
		});
	}

	private static void ConfigureStartViewIdleTimer()
	{
		IIdleTimer startIdleTimer = IdleTimerService?.CreateIdleTimer(Constants.StartIdleTimerName);
		startIdleTimer?.Initialize(Constants.StartIdleTimerDuration, delegate
		{
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			if (service?.Peek() == service?.GetStartOverViewName())
			{
				if (_startIdleTimerCallback != null)
				{
					_startIdleTimerCallback();
				}
				LogHelper.Instance.Log("Current Culture: {0} Default Culture: {1}", OrchestrationService.GetCulture(), OrchestrationService.GetDefaultCulture());
				if (OrchestrationService.GetCulture() != OrchestrationService.GetDefaultCulture())
				{
					ToggleLocalizationCulture();
				}
			}
			startIdleTimer?.Reset();
		});
		startIdleTimer?.Start();
	}

	private static void ConfigureBurnInTimers(StartViewModel startViewModel)
	{
		ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
		IIdleTimer burnInIdleTimer = IdleTimerService?.CreateIdleTimer(Constants.BurnInIdleTimerName);
		burnInIdleTimer.Initialize(Redbox.Rental.Services.Configuration.Configuration.Instance.BurnInTimeout, delegate
		{
			burnInIdleTimer.Stop();
			startViewModel.BurnInVisibility = Visibility.Collapsed;
			timerService.GetTimer(Constants.BurnInViewDurationTimerName)?.Start();
		});
		burnInIdleTimer.Start();
		Action turnOffBurnInView = delegate
		{
			startViewModel.BurnInVisibility = Visibility.Visible;
			timerService.GetTimer(Constants.BurnInViewDurationTimerName)?.Stop();
			burnInIdleTimer.Start();
		};
		timerService.CreateTimer(Constants.BurnInViewDurationTimerName, Redbox.Rental.Services.Configuration.Configuration.Instance.BurnInViewDuration, Redbox.Rental.Services.Configuration.Configuration.Instance.BurnInViewDuration, delegate
		{
			turnOffBurnInView();
		});
		startViewModel.OnExitBurnInView += turnOffBurnInView;
	}

	private static void ToggleAdaMode()
	{
		CloseTitleRollupOverlay();
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service == null || !service.TTSEnabled)
		{
			if (ApplicationState.Instance.IsABEOn)
			{
				TurnAdaOff();
			}
			else
			{
				TurnAdaOn();
			}
			ConfigureCarousel(GetStartViewModelFromView());
		}
	}

	private static void TurnAdaOff()
	{
		SetAdaMode(adaEnabled: false);
		IdleTimerService?.GetIdleTimer(Constants.AdaIdleTimerName)?.Stop();
	}

	private static void TurnAdaOn()
	{
		SetAdaMode(adaEnabled: true);
		IdleTimerService?.GetIdleTimer(Constants.AdaIdleTimerName)?.Start();
	}

	private static void SetAdaMode(bool adaEnabled)
	{
		ApplicationState.Instance.IsADAAccessible = adaEnabled;
		LogHelper.Instance.Log("ADA mode turned {0}.", ApplicationState.Instance.IsADAAccessible ? "On" : "Off");
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null)
		{
			startViewModelFromView.AdaTurnedOnLabelVisibility = ((!adaEnabled) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private static StartViewModel GetStartViewModelFromView()
	{
		StartViewModel result = null;
		IActor actor = ActorFunctions.GetActor("StartViewUserControl");
		if (actor != null)
		{
			result = actor.WPFFrameworkElement.DataContext as StartViewModel;
		}
		return result;
	}

	private static void ToggleLanguage()
	{
		CloseTitleRollupOverlay();
		LogHelper.Instance.Log("language_toggle, setting language_switch=true");
		OrchestrationService.CreateStartViewAnalytics(AnalyticsSessionType.BrowseVend);
		SessionAnalyticsService?.AddViewEvent("buttonPress", "language_toggle");
		LogHelper.Instance.Log("Function:switch_language()");
		ToggleLocalizationCulture();
	}

	private static void ConfigureCarousel(StartViewModel startViewModel)
	{
		CarouselModel carouselModel = new CarouselModel();
		carouselModel.OnDisplayProductModelSelected += CarouselModel_OnDisplayProductModelSelected;
		carouselModel.OnAddDisplayProductModel += CarouselModel_OnAddDisplayProductModel;
		carouselModel.OnTitleRollupOverlayCancel += CarouselModel_OnTitleRollupOverlayCancel;
		carouselModel.OnTitleRollupOverlayTimeout += CarouselModel_OnTitleRollupOverlayTimeout;
		carouselModel.OnTitleRollupOverlayAddDVD += CarouselModel_OnTitleRollupOverlayAddDVDProduct;
		carouselModel.OnTitleRollupOverlayAddBluray += CarouselModel_OnTitleRollupOverlayAddBlurayProduct;
		carouselModel.OnTitleRollupOverlayVisibilityChange += CarouselModel_OnTitleRollupOverlayVisibilityChange;
		carouselModel.ShowAddButton = !(ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart?.IsFull ?? true);
		OrchestrationService.RentalRefreshIfNeeded();
		List<IKioskProduct> list = OrchestrationService.RentalGetCarouselProducts();
		List<DisplayProductModel> list2 = new List<DisplayProductModel>();
		ServiceLocator.Instance.GetService<IBitmapCacheService>();
		foreach (IKioskProduct item in list)
		{
			DisplayProductModel displayProductModel = new DisplayProductModel
			{
				Data = item
			};
			displayProductModel.ConfigureForCarousel();
			displayProductModel.AddButtonLabelText = BundleFunctions.GetDictionaryStringValue("strings", "carousel_add_button_label");
			displayProductModel.Image = OrchestrationService.GetProductBitmapImage(item);
			if (item.KioskProductType == KioskProductType.Collection && item is ICollectionProduct)
			{
				displayProductModel.AddButtonLabelText = BundleFunctions.GetDictionaryStringValue("strings", "carousel_add_item_to_cart_button_for_collection");
			}
			list2.Add(displayProductModel);
		}
		carouselModel.CarouselDisplayProductModels = list2;
		if (startViewModel != null)
		{
			startViewModel.CarouselModel = carouselModel;
			bool flag = (startViewModel?.CarouselModel?.CarouselDisplayProductModels?.Count ?? 0) >= Constants.MinimumCarouselItems;
			startViewModel.LoadingTitlesImageVisibility = (flag ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private static void CarouselModel_OnTitleRollupOverlayVisibilityChange(Visibility newVisibility)
	{
		if (newVisibility == Visibility.Visible)
		{
			IKioskProduct centerCarouselProduct = GetStartViewModelFromView()?.CarouselModel?.TitleRollupOverlayModel?.BrowseItemModel?.Data as IKioskProduct;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.CenterCarouselProduct = centerCarouselProduct;
		}
		else
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.CenterCarouselProduct = null;
		}
	}

	private static void CarouselModel_OnTitleRollupOverlayAddBlurayProduct(IBrowseItemModel browseItemModel, object parameter)
	{
		if (browseItemModel?.Data is TitleRollupProduct titleRollupProduct)
		{
			AddProductToCart(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.Bluray));
		}
	}

	private static void CarouselModel_OnTitleRollupOverlayAddDVDProduct(IBrowseItemModel browseItemModel, object parameter)
	{
		if (browseItemModel?.Data is TitleRollupProduct titleRollupProduct)
		{
			AddProductToCart(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.DVD));
		}
	}

	private static void CarouselModel_OnTitleRollupOverlayTimeout(IBrowseItemModel browseItemModel, object parameter)
	{
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null)
		{
			startViewModelFromView.CarouselModel.IsAnimationOn = true;
		}
	}

	private static void CarouselModel_OnTitleRollupOverlayCancel(IBrowseItemModel browseItemModel, object parameter)
	{
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null)
		{
			OrchestrationService.CreateStartViewAnalytics(AnalyticsSessionType.BrowseVend);
			SessionAnalyticsService?.AddViewEvent("buttonPress", "carousel-cancel-add-item");
			startViewModelFromView.CarouselModel.IsAnimationOn = true;
		}
	}

	private static void CarouselModel_OnAddDisplayProductModel(IBrowseItemModel browseItemModel, object parameter)
	{
		if (!(browseItemModel.Data is IKioskProduct kioskProduct))
		{
			return;
		}
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null)
		{
			switch (kioskProduct.KioskProductType)
			{
			case KioskProductType.Title:
			case KioskProductType.MDV:
				startViewModelFromView.CarouselModel.IsAnimationOn = false;
				AddProductToCart(kioskProduct as ITitleProduct);
				break;
			case KioskProductType.TitleRollup:
				startViewModelFromView.CarouselModel.IsAnimationOn = false;
				TitleRollupUtilities.ConfigureTitleRollupOverlay(browseItemModel, startViewModelFromView.CarouselModel.TitleRollupOverlayModel, showDigitalCodeInfo: false, isPurchase: false);
				break;
			case KioskProductType.Collection:
				startViewModelFromView.CarouselModel.IsAnimationOn = false;
				OrchestrationService.CreateStartViewAnalytics(AnalyticsSessionType.BrowseVend);
				SessionAnalyticsService?.AddViewEvent("buttonPress", "add_to_cart");
				PrepareToLeaveStartView();
				ShowCollectionBrowseView(kioskProduct);
				break;
			case KioskProductType.None:
				break;
			}
		}
	}

	private static void AddProductToCart(ITitleProduct titleProduct)
	{
		if (titleProduct != null)
		{
			OrchestrationService.CreateStartViewAnalytics(AnalyticsSessionType.BrowseVend);
			SessionAnalyticsService?.AddViewEvent("buttonPress", "add_to_cart");
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductAddedFromCarousel = true;
			SetBrowseFiltersBasedOnSelectedProduct(titleProduct);
			BrowseFunctions.AddProductToCart(titleProduct, isPurchase: false);
		}
	}

	private static void SetBrowseFiltersBasedOnSelectedProduct(ITitleProduct titleProduct)
	{
		if (titleProduct != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily = titleProduct.TitleFamily;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleType = TitleType.All;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.Genre = null;
		}
	}

	private static void CarouselModel_OnDisplayProductModelSelected(IBrowseItemModel browseItemModel, object parameter)
	{
		if (!(browseItemModel.Data is IKioskProduct kioskProduct))
		{
			return;
		}
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct = kioskProduct;
		OrchestrationService.CreateStartViewAnalytics(AnalyticsSessionType.BrowseVend);
		SessionAnalyticsService?.AddViewEvent("buttonPress", "carousel_art_button");
		switch (kioskProduct.KioskProductType)
		{
		case KioskProductType.Title:
		case KioskProductType.MDV:
		case KioskProductType.TitleRollup:
			if (kioskProduct is ITitleProduct browseFiltersBasedOnSelectedProduct)
			{
				StartViewModel startViewModelFromView = GetStartViewModelFromView();
				if (startViewModelFromView != null)
				{
					startViewModelFromView.CarouselModel.IsAnimationOn = false;
					SetBrowseFiltersBasedOnSelectedProduct(browseFiltersBasedOnSelectedProduct);
					OrchestrationService.ViewShow("browse_view");
					Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductSelectedFromCarousel = true;
					IViewService service = ServiceLocator.Instance.GetService<IViewService>();
					Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductDetailShownFromView = service.Peek();
					PrepareToLeaveStartView();
					ServiceLocator.Instance.GetService<ISessionAnalyticsService>()?.AddViewEvent("context", ("productDisplayed," + ((ITitleProduct)kioskProduct)?.ProductId) ?? string.Empty);
					OrchestrationService.ViewShow("product_detail_view");
				}
			}
			break;
		case KioskProductType.Collection:
			PrepareToLeaveStartView();
			ShowCollectionBrowseView(kioskProduct);
			break;
		case KioskProductType.None:
			break;
		}
	}

	private static void ShowCollectionBrowseView(IKioskProduct kioskProduct)
	{
		ICollectionProduct collectionProduct = kioskProduct as ICollectionProduct;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.CarouselCollectionsSelected = true;
		OrchestrationService.ViewShow("browse_view");
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.PushBrowseStateSet();
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseCollections = collectionProduct == null || collectionProduct.Id == 0;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseCollectionProduct = ((collectionProduct == null || collectionProduct.Id == 0L) ? null : collectionProduct);
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct = Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseCollectionProduct;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily = TitleFamily.Movies;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleType = TitleType.All;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseMode = BrowseViewFilter.All;
		OrchestrationService.ViewShow("browse_view");
	}

	private static void ToggleLocalizationCulture()
	{
		string text = null;
		text = ((!(OrchestrationService.GetCulture() == OrchestrationService.GetDefaultCulture())) ? OrchestrationService.GetDefaultCulture() : OrchestrationService.GetOtherCulture());
		LogHelper.Instance.Log("Toggling Culture to {0}", text);
		OrchestrationService.SetCulture(text);
		if (_toggleCultureCallback != null)
		{
			_toggleCultureCallback();
		}
	}

	private static void SetCultureSpecificText(StartViewModel startViewModel, FrameworkElement startViewUserControl, bool hasInstockGames)
	{
		OrchestrationService.SetCurrentUICulture();
		ServiceLocator.Instance.GetService<IStoreManager>();
		startViewModel.AllMoviesButtonTextLine1 = BundleFunctions.GetDictionaryStringValue("strings", "movie_button_rent_label");
		startViewModel.AllMoviesButtonTextLine2 = BundleFunctions.GetDictionaryStringValue("strings", "Alt_StartView_Movies");
		startViewModel.PickUpButtonText = BundleFunctions.GetDictionaryStringValue("strings", "pickup");
		startViewModel.ReturnButtonText = BundleFunctions.GetDictionaryStringValue("strings", "return_button_label");
		startViewModel.ToggleLanguageButtonText = BundleFunctions.GetDictionaryStringValue("strings", "toggle_language_button_label");
		startViewModel.BurnInViewLabelText = BundleFunctions.GetDictionaryStringValue("strings", "burn_in_view_label");
		startViewModel.BuyMoviesOrGamesButtonTextLine1 = BundleFunctions.GetDictionaryStringValue("strings", "buy_movies_or_games_line1");
		startViewModel.BuyMoviesOrGamesButtonTextLine2 = BundleFunctions.GetDictionaryStringValue("strings", "buy_movies_or_games_line2");
		startViewModel.BuyGamesButtonTextLine1 = BundleFunctions.GetDictionaryStringValue("strings", "buy_games_line1");
		startViewModel.BuyGamesButtonTextLine2 = BundleFunctions.GetDictionaryStringValue("strings", "buy_games_line2");
		startViewModel.BuyMoviesOrGamesButtonTextLine1Style = (OrchestrationService.IsCultureSpanish() ? WPFFunctions.GetStyle(startViewUserControl, "font_montserrat_regular_16") : WPFFunctions.GetStyle(startViewUserControl, "font_montserrat_regular_22"));
		startViewModel.BuyMoviesOrGamesButtonTextLine2Style = (OrchestrationService.IsCultureSpanish() ? WPFFunctions.GetStyle(startViewUserControl, "font_museo_slab_900_18") : WPFFunctions.GetStyle(startViewUserControl, "font_museo_slab_900_24"));
		startViewModel.BuyGamesButtonTextLine1Style = (OrchestrationService.IsCultureSpanish() ? WPFFunctions.GetStyle(startViewUserControl, "font_montserrat_regular_16") : WPFFunctions.GetStyle(startViewUserControl, "font_montserrat_regular_22"));
		startViewModel.BuyGamesButtonTextLine2Style = (OrchestrationService.IsCultureSpanish() ? WPFFunctions.GetStyle(startViewUserControl, "font_museo_slab_900_18") : WPFFunctions.GetStyle(startViewUserControl, "font_museo_slab_900_24"));
		startViewModel.AllMoviesButtonTextLine2Style = WPFFunctions.GetStyle(startViewUserControl, OrchestrationService.IsCultureSpanish() ? "font_museo_slab_900_28" : "font_museo_slab_900_32");
		startViewModel.GamesButtonTextLine1 = BundleFunctions.GetDictionaryStringValue("strings", "games_button_rent_label");
		startViewModel.GamesButtonTextLine2 = BundleFunctions.GetDictionaryStringValue("strings", "games_button_games_label");
		startViewModel.AdaTurnedOnLabelVisibility = ((!ApplicationState.Instance.IsADAAccessible) ? Visibility.Collapsed : Visibility.Visible);
		startViewModel.AdaTurnedOnLabelText = BundleFunctions.GetDictionaryStringValue("strings", "ada_turned_on_label_text");
		startViewModel.ToggleSignInButtonText = (PersonalizationFunctions.IsSignedIn ? Resources.sign_out_button : Resources.button_sign_in);
	}

	private static void PrepareToLeaveStartView()
	{
		CloseTitleRollupOverlay();
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null && startViewModelFromView.CarouselModel != null)
		{
			startViewModelFromView.CarouselModel.IsAnimationOn = false;
		}
		if (ApplicationState.Instance.IsABEOn)
		{
			ApplicationState.Instance.IsADAAccessible = true;
		}
	}

	private static void GoToMovieBrowseView()
	{
		PrepareToLeaveStartView();
		GoToBrowseView("all_movies_button", TitleFamily.Movies);
	}

	private static void GoToForSaleMovieBrowseView()
	{
		PrepareToLeaveStartView();
		GoToBrowseView("buy_movies_button", TitleFamily.Movies);
	}

	private static void GoToForSaleGamesBrowseView()
	{
		PrepareToLeaveStartView();
		GoToBrowseView("buy_games_button", TitleFamily.Games);
	}

	private static void GoToGameView()
	{
		PrepareToLeaveStartView();
		_ = Redbox.Rental.Services.Configuration.Configuration.Instance;
		GoToBrowseView("games_button", TitleFamily.Games);
	}

	private static void GoToBrowseView(string pressedButtonName, TitleFamily titleFamily)
	{
		OrchestrationService.CreateStartViewAnalytics(AnalyticsSessionType.BrowseVend);
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", pressedButtonName);
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.SetDefaultBrowseStateSet();
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily = titleFamily;
		if (pressedButtonName == "buy_movies_button" || pressedButtonName == "buy_games_button")
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseMode = BrowseViewFilter.ForSale;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectedSort = "for-sale";
		}
		OrchestrationService.ViewShow("browse_view");
	}

	private static void GoToPickupReservationSwipeView()
	{
		PrepareToLeaveStartView();
		OrchestrationService.ExecuteScript("pickup_reserved_product");
	}

	private static void GoToReturnProductView()
	{
		PrepareToLeaveStartView();
		OrchestrationService.ExecuteScript("return_product");
	}

	private static void GoToHelpDocumentView()
	{
		PrepareToLeaveStartView();
		OrchestrationService.ExecuteScript("help");
	}

	private static void ProcessSignInSignOutCommand()
	{
		PrepareToLeaveStartView();
		OrchestrationService.CreateStartViewAnalytics(AnalyticsSessionType.BrowseVend);
		ISessionAnalyticsService sessionAnalyticsService = SessionAnalyticsService;
		if (sessionAnalyticsService != null)
		{
			IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession;
			sessionAnalyticsService.AddViewEvent("buttonPress", (personalizationSession != null && personalizationSession.IsLoggedIn) ? "SignOut" : "SignIn");
		}
		IPersonalizationSession personalizationSession2 = Redbox.Rental.Services.Session.Session.Instance.PersonalizationSession;
		if (personalizationSession2 != null && personalizationSession2.IsLoggedIn)
		{
			PersonalizationFunctions.ShowLogoutDialog(setKeepSessionOnStartViewEnterToTrue: true);
			return;
		}
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.SetDefaultBrowseStateSet();
		service.Push("browse_view");
		OrchestrationService.ViewShow("prompt_sign_in_view");
	}
}
