using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

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
		ConfigureTTS(formatSelectionPopupViewModel);
		viewUserControl.DataContext = formatSelectionPopupViewModel;
	}

	private static void AddFormatButtonData(FormatSelectionPopupViewModel viewModel, ITitleProduct kioskProduct, IPricingService pricingService, bool isPurchase)
	{
		bool showOutOfStock = !(Redbox.Rental.Services.Configuration.Configuration.Instance.OutOfStockTitleDetailsABTestVersion == "B") || kioskProduct.TitleFamily != TitleFamily.Movies || isPurchase;
		TitleRollupFormatButtonProperties rollupFormatButtonProperties = TitleRollupUtilities.GetRollupFormatButtonProperties(kioskProduct, isPurchase, new TitleRollupOverlayButtonTextValues(), showOutOfStock);
		if (rollupFormatButtonProperties.Visibility == Visibility.Visible)
		{
			if (kioskProduct.TitleType == TitleType.DigitalCode && rollupFormatButtonProperties.IsEnabled)
			{
				rollupFormatButtonProperties.ButtonText += "*";
			}
			viewModel.ButtonData.Add(new FormatButtonData(rollupFormatButtonProperties.ButtonText, rollupFormatButtonProperties.TitleType, rollupFormatButtonProperties.ButtonTextLine2, rollupFormatButtonProperties.IsEnabled));
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

	private static void ConfigureTTS(FormatSelectionPopupViewModel formatSelectionPopupViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			formatSelectionPopupViewModel.OnGetSpeechControl = () => PurchaseFormatSelectionPopupViewTextToSpeech.GetSpeechControl(GetViewModel);
		}
	}

	private static bool IsTitleTypeAvailable(TitleType titleType)
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		List<IKioskProduct> products = null;
		service?.GetProducts(BrowseViewFilter.All, TitleFamily.Movies, titleType, BrowseSort.MerchandiseDate, null, null, null, isADAView: false, null, null, out products);
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
		if (rollupProduct == null)
		{
			return;
		}
		ITitleProduct titleProduct = rollupProduct.KioskProducts.FirstOrDefault((ITitleProduct x) => x.TitleType == type);
		if (titleProduct.InStock | isPurchase)
		{
			IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
			bool flag = rentalShoppingCart != null && rentalShoppingCart.InCartPromoOfferMade && !rentalShoppingCart.InCartPromoOfferCancel;
			bool flag2 = Redbox.Rental.Services.Configuration.Configuration.Instance.PrmUpsellOnMnpABTestVersion == "C" && !titleProduct.HasMultiNightPrice && titleProduct.IsSellableAndInStock && titleProduct.IsMovie;
			bool flag3 = Redbox.Rental.Services.Configuration.Configuration.Instance.PrmUpsellOnMnpABTestVersion == "D" && titleProduct.IsSellableAndInStock && titleProduct.IsMovie;
			if ((titleProduct.HasMultiNightPrice | flag2 | flag3) && !isPurchase && !flag)
			{
				string viewAnalyticsName = "multi_night_pricing_popup_view";
				if (Redbox.Rental.Services.Configuration.Configuration.Instance.PrmUpsellOnMnpABTestVersion == "B" && titleProduct.HasMultiNightPrice && titleProduct.IsSellableAndInStock)
				{
					viewAnalyticsName = "multi_night_pricing_with_buy_button_popup_view";
				}
				else if (Redbox.Rental.Services.Configuration.Configuration.Instance.PrmUpsellOnMnpABTestVersion == "C" && !titleProduct.HasMultiNightPrice && titleProduct.IsSellableAndInStock)
				{
					viewAnalyticsName = "non_multi_night_pricing_with_buy_button_popup_view";
				}
				else if (Redbox.Rental.Services.Configuration.Configuration.Instance.PrmUpsellOnMnpABTestVersion == "D" && titleProduct.HasMultiNightPrice && titleProduct.IsSellableAndInStock)
				{
					viewAnalyticsName = "multi_night_pricing_with_buy_button_popup_view";
				}
				else if (Redbox.Rental.Services.Configuration.Configuration.Instance.PrmUpsellOnMnpABTestVersion == "D" && !titleProduct.HasMultiNightPrice && titleProduct.IsSellableAndInStock)
				{
					viewAnalyticsName = "non_multi_night_pricing_with_buy_button_popup_view";
				}
				OrchestrationService.ViewShow("multi_night_pricing_popup_view", new MultiNightPricingPopupInfo
				{
					TitleProduct = titleProduct,
					ShownFromView = "purchase_popup_view",
					ViewAnalyticsName = viewAnalyticsName
				});
			}
			else
			{
				BrowseFunctions.AddProductToCart(titleProduct, isPurchase);
			}
		}
		else
		{
			ShowOutOfStockOptionsDialog(rollupProduct, type);
		}
	}

	private static void ShowOutOfStockOptionsDialog(ITitleProduct kioskProduct, TitleType titleType)
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
			ServiceLocator.Instance.GetService<IViewService>()?.GoBack("OutOfStockDialogCancel", popOnly: false);
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
					ProcessButtonClick((TitleRollupProduct)kioskProduct, inStockFormat.TitleType, inStockFormat.TitleType.GetDisplayName(), isPurchase: false);
				}, (Func<bool>)null));
				observableCollection.Add(item);
			}
		}
		observableCollection.Add(new ButtonData(Resources.related_movies, DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
		{
			BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "out_of_stock_related_movies");
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
			OrchestrationService.ViewGoBackSafe("ReserveOnlineRentDialogBack", popOnly: false);
		}, delegate
		{
			OrchestrationService.AnalyticsAddViewEvent("buttonPress", "reserve_online_rent_gotit");
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
			OrchestrationService.ViewGoBackSafe("OnDemandRentBack", popOnly: false);
		}, delegate
		{
			OrchestrationService.AnalyticsAddViewEvent("buttonPress", "onDemand_rent_gotit");
			OrchestrationService.ViewGoBackSafe("OnDemandRentDialogGotIt", popOnly: false);
			OrchestrationService.ViewGoBackSafe("OnDemandRentDialogGotIt", popOnly: false);
		});
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
