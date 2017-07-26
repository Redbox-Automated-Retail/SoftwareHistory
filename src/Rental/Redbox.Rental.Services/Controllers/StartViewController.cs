using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.REDS.Framework;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Marketing;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Views;

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

	private class TitleRollupOverlayButtonProperties
	{
		public string ButtonText { get; set; }

		public Visibility Visibility { get; set; }

		public bool Enabled { get; set; }
	}

	private static Action _toggleCultureCallback;

	private static Action _startIdleTimerCallback;

	private static Action _showBrowseViewCollectionsCallback;

	private static Action<LuaTable> _showCollectionItemsCallback;

	private static Action<LuaTable, bool, LuaTable, string> _checkForWarningPopupsCallback;

	private static Func<LuaTable, bool> _isProductInShoppingCartCallback;

	private static Func<LuaTable, bool> _isProductEarlyReleaseCallback;

	private static IIdleTimerService IdleTimerService => ServiceLocator.Instance.GetService<IIdleTimerService>();

	private static ITimerService TimerService => ServiceLocator.Instance.GetService<ITimerService>();

	private static IMarketingAdService MarketingAdService => ServiceLocator.Instance.GetService<IMarketingAdService>();

	private static ISessionAnalyticsService SessionAnalyticsService => ServiceLocator.Instance.GetService<ISessionAnalyticsService>();

	public static void ConfigureStartView(Action toggleCultureCallback, Action startIdleTimerCallback, Action showBrowseViewCollectionsCallback, Action<LuaTable> showCollectionItemsCallback, Action<LuaTable, bool, LuaTable, string> checkForWarningPopupsCallback, Func<LuaTable, bool> isProductInShoppingCart, Func<LuaTable, bool> isProductEarlyRelease)
	{
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectedSort = "Newest";
		FrameworkElement startViewUserControl = GetStartViewUserControl();
		if (startViewUserControl != null)
		{
			_toggleCultureCallback = toggleCultureCallback;
			_startIdleTimerCallback = startIdleTimerCallback;
			_showBrowseViewCollectionsCallback = showBrowseViewCollectionsCallback;
			_showCollectionItemsCallback = showCollectionItemsCallback;
			_checkForWarningPopupsCallback = checkForWarningPopupsCallback;
			_isProductInShoppingCartCallback = isProductInShoppingCart;
			_isProductEarlyReleaseCallback = isProductEarlyRelease;
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
			startViewModel.OnBuyMoviesOrGamesButtonClicked += GoToForSaleMovieBrowseView;
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
			bool flag = (ServiceLocator.Instance.GetService<IDataService>()?.GetInStockGameCount() ?? 0) > 0;
			SetCultureSpecificText(startViewModel, startViewUserControl, flag);
			startViewModel.RentOrBuyGamesButtonVisibility = ((!flag) ? Visibility.Collapsed : Visibility.Visible);
			IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
			bool canSellUsedMovies = service.CanSellUsedMovies;
			bool flag2 = service.PurchasePrice > 0m;
			startViewModel.BuyMoviesOrGamesButtonVisibility = ((!(canSellUsedMovies & flag2)) ? Visibility.Collapsed : Visibility.Visible);
			ServiceLocator.Instance.GetService<IMacroService>()["Start:BuyMoviesOrGames"] = "Movies" + (service.CanSellUsedGames ? " and Games" : string.Empty);
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
			OrchestrationService.ExecuteScript("browse_products");
			break;
		case StartViewBannerType.game:
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily = TitleFamily.Games;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleType = TitleType.All;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.Genre = null;
			PrepareToLeaveStartView();
			OrchestrationService.ExecuteScript("browse_products");
			break;
		case StartViewBannerType.collections:
		{
			LuaTable luaTable = null;
			if (startViewModelFromView.BannerCollectionId != "ALL")
			{
				long result = 0L;
				long.TryParse(startViewModelFromView.BannerCollectionId, out result);
				luaTable = OrchestrationService.RentalGetCollection(result, null, null, null, null);
				if (luaTable == null)
				{
					LogHelper.Instance.Log("collection banner click: invalid collection id {0}.  Showing all collections instead.", startViewModelFromView.BannerCollectionId);
				}
			}
			PrepareToLeaveStartView();
			OrchestrationService.ViewShow("browse_view");
			if (luaTable == null)
			{
				if (_showBrowseViewCollectionsCallback != null)
				{
					_showBrowseViewCollectionsCallback();
				}
			}
			else if (_showCollectionItemsCallback != null)
			{
				_showCollectionItemsCallback(luaTable);
			}
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
		BitmapImage bitmapImage = null;
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
		OrchestrationService.RentalRefreshIfNeeded();
		List<IKioskProduct> list = OrchestrationService.RentalGetCarouselProducts();
		List<DisplayProductModel> list2 = new List<DisplayProductModel>();
		IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		foreach (IKioskProduct item in list)
		{
			DisplayProductModel displayProductModel = new DisplayProductModel
			{
				Data = item
			};
			displayProductModel.ConfigureForCarousel();
			displayProductModel.AddButtonLabelText = BundleFunctions.GetDictionaryStringValue("strings", "carousel_add_button_label");
			BitmapImage image = null;
			switch (item.KioskProductType)
			{
			case KioskProductType.None:
				image = Redbox.Rental.UI.Views.Constants.Images.MovieMissingBoxArt;
				break;
			case KioskProductType.Title:
			case KioskProductType.MDV:
			case KioskProductType.TitleRollup:
				if (item is ITitleProduct titleProduct)
				{
					if (OrchestrationService.DataServiceRegisterImage(titleProduct.ImageFile, titleProduct.ImageFile))
					{
						image = service.GetBitmapImage(titleProduct.ImageFile);
					}
					else if (titleProduct.TitleFamily == TitleFamily.Movies)
					{
						image = Redbox.Rental.UI.Views.Constants.Images.MovieMissingBoxArt;
					}
					else if (titleProduct.TitleFamily == TitleFamily.Games)
					{
						image = Redbox.Rental.UI.Views.Constants.Images.GameMissingBoxArt;
					}
				}
				break;
			case KioskProductType.Collection:
				if (item is ICollectionProduct)
				{
					image = Redbox.Rental.UI.Views.Constants.Images.CollectionsCarousel;
					displayProductModel.AddButtonLabelText = BundleFunctions.GetDictionaryStringValue("strings", "carousel_add_item_to_cart_button_for_collection");
				}
				break;
			default:
				image = Redbox.Rental.UI.Views.Constants.Images.MovieMissingBoxArt;
				break;
			}
			displayProductModel.Image = image;
			list2.Add(displayProductModel);
		}
		carouselModel.CarouselDisplayProductModels = list2;
		startViewModel.CarouselModel = carouselModel;
		bool flag = (startViewModel?.CarouselModel?.CarouselDisplayProductModels?.Count ?? 0) >= Constants.MinimumCarouselItems;
		startViewModel.LoadingTitlesImageVisibility = (flag ? Visibility.Collapsed : Visibility.Visible);
	}

	private static void CarouselModel_OnTitleRollupOverlayVisibilityChange(Visibility newVisibility)
	{
		if (newVisibility == Visibility.Visible)
		{
			IKioskProduct centerCarouselProduct = GetStartViewModelFromView()?.CarouselModel?.TitleRollupOverlayModel?.DisplayProductModel?.Data as IKioskProduct;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.CenterCarouselProduct = centerCarouselProduct;
		}
		else
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.CenterCarouselProduct = null;
		}
	}

	private static void CarouselModel_OnTitleRollupOverlayAddBlurayProduct(DisplayProductModel displayProductModel)
	{
		if (displayProductModel?.Data is TitleRollupProduct titleRollupProduct)
		{
			AddProductToCart(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.Bluray));
		}
	}

	private static void CarouselModel_OnTitleRollupOverlayAddDVDProduct(DisplayProductModel displayProductModel)
	{
		if (displayProductModel?.Data is TitleRollupProduct titleRollupProduct)
		{
			AddProductToCart(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.DVD));
		}
	}

	private static void CarouselModel_OnTitleRollupOverlayTimeout(DisplayProductModel displayProductModel)
	{
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null)
		{
			startViewModelFromView.CarouselModel.IsAnimationOn = true;
		}
	}

	private static void CarouselModel_OnTitleRollupOverlayCancel(DisplayProductModel displayProductModel)
	{
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null)
		{
			OrchestrationService.CreateStartViewAnalytics(AnalyticsSessionType.BrowseVend);
			SessionAnalyticsService?.AddViewEvent("buttonPress", "carousel-cancel-add-item");
			startViewModelFromView.CarouselModel.IsAnimationOn = true;
		}
	}

	private static void CarouselModel_OnAddDisplayProductModel(DisplayProductModel displayProductModel)
	{
		if (!(displayProductModel.Data is IKioskProduct kioskProduct))
		{
			return;
		}
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		switch (kioskProduct.KioskProductType)
		{
		case KioskProductType.Title:
		case KioskProductType.MDV:
			startViewModelFromView.CarouselModel.IsAnimationOn = false;
			AddProductToCart(kioskProduct as ITitleProduct);
			break;
		case KioskProductType.TitleRollup:
			startViewModelFromView.CarouselModel.IsAnimationOn = false;
			if (kioskProduct is TitleRollupProduct titleRollupProduct)
			{
				TitleRollupOverlayButtonProperties titleRollupOverlayButtonProperties = GetTitleRollupOverlayButtonProperties(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.DVD), isPurchase: false);
				startViewModelFromView.CarouselModel.TitleRollupOverlayModel.DVDButtonText = titleRollupOverlayButtonProperties.ButtonText;
				startViewModelFromView.CarouselModel.TitleRollupOverlayModel.DVDButtonVisibility = titleRollupOverlayButtonProperties.Visibility;
				startViewModelFromView.CarouselModel.TitleRollupOverlayModel.DVDButtonEnabled = titleRollupOverlayButtonProperties.Enabled;
				titleRollupOverlayButtonProperties = GetTitleRollupOverlayButtonProperties(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.Bluray), isPurchase: false);
				startViewModelFromView.CarouselModel.TitleRollupOverlayModel.BlurayButtonText = titleRollupOverlayButtonProperties.ButtonText;
				startViewModelFromView.CarouselModel.TitleRollupOverlayModel.BlurayButtonVisibility = titleRollupOverlayButtonProperties.Visibility;
				startViewModelFromView.CarouselModel.TitleRollupOverlayModel.BlurayButtonEnabled = titleRollupOverlayButtonProperties.Enabled;
				startViewModelFromView.CarouselModel.TitleRollupOverlayModel.Visibility = Visibility.Visible;
			}
			break;
		case KioskProductType.Collection:
			startViewModelFromView.CarouselModel.IsAnimationOn = false;
			OrchestrationService.CreateStartViewAnalytics(AnalyticsSessionType.BrowseVend);
			SessionAnalyticsService?.AddViewEvent("buttonPress", "add_to_cart");
			ShowCollectionBrowseView(kioskProduct);
			break;
		case KioskProductType.None:
			break;
		}
	}

	private static void AddProductToCart(ITitleProduct titleProduct)
	{
		if (titleProduct == null)
		{
			return;
		}
		OrchestrationService.CreateStartViewAnalytics(AnalyticsSessionType.BrowseVend);
		SessionAnalyticsService?.AddViewEvent("buttonPress", "add_to_cart");
		if (_checkForWarningPopupsCallback == null)
		{
			return;
		}
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.ItemAddedFromView = service?.Peek();
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductAddedFromCarousel = true;
		SetBrowseFiltersBasedOnSelectedProduct(titleProduct);
		LuaTable luaTable = titleProduct?.ToDictionary()?.ToLuaTable();
		if (_isProductEarlyReleaseCallback != null && _isProductEarlyReleaseCallback(luaTable))
		{
			StartViewModel startViewModelFromView = GetStartViewModelFromView();
			if (startViewModelFromView != null)
			{
				startViewModelFromView.CarouselModel.IsAnimationOn = true;
			}
		}
		else
		{
			_checkForWarningPopupsCallback(luaTable, arg2: false, null, "rental");
		}
	}

	private static TitleRollupOverlayButtonProperties GetTitleRollupOverlayButtonProperties(ITitleProduct titleProduct, bool isPurchase)
	{
		TitleRollupOverlayButtonProperties titleRollupOverlayButtonProperties = new TitleRollupOverlayButtonProperties
		{
			Visibility = Visibility.Collapsed
		};
		if (titleProduct != null)
		{
			string arg = ((titleProduct.TitleType == TitleType.DVD) ? "DVD" : ((titleProduct.TitleType == TitleType.Bluray) ? "BLU-RAY" : null));
			titleRollupOverlayButtonProperties.ButtonText = BundleFunctions.GetDictionaryStringValue("strings", "title_rollup_overlay_unavailable_text");
			if (_isProductInShoppingCartCallback != null && _isProductInShoppingCartCallback(titleProduct.ToDictionary().ToLuaTable()))
			{
				titleRollupOverlayButtonProperties.ButtonText = string.Format(BundleFunctions.GetDictionaryStringValue("strings", "title_rollup_overlay_cart_text"), arg);
				titleRollupOverlayButtonProperties.Visibility = Visibility.Visible;
			}
			else if (!titleProduct.InStock)
			{
				titleRollupOverlayButtonProperties.ButtonText = string.Format(BundleFunctions.GetDictionaryStringValue("strings", "title_rollup_overlay_oos_text"), arg);
				titleRollupOverlayButtonProperties.Visibility = Visibility.Visible;
			}
			else if (isPurchase)
			{
				decimal moviePurchasePrice = titleProduct.MoviePurchasePrice;
				if (moviePurchasePrice > 0m)
				{
					titleRollupOverlayButtonProperties.ButtonText = string.Format(BundleFunctions.GetDictionaryStringValue("strings", "title_rollup_overlay_buy_text"), arg, BundleFunctions.GetDictionaryStringValue("strings", "currency_symbol"), $"{moviePurchasePrice:N2}");
					titleRollupOverlayButtonProperties.Visibility = Visibility.Visible;
					titleRollupOverlayButtonProperties.Enabled = true;
				}
			}
			else if (titleProduct.IsAvailable && titleProduct.InStock)
			{
				titleRollupOverlayButtonProperties.ButtonText = string.Format(BundleFunctions.GetDictionaryStringValue("strings", "title_rollup_overlay_add_text"), arg);
				titleRollupOverlayButtonProperties.Visibility = Visibility.Visible;
				titleRollupOverlayButtonProperties.Enabled = true;
			}
		}
		return titleRollupOverlayButtonProperties;
	}

	private static void SetBrowseFiltersBasedOnSelectedProduct(ITitleProduct titleProduct)
	{
		if (titleProduct != null)
		{
			string key = ((titleProduct.TitleFamily == TitleFamily.Games) ? "browse_family_games" : "browse_family_movies");
			OrchestrationService.SetMacro("Browse:ProductFamily", BundleFunctions.GetDictionaryStringValue("strings", key));
			string key2 = ((titleProduct.TitleFamily == TitleFamily.Games) ? "Games" : "Movies");
			OrchestrationService.SetMacro("TTS:BrowsingFamily", BundleFunctions.GetDictionaryStringValue("strings", key2));
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily = titleProduct.TitleFamily;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleType = TitleType.All;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.Genre = null;
		}
	}

	private static void CarouselModel_OnDisplayProductModelSelected(DisplayProductModel displayProductModel)
	{
		if (!(displayProductModel.Data is IKioskProduct kioskProduct))
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
				GetStartViewModelFromView().CarouselModel.IsAnimationOn = false;
				SetBrowseFiltersBasedOnSelectedProduct(browseFiltersBasedOnSelectedProduct);
				OrchestrationService.ExecuteScript("browse_products");
				Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductSelectedFromCarousel = true;
				IViewService service = ServiceLocator.Instance.GetService<IViewService>();
				Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductDetailShownFromView = service.Peek();
				PrepareToLeaveStartView();
				OrchestrationService.ViewShow("product_detail_view");
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
		if (!(kioskProduct is ICollectionProduct collectionProduct))
		{
			return;
		}
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.CarouselCollectionsSelected = true;
		LuaTable luaTable = null;
		if (collectionProduct.Id != 0L)
		{
			luaTable = OrchestrationService.RentalGetCollection(collectionProduct.Id, null, null, null, null);
		}
		OrchestrationService.ViewShow("browse_view");
		if (luaTable != null)
		{
			if (_showCollectionItemsCallback != null)
			{
				_showCollectionItemsCallback(luaTable);
			}
		}
		else if (_showBrowseViewCollectionsCallback != null)
		{
			_showBrowseViewCollectionsCallback();
		}
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
		bool flag = ServiceLocator.Instance.GetService<IStoreManager>()?.CanSellUsedGames ?? false;
		startViewModel.AllMoviesButtonTextLine1 = BundleFunctions.GetDictionaryStringValue("strings", "movie_button_rent_label");
		startViewModel.AllMoviesButtonTextLine2 = BundleFunctions.GetDictionaryStringValue("strings", "Alt_StartView_Movies");
		startViewModel.PickUpButtonText = BundleFunctions.GetDictionaryStringValue("strings", "pickup");
		startViewModel.ReturnButtonText = BundleFunctions.GetDictionaryStringValue("strings", "return_button_label");
		startViewModel.ToggleLanguageButtonText = BundleFunctions.GetDictionaryStringValue("strings", "toggle_language_button_label");
		startViewModel.BurnInViewLabelText = BundleFunctions.GetDictionaryStringValue("strings", "burn_in_view_label");
		startViewModel.BuyMoviesOrGamesButtonTextLine1 = BundleFunctions.GetDictionaryStringValue("strings", "buy_movies_or_games_line1");
		if (flag)
		{
			startViewModel.BuyMoviesOrGamesButtonTextLine1 = startViewModel.BuyMoviesOrGamesButtonTextLine1 + " " + BundleFunctions.GetDictionaryStringValue("strings", "buy_movies_or_games_line1_games");
		}
		startViewModel.BuyMoviesOrGamesButtonTextLine2 = BundleFunctions.GetDictionaryStringValue("strings", "buy_movies_or_games_line2");
		startViewModel.BuyMoviesOrGamesButtonTextLine1Style = (OrchestrationService.IsCultureSpanish() ? GetStyle(startViewUserControl, "font_bryant_bold_14") : GetStyle(startViewUserControl, "font_bryant_bold_16"));
		startViewModel.BuyMoviesOrGamesButtonTextLine2Style = GetStyle(startViewUserControl, "font_bryant_bold_28");
		startViewModel.AllMoviesButtonTextLine2Style = GetStyle(startViewUserControl, OrchestrationService.IsCultureSpanish() ? "font_bryant_bold_28" : "font_bryant_bold_34");
		if (hasInstockGames)
		{
			if (flag)
			{
				startViewModel.GamesButtonTextLine1 = BundleFunctions.GetDictionaryStringValue("strings", "games_button_rent_and_buy_label");
			}
			else
			{
				startViewModel.GamesButtonTextLine1 = BundleFunctions.GetDictionaryStringValue("strings", "games_button_rent_label");
			}
			startViewModel.GamesButtonTextLine1Style = GetStyle(startViewUserControl, (flag && OrchestrationService.IsCultureSpanish()) ? "font_bryant_bold_18" : "font_bryant_bold_20");
			startViewModel.GamesButtonTextLine2 = BundleFunctions.GetDictionaryStringValue("strings", "games_button_games_label");
		}
		startViewModel.AdaTurnedOnLabelVisibility = ((!ApplicationState.Instance.IsADAAccessible) ? Visibility.Collapsed : Visibility.Visible);
		startViewModel.AdaTurnedOnLabelText = BundleFunctions.GetDictionaryStringValue("strings", "ada_turned_on_label_text");
	}

	private static Style GetStyle(FrameworkElement frameworkElement, string styleName)
	{
		Style result = null;
		object obj = frameworkElement.FindResource(styleName);
		if (obj != null)
		{
			result = obj as Style;
		}
		return result;
	}

	private static void PrepareToLeaveStartView()
	{
		CloseTitleRollupOverlay();
		StartViewModel startViewModelFromView = GetStartViewModelFromView();
		if (startViewModelFromView != null && startViewModelFromView.CarouselModel != null)
		{
			startViewModelFromView.CarouselModel.IsAnimationOn = false;
		}
	}

	private static void GoToMovieBrowseView()
	{
		PrepareToLeaveStartView();
		GoToBrowseView("all_movies_button", TitleFamily.Movies, "browse_products");
	}

	private static void GoToForSaleMovieBrowseView()
	{
		PrepareToLeaveStartView();
		GoToBrowseView("buy_movies_or_games_button", TitleFamily.Movies, "browse_products");
	}

	private static void GoToGameView()
	{
		PrepareToLeaveStartView();
		string scriptToRun = ((Redbox.Rental.Services.Configuration.Configuration.Instance.EnablePlatformScreen && !ApplicationState.Instance.IsABEOn) ? "select_platform" : "browse_products");
		GoToBrowseView("games_button", TitleFamily.Games, scriptToRun);
	}

	private static void GoToBrowseView(string pressedButtonName, TitleFamily titleFamily, string scriptToRun)
	{
		OrchestrationService.CreateStartViewAnalytics(AnalyticsSessionType.BrowseVend);
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", pressedButtonName);
		OrchestrationService.RemoveTimer("carousel-wait");
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleFamily = titleFamily;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleType = TitleType.All;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.Genre = null;
		if (pressedButtonName == "buy_movies_or_games_button")
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseMode = BrowseViewFilter.ForSale;
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectedSort = "for-sale";
		}
		if (ApplicationState.Instance.IsABEOn)
		{
			ApplicationState.Instance.IsADAAccessible = true;
			string key = ((titleFamily == TitleFamily.Games) ? "browse_family_games" : "browse_family_movies");
			OrchestrationService.SetMacro("Browse:ProductFamily", BundleFunctions.GetDictionaryStringValue("strings", key));
		}
		string key2 = ((titleFamily == TitleFamily.Games) ? "Games" : "Movies");
		OrchestrationService.SetMacro("TTS:BrowsingFamily", BundleFunctions.GetDictionaryStringValue("strings", key2));
		OrchestrationService.ExecuteScript(scriptToRun);
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
}
