using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class NewCartConfirmViewController : BaseViewController
{
	private static NewCartConfirmviewParameters _parameters;

	private static NewCartConfirmViewModel _viewModel;

	private static Action _cancelAction;

	private static Action _continueAction;

	private static Action _timeoutAction;

	public static void ConfigureView(NewCartConfirmviewParameters parameters)
	{
		BaseViewController.StopIdleTimer();
		CreateAndStartTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			NewCartConfirmViewModel newCartConfirmViewModel = new NewCartConfirmViewModel();
			_parameters = parameters;
			_viewModel = newCartConfirmViewModel;
			ConfigureTextAndVisibility(newCartConfirmViewModel, parameters);
			ConfigureCommands(newCartConfirmViewModel, parameters);
			ConfigureShoppingCartDisplay(newCartConfirmViewModel, parameters);
			ConfigureTTS(newCartConfirmViewModel, parameters);
			_cancelAction = parameters.CancelAction;
			_continueAction = parameters.ContinueAction;
			_timeoutAction = parameters.TimeoutAction;
			viewUserControl.DataContext = newCartConfirmViewModel;
		}
	}

	private static void ConfigureTTS(NewCartConfirmViewModel viewModel, NewCartConfirmviewParameters parameters)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (service != null && service.TTSEnabled && viewUserControl != null)
		{
			viewModel.OnGetSpeechControl = () => NewCartConfirmViewTextToSpeech.GetSpeechControl(GetShoppingCartViewModelFromView);
		}
	}

	private static NewCartConfirmViewModel GetShoppingCartViewModelFromView()
	{
		FrameworkElement shoppingCartViewUserControl = BaseViewController.GetViewUserControl();
		if (shoppingCartViewUserControl != null)
		{
			return shoppingCartViewUserControl.Dispatcher.Invoke(() => shoppingCartViewUserControl?.DataContext as NewCartConfirmViewModel);
		}
		return null;
	}

	private static void ConfigureShoppingCartDisplay(NewCartConfirmViewModel viewModel, NewCartConfirmviewParameters parameters)
	{
		viewModel.DisplayProductModels = CreateDisplayProductModelsFromCartItems(viewModel, parameters);
	}

	private static List<DisplayCheckoutProductModel> CreateDisplayProductModelsFromCartItems(NewCartConfirmViewModel viewModel, NewCartConfirmviewParameters parameters)
	{
		List<DisplayCheckoutProductModel> list = new List<DisplayCheckoutProductModel>();
		IRentalShoppingCart newCart = parameters.NewCart;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (newCart != null && service != null && newCart.Items.Count > 0)
		{
			foreach (IRentalShoppingCartItem item in newCart.Items)
			{
				IKioskProduct kioskProduct = null;
				service.GetProduct(item.ProductId, excludeTitleRollup: true, out kioskProduct);
				if (kioskProduct != null)
				{
					DisplayCheckoutProductModel displayCheckoutProductModel = new DisplayCheckoutProductModel
					{
						Name = kioskProduct.ProductName,
						Data = item,
						Image = OrchestrationService.GetProductBitmapImage(kioskProduct)
					};
					bool flag = item.ProductId == parameters.promoProduct?.ProductId;
					PopulateDisplayCheckoutProductModel(displayCheckoutProductModel, flag);
					if (flag)
					{
						displayCheckoutProductModel.TopCancelButtonVisibility = (BaseViewController.IsADAMode ? Visibility.Hidden : Visibility.Visible);
						displayCheckoutProductModel.BottomCancelButtonVisibility = ((!BaseViewController.IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
						list.Insert(0, displayCheckoutProductModel);
					}
					else
					{
						displayCheckoutProductModel.TopCancelButtonVisibility = (BaseViewController.IsADAMode ? Visibility.Hidden : Visibility.Hidden);
						displayCheckoutProductModel.BottomCancelButtonVisibility = (BaseViewController.IsADAMode ? Visibility.Hidden : Visibility.Collapsed);
						list.Add(displayCheckoutProductModel);
					}
				}
			}
		}
		return list;
	}

	private static void PopulateDisplayCheckoutProductModel(DisplayCheckoutProductModel displayCheckoutProductModel, bool isSelectedProduct)
	{
		IRentalShoppingCartItem eachRentalShoppingCartItem = displayCheckoutProductModel?.Data as IRentalShoppingCartItem;
		if (eachRentalShoppingCartItem != null)
		{
			displayCheckoutProductModel.ConfigureForCheckout();
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			IKioskProduct kioskProduct = null;
			service.GetProduct(eachRentalShoppingCartItem.ProductId, excludeTitleRollup: false, out kioskProduct);
			ITitleProduct titleProduct = kioskProduct as ITitleProduct;
			displayCheckoutProductModel.ProductTitleText = BrowseFunctions.GetProductDisplayName(titleProduct);
			MDVProduct mDVProduct = titleProduct as MDVProduct;
			displayCheckoutProductModel.MDVFieldVisibility = ((mDVProduct == null) ? Visibility.Collapsed : Visibility.Visible);
			displayCheckoutProductModel.MDVNumberOfDiscsText = ((mDVProduct != null) ? $"({mDVProduct.TitleProducts.Count} {Resources.multi_disc_vend_discs_text_all_caps})**" : null);
			displayCheckoutProductModel.MDVDiscVendText = ((mDVProduct != null) ? $"{mDVProduct.TitleProducts.Count} {Resources.disc_vend}" : null);
			TitleTypeProperty value = null;
			TitleTypeProperties.Properties.TryGetValue(titleProduct.TitleType, out value);
			string productTypeText = $" - {value?.GetName(Resources.ResourceManager)}";
			displayCheckoutProductModel.ProductTypeText = productTypeText;
			string text = null;
			if (eachRentalShoppingCartItem.Action == RentalShoppingCartItemAction.purchase)
			{
				text = Resources.cart_purchase;
			}
			else
			{
				string arg = $"{eachRentalShoppingCartItem?.PricingRecord?.InitialDays ?? 1}-{Resources.night_rental}";
				string arg2 = $"{Resources.extra_night} {eachRentalShoppingCartItem?.PricingRecord.ExtraNight:C}";
				text = $"{arg} | {arg2}";
			}
			displayCheckoutProductModel.ProductDescriptionLine2Text = text;
			IPromotionService service2 = ServiceLocator.Instance.GetService<IPromotionService>();
			bool flag = service2.GetPromotion()?.PromotionItems.Any((IPromotionItem i) => i.RentalShoppingCartItem.ProductId == eachRentalShoppingCartItem.ProductId) ?? false;
			if (isSelectedProduct | flag)
			{
				decimal price = service2.GetPromotion().Price;
				displayCheckoutProductModel.ProductDescriptionLine3Text = string.Format(Resources.discount_applied_text, price);
				displayCheckoutProductModel.ProductCartPriceText = $"{eachRentalShoppingCartItem.Price - price:C}";
			}
			else
			{
				displayCheckoutProductModel.ProductDescriptionLine3Text = "";
				displayCheckoutProductModel.ProductCartPriceText = $"{eachRentalShoppingCartItem.Price:C}";
			}
			displayCheckoutProductModel.ProductDescriptionLine3TextVisibility = (string.IsNullOrEmpty(displayCheckoutProductModel.ProductDescriptionLine3Text) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private static void ConfigureCommands(NewCartConfirmViewModel viewModel, NewCartConfirmviewParameters parameters)
	{
		viewModel.CancelButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
		{
			CancelCommand_Execute(parameters.CancelAction);
		}, (Func<bool>)null);
		viewModel.SubmitButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
		{
			SubmitCommand_Execute(parameters.ContinueAction);
		}, (Func<bool>)null);
		viewModel.OnCancelBrowseItemModel += ViewModel_OnCancelBrowseItemModel;
	}

	private static void ViewModel_OnCancelBrowseItemModel(DisplayProductModel model, object arg2)
	{
		object obj = model?.Data;
		BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", $"delete {(obj as IRentalShoppingCartItem)?.ProductId}");
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		service.RemovePromoOfferFromCart(service.CurrentRentalShoppingCart.ShoppingSessionId);
		UpdateCartDisplay();
	}

	private static void UpdateCartDisplay()
	{
		ConfigureTextAndVisibility(_viewModel, _parameters);
		ConfigureShoppingCartDisplay(_viewModel, _parameters);
	}

	private static void SubmitCommand_Execute(Action continueAction)
	{
		BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "submit");
		OrchestrationService.ViewGoBackSafe("NewCartConfirm_Submit", popOnly: false);
		continueAction();
	}

	private static void CancelCommand_Execute(Action cancelAction)
	{
		BaseViewController.SessionAnalyticsService?.AddViewEvent("buttonPress", "skip_offer");
		OrchestrationService.ViewGoBackSafe("NewCartConfirm_SkipOffer", popOnly: false);
		CancelWithConfirm(cancelAction);
	}

	private static void CancelWithConfirm(Action cancelAction)
	{
		if (Redbox.Rental.Services.Session.Session.Instance.PromoOfferState.SelectedOffer.CustomerSegmentType != CustomerSegmentType.BasketStretch)
		{
			OrchestrationService.ViewShow("OfferAbandonConfirmView", new OfferAbandonConfirmInfo
			{
				IsInCart = false,
				ContinueAction = cancelAction
			});
		}
		else
		{
			cancelAction();
		}
	}

	private static void ConfigureTextAndVisibility(NewCartConfirmViewModel viewModel, NewCartConfirmviewParameters parameters)
	{
		if (viewModel != null)
		{
			OrchestrationService.SetCurrentUICulture();
			IRentalShoppingCart newCart = parameters.NewCart;
			_ = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
			viewModel.LabelTop = Resources.my_bag;
			viewModel.PriceHeaderText = Resources.summary;
			viewModel.BackButtonText = Resources.skip_offer_button_text;
			viewModel.PassiveSaleNote = Resources.passive_sale_note;
			viewModel.MDVFootnoteVisibility = ((!newCart.Items.Any((IRentalShoppingCartItem x) => x.MultiDisc)) ? Visibility.Collapsed : Visibility.Visible);
			viewModel.MDVFootnote = (newCart.Items.Any((IRentalShoppingCartItem x) => x.MultiDisc) ? Resources.multi_disc_vend_footnote : null);
			int num = (int)((newCart?.DefaultServiceFee ?? 0m) * 100m);
			viewModel.ServiceFeeText = string.Format(Resources.service_fee_text, num);
			viewModel.ServiceFeeTextVisibility = ((num <= 0) ? Visibility.Collapsed : Visibility.Visible);
			viewModel.PricingModel.DiscountLabelText = Resources.discount_label;
			viewModel.PricingModel.ServiceFeeLabelText = Resources.service_fee_label;
			viewModel.PricingModel.SubtotalLabelText = Resources.subtotal_label;
			viewModel.PricingModel.TaxLabelText = Resources.tax_label;
			viewModel.PricingModel.TotalLabelText = Resources.total_label;
			viewModel.PricingModel.SubtotalValueText = $"{newCart?.Totals?.SubTotal:C}";
			string text = (ServiceLocator.Instance.GetService<IDataService>()?.GetStore())?.State;
			viewModel.PricingModel.TaxLineVisibility = ((text == "SC" || text == "MD") ? Visibility.Collapsed : Visibility.Visible);
			viewModel.PricingModel.TaxValueText = $"{newCart?.Totals?.TaxAmount:C}";
			viewModel.PricingModel.TotalValueText = $"{newCart?.Totals?.GrandTotal:f2}";
			string text2 = $"{((!((newCart?.Totals?.DiscountAmountUsed ?? 0m) != 0m)) ? ((decimal?)null) : newCart?.Totals?.DiscountAmountUsed):C}";
			if (!string.IsNullOrEmpty(text2))
			{
				text2 = $"-{text2}";
			}
			viewModel.PricingModel.DiscountValueText = text2;
			viewModel.PricingModel.DiscountLineVisibility = ((!((newCart?.Totals?.DiscountAmountUsed ?? 0m) > 0m)) ? Visibility.Collapsed : Visibility.Visible);
			viewModel.PricingModel.ServiceFeeValueText = $"{newCart?.Totals?.ServiceFee:C}";
			viewModel.PricingModel.ServiceFeeLineVisibility = ((!((newCart?.Totals?.ServiceFee ?? 0m) > 0m)) ? Visibility.Collapsed : Visibility.Visible);
			viewModel.SubmitButtonText = Resources.submit_button;
			viewModel.AdaSkipOfferButtonVisibility = ((!BaseViewController.IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
			viewModel.SkipOfferButtonVisibility = ((BaseViewController.IsADAMode || parameters.promoProduct != null) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	private static void CreateAndStartTimer()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		ITextToSpeechService service2 = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		int timeOut = ((service2 != null && service2.TTSEnabled) ? 90000 : OrchestrationService.TTSGetTimeout("medium", 15000));
		IIdleTimer obj = service?.CreateIdleTimer("NewCartConfirmViewTimer");
		obj?.Initialize(timeOut, delegate
		{
			OrchestrationService.AnalyticsAddViewEvent("context", "timeOut");
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			MessagePopupViewController.ShowOfferTimeoutMessage(delegate
			{
				OrchestrationService.ViewGoBackSafe("NewCartConfirmViewController time out", popOnly: false);
				CancelWithConfirm(_timeoutAction);
			}, delegate
			{
				OrchestrationService.ViewGoBackSafe("NewCartConfirmViewController time out", popOnly: false);
				CancelWithConfirm(_cancelAction);
			}, delegate
			{
				OrchestrationService.ViewGoBackSafe("NewCartConfirmViewController time out", popOnly: false);
			});
		});
		obj?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		(service?.GetIdleTimer("NewCartConfirmViewTimer"))?.Stop();
		service?.RemoveIdleTimer("PromoOfferBrowseViewTimer");
	}

	public static void LeaveView()
	{
		StopAndRemoveTimer();
	}
}
