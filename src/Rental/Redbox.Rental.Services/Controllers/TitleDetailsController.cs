using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
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
		titleDetailModel.AddPrimary += OnRentDvd;
		titleDetailModel.AddBluRay += OnRentBluRay;
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
		ApplicationState.Instance.BrowseProducts = GetBrowseKioskProducts();
		BrowseControlModel browseProductControl = new BrowseControlModel();
		browseProductControl.BackgroundColor = Brushes.Transparent.Color;
		browseProductControl.PageNumberVisibility = Visibility.Collapsed;
		browseProductControl.ShowNoResultsMessage = false;
		browseProductControl.BorderShadowVisibility = Visibility.Collapsed;
		browseProductControl.ShowBorderItems = false;
		browseProductControl.OnAddBrowseItemModel += OnBuyButtonHit;
		browseProductControl.TitleRollupOverlayModel.OnTitleRollupOverlayAddDVD += OnBuyDvd;
		browseProductControl.TitleRollupOverlayModel.OnTitleRollupOverlayAddBluray += OnBuyBluRay;
		browseProductControl.TitleRollupOverlayModel.OnTitleRollupOverlayAddDigitalCode += OnBuyDigitalCode;
		browseProductControl.TitleRollupOverlayModel.OnTitleRollupOverlayDigitalCodeInfo += OnDigitalCodeInfo;
		browseProductControl.UseFullPageWidth = true;
		browseProductControl.BrowseItemMargin = new Thickness(0.0, 25.0, 0.0, 0.0);
		browseProductControl.RowCount = 1;
		browseProductControl.ColumnCount = 1;
		browseProductControl.BrowseItems = Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseItemModels?.Where((IBrowseItemModel x) => x is DisplayProductModel).ToList() ?? new List<IBrowseItemModel>();
		browseProductControl.BrowseItems.ForEach(delegate(IBrowseItemModel x)
		{
			((DisplayProductModel)x).ConfigureForTitleDetail();
		});
		IKioskProduct selectedProduct = Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectionSelectedProduct;
		bool isAda = ApplicationState.Instance.IsADAAccessible;
		browseProductControl.SetPageNumber(browseProductControl.BrowseItems.FindIndex((IBrowseItemModel x) => (!isAda || !(selectedProduct is TitleRollupProduct)) ? (x.Data == selectedProduct) : ((TitleRollupProduct)selectedProduct).KioskProducts.Any((ITitleProduct y) => y == x.Data)) + 1, PageChangeSource.None);
		browseProductControl.OnPageChanged += delegate(int prevPageNumber, int newPageNumber, PageChangeSource pageSource)
		{
			string subItemValue = ((prevPageNumber < newPageNumber) ? "product-detail-page-next" : "product-detail-page-back");
			BrowseState.BrowseSelectionSelectedProduct = browseProductControl.BrowseItems[newPageNumber - 1].Data as IKioskProduct;
			ServiceLocator.Instance.GetService<ISessionAnalyticsService>()?.AddViewEvent("pageChange", subItemValue);
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.ProductSelectedFromCarousel = false;
			browseProductControl.TitleRollupOverlayModel.Visibility = Visibility.Collapsed;
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

	private static List<IKioskProduct> GetBrowseKioskProducts()
	{
		IBrowseState browseState = Redbox.Rental.Services.Session.Session.Instance.BrowseState;
		LogHelper.Instance.Log("RentalGetProducts by browseMode: {0} family: {1} familyType: {2} selectedSort: {3} selectedGenere: {4}, isADAView: {5} ", browseState.BrowseMode, browseState.TitleFamily, browseState.TitleType, browseState.BrowseSelectedSort, browseState.Genre, ApplicationState.Instance.IsADAAccessible);
		try
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			BrowseSort browseSort = browseState.BrowseSelectedSort.ConvertToBrowseSort(browseState.BrowseMode);
			List<long> list = new List<long>();
			List<long> list2 = new List<long>();
			IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
			if (rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel && !PromotionService.Instance.IsFreeInCartOfferRunning && rentalShoppingCart.Promotion != null && rentalShoppingCart.Promotion.CampaignTitles != null)
			{
				if (rentalShoppingCart.Promotion.CampaignTitles.Include)
				{
					list.AddRange(rentalShoppingCart.Promotion.CampaignTitles.Titles);
				}
				else
				{
					list2.AddRange(rentalShoppingCart.Promotion.CampaignTitles.Titles);
				}
			}
			ErrorList products = service.GetProducts(browseState.BrowseMode, browseState.TitleFamily, browseState.TitleType, browseSort, (long?)browseState.Genre, list, list2, ApplicationState.Instance.IsADAAccessible, out var products2);
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

	private static void OnGoBack()
	{
		GoBack("go_back");
	}

	private static void OnBuyButtonHit(IBrowseItemModel displayProductModel, object commandType)
	{
		TitleDetailModel titleDetailModel = GetUserControl().DataContext as TitleDetailModel;
		if (titleDetailModel?.BrowseProductControlModel?.TitleRollupOverlayModel != null)
		{
			titleDetailModel.BrowseProductControlModel.TitleRollupOverlayModel.Visibility = Visibility.Collapsed;
		}
		IKioskProduct kioskProduct = displayProductModel.Data as IKioskProduct;
		TitleRollupProduct obj = kioskProduct as TitleRollupProduct;
		if (obj == null || obj.IsEmptyCase)
		{
			ITitleProduct titleProduct = kioskProduct as ITitleProduct;
			SessionAnalyticsService?.AddViewEvent("buttonPress", "buy," + titleProduct.ProductId);
			BrowseFunctions.AddProductToCart(titleProduct, isPurchase: true);
		}
		else
		{
			TitleRollupUtilities.ConfigureTitleRollupOverlay(displayProductModel, titleDetailModel.BrowseProductControlModel.TitleRollupOverlayModel, showDigitalCodeInfo: true, isPurchase: true);
		}
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

	public static void OnRentDvd(IBrowseItemModel displayProductModel, object commandType)
	{
		if (displayProductModel?.Data is ITitleProduct titleProduct)
		{
			ITitleProduct titleProduct2 = ((titleProduct is TitleRollupProduct) ? (((TitleRollupProduct)titleProduct).PrimaryProduct as ITitleProduct) : titleProduct);
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

	public static void OnRentBluRay(IBrowseItemModel displayProductModel, object commandType)
	{
		if (displayProductModel?.Data is ITitleProduct titleProduct)
		{
			ITitleProduct titleProduct2 = ((titleProduct is TitleRollupProduct) ? ((TitleRollupProduct)titleProduct).KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.Bluray) : titleProduct);
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

	private static void OnBuyDvd(IBrowseItemModel displayProductModel, object commandType)
	{
		if (displayProductModel?.Data is TitleRollupProduct titleRollupProduct)
		{
			AddToCart(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.DVD), isPurchase: true);
		}
	}

	private static void OnBuyBluRay(IBrowseItemModel displayProductModel, object commandType)
	{
		if (displayProductModel?.Data is TitleRollupProduct titleRollupProduct)
		{
			AddToCart(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.Bluray), isPurchase: true);
		}
	}

	private static void OnBuyDigitalCode(IBrowseItemModel displayProductModel, object commandType)
	{
		if (displayProductModel?.Data is TitleRollupProduct titleRollupProduct)
		{
			AddToCart(titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.DigitalCode), isPurchase: true);
		}
	}

	private static void OnDigitalCodeInfo(IBrowseItemModel displayProductModel, object commandType)
	{
		if (displayProductModel?.Data is TitleRollupProduct titleRollupProduct)
		{
			SessionAnalyticsService?.AddViewEvent("context", "whatdigitalcode_message_view");
			ITitleProduct product = titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.DigitalCode);
			MessagePopupViewController.ShowMessagePopupViewDigitalCode($"{product.PurchasePrice:N2}", delegate
			{
				SessionAnalyticsService?.AddViewEvent("buttonPress", "cancel-whatdigitalcode-mesage-view");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code info back button", popOnly: false);
			}, delegate
			{
				SessionAnalyticsService?.AddViewEvent("buttonPress", "buy-whatdigitalcode-mesage-view");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code info buy button", popOnly: false);
				AddToCart(product, isPurchase: true);
			}, delegate
			{
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code info time out", popOnly: false);
			});
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
}
