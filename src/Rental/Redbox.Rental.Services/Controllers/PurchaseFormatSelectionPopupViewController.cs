using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class PurchaseFormatSelectionPopupViewController : BaseViewController
{
	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance.BrowseState;

	public static void ConfigureRentalFormatSelectionPopupView(RentalFormatSelectionViewParameter parameter)
	{
		BaseViewController.StopIdleTimer();
		CreateAndStartTimer();
		if (BrowseFunctions.IsPurchase)
		{
			OrchestrationService.AnalyticsAddViewEvent("context", "PurchaseDialog");
		}
		else
		{
			OrchestrationService.AnalyticsAddViewEvent("context", "RentalDialog");
		}
		IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl == null)
		{
			return;
		}
		FormatSelectionPopupViewModel formatSelectionPopupViewModel = GetViewModel();
		if (formatSelectionPopupViewModel == null)
		{
			formatSelectionPopupViewModel = new FormatSelectionPopupViewModel();
		}
		formatSelectionPopupViewModel.CancelButtonText = Resources.cancel_button;
		formatSelectionPopupViewModel.OnCancelButtonClicked = delegate
		{
			ProcessCancelButtonClick();
		};
		if (parameter == null || parameter.RollupProduct == null)
		{
			return;
		}
		TitleRollupProduct rollupProduct = parameter.RollupProduct;
		bool isPurchase = parameter.IsPurchase;
		formatSelectionPopupViewModel.HeaderText = rollupProduct.LongName;
		formatSelectionPopupViewModel.DescriptionText = (isPurchase ? Resources.purchase_format_dialog_description : Resources.rental_format_dialog_description);
		formatSelectionPopupViewModel.DigitalCodeInfoText = Resources.title_rollup_format_digital_code_info;
		formatSelectionPopupViewModel.OnFormatButtonClicked = delegate(TitleType type)
		{
			ProcessButtonClick(rollupProduct, type, type.GetDisplayName(), isPurchase);
		};
		formatSelectionPopupViewModel.OnDigitalCodeInfoButtonClicked = delegate
		{
			ProcessDigitalCodeInfoClicked(rollupProduct);
		};
		foreach (ITitleProduct item in rollupProduct.KioskProducts.OrderBy((ITitleProduct p) => (int)((p.TitleType != TitleType.DigitalCode) ? p.TitleType : ((TitleType)999))))
		{
			AddFormatButtonData(formatSelectionPopupViewModel, item, service, isPurchase);
		}
		int count = formatSelectionPopupViewModel.ButtonData.Count;
		if (count < 4)
		{
			formatSelectionPopupViewModel.DialogHeight = 350;
		}
		else
		{
			formatSelectionPopupViewModel.DialogHeight = 443;
		}
		if (formatSelectionPopupViewModel.DigitalCodeInfoVisibility == Visibility.Visible)
		{
			formatSelectionPopupViewModel.DialogHeight += 75;
		}
		if (count == 3)
		{
			formatSelectionPopupViewModel.DialogWidth = 900;
		}
		else
		{
			formatSelectionPopupViewModel.DialogWidth = 750;
		}
		viewUserControl.DataContext = formatSelectionPopupViewModel;
	}

	private static void AddFormatButtonData(FormatSelectionPopupViewModel viewModel, ITitleProduct kioskProduct, IPricingService pricingService, bool isPurchase)
	{
		TitleRollupFormatButtonProperties rollupFormatSelectionButtonProperties = TitleRollupUtilities.GetRollupFormatSelectionButtonProperties(kioskProduct, isPurchase);
		if (rollupFormatSelectionButtonProperties.Visibility == Visibility.Visible)
		{
			if (kioskProduct.TitleType == TitleType.DigitalCode && rollupFormatSelectionButtonProperties.IsEnabled)
			{
				rollupFormatSelectionButtonProperties.ButtonText += "*";
			}
			viewModel.ButtonData.Add(new FormatButtonData(rollupFormatSelectionButtonProperties.ButtonText, rollupFormatSelectionButtonProperties.TitleType, rollupFormatSelectionButtonProperties.ButtonTextLine2, rollupFormatSelectionButtonProperties.IsEnabled));
		}
	}

	private static FormatSelectionPopupViewModel GetViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as FormatSelectionPopupViewModel;
		}
		return null;
	}

	private static bool IsTitleTypeAvailable(TitleType titleType)
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		List<IKioskProduct> products = null;
		service?.GetProducts(BrowseViewFilter.All, TitleFamily.Movies, titleType, BrowseSort.MerchandiseDate, null, null, null, isADAView: false, out products);
		if (products != null)
		{
			return products.Count > 0;
		}
		return false;
	}

	private static void ProcessButtonClick(TitleRollupProduct rollupProduct, TitleType type, string analysisText, bool isPurchase)
	{
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", analysisText);
		OrchestrationService.ViewGoBackSafe($"RentalFormatSelectionPopupViewController {analysisText}", popOnly: false);
		if (rollupProduct != null)
		{
			ITitleProduct titleProduct = rollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == type);
			IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
			bool flag = rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel;
			if (titleProduct.HasMultiNightPrice && !isPurchase && !flag)
			{
				OrchestrationService.ViewShow("multi_night_pricing_popup_view", titleProduct);
			}
			else
			{
				BrowseFunctions.AddProductToCart(titleProduct, isPurchase);
			}
		}
	}

	private static void ProcessCancelButtonClick()
	{
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "cancel");
		OrchestrationService.ViewGoBackSafe("RentalFormatSelectionPopupViewController cancel", popOnly: false);
	}

	private static void ProcessDigitalCodeInfoClicked(TitleRollupProduct titleRollupProduct)
	{
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "digitalcodeinformation");
		if (titleRollupProduct != null)
		{
			ITitleProduct product = titleRollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == TitleType.DigitalCode);
			OrchestrationService.ViewGoBackSafe("RentalFormatSelectionPopupViewController Digital code Info button", popOnly: false);
			MessagePopupViewController.ShowMessagePopupViewDigitalCode($"{product.PurchasePrice:N2}", delegate
			{
				BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "cancel-whatdigitalcode-mesage-view");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code info back button", popOnly: false);
			}, delegate
			{
				BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "buy-whatdigitalcode-mesage-view");
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code info buy button", popOnly: false);
				BrowseFunctions.AddProductToCart(product, isPurchase: true);
			}, delegate
			{
				ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe("Digital Code info time out", popOnly: false);
			});
		}
	}

	private static void CreateAndStartTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		int value = OrchestrationService.TTSGetTimeout("medium", 15000);
		(service?.CreateTimer("RentalFormatSelectionPopupViewTimer", value, value, delegate
		{
			OrchestrationService.AnalyticsAddViewEvent("context", "timeOut");
			OrchestrationService.ViewGoBackSafe("RentalFormatSelectionViewController time out", popOnly: false);
		}))?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		(service?.GetTimer("RentalFormatSelectionPopupViewTimer"))?.Stop();
		service?.RemoveTimer("RentalFormatSelectionPopupViewTimer");
	}

	public static void LeaveRentalFormatSelectionPopupView()
	{
		StopAndRemoveTimer();
		BaseViewController.StartIdleTimer();
	}
}
