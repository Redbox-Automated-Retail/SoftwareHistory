using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class TitleDetailsController : BaseViewController
{
	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance?.BrowseState;

	private static IViewService ViewService => ServiceLocator.Instance.GetService<IViewService>();

	public static void ConfigureView()
	{
		FrameworkElement userControl = GetUserControl();
		if (userControl != null)
		{
			userControl.DataContext = GetModel();
		}
		CommandManager.InvalidateRequerySuggested();
		ConfigureTTS();
		BaseViewController.StartIdleTimer();
	}

	private static FrameworkElement GetUserControl()
	{
		return BaseViewController.GetViewUserControl();
	}

	private static TitleDetailModel GetModel()
	{
		TitleDetailModel titleDetailModel = new TitleDetailModel();
		titleDetailModel.BackgroundImageVisibility = Visibility.Collapsed;
		titleDetailModel.BrowseProductControlModel = ConfigureBrowseControl();
		titleDetailModel.BuyButtonHit += OnBuyButtonHit;
		titleDetailModel.RentProduct += OnRentProduct;
		titleDetailModel.CollectionButtonHit += OnCollectionButton;
		titleDetailModel.MoreInfoHit += OnMoreInfoHit;
		titleDetailModel.GoBackHit += OnGoBack;
		titleDetailModel.BackText = Resources.back_button;
		return titleDetailModel;
	}

	private static void ConfigureTTS()
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement userControl = GetUserControl();
		TitleDetailModel titleDetailModel = userControl.DataContext as TitleDetailModel;
		if (service != null && service.TTSEnabled && userControl != null)
		{
			titleDetailModel.OnGetSpeechControl = () => TitleDetailViewTextToSpeech.GetSpeechControl(GetModelFromView);
		}
	}

	private static TitleDetailModel GetModelFromView()
	{
		return GetUserControl()?.DataContext as TitleDetailModel;
	}

	private static BrowseControlModel ConfigureBrowseControl()
	{
		OrchestrationService.SetCurrentUICulture();
		BrowseControlModel browseProductControl = new BrowseControlModel();
		browseProductControl.BackgroundColor = Brushes.Transparent.Color;
		browseProductControl.PageNumberVisibility = Visibility.Collapsed;
		browseProductControl.ShowPaddles = true;
		browseProductControl.ShowNoResultsMessage = false;
		browseProductControl.BorderShadowVisibility = Visibility.Collapsed;
		browseProductControl.ShowBorderItems = false;
		browseProductControl.OnAddBrowseItemModel += OnBuyButtonHit;
		browseProductControl.UseFullPageWidth = true;
		browseProductControl.BrowseItemMargin = new Thickness(0.0, 25.0, 0.0, 0.0);
		browseProductControl.RowCount = 1;
		browseProductControl.ColumnCount = 1;
		browseProductControl.BrowseItems = Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseItemModels?.Where((IBrowseItemModel x) => x is DisplayProductModel).ToList() ?? new List<IBrowseItemModel>();
		IKioskProduct selectedProduct = Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct;
		bool isAda = BaseViewController.IsADAMode;
		int num = browseProductControl.BrowseItems.FindIndex((IBrowseItemModel x) => (!isAda || !(selectedProduct is TitleRollupProduct)) ? (x.Data == selectedProduct) : TitleRollupUtilities.IsAdaDefaultTitle((ITitleProduct)x.Data, (TitleRollupProduct)selectedProduct)) + 1;
		ConfigurePage(browseProductControl, num - 1);
		ConfigurePage(browseProductControl, num);
		ConfigurePage(browseProductControl, num + 1);
		browseProductControl.SetPageNumber(num, PageChangeSource.None);
		browseProductControl.OnPageChanged += delegate(int prevPageNumber, int newPageNumber, PageChangeSource pageSource)
		{
			string subItemValue = ((prevPageNumber < newPageNumber) ? "product-detail-page-next" : "product-detail-page-back");
			BrowseState.BrowseSelectionSelectedProduct = browseProductControl.BrowseItems[newPageNumber - 1].Data as IKioskProduct;
			if (newPageNumber > prevPageNumber)
			{
				ConfigurePage(browseProductControl, newPageNumber + 1);
			}
			else if (newPageNumber < prevPageNumber)
			{
				ConfigurePage(browseProductControl, newPageNumber - 1);
			}
			ServiceLocator.Instance.GetService<ISessionAnalyticsService>()?.AddViewEvent("pageChange", subItemValue);
			ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddButtonPressEvent((prevPageNumber < newPageNumber) ? "TitleDetailsPageNext" : "TitleDetailsPageBack");
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductSelectedFromCarousel = false;
			ConfigureTTS();
		};
		if (selectedProduct is ITitleProduct)
		{
			ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddData(ProductData.ToProductData(selectedProduct as ITitleProduct));
		}
		if (browseProductControl.BrowseItems.FirstOrDefault((IBrowseItemModel x) => x.Data == Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct) is DisplayProductModel displayProductModel)
		{
			displayProductModel.ComingSoonButtonText = string.Format("{0}", BundleFunctions.GetDictionaryStringValue("strings", "email_optin_button_text"));
		}
		browseProductControl.BrowseControls = new List<Control>();
		browseProductControl.BrowseControls.Add(new TitleDetailDisplayProductUserControl());
		return browseProductControl;
	}

	private static void ConfigurePage(BrowseControlModel browseControlModel, int pageNumber)
	{
		if (browseControlModel?.BrowseItems != null && pageNumber >= 1 && pageNumber <= browseControlModel.BrowseItems.Count && browseControlModel.BrowseItems[pageNumber - 1] is DisplayProductModel displayProductModel)
		{
			BrowseViewController.ConfigureDisplayProductModel(displayProductModel.Data as IKioskProduct, displayProductModel, isADAMiniCart: false, displayProductModel.DisplayProductModelConstants);
			displayProductModel?.ConfigureForTitleDetail();
			PopulateTitleDetailProperties(displayProductModel);
		}
	}

	private static void OnGoBack()
	{
		GoBack("go_back");
	}

	private static void OnBuyButtonHit(IBrowseItemModel displayProductModel, object commandType)
	{
		_ = GetUserControl().DataContext;
		IKioskProduct kioskProduct = displayProductModel.Data as IKioskProduct;
		if (kioskProduct is TitleRollupProduct { IsEmptyCase: false } titleRollupProduct)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsBuy");
			OrchestrationService.ViewShow("purchase_popup_view", new RentalFormatSelectionViewParameter(titleRollupProduct, isPurchase: true));
			return;
		}
		ITitleProduct titleProduct = kioskProduct as ITitleProduct;
		BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "buy," + titleProduct.ProductId);
		BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsBuy")?.AddData(ProductData.ToProductData(titleProduct));
		BrowseFunctions.AddProductToCart(titleProduct, isPurchase: true);
	}

	private static void OnMoreInfoHit(IBrowseItemModel model)
	{
		IComingSoonOptinInfo comingSoonOptinInfo = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart?.ComingSoonOptInInfo;
		BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "see-related-collection");
		BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsSeeRelatedCollection");
		ITitleProduct titleProduct = model?.Data as ITitleProduct;
		if (titleProduct == null)
		{
			return;
		}
		OrchestrationService.ViewShow("keyboard_email_coming_soon_view", new KeyboardInfo
		{
			MaxLength = 64,
			KeyboardText = comingSoonOptinInfo.EmailAddress,
			CancelAction = delegate
			{
				ViewService?.GoBack("go back", popOnly: false);
			},
			IsValidEntry = ValidationServices.IsValidEmailAddress,
			ContinueAction = delegate(string keyboardText)
			{
				if (comingSoonOptinInfo != null && ViewService != null)
				{
					comingSoonOptinInfo.EmailAddress = keyboardText;
					comingSoonOptinInfo.AddProduct(titleProduct.ProductId);
					ViewService.UnwindTo("browse_view", discard: true, "KeyboardView: KeyboardMode.EMAIL_COMING_SOON  - NEXT button");
				}
			}
		});
	}

	private static void OnRent(long productId)
	{
		if (Redbox.Rental.Services.Session.Session.Instance.BrowseState.CarouselCollectionsSelected)
		{
			BaseViewController.SessionAnalyticsService?.AddViewEvent("CarouselCollection", "rent," + productId);
		}
		else if (Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductSelectedFromCarousel)
		{
			BaseViewController.SessionAnalyticsService?.AddViewEvent("Carousel", "rent," + productId);
		}
	}

	public static void OnRentProduct(IBrowseItemModel displayProductModel, object commandType, TitleType titleType, bool fromOosSelection = false)
	{
		if (!(displayProductModel?.Data is ITitleProduct titleProduct))
		{
			return;
		}
		ITitleProduct titleProduct2 = ((titleProduct is TitleRollupProduct) ? ((TitleRollupProduct)titleProduct).KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == titleType) : titleProduct);
		if (!fromOosSelection)
		{
			BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "rent," + titleProduct2.ProductId);
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsRentButton")?.AddData(ProductData.ToProductData(titleProduct2));
			OnRent(titleProduct2.ProductId);
		}
		if (titleProduct2.InStock | fromOosSelection)
		{
			IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
			bool flag = rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel;
			if ((titleProduct2.HasMultiNightPrice || (titleProduct2.IsSellableAndInStock && titleProduct2.IsMovie)) && !flag)
			{
				string viewAnalyticsName = "multi_night_pricing_popup_view";
				if (titleProduct2.HasMultiNightPrice && titleProduct2.IsSellableAndInStock)
				{
					viewAnalyticsName = "multi_night_pricing_with_buy_button_popup_view";
				}
				else if (!titleProduct2.HasMultiNightPrice && titleProduct2.IsSellableAndInStock)
				{
					viewAnalyticsName = "non_multi_night_pricing_with_buy_button_popup_view";
				}
				OrchestrationService.ViewShow("multi_night_pricing_popup_view", new MultiNightPricingPopupInfo
				{
					TitleProduct = titleProduct2,
					ShownFromView = "product_detail_view",
					ViewAnalyticsName = viewAnalyticsName
				});
			}
			else
			{
				AddToCart(titleProduct2, isPurchase: false);
			}
		}
		else
		{
			ShowOutOfStockOptionsDialog(displayProductModel, titleProduct, titleType, commandType);
		}
	}

	private static void ShowOutOfStockOptionsDialog(IBrowseItemModel displayProductModel, ITitleProduct kioskProduct, TitleType titleType, object commandType)
	{
		bool flag = kioskProduct is TitleRollupProduct;
		ITitleProduct selectedProduct = (flag ? ((TitleRollupProduct)kioskProduct).KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == titleType) : kioskProduct);
		OptionSelectionDialogInfo optionSelectionDialogInfo = new OptionSelectionDialogInfo
		{
			ViewAnalyticsName = "message_popup_view",
			ViewAnalyticsContext = "OutOfStockDialog",
			HeaderText = kioskProduct.ProductName,
			CancelButtonText = Resources.cancel_button
		};
		optionSelectionDialogInfo.CancelCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
		{
			BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "out_of_stock_cancel");
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsOutofStockCancel");
			GoBack("OutOfStockDialogCancel");
		}, (Func<bool>)null);
		optionSelectionDialogInfo.DescriptionText = Resources.out_of_stock_movie_message;
		ObservableCollection<ButtonData> observableCollection = new ObservableCollection<ButtonData>();
		if (flag && ((ITitleRollupProduct)kioskProduct).KioskProducts.Any((ITitleProduct p) => p.InStock))
		{
			optionSelectionDialogInfo.DescriptionText = string.Format(Resources.out_of_stock_format_message, titleType.GetDisplayDescription());
			foreach (ITitleProduct inStockFormat in ((ITitleRollupProduct)kioskProduct).KioskProducts.Where((ITitleProduct p) => p.InStock && p.TitleType != TitleType.DigitalCode))
			{
				IPricingRecord pricingRecord = ServiceLocator.Instance.GetService<IPricingService>()?.GetOneDayProductPricingRecord(inStockFormat.ProductId);
				new TitleRollupUtilities.TitleDetailFormatRentalButtonTextValues();
				string displayDescription = inStockFormat.TitleType.GetDisplayDescription();
				ButtonData item = new ButtonData((pricingRecord != null) ? string.Format("{0} ${1}", displayDescription, pricingRecord.InitialNight.ToString("0.00")) : displayDescription, DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
				{
					BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "out_of_stock_add_format");
					BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsOutofStockAddFormat");
					OnRentProduct(displayProductModel, commandType, inStockFormat.TitleType, fromOosSelection: true);
				}, (Func<bool>)null));
				observableCollection.Add(item);
			}
		}
		observableCollection.Add(new ButtonData(Resources.related_movies, DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
		{
			BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "out_of_stock_related_movies");
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsOutofStockRelatedMovies");
			BrowseFunctions.ShowRelatedProductsDialog(selectedProduct);
		}, (Func<bool>)null)));
		optionSelectionDialogInfo.ButtonData = observableCollection;
		OrchestrationService.ViewShow("option_selection_dialog_view", optionSelectionDialogInfo);
	}

	private static void ShowReserveOnlineMessage()
	{
		MessagePopupViewController.ShowReserveOnlineRentDialog(delegate
		{
			OrchestrationService.ViewGoBackSafe("ReserveOnlineRentDialogTimeout", popOnly: false);
		}, delegate
		{
			OrchestrationService.AnalyticsAddViewEvent("buttonPress", "reserve_online_rent_back");
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsReserveOnlineRentBack");
			OrchestrationService.ViewGoBackSafe("ReserveOnlineRentDialogBack", popOnly: false);
		}, delegate
		{
			OrchestrationService.AnalyticsAddViewEvent("buttonPress", "reserve_online_rent_gotit");
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsReserveOnlineRentGotit");
			OrchestrationService.ViewGoBackSafe("ReserveOnlineRentDialogGotIt", popOnly: false);
			OrchestrationService.ViewGoBackSafe("ReserveOnlineRentDialogGotIt", popOnly: false);
		});
	}

	private static void ShowOnDemandMessage(decimal price)
	{
		MessagePopupViewController.ShowOnDemandMessage(price, delegate
		{
			OrchestrationService.ViewGoBackSafe("OnDemandRentTimeout", popOnly: false);
		}, delegate
		{
			OrchestrationService.AnalyticsAddViewEvent("buttonPress", "onDemand_rent_back");
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsOnDemandRentBack");
			OrchestrationService.ViewGoBackSafe("OnDemandRentBack", popOnly: false);
		}, delegate
		{
			OrchestrationService.AnalyticsAddViewEvent("buttonPress", "onDemand_rent_gotit");
			BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsOnDemandRentGotit");
			OrchestrationService.ViewGoBackSafe("OnDemandRentDialogGotIt", popOnly: false);
			OrchestrationService.ViewGoBackSafe("OnDemandRentDialogGotIt", popOnly: false);
		});
	}

	private static void OnCollectionButton(IBrowseItemModel displayProductModel)
	{
		BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "see-related-collection");
		BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsSeeRelatedCollection");
		BrowseState.CollectionsPopupShown = true;
		long? num = (displayProductModel?.Data as ITitleProduct)?.Collections?.FirstOrDefault();
		if (num.HasValue)
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			ICollectionProduct collection = null;
			service?.GetCollection(num.Value, out collection);
			if (collection != null)
			{
				BrowseState.PushNonBrowseViewBrowseStateSet();
				BrowseState.PushCollectionBrowseStateSet(collection);
				ViewService?.Push("browse_view");
				ViewService?.Show();
			}
		}
	}

	private static void AddToCart(IKioskProduct product, bool isPurchase)
	{
		BrowseFunctions.AddProductToCart((ITitleProduct)product, isPurchase);
	}

	private static void GoBack(string reason, bool popOnly = false)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductSelectedFromCarousel = false;
		service.GoBack(reason, popOnly: false);
	}

	private static void PopulateTitleDetailProperties(DisplayProductModel displayProductModel)
	{
		if (displayProductModel.TitleDetailPropertiesInitialized)
		{
			return;
		}
		displayProductModel.TitleDetailPropertiesInitialized = true;
		ITitleProduct titleProduct = displayProductModel.Data as ITitleProduct;
		RatingsProperty value = null;
		RatingsProperties.Properties.TryGetValue((Ratings)(titleProduct?.RatingId ?? 0), out value);
		TitleTypeProperty value2 = null;
		string format = null;
		ITitleProduct titleProduct2 = titleProduct;
		if (titleProduct2 != null)
		{
			_ = titleProduct2.TitleType;
			if (true)
			{
				TitleTypeProperties.Properties.TryGetValue(titleProduct.TitleType, out value2);
				format = value2?.GetName(Resources.ResourceManager);
			}
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
		ITitleProduct titleProduct3 = titleProduct;
		Visibility showCollections = ((titleProduct3 == null || titleProduct3.ComingSoon || !displayProductModel.DisplayProductModelConstants.ShowCollections || ((!(titleProduct?.Collections?.Any())) ?? true)) ? Visibility.Collapsed : Visibility.Visible);
		IList<IPricingRecord> list = displayProductModel.DisplayProductModelConstants.PricingService?.GetProductPricingRecords(titleProduct.ProductId);
		bool flag = list != null && list.Count > 1;
		IRentalShoppingCart rentalShoppingCart = displayProductModel.DisplayProductModelConstants.RentalShoppingCartService?.CurrentRentalShoppingCart;
		bool flag2 = rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel;
		int num = (int)(((rentalShoppingCart != null && rentalShoppingCart.ServiceFeeEnabled) ? rentalShoppingCart.DefaultServiceFee : 0m) * 100m);
		displayProductModel.Description = titleProduct.ProductDescription;
		displayProductModel.Rating = value?.Name;
		displayProductModel.Format = format;
		displayProductModel.RunningTime = titleProduct?.RunningTime;
		displayProductModel.ReleaseYear = titleProduct?.NationalStreetDate?.Year.ToString();
		ITitleProduct titleProduct4 = titleProduct;
		int cCSupported;
		if (titleProduct4 != null && !titleProduct4.IsEmptyCase)
		{
			ITitleProduct titleProduct5 = titleProduct;
			if (titleProduct5 != null && titleProduct5.IsMovie)
			{
				ITitleProduct titleProduct6 = titleProduct;
				if (titleProduct6 != null && titleProduct6.ClosedCaptioned)
				{
					cCSupported = 0;
					goto IL_033b;
				}
			}
		}
		cCSupported = 2;
		goto IL_033b;
		IL_033b:
		displayProductModel.CCSupported = (Visibility)cCSupported;
		displayProductModel.Starring = string.Join(", ", titleProduct?.Starring ?? new List<string>());
		displayProductModel.StarringLabel = displayProductModel.DisplayProductModelConstants?.StarringLabel;
		displayProductModel.Genres = GetGenreText(titleProduct?.Genres);
		displayProductModel.GenresLabel = displayProductModel.DisplayProductModelConstants?.GenresLabel;
		displayProductModel.DirectedBy = string.Join(", ", titleProduct?.Directors ?? new List<string>());
		displayProductModel.OneNightPrice = GetPriceText(titleProduct, 0, displayProductModel.DisplayProductModelConstants);
		displayProductModel.OneNightPriceLabel = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? displayProductModel.DisplayProductModelConstants.OneNightPriceLabel : null);
		displayProductModel.MultiNightPrice = GetPriceText(titleProduct, 1, displayProductModel.DisplayProductModelConstants);
		displayProductModel.MultiNightPriceLabel = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? displayProductModel.DisplayProductModelConstants.MultiNightPriceLabel : null);
		displayProductModel.MultiNightPriceVisibility = ((!flag || flag2 || titleProduct.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.ServiceFeeTextVisibility = ((num <= 0) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.ServiceFeeLabel = Resources.service_fee_label;
		displayProductModel.ServiceFeeText = string.Format(Resources.service_fee_text, num);
		displayProductModel.RatingLabel = ratingLabel;
		displayProductModel.RatingDescription = ratingDescription;
		displayProductModel.RatingVisibility = ratingVisibility;
		displayProductModel.OneNightReturnTime = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? GetReturnTimeText(titleProduct, 0, displayProductModel.DisplayProductModelConstants) : null);
		displayProductModel.OneNightReturnTimeLabel = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? displayProductModel.DisplayProductModelConstants.OneNightReturnTimeLabel : null);
		displayProductModel.MultiNightReturnTime = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? GetReturnTimeText(titleProduct, 1, displayProductModel.DisplayProductModelConstants) : null);
		displayProductModel.MultiNightReturnTimeLabel = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? displayProductModel.DisplayProductModelConstants.MultiNightReturnTimeLabel : null);
		displayProductModel.MultiNightReturnTimeVisibility = ((!flag || flag2 || titleProduct.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.DealText = GetDealText(titleProduct);
		displayProductModel.HotDealTextVisibility = ((!titleProduct.HasDeal) ? Visibility.Collapsed : Visibility.Visible);
		ITitleProduct titleProduct7 = titleProduct;
		int runningTimeVisibility;
		if (titleProduct7 != null && !titleProduct7.IsEmptyCase)
		{
			ITitleProduct titleProduct8 = titleProduct;
			if (titleProduct8 != null && titleProduct8.IsMovie && !string.IsNullOrEmpty(titleProduct?.RunningTime))
			{
				runningTimeVisibility = 0;
				goto IL_0615;
			}
		}
		runningTimeVisibility = 2;
		goto IL_0615;
		IL_0615:
		displayProductModel.RunningTimeVisibility = (Visibility)runningTimeVisibility;
		ITitleProduct titleProduct9 = titleProduct;
		int releaseYearVisibility;
		if (titleProduct9 != null && !titleProduct9.IsEmptyCase)
		{
			ITitleProduct titleProduct10 = titleProduct;
			if (titleProduct10 != null)
			{
				DateTime? nationalStreetDate = titleProduct10.NationalStreetDate;
				if (nationalStreetDate.HasValue)
				{
					_ = nationalStreetDate.GetValueOrDefault().Year;
					if (true)
					{
						releaseYearVisibility = 0;
						goto IL_066a;
					}
				}
			}
		}
		releaseYearVisibility = 2;
		goto IL_066a;
		IL_066a:
		displayProductModel.ReleaseYearVisibility = (Visibility)releaseYearVisibility;
		ITitleProduct titleProduct11 = titleProduct;
		int formatVisibility;
		if (titleProduct11 != null && !titleProduct11.IsEmptyCase)
		{
			ITitleProduct titleProduct12 = titleProduct;
			if (titleProduct12 != null && titleProduct12.IsGame)
			{
				formatVisibility = 0;
				goto IL_069f;
			}
		}
		formatVisibility = 2;
		goto IL_069f;
		IL_069f:
		displayProductModel.FormatVisibility = (Visibility)formatVisibility;
		displayProductModel.AdImageVisibility = ((displayProductModel.DisplayProductModelConstants.AdImage == null) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.AdImage = displayProductModel.DisplayProductModelConstants.AdImage;
		displayProductModel.AdHeight = displayProductModel.DisplayProductModelConstants.AdHeight;
		displayProductModel.MainHeight = displayProductModel.DisplayProductModelConstants.MainHeight;
		ITitleProduct titleProduct13 = titleProduct;
		displayProductModel.NotEmptyCaseVisibility = ((titleProduct13 != null && titleProduct13.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		ITitleProduct titleProduct14 = titleProduct;
		displayProductModel.SubPlatformImage = ((titleProduct14 != null && titleProduct14.IsGame) ? GetSubPlatformBitmapImage(titleProduct) : null);
		ITitleProduct titleProduct15 = titleProduct;
		int subPlatformVisibility;
		if (titleProduct15 != null && titleProduct15.IsGame)
		{
			ITitleProduct titleProduct16 = titleProduct;
			if (titleProduct16 != null && titleProduct16.SubPlatformId.HasValue)
			{
				subPlatformVisibility = 0;
				goto IL_076a;
			}
		}
		subPlatformVisibility = 2;
		goto IL_076a;
		IL_076a:
		displayProductModel.SubPlatformVisibility = (Visibility)subPlatformVisibility;
		displayProductModel.ShowCollections = showCollections;
		displayProductModel.CollectionsEnabled = titleProduct?.Collections?.Any() ?? false;
		displayProductModel.CollectionTextLine1 = displayProductModel.DisplayProductModelConstants.CollectionTextLine1;
		displayProductModel.BuyText = displayProductModel.DisplayProductModelConstants.BuyText;
		displayProductModel.BuyButtonTextStyle = displayProductModel.DisplayProductModelConstants.FontMuseo20Style;
		displayProductModel.BuyPriceText = GetBuyPriceText(titleProduct);
		ITitleProduct titleProduct17 = titleProduct;
		int starringVisibility;
		if (titleProduct17 != null && titleProduct17.IsMovie)
		{
			ITitleProduct titleProduct18 = titleProduct;
			if (titleProduct18 != null && !titleProduct18.IsEmptyCase)
			{
				starringVisibility = 0;
				goto IL_083b;
			}
		}
		starringVisibility = 2;
		goto IL_083b;
		IL_083b:
		displayProductModel.StarringVisibility = (Visibility)starringVisibility;
		string comingSoonLabel = displayProductModel.DisplayProductModelConstants.ComingSoonLabel;
		ITitleProduct titleProduct19 = titleProduct;
		displayProductModel.ComingSoonLabelText = comingSoonLabel + " " + ((titleProduct19 != null && titleProduct19.ReleaseDate.HasValue) ? titleProduct.ReleaseDate.Value.ToString("MM/dd") : string.Empty);
		displayProductModel.OutOfStockLine1Text = displayProductModel.DisplayProductModelConstants.OutOfStockLine1Text;
		string outOfStockLine2Text = displayProductModel.DisplayProductModelConstants.OutOfStockLine2Text;
		ITitleProduct titleProduct20 = titleProduct;
		displayProductModel.OutOfStockLine2Text = string.Format(outOfStockLine2Text, (titleProduct20 != null && titleProduct20.IsMovie) ? displayProductModel.DisplayProductModelConstants.Movie : displayProductModel.DisplayProductModelConstants.Game);
		displayProductModel.OutOfStockLine3Text = displayProductModel.DisplayProductModelConstants.OutOfStockLine3Text;
		displayProductModel.ReviewsButtonText = Resources.reviews_button_text;
		if (titleProduct.IsMovie && displayProductModel.DisplayProductModelConstants.StarRatingTitleDetailsABTestVersion != "A")
		{
			List<Review> reviews = ServiceLocator.Instance.GetService<IDataService>().GetReviews(titleProduct.ProductId);
			displayProductModel.StarRating = ((titleProduct.Stars > 0) ? new int?(titleProduct.Stars) : ((int?)null));
			displayProductModel.ReviewsButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				ReviewButtonCommand_Execute(titleProduct);
			}, (Func<bool>)null);
			displayProductModel.ReviewButtonVisibility = ((titleProduct.Stars <= 0 || (reviews?.Count ?? 0) <= 0) ? Visibility.Collapsed : Visibility.Visible);
		}
		else
		{
			displayProductModel.StarRating = null;
			displayProductModel.ReviewsButtonCommand = null;
			displayProductModel.ReviewButtonVisibility = Visibility.Collapsed;
		}
	}

	private static void ReviewButtonCommand_Execute(ITitleProduct titleProduct)
	{
		BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "see_reviews");
		BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsSeeReviews");
		if (titleProduct != null)
		{
			OrchestrationService.ViewShow("review_popup_view", new ReviewPopupInfo
			{
				TitleProduct = titleProduct
			});
		}
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

	private static string GetPriceText(IKioskProduct kioskProduct, int pricingRecordIndex, DisplayProductModelConstants displayProductModelConstants)
	{
		List<string> priceText = new List<string>();
		((kioskProduct is ITitleRollupProduct) ? ((ITitleRollupProduct)kioskProduct).KioskProducts : ((kioskProduct is ITitleProduct) ? new List<ITitleProduct> { (ITitleProduct)kioskProduct } : null))?.ForEach(delegate(ITitleProduct tp)
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
		((kioskProduct is ITitleRollupProduct) ? ((ITitleRollupProduct)kioskProduct).KioskProducts : ((kioskProduct is ITitleProduct) ? new List<ITitleProduct> { (ITitleProduct)kioskProduct } : null))?.ForEach(delegate(ITitleProduct tp)
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

	private static string GetDealText(IKioskProduct kioskProduct)
	{
		List<string> dealText = new List<string>();
		((kioskProduct is ITitleRollupProduct) ? ((ITitleRollupProduct)kioskProduct).KioskProducts : ((kioskProduct is ITitleProduct) ? new List<ITitleProduct> { (ITitleProduct)kioskProduct } : null))?.ForEach(delegate(ITitleProduct tp)
		{
			if (tp.TitleType != TitleType.DigitalCode)
			{
				(ServiceLocator.Instance.GetService<IPricingService>()?.GetProductPricingRecords(tp.ProductId))?.ForEach(delegate(IPricingRecord pr)
				{
					if (pr != null && pr.IsDeal)
					{
						string arg = ((TitleTypeProperties.Properties.ContainsKey(tp.TitleType) && TitleTypeProperties.Properties[tp.TitleType].GetName(Resources.ResourceManager) != null) ? $" {TitleTypeProperties.Properties[tp.TitleType].GetName(Resources.ResourceManager)}" : string.Empty);
						dealText.Add($"{pr.ExtraNight:C}{arg}");
					}
				});
			}
		});
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.EnableFlashDeals)
		{
			return string.Format(Resources.flash_deal_text, string.Join(", ", dealText));
		}
		return string.Format(Resources.hot_deal_text, string.Join(", ", dealText));
	}

	public static BitmapImage GetSubPlatformBitmapImage(ITitleProduct kioskProduct)
	{
		BitmapImage result = null;
		long? num = kioskProduct?.SubPlatformId;
		if (num.HasValue && num > 0)
		{
			result = Constants.SubPlatformImages.GetSubPlatformImage((Constants.SubPlatforms)num.Value);
		}
		return result;
	}

	private static string GetBuyPriceText(ITitleProduct titleProduct)
	{
		List<ITitleProduct> list = ((titleProduct is ITitleRollupProduct) ? ((ITitleRollupProduct)titleProduct).KioskProducts : new List<ITitleProduct> { titleProduct }).Where((ITitleProduct tp) => tp.IsSellableAndInStock).ToList();
		if (list.Count == 0)
		{
			return string.Empty;
		}
		decimal price = list.Min((ITitleProduct tp) => tp.PurchasePrice);
		bool flag = list.Count == 1 || list.All((ITitleProduct tp) => tp.PurchasePrice == price);
		return string.Format("${0:F2}{1}", price, flag ? "" : "+");
	}
}
