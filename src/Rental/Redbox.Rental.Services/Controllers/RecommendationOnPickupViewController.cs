using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Promotion;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class RecommendationOnPickupViewController : BaseLoyaltyController
{
	private class Constants
	{
		public static string RecommendationOnPickupIdleTimerName = "recommendation_on_pickup_idle_timer";

		public static int RecommendationOnPickupIdleTimerDuration = 40000;
	}

	private class ReservationValues
	{
		public string EmailAddress { get; set; }

		public string ReferenceNumber { get; set; }

		public List<long> CustomerHistory { get; set; }

		public decimal PromoAmount { get; set; }

		public string PromoCode { get; set; }

		public ReservationValues()
		{
			CustomerHistory = new List<long>();
		}
	}

	private static ReservationValues _reservationValues;

	private static RentalShoppingCartChanged ShoppingCartChangeEventHandler;

	private static IIdleTimerService IdleTimerService => ServiceLocator.Instance.GetService<IIdleTimerService>();

	private static IRentalShoppingCart RentalShoppingCart => BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;

	public static void ConfigureView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.StopIdleTimer("idle_timer");
			RecommendationOnPickupViewModel recommendationOnPickupViewModel = new RecommendationOnPickupViewModel();
			ConfigureShoppingCart(recommendationOnPickupViewModel);
			ConfigureShoppingCartChangeHandler();
			ConfigureRecommendedTitlesBrowse(recommendationOnPickupViewModel);
			ConfigureTextAndVisibility(recommendationOnPickupViewModel);
			ConfigureCommands(recommendationOnPickupViewModel);
			ConfigureRecommendationOnPickupViewIdleTimer();
			viewUserControl.DataContext = recommendationOnPickupViewModel;
			CommandManager.InvalidateRequerySuggested();
		}
	}

	private static void ConfigureRecommendationOnPickupViewIdleTimer()
	{
		IIdleTimer obj = IdleTimerService?.CreateIdleTimer(Constants.RecommendationOnPickupIdleTimerName);
		obj?.Initialize(Constants.RecommendationOnPickupIdleTimerDuration, delegate
		{
			OrchestrationService.StopIdleTimer(Constants.RecommendationOnPickupIdleTimerName);
			LogHelper.Instance.Log("recommendation on pickup view: idle timer expired");
			if (ServiceLocator.Instance.GetService<IViewService>()?.Peek() == "recommendation_on_pickup_view")
			{
				MessagePopupViewController.ShowRecommendationOnPickupPrompt(delegate
				{
					OrchestrationService.ViewGoBackSafe("recommendation on pickup prompt timed out", popOnly: false);
					ReservationFunctions.InvokeVendOnlyReservedTitlesCallback(_reservationValues?.EmailAddress, _reservationValues?.ReferenceNumber);
				}, delegate
				{
					OrchestrationService.ViewGoBackSafe("recommendation on pickup prompt - vend only reserved titles", popOnly: false);
					ReservationFunctions.InvokeVendOnlyReservedTitlesCallback(_reservationValues?.EmailAddress, _reservationValues?.ReferenceNumber);
				}, delegate
				{
					OrchestrationService.AnalyticsAddViewEvent("buttonPress", "keep_browsing_button");
					OrchestrationService.ViewGoBackSafe("recommendation on pickup prompt - keep browsing", popOnly: false);
				});
			}
		});
		obj?.Start();
	}

	private static void ConfigureRecommendedTitlesBrowse(RecommendationOnPickupViewModel recommendationOnPickupViewModel)
	{
		LogHelper.Instance.Log("RecommendationOnPickupController.ConfigureRecommendedTitlesBrowse");
		bool includeBluray = RentalShoppingCart?.Items.Any((IRentalShoppingCartItem x) => x.SourceType == ItemSourceType.Reservation && x.TitleType == TitleType.Bluray) ?? false;
		List<long> list = new List<long>();
		list.AddRange(_reservationValues.CustomerHistory);
		list.AddRange(RentalShoppingCart?.Items.Where((IRentalShoppingCartItem x) => x.SourceType == ItemSourceType.Reservation).Select((IRentalShoppingCartItem x) => x.ProductId));
		List<IKioskProduct> list2 = GenerateRecommendations(includeBluray, list);
		if (Redbox.Rental.Services.Session.Session.Instance?.BrowseState != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseProductsShown = list2;
		}
		BrowseControlModel browseControlModel = new BrowseControlModel();
		browseControlModel.RowCount = 1;
		browseControlModel.ColumnCount = 6;
		browseControlModel.BrowseItemMargin = new Thickness(6.0, 18.0, 6.0, 8.0);
		browseControlModel.ShowBorderItems = false;
		browseControlModel.BorderShadowVisibility = Visibility.Collapsed;
		browseControlModel.ShowNoResultsMessage = false;
		browseControlModel.PageNumberVisibility = Visibility.Collapsed;
		browseControlModel.OnAddBrowseItemModel += BrowseProductControl_OnAddDisplayProductModel;
		browseControlModel.BrowseItems = CreateBrowseItemsFromKioskProducts(list2);
		for (int num = 0; num < browseControlModel.RowCount * browseControlModel.ColumnCount * 3; num++)
		{
			browseControlModel.BrowseControls.Add(new DisplayProductWithTextFooterUserControl
			{
				Name = $"product_{num + 1}",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			});
		}
		recommendationOnPickupViewModel.BrowseProductControlModel = browseControlModel;
	}

	private static void ConfigureShoppingCartChangeHandler()
	{
		if (ShoppingCartChangeEventHandler == null)
		{
			ShoppingCartChangeEventHandler = CurrentRentalShoppingCart_OnChange;
			RentalShoppingCart.OnChange += ShoppingCartChangeEventHandler;
		}
	}

	public static void CleanupRecommendationOnPickupView()
	{
		OrchestrationService.RemoveIdleTimer(Constants.RecommendationOnPickupIdleTimerName);
		if (RentalShoppingCart != null && ShoppingCartChangeEventHandler != null)
		{
			try
			{
				RentalShoppingCart.OnChange -= ShoppingCartChangeEventHandler;
			}
			catch
			{
			}
			ShoppingCartChangeEventHandler = null;
		}
	}

	private static RecommendationOnPickupViewModel GetRecommendationOnPickupViewModelFromView()
	{
		FrameworkElement recommendationOnPickupViewUserControl = BaseViewController.GetViewUserControl();
		if (recommendationOnPickupViewUserControl != null)
		{
			return recommendationOnPickupViewUserControl.Dispatcher.Invoke(() => recommendationOnPickupViewUserControl?.DataContext as RecommendationOnPickupViewModel);
		}
		return null;
	}

	private static void CurrentRentalShoppingCart_OnChange(IRentalShoppingCart rentalShoppingCart)
	{
		RecommendationOnPickupViewModel recommendationOnPickupViewModelFromView = GetRecommendationOnPickupViewModelFromView();
		if (recommendationOnPickupViewModelFromView != null)
		{
			ConfigureShoppingCart(recommendationOnPickupViewModelFromView);
			ConfigureTextAndVisibility(recommendationOnPickupViewModelFromView);
			if (recommendationOnPickupViewModelFromView?.BrowseProductControlModel?.BrowseItems != null)
			{
				UpdateDisplayProductModels(recommendationOnPickupViewModelFromView.BrowseProductControlModel.BrowseItems);
			}
		}
	}

	private static void UpdateDisplayProductModels(List<IBrowseItemModel> displayProductModels)
	{
		foreach (IBrowseItemModel displayProductModel in displayProductModels)
		{
			if (displayProductModel is DisplayProductWithTextFooterModel displayProductWithTextFooterModel)
			{
				ConfigureDisplayProductModel(displayProductWithTextFooterModel.Data as IKioskProduct, displayProductWithTextFooterModel);
			}
		}
	}

	private static List<IBrowseItemModel> CreateBrowseItemsFromKioskProducts(List<IKioskProduct> kioskProducts)
	{
		List<IBrowseItemModel> list = new List<IBrowseItemModel>();
		if (kioskProducts != null)
		{
			foreach (IKioskProduct kioskProduct in kioskProducts)
			{
				DisplayProductWithTextFooterModel displayProductWithTextFooterModel = new DisplayProductWithTextFooterModel
				{
					Name = kioskProduct.ProductName,
					Data = kioskProduct,
					Image = OrchestrationService.GetProductBitmapImage(kioskProduct),
					LockAspectRatio = true
				};
				ConfigureDisplayProductModel(kioskProduct, displayProductWithTextFooterModel);
				list.Add(displayProductWithTextFooterModel);
			}
		}
		return list;
	}

	private static void ConfigureDisplayProductModel(IKioskProduct eachKioskProduct, DisplayProductWithTextFooterModel browseItemModel)
	{
		browseItemModel.ConfigureForBrowse();
		ITitleProduct titleProduct = eachKioskProduct as ITitleProduct;
		IRentalShoppingCart rentalShoppingCart = RentalShoppingCart;
		bool flag = (browseItemModel.CanAdd = rentalShoppingCart != null && !rentalShoppingCart.HasItem(titleProduct.ProductId) && !(RentalShoppingCart?.IsFull ?? true));
		browseItemModel.ImageOpacity = (flag ? 1.0 : 0.4);
		browseItemModel.CornerAddButtonVisibility = ((!flag) ? Visibility.Collapsed : Visibility.Visible);
		browseItemModel.AddButtonStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "classic_rubine_button_style");
		browseItemModel.FooterTextStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "font_gray_montserrat_medium_16");
		IRentalShoppingCart rentalShoppingCart2 = RentalShoppingCart;
		if (rentalShoppingCart2 != null && rentalShoppingCart2.HasItem(titleProduct.ProductId))
		{
			browseItemModel.FooterText = Resources.in_cart;
		}
		else
		{
			IPricingRecord pricingRecord = (ServiceLocator.Instance.GetService<IPricingService>()?.GetProductPricingRecords(titleProduct))?[0];
			browseItemModel.FooterText = ((pricingRecord != null) ? $"{pricingRecord.InitialNight:c2} / {Resources.night}" : null);
		}
		BannerType bannerType = BannerType.None;
		bannerType = BrowseFunctions.GetProductBannerType(titleProduct);
		bool hideBanner = false;
		BrowseFunctions.ConfigureBanner(browseItemModel, titleProduct, hideBanner, bannerType, BaseViewController.GetViewUserControl());
	}

	private static void BrowseProductControl_OnAddDisplayProductModel(IBrowseItemModel browseItemModel, object parameter)
	{
		BaseViewController.ProcessWPFHit();
		if (browseItemModel.Data is ITitleProduct titleProduct)
		{
			BrowseFunctions.AddProductToCart(titleProduct, isPurchase: false, null, ItemSourceType.Recommendation);
		}
	}

	public static List<IKioskProduct> GenerateRecommendations(bool includeBluray, List<long> excludeProducts)
	{
		List<IKioskProduct> list = new List<IKioskProduct>();
		LogHelper.Instance.Log("Generating recommendation list on top 20");
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (service != null)
		{
			List<long> list2 = new List<long> { 1139L };
			list2.AddRange(excludeProducts);
			List<IKioskProduct> products = null;
			service.GetProducts(BrowseViewFilter.All, TitleFamily.All, TitleType.All, BrowseSort.MerchandiseDate, 1093L, null, list2, isADAView: true, out products);
			List<IKioskProduct> list3 = new List<IKioskProduct>();
			foreach (IKioskProduct item in products)
			{
				if (item is ITitleProduct { IsAvailable: not false, InStock: not false, SellThruNew: false, DoNotRent: false, KioskProductType: not KioskProductType.MDV } titleProduct && (titleProduct.TitleType == TitleType.DVD || titleProduct.TitleType == TitleType.Bluray) && (includeBluray || titleProduct.TitleType == TitleType.DVD))
				{
					list3.Add(item);
				}
			}
			new List<IKioskProduct>();
			list3.Sort(delegate(IKioskProduct x, IKioskProduct y)
			{
				ITitleProduct titleProduct3 = x as ITitleProduct;
				ITitleProduct titleProduct4 = y as ITitleProduct;
				if (titleProduct3 == null)
				{
					return -1;
				}
				if (titleProduct4 == null)
				{
					return 1;
				}
				int num = titleProduct3.TitleType.CompareTo(titleProduct4.TitleType) * -1;
				if (num == 0)
				{
					num = titleProduct3.MerchandiseDate.CompareTo(titleProduct4.MerchandiseDate);
					if (num == 0)
					{
						num = titleProduct3.ProductId.CompareTo(titleProduct4.ProductId);
					}
				}
				return num;
			});
			list.AddRange(list3.Take(6));
		}
		foreach (IKioskProduct item2 in list)
		{
			ITitleProduct titleProduct2 = item2 as ITitleProduct;
			LogHelper.Instance.Log($"recommendation list: adding {titleProduct2?.ProductId}");
		}
		return list;
	}

	private static void ConfigureCommands(RecommendationOnPickupViewModel recommendationOnPickupViewModel)
	{
		recommendationOnPickupViewModel.OnPickupButtonClicked = delegate
		{
			OrchestrationService.AnalyticsAddViewEvent("buttonPress", "pickup_button");
			ReservationFunctions.InvokeVendOnlyReservedTitlesCallback(_reservationValues?.EmailAddress, _reservationValues?.ReferenceNumber);
		};
		recommendationOnPickupViewModel.OnPayButtonClicked = delegate
		{
			OrchestrationService.AnalyticsAddViewEvent("buttonPress", "pay_button");
			ReservationFunctions.InvokeReservationAuthorizeCallback(_reservationValues?.EmailAddress, _reservationValues?.ReferenceNumber);
		};
		recommendationOnPickupViewModel.OnTermsAndPrivacyButtonClicked = delegate
		{
			OrchestrationService.ExecuteScript("help");
		};
	}

	private static void ConfigureShoppingCart(RecommendationOnPickupViewModel recommendationOnPickupViewModel)
	{
		if (RentalShoppingCart != null)
		{
			RentalShoppingCart.PickupRecommendationsShown = true;
			PopulateShoppingCartWithReservationItems(RentalShoppingCart);
			ConfigureShoppingCartControl(recommendationOnPickupViewModel);
		}
	}

	private static void PopulateShoppingCartWithReservationItems(IRentalShoppingCart rentalShoppingCart)
	{
		_reservationValues = new ReservationValues();
		ReservationInfo reservationInfo = ReservationFunctions.InvokeGetReservationInfoCallback();
		if (reservationInfo != null)
		{
			_reservationValues.EmailAddress = reservationInfo.EmailAddress;
			_reservationValues.ReferenceNumber = reservationInfo.ReferenceNumber;
			if (reservationInfo.CustomerHistory != null)
			{
				_reservationValues.CustomerHistory.AddRange(reservationInfo.CustomerHistory);
			}
			rentalShoppingCart?.SetDefaultServiceFee(reservationInfo.DefaultServiceFee);
			LogHelper.Instance.Log($"Setting cart default service fee to {reservationInfo.DefaultServiceFee} from reservation.");
			decimal promoAmount = 0m;
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			if (reservationInfo.Barcodes != null)
			{
				foreach (string barcode in reservationInfo.Barcodes)
				{
					IKioskProduct kioskProduct = null;
					ErrorList productByBarcode = service.GetProductByBarcode(barcode, out kioskProduct);
					if (productByBarcode == null || productByBarcode.Count == 0)
					{
						ITitleProduct titleProduct = kioskProduct as ITitleProduct;
						if (titleProduct != null)
						{
							ReservationTitle reservationTitle = reservationInfo?.ReservationData?.Titles.FirstOrDefault((ReservationTitle x) => x.ProductId == titleProduct.ProductId);
							if (reservationTitle != null)
							{
								IRentalShoppingCartItem rentalShoppingCartItem = BaseViewController.RentalShoppingCartService?.CreateRentalShoppingCartItemFromTitleProduct(titleProduct);
								rentalShoppingCartItem.SourceType = ItemSourceType.Reservation;
								rentalShoppingCartItem.Barcode = barcode;
								rentalShoppingCartItem.Action = reservationTitle.Action;
								rentalShoppingCartItem.Quantity = 1m;
								rentalShoppingCartItem.Status = LineItemStatus.Pending;
								IPricingRecord pricingRecord = reservationInfo.PricingSet?.PriceRecords.FirstOrDefault((IPricingRecord x) => (titleProduct.TitleFamily == TitleFamily.Movies && titleProduct.TitleType == TitleType.DVD && x.TitleType == TitleType.DVD && x.TitleFamily == TitleFamily.Movies) || (titleProduct.TitleFamily == TitleFamily.Movies && titleProduct.TitleType == TitleType.Bluray && x.TitleType == TitleType.Bluray && x.TitleFamily == TitleFamily.Movies) || (titleProduct.TitleFamily == TitleFamily.Movies && titleProduct.TitleType == TitleType._4KUHD && x.TitleType == TitleType._4KUHD && x.TitleFamily == TitleFamily.Movies) || (titleProduct.TitleFamily == TitleFamily.Games && x.TitleType == TitleType.All && x.TitleFamily == TitleFamily.Games));
								if (pricingRecord != null)
								{
									pricingRecord = pricingRecord.Clone();
									pricingRecord.InitialNight = reservationTitle.Price;
								}
								if (reservationInfo.ReservationData.AppliedTitleMarketing)
								{
									promoAmount += reservationTitle.Discount;
								}
								if (!rentalShoppingCart.HasItem(titleProduct.ProductId))
								{
									rentalShoppingCart.AddItem(rentalShoppingCartItem, pricingRecord);
									LogHelper.Instance.Log($"Adding reserved product {rentalShoppingCartItem.ProductId} to recommendation on pickup cart");
								}
							}
						}
						else
						{
							LogHelper.Instance.Log($"RecommendationOnPickupViewController.PopulateShoppingCartWithReservationItems - barcode {barcode} couldn't find a product.", LogEntryType.Info);
						}
					}
					else
					{
						LogHelper.Instance.Log($"RecommendationOnPickupViewController.PopulateShoppingCartWithReservationItems - dataService.GetProductByBarcode returned errors: {productByBarcode}", LogEntryType.Info);
					}
				}
			}
			else
			{
				LogHelper.Instance.Log("RecommendationOnPickupViewController.PopulateShoppingCartWithReservationItems barcodes is null.", LogEntryType.Info);
			}
			if (reservationInfo.ReservationData.AppliedHiveOnlinePromo)
			{
				_reservationValues.PromoAmount = reservationInfo.ReservationData.HiveOnlinePromoDiscount;
				LogHelper.Instance.Log($"recommendation_on_pickup_view_enter:- applied hive online promo, reserved discount amount: {_reservationValues.PromoAmount}");
			}
			else if (reservationInfo.ReservationData.AppliedTitleMarketing)
			{
				_reservationValues.PromoAmount = promoAmount;
				LogHelper.Instance.Log($"recommendation_on_pickup_view_enter:- applied title marketing promo, reserved discount amount: {_reservationValues.PromoAmount}");
			}
			if (_reservationValues.PromoAmount > 0m)
			{
				IPromotion promotion = new Redbox.Rental.Services.Promotion.Promotion
				{
					Name = reservationInfo.ReservationData.PromoCode,
					OriginalPrice = _reservationValues.PromoAmount,
					Price = _reservationValues.PromoAmount,
					Quantity = 1,
					Intent = PromotionIntentCode.FixedValue,
					OriginalPercent = 0m,
					Percent = 0m,
					RentQuantity = 0,
					RentFormat = null,
					GetQuantity = 0,
					GetFormat = null,
					CampaignTitles = null
				};
				PromotionService.Instance.SetPromotion(promotion);
				PromotionService.Instance.SetShoppingCartDiscount(rentalShoppingCart?.ShoppingSessionId.ToString(), promotion);
				BaseViewController.RentalShoppingCartService.CurrentRentalShoppingCart.ValidateAndUpdatePromoCode();
				BaseViewController.RentalShoppingCartService.CurrentRentalShoppingCart.UpdateServiceFee();
			}
		}
		else
		{
			LogHelper.Instance.Log("RecommendationOnPickupViewController.PopulateShoppingCartWithReservationItems reservationInfo is null.", LogEntryType.Info);
		}
	}

	private static void ConfigureShoppingCartControl(RecommendationOnPickupViewModel recommendationOnPickupViewModel)
	{
		if (recommendationOnPickupViewModel != null)
		{
			BrowseControlModel browseControlModel = new BrowseControlModel();
			browseControlModel.BackgroundColor = Colors.Transparent;
			browseControlModel.ShowBorderItems = false;
			browseControlModel.PageNumberVisibility = Visibility.Collapsed;
			browseControlModel.BorderShadowVisibility = Visibility.Collapsed;
			browseControlModel.Margin = new Thickness(0.0, 0.0, 0.0, 8.0);
			browseControlModel.BrowseItemMargin = new Thickness(16.0, 12.0, 16.0, 0.0);
			browseControlModel.RowCount = 1;
			browseControlModel.ColumnCount = 5;
			for (int i = 0; i < RentalShoppingCart.MaximumCapacity; i++)
			{
				browseControlModel.BrowseControls.Add(new DisplayProductWithTextFooterUserControl
				{
					Name = $"shopping_cart_product_{i + 1}",
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				});
			}
			browseControlModel.OnCancelBrowseItemModel += ShoppingCartControl_OnCancelDisplayProductModel;
			browseControlModel.BrowseItems = CreateBrowseItemsFromCartItems();
			recommendationOnPickupViewModel.ShoppingCartControlModel = browseControlModel;
		}
		else
		{
			LogHelper.Instance.Log("RecommendationOnPickupViewController.ConfigureShoppingCartControl recommendationOnPickupViewModel is null.", LogEntryType.Info);
		}
	}

	private static void ShoppingCartControl_OnCancelDisplayProductModel(IBrowseItemModel browseItemModel, object parameter)
	{
		BaseViewController.ProcessWPFHit();
		if (browseItemModel == null)
		{
			return;
		}
		IRentalShoppingCartItem rentalShoppingCartItem = browseItemModel.Data as IRentalShoppingCartItem;
		if (RentalShoppingCart != null && rentalShoppingCartItem != null)
		{
			IRentalShoppingCartItem rentalShoppingCartItem2 = RentalShoppingCart.Items.FirstOrDefault((IRentalShoppingCartItem x) => x.ProductId == rentalShoppingCartItem.ProductId);
			if (rentalShoppingCartItem2 != null)
			{
				BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "shopping-cart-remove-item");
				RentalShoppingCart.RemoveItem(rentalShoppingCartItem2, userRemoved: true);
			}
		}
	}

	private static List<IBrowseItemModel> CreateBrowseItemsFromCartItems()
	{
		List<IBrowseItemModel> list = new List<IBrowseItemModel>();
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (rentalShoppingCart != null && service != null)
		{
			foreach (IRentalShoppingCartItem item in rentalShoppingCart.Items)
			{
				IKioskProduct kioskProduct = null;
				service.GetProduct(item.ProductId, excludeTitleRollup: true, out kioskProduct);
				if (kioskProduct != null)
				{
					DisplayProductWithTextFooterModel displayProductWithTextFooterModel = new DisplayProductWithTextFooterModel
					{
						Name = kioskProduct.ProductName,
						Data = item,
						Image = OrchestrationService.GetProductBitmapImage(kioskProduct)
					};
					displayProductWithTextFooterModel.ConfigureForRecommendationOnPickupCart();
					displayProductWithTextFooterModel.RecommendationOnPickupCartCancelButtonVisibility = ((item.SourceType == ItemSourceType.Reservation) ? Visibility.Collapsed : Visibility.Visible);
					displayProductWithTextFooterModel.FooterTextStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "font_white_montserrat_medium_16");
					if (item.SourceType == ItemSourceType.Reservation)
					{
						displayProductWithTextFooterModel.FooterText = Resources.reserved_title;
					}
					else
					{
						displayProductWithTextFooterModel.FooterText = $"{item.Price:c2} / {Resources.night}";
					}
					ITitleProduct titleProduct = kioskProduct as ITitleProduct;
					BannerType bannerType = BannerType.None;
					bannerType = BrowseFunctions.GetProductBannerType(titleProduct);
					bool hideBanner = false;
					BrowseFunctions.ConfigureBanner(displayProductWithTextFooterModel, titleProduct, hideBanner, bannerType, BaseViewController.GetViewUserControl());
					list.Add(displayProductWithTextFooterModel);
				}
				else
				{
					LogHelper.Instance.Log($"RecommendationOnPickupViewController.CreateBrowseItemsFromCartItems productId: {item.ProductId} was not found", LogEntryType.Info);
				}
			}
		}
		else
		{
			LogHelper.Instance.Log("RecommendationOnPickupViewController.CreateBrowseItemsFromCartItems - IDataService or RentalShoppingCart not found", LogEntryType.Info);
		}
		return list;
	}

	private static void ConfigureTextAndVisibility(RecommendationOnPickupViewModel recommendationOnPickupViewModel)
	{
		if (recommendationOnPickupViewModel == null)
		{
			return;
		}
		OrchestrationService.SetCurrentUICulture();
		recommendationOnPickupViewModel.AddMoreText = Resources.add_more;
		recommendationOnPickupViewModel.TermsAndPrivacyButtonText = Resources.terms_privacy;
		recommendationOnPickupViewModel.ChargedOnceText = Resources.charged_once;
		recommendationOnPickupViewModel.AgreeToTermsText = Resources.by_pressing_pay;
		int num = RentalShoppingCart?.Items.Count((IRentalShoppingCartItem x) => x.SourceType != ItemSourceType.Reservation) ?? 0;
		recommendationOnPickupViewModel.PayButtonText = Resources.pay_button;
		recommendationOnPickupViewModel.PayButtonVisibility = ((num <= 0) ? Visibility.Collapsed : Visibility.Visible);
		recommendationOnPickupViewModel.PickupButtonText = Resources.pickup;
		recommendationOnPickupViewModel.PickupButtonVisibility = ((num != 0) ? Visibility.Collapsed : Visibility.Visible);
		int num2 = RentalShoppingCart?.Items.Count((IRentalShoppingCartItem x) => x.TitleFamily == TitleFamily.Movies && x.SourceType == ItemSourceType.Reservation) ?? 0;
		int num3 = RentalShoppingCart?.Items.Count((IRentalShoppingCartItem x) => x.TitleFamily == TitleFamily.Games && x.SourceType == ItemSourceType.Reservation) ?? 0;
		int num4 = RentalShoppingCart?.Items.Count((IRentalShoppingCartItem x) => x.SourceType == ItemSourceType.Reservation) ?? 0;
		string format = Resources.pickup_discs;
		if (num4 == 1)
		{
			if (num2 == 1)
			{
				format = Resources.pickup_movie;
			}
			else if (num3 == 1)
			{
				format = Resources.pickup_game;
			}
		}
		else if (num2 == num4)
		{
			format = Resources.pickup_movies;
		}
		else if (num3 == num4)
		{
			format = Resources.pickup_games;
		}
		recommendationOnPickupViewModel.TitleText = string.Format(format, num4);
		bool flag = (RentalShoppingCart?.ServiceFee?.ActualAmount ?? 0m) > 0m;
		recommendationOnPickupViewModel.ServiceFeeMessage = (flag ? string.Format(Resources.service_fee_text, $"{(RentalShoppingCart?.ServiceFee.DefaultAmount ?? 0m) * 100m:f0}") : null);
		recommendationOnPickupViewModel.PricingModel.PricingLineMargin = new Thickness(0.0, 2.0, 0.0, 2.0);
		recommendationOnPickupViewModel.PricingModel.ReservedDiscsLabelText = Resources.totals_reserved_discs;
		recommendationOnPickupViewModel.PricingModel.AddedDiscsLabelText = Resources.totals_added_discs;
		recommendationOnPickupViewModel.PricingModel.ServiceFeeLabelText = Resources.service_fee_label;
		recommendationOnPickupViewModel.PricingModel.TaxLabelText = Resources.tax_label;
		recommendationOnPickupViewModel.PricingModel.TotalLabelText = Resources.total_label;
		recommendationOnPickupViewModel.PricingModel.DiscountLabelText = Resources.totals_promo_code;
		recommendationOnPickupViewModel.PricingModel.SubtotalLineVisibility = Visibility.Collapsed;
		recommendationOnPickupViewModel.PricingModel.ServiceFeeLineVisibility = ((!flag) ? Visibility.Collapsed : Visibility.Visible);
		string text = (ServiceLocator.Instance.GetService<IDataService>()?.GetStore())?.State;
		string text2 = $"{((!((RentalShoppingCart?.Totals?.DiscountAmountUsed ?? 0m) != 0m)) ? ((decimal?)null) : RentalShoppingCart?.Totals?.DiscountAmountUsed):C}";
		if (!string.IsNullOrEmpty(text2))
		{
			text2 = $"-{text2}";
		}
		recommendationOnPickupViewModel.PricingModel.DiscountValueText = text2;
		recommendationOnPickupViewModel.PricingModel.DiscountLineVisibility = ((!((RentalShoppingCart?.Totals?.DiscountAmountUsed ?? 0m) > 0m)) ? Visibility.Collapsed : Visibility.Visible);
		decimal num5 = RentalShoppingCart.Items.Where((IRentalShoppingCartItem x) => x.SourceType == ItemSourceType.Reservation).Sum((IRentalShoppingCartItem x) => x.Price);
		recommendationOnPickupViewModel.PricingModel.ReservedDiscsValueText = $"{num5:C}";
		decimal num6 = RentalShoppingCart.Items.Where((IRentalShoppingCartItem x) => x.SourceType != ItemSourceType.Reservation).Sum((IRentalShoppingCartItem x) => x.Price);
		recommendationOnPickupViewModel.PricingModel.AddedDiscsValueText = $"{num6:C}";
		recommendationOnPickupViewModel.PricingModel.ServiceFeeValueText = $"{RentalShoppingCart?.ServiceFee?.ActualAmount ?? 0m:C}";
		recommendationOnPickupViewModel.PricingModel.TaxLineVisibility = ((text == "SC" || text == "MD") ? Visibility.Collapsed : Visibility.Visible);
		recommendationOnPickupViewModel.PricingModel.TaxValueText = $"{RentalShoppingCart?.Totals?.TaxAmount:C}";
		recommendationOnPickupViewModel.PricingModel.TotalValueText = $"{RentalShoppingCart?.Totals?.GrandTotal:f2}";
	}
}
