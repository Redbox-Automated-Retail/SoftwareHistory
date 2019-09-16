using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ApplyPromoViewController : BaseViewController
{
	public static void ConfigureApplyPromoView(ApplyPromoInfo info)
	{
		BaseViewController.StopIdleTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			ApplyPromoViewModel applyPromoViewModel = GetViewModel();
			if (applyPromoViewModel == null)
			{
				applyPromoViewModel = new ApplyPromoViewModel();
			}
			applyPromoViewModel.CancelButtonText = Resources.perks_cancel_btn_text;
			applyPromoViewModel.EnterPromoButtonText = Resources.apply_promo_enter_promo_button_text;
			applyPromoViewModel.EnterPromoMessage = Resources.apply_promo_enter_promo_message;
			applyPromoViewModel.TitleText = Resources.apply_promo_title;
			applyPromoViewModel.TopMessage = Resources.apply_promo_message_text;
			PromoListLogic logic = new PromoListLogic(applyPromoViewModel);
			applyPromoViewModel.PromoLogic = logic;
			applyPromoViewModel.CancelCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), CancelCommand_Execute);
			applyPromoViewModel.EnterPromoCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				EnterPromoCommand_Execute(info.ApplyPromoCodeAction);
			}, (Func<bool>)null);
			applyPromoViewModel.NextPagePressedCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), logic.AddNextPagePressedAnalytics);
			applyPromoViewModel.PreviousPagePressedCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), logic.AddPreviousPagePressedAnalytics);
			applyPromoViewModel.PageNumberPressedCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), logic.AddPageNumberPressedAnalytics);
			applyPromoViewModel.AddOrRemovePromoCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), delegate(StoredPromoCodeModel obj)
			{
				AddOrRemovePromoCommand_Execute(obj, info.ApplyPromoCodeAction);
			});
			applyPromoViewModel.OnLoadedCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				logic.AddAnalyticsPromosShown();
			}, (Func<bool>)null);
			UpdateOfferList(applyPromoViewModel);
			applyPromoViewModel.PromosPerPage = 3;
			applyPromoViewModel.CurrentPageNumber = PromoListLogic.FindPageOfSelectedPromo(applyPromoViewModel);
			viewUserControl.DataContext = applyPromoViewModel;
			ConfigureTTS(applyPromoViewModel);
		}
		BaseViewController.StartIdleTimer();
	}

	private static void CancelCommand_Execute()
	{
		BaseViewController.ProcessWPFHit();
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "Cancel");
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ApplyPromoCancel");
		OrchestrationService.ViewGoBackSafe("ApplyPromoView cancel", popOnly: false);
	}

	internal static void LeaveApplyPromoView()
	{
	}

	private static void UpdateOfferList(ApplyPromoViewModel viewModel)
	{
		IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
		string activePromoCode = string.Empty;
		if (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart != null)
		{
			activePromoCode = BaseViewController.RentalShoppingCartService.CurrentRentalShoppingCart.Promotion?.Name;
		}
		PromoListLogic.CreateOfferListItemModels(viewModel, personalizationSession, activePromoCode, 'X', includeOffers: false);
	}

	private static void AddOrRemovePromoCommand_Execute(StoredPromoCodeModel model, Action<string> applyPromoAction)
	{
		BaseViewController.ProcessWPFHit();
		model.Logic.AddRemovePromoButtonPressedAnalytics(model);
		if (!model.IsAdded)
		{
			PromoConfirmationInfo promoConfirmationInfo = new PromoConfirmationInfo();
			promoConfirmationInfo.promoCodeModel = model;
			promoConfirmationInfo.ApplyPromoCodeAction = delegate(string p)
			{
				OrchestrationService.ViewGoBackSafe("ApplyPromoView AddOrRemovePromoSelected", popOnly: false);
				applyPromoAction(p);
			};
			promoConfirmationInfo.CancelAction = delegate
			{
			};
			OrchestrationService.ViewShow("promo_confirmation_view", promoConfirmationInfo);
		}
		else
		{
			if (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart != null)
			{
				BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.ClearPromoCode();
				BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.ClearLoyaltyPointsToBeEarned();
			}
			OrchestrationService.ViewGoBackSafe("ApplyPromoView AddOrRemovePromoSelected", popOnly: false);
		}
	}

	private static void EnterPromoCommand_Execute(Action<string> applyPromoAction)
	{
		BaseViewController.ProcessWPFHit();
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "EnterPromo");
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ApplyPromoEnterPromo");
		OrchestrationService.ViewGoBackSafe("ApplyPromoView EnterPromoSelected", popOnly: false);
		OrchestrationService.ViewShow("keyboard_promo_view", new KeyboardInfo
		{
			MaxLength = 31,
			KeyboardText = "",
			IsValidEntry = ValidationServices.IsValidPromoCode,
			CancelAction = delegate
			{
				OrchestrationService.ViewGoBack("go back", popOnly: false);
			},
			ContinueAction = applyPromoAction
		});
	}

	private static void ConfigureTTS(ApplyPromoViewModel viewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			viewModel.OnGetSpeechControl = () => ApplyPromoViewTextToSpeech.GetSpeechControl(GetViewModel);
		}
	}

	private static ApplyPromoViewModel GetViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as ApplyPromoViewModel;
		}
		return null;
	}
}
