using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Promotion;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ReturnVisitPromoOfferViewController : BaseViewController
{
	private static string _name;

	private static decimal _price;

	private static Action _closeAction;

	public static void ConfigureReturnVisitPromoOfferView(ReturnVisitPromoOfferViewParameters returnVisitPromoOfferViewParameters)
	{
		BaseViewController.StopIdleTimer();
		CreateAndStartTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			ReturnVisitPromoOfferViewModel returnVisitPromoOfferViewModel = new ReturnVisitPromoOfferViewModel();
			returnVisitPromoOfferViewModel.TitleText = Resources.return_visit_promo_offer_title_text;
			returnVisitPromoOfferViewModel.AmountText = string.Format(Resources.return_visit_promo_offer_amount_text, returnVisitPromoOfferViewParameters.Price);
			_price = returnVisitPromoOfferViewParameters.Price;
			returnVisitPromoOfferViewModel.MessageText = Resources.return_visit_promo_offer_message_text;
			returnVisitPromoOfferViewModel.PromoCodeText = string.Format(Resources.return_visit_promo_offer_promo_code_text, returnVisitPromoOfferViewParameters.Name);
			_name = returnVisitPromoOfferViewParameters.Name;
			returnVisitPromoOfferViewModel.ExpirationText = string.Format(Resources.return_visit_promo_offer_expiration_text, returnVisitPromoOfferViewParameters.ExpirationDate);
			returnVisitPromoOfferViewModel.LegalText = Resources.return_visit_promo_offer_legal_text;
			returnVisitPromoOfferViewModel.CloseButtonText = Resources.close_button.ToUpper();
			returnVisitPromoOfferViewModel.UseNowButtonText = Resources.use_now_button_text.ToUpper();
			_closeAction = returnVisitPromoOfferViewParameters.CloseAction;
			returnVisitPromoOfferViewModel.OnCloseButtonClicked = (Action)Delegate.Combine(returnVisitPromoOfferViewModel.OnCloseButtonClicked, new Action(ReturnVisitPromoOfferViewModel_OnCloseButtonClicked));
			returnVisitPromoOfferViewModel.OnUseNowButtonClicked = (Action)Delegate.Combine(returnVisitPromoOfferViewModel.OnUseNowButtonClicked, new Action(ReturnVisitPromoOfferViewModel_OnUseNowButtonClicked));
			viewUserControl.DataContext = returnVisitPromoOfferViewModel;
		}
	}

	public static void LeaveReturnVisitPromoOfferView()
	{
		StopAndRemoveTimer();
		BaseViewController.StartIdleTimer();
	}

	private static void CreateAndStartTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		int timeOut = OrchestrationService.TTSGetTimeout("long", 40000);
		(service?.CreateTimer("ReturnVisitPromoOfferViewTimer", timeOut, timeOut, delegate
		{
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			_closeAction?.Invoke();
		}))?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		(service?.GetTimer("ReturnVisitPromoOfferViewTimer"))?.Stop();
		service?.RemoveTimer("ReturnVisitPromoOfferViewTimer");
	}

	private static void ReturnVisitPromoOfferViewModel_OnCloseButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ReturnVisitPromoOfferClose");
		_closeAction?.Invoke();
	}

	private static void ReturnVisitPromoOfferViewModel_OnUseNowButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ReturnVisitPromoOfferUseNow");
		_closeAction?.Invoke();
		IPromotion promotion = CreatePromotionFromReturnVisitPromo(_name, _price);
		if (promotion != null)
		{
			OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.ShowBrowseView));
			Redbox.Rental.Services.Session.Session.Instance?.BrowseState?.SetDefaultBrowseStateSet();
			OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
			PromotionService.Instance?.SetPromotion(promotion);
			IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			service?.CurrentRentalShoppingCart?.ValidateAndUpdatePromoCode();
			service?.CurrentRentalShoppingCart?.UpdateServiceFee();
			service?.CurrentRentalShoppingCart?.ClearLoyaltyPointsToBeEarned();
		}
	}

	private static IPromotion CreatePromotionFromReturnVisitPromo(string name, decimal price)
	{
		if (!string.IsNullOrEmpty(name) && price > 0m)
		{
			return new Redbox.Rental.Services.Promotion.Promotion
			{
				Name = name,
				OriginalPrice = price,
				Price = price,
				Quantity = 1,
				Intent = PromotionIntentCode.FixedValue,
				OriginalPercent = 0m,
				Percent = 0m,
				RentQuantity = 0,
				RentFormat = null,
				GetQuantity = 0,
				GetFormat = null,
				CampaignTitles = null,
				ProductTypeId = null,
				FormatIds = null,
				ActionCode = PromotionActionCode.Rental,
				AllowFullDiscount = true,
				DefaultPromo = null,
				CampaignId = 0
			};
		}
		return null;
	}
}
