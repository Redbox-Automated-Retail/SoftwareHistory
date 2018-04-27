using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
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

public class TitleDetailsController
{
	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance?.BrowseState;

	private static IViewService ViewService => ServiceLocator.Instance.GetService<IViewService>();

	private static ISessionAnalyticsService SessionAnalyticsService => ServiceLocator.Instance.GetService<ISessionAnalyticsService>();

	public static void ConfigureView()
	{
		FrameworkElement userControl = GetUserControl();
		if (userControl != null)
		{
			userControl.DataContext = GetModel();
		}
		CommandManager.InvalidateRequerySuggested();
		ConfigureTTS();
	}

	private static FrameworkElement GetUserControl()
	{
		return ActorFunctions.GetActor("TitleDetailUserControl")?.WPFFrameworkElement;
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
		BrowseControlModel browseProductControl = new BrowseControlModel();
		browseProductControl.BackgroundColor = Brushes.Transparent.Color;
		browseProductControl.PageNumberVisibility = Visibility.Collapsed;
		browseProductControl.ShowNoResultsMessage = false;
		browseProductControl.BorderShadowVisibility = Visibility.Collapsed;
		browseProductControl.ShowBorderItems = false;
		browseProductControl.OnAddBrowseItemModel += OnBuyButtonHit;
		browseProductControl.UseFullPageWidth = true;
		browseProductControl.BrowseItemMargin = new Thickness(0.0, 25.0, 0.0, 0.0);
		browseProductControl.RowCount = 1;
		browseProductControl.ColumnCount = 1;
		browseProductControl.BrowseItems = Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseItemModels?.Where((IBrowseItemModel x) => x is DisplayProductModel).ToList() ?? new List<IBrowseItemModel>();
		browseProductControl.BrowseItems.ForEach(delegate(IBrowseItemModel x)
		{
			DisplayProductModel obj = x as DisplayProductModel;
			obj?.ConfigureForTitleDetail();
			PopulateTitleDetailProperties(obj);
		});
		IKioskProduct selectedProduct = Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct;
		bool isAda = ApplicationState.Instance.IsADAAccessible;
		browseProductControl.SetPageNumber(browseProductControl.BrowseItems.FindIndex((IBrowseItemModel x) => (!isAda || !(selectedProduct is TitleRollupProduct)) ? (x.Data == selectedProduct) : TitleRollupUtilities.IsAdaDefaultTitle((ITitleProduct)x.Data, (TitleRollupProduct)selectedProduct)) + 1, PageChangeSource.None);
		browseProductControl.OnPageChanged += delegate(int prevPageNumber, int newPageNumber, PageChangeSource pageSource)
		{
			string subItemValue = ((prevPageNumber < newPageNumber) ? "product-detail-page-next" : "product-detail-page-back");
			BrowseState.BrowseSelectionSelectedProduct = browseProductControl.BrowseItems[newPageNumber - 1].Data as IKioskProduct;
			ServiceLocator.Instance.GetService<ISessionAnalyticsService>()?.AddViewEvent("pageChange", subItemValue);
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductSelectedFromCarousel = false;
			ConfigureTTS();
		};
		if (browseProductControl.BrowseItems.FirstOrDefault((IBrowseItemModel x) => x.Data == Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct) is DisplayProductModel displayProductModel)
		{
			displayProductModel.ComingSoonButtonText = string.Format("{0}", BundleFunctions.GetDictionaryStringValue("strings", "email_optin_button_text"));
		}
		browseProductControl.BrowseControls = new List<Control>();
		browseProductControl.BrowseControls.Add(new TitleDetailDisplayProductUserControl());
		return browseProductControl;
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
			OrchestrationService.ViewShow("purchase_popup_view", new RentalFormatSelectionViewParameter(titleRollupProduct, isPurchase: true));
			return;
		}
		ITitleProduct titleProduct = kioskProduct as ITitleProduct;
		SessionAnalyticsService?.AddViewEvent("buttonPress", "buy," + titleProduct.ProductId);
		BrowseFunctions.AddProductToCart(titleProduct, isPurchase: true);
	}

	private static void OnMoreInfoHit(IBrowseItemModel model)
	{
		IComingSoonOptinInfo comingSoonOptinInfo = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart?.ComingSoonOptInInfo;
		SessionAnalyticsService?.AddViewEvent("buttonPress", "see-related-collection");
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
			SessionAnalyticsService?.AddViewEvent("CarouselCollection", "rent," + productId);
		}
		else if (Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductSelectedFromCarousel)
		{
			SessionAnalyticsService?.AddViewEvent("Carousel", "rent," + productId);
		}
	}

	public static void OnRentProduct(IBrowseItemModel displayProductModel, object commandType, TitleType titleType)
	{
		if (displayProductModel?.Data is ITitleProduct titleProduct)
		{
			ITitleProduct titleProduct2 = ((titleProduct is TitleRollupProduct) ? ((TitleRollupProduct)titleProduct).KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == titleType) : titleProduct);
			SessionAnalyticsService?.AddViewEvent("buttonPress", "rent," + titleProduct2.ProductId);
			OnRent(titleProduct2.ProductId);
			IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
			bool flag = rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel;
			if (titleProduct2.HasMultiNightPrice && !flag)
			{
				OrchestrationService.ViewShow("multi_night_pricing_popup_view", titleProduct2);
			}
			else
			{
				AddToCart(titleProduct2, isPurchase: false);
			}
		}
	}

	private static void OnCollectionButton(IBrowseItemModel displayProductModel)
	{
		SessionAnalyticsService?.AddViewEvent("buttonPress", "see-related-collection");
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
		Visibility showCollections = ((titleProduct == null || titleProduct.ComingSoon || !displayProductModel.DisplayProductModelConstants.ShowCollections || ((!(titleProduct?.Collections?.Any())) ?? true)) ? Visibility.Collapsed : Visibility.Visible);
		bool flag = titleProduct is ITitleRollupProduct && titleProduct.KioskProductType == KioskProductType.TitleRollup;
		IList<IPricingRecord> list = displayProductModel.DisplayProductModelConstants.PricingService?.GetProductPricingRecords(titleProduct.ProductId);
		bool flag2 = list != null && list.Count > 1;
		decimal num = (titleProduct.IsEmptyCase ? displayProductModel.DisplayProductModelConstants.PricingService.GetEmptyCasePrice() : titleProduct.PurchasePrice);
		string format2 = ((num - decimal.Round(num) != 0m) ? "${0:F2}" : "${0:F0}");
		IRentalShoppingCart rentalShoppingCart = displayProductModel.DisplayProductModelConstants.RentalShoppingCartService?.CurrentRentalShoppingCart;
		bool flag3 = rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel;
		int num2 = (int)(((rentalShoppingCart != null && rentalShoppingCart.ServiceFeeEnabled) ? rentalShoppingCart.DefaultServiceFee : 0m) * 100m);
		displayProductModel.Description = titleProduct.ProductDescription;
		displayProductModel.Rating = value?.Name;
		displayProductModel.Format = format;
		displayProductModel.RunningTime = titleProduct?.RunningTime;
		displayProductModel.ReleaseYear = titleProduct?.NationalStreetDate?.Year.ToString();
		displayProductModel.CCSupported = ((titleProduct == null || titleProduct.IsEmptyCase || titleProduct == null || !titleProduct.IsMovie || titleProduct == null || !titleProduct.ClosedCaptioned) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.Starring = string.Join(", ", titleProduct?.Starring ?? new List<string>());
		displayProductModel.StarringLabel = displayProductModel.DisplayProductModelConstants?.StarringLabel;
		displayProductModel.Genres = GetGenreText(titleProduct?.Genres);
		displayProductModel.GenresLabel = displayProductModel.DisplayProductModelConstants?.GenresLabel;
		displayProductModel.DirectedBy = string.Join(", ", titleProduct?.Directors ?? new List<string>());
		displayProductModel.OneNightPrice = GetPriceText(titleProduct, 0, displayProductModel.DisplayProductModelConstants);
		displayProductModel.OneNightPriceLabel = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? displayProductModel.DisplayProductModelConstants.OneNightPriceLabel : null);
		displayProductModel.MultiNightPrice = GetPriceText(titleProduct, 1, displayProductModel.DisplayProductModelConstants);
		displayProductModel.MultiNightPriceLabel = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? displayProductModel.DisplayProductModelConstants.MultiNightPriceLabel : null);
		displayProductModel.MultiNightPriceVisibility = ((!flag2 || flag3 || titleProduct.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.ServiceFeeTextVisibility = ((num2 <= 0) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.ServiceFeeLabel = Resources.service_fee_label;
		displayProductModel.ServiceFeeText = string.Format(Resources.service_fee_text, num2);
		displayProductModel.RatingLabel = ratingLabel;
		displayProductModel.RatingDescription = ratingDescription;
		displayProductModel.RatingVisibility = ratingVisibility;
		displayProductModel.OneNightReturnTime = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? GetReturnTimeText(titleProduct, 0, displayProductModel.DisplayProductModelConstants) : null);
		displayProductModel.OneNightReturnTimeLabel = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? displayProductModel.DisplayProductModelConstants.OneNightReturnTimeLabel : null);
		displayProductModel.MultiNightReturnTime = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? GetReturnTimeText(titleProduct, 1, displayProductModel.DisplayProductModelConstants) : null);
		displayProductModel.MultiNightReturnTimeLabel = (((titleProduct?.TitleType ?? TitleType.All) != TitleType.DigitalCode) ? displayProductModel.DisplayProductModelConstants.MultiNightReturnTimeLabel : null);
		displayProductModel.MultiNightReturnTimeVisibility = ((!flag2 || flag3 || titleProduct.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.DealText = GetDealText(titleProduct);
		displayProductModel.HotDealTextVisibility = ((!titleProduct.HasDeal) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.RunningTimeVisibility = ((titleProduct == null || titleProduct.IsEmptyCase || titleProduct == null || !titleProduct.IsMovie || string.IsNullOrEmpty(titleProduct?.RunningTime)) ? Visibility.Collapsed : Visibility.Visible);
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
					goto IL_05c5;
				}
			}
		}
		releaseYearVisibility = 2;
		goto IL_05c5;
		IL_05c5:
		displayProductModel.ReleaseYearVisibility = (Visibility)releaseYearVisibility;
		displayProductModel.FormatVisibility = ((titleProduct == null || titleProduct.IsEmptyCase || titleProduct == null || !titleProduct.IsGame) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.AdImageVisibility = ((displayProductModel.DisplayProductModelConstants.AdImage == null) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.AdImage = displayProductModel.DisplayProductModelConstants.AdImage;
		displayProductModel.AdHeight = displayProductModel.DisplayProductModelConstants.AdHeight;
		displayProductModel.MainHeight = displayProductModel.DisplayProductModelConstants.MainHeight;
		displayProductModel.NotEmptyCaseVisibility = ((titleProduct != null && titleProduct.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.SubPlatformImage = ((titleProduct != null && titleProduct.IsGame) ? GetSubPlatformBitmapImage(titleProduct) : null);
		displayProductModel.SubPlatformVisibility = ((titleProduct == null || !titleProduct.IsGame || titleProduct == null || !titleProduct.SubPlatformId.HasValue) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.ShowCollections = showCollections;
		displayProductModel.CollectionsEnabled = titleProduct?.Collections?.Any() ?? false;
		displayProductModel.CollectionTextLine1 = displayProductModel.DisplayProductModelConstants.CollectionTextLine1;
		displayProductModel.BuyText = displayProductModel.DisplayProductModelConstants.BuyText;
		displayProductModel.BuyButtonTextStyle = displayProductModel.DisplayProductModelConstants.FontMuseo20Style;
		displayProductModel.BuyPriceText = string.Format(format2, num);
		displayProductModel.BuyPriceVisibility = ((flag && !titleProduct.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.BuyChevronVisibility = ((!flag || titleProduct.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.StarringVisibility = ((titleProduct == null || !titleProduct.IsMovie || titleProduct == null || titleProduct.IsEmptyCase) ? Visibility.Collapsed : Visibility.Visible);
		displayProductModel.ComingSoonLabelText = displayProductModel.DisplayProductModelConstants.ComingSoonLabel + " " + ((titleProduct != null && titleProduct.ReleaseDate.HasValue) ? titleProduct.ReleaseDate.Value.ToString("MM/dd") : string.Empty);
		displayProductModel.OutOfStockLine1Text = displayProductModel.DisplayProductModelConstants.OutOfStockLine1Text;
		displayProductModel.OutOfStockLine2Text = string.Format(displayProductModel.DisplayProductModelConstants.OutOfStockLine2Text, (titleProduct != null && titleProduct.IsMovie) ? displayProductModel.DisplayProductModelConstants.Movies : displayProductModel.DisplayProductModelConstants.Games);
		displayProductModel.OutOfStockLine3Text = displayProductModel.DisplayProductModelConstants.OutOfStockLine3Text;
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
}
