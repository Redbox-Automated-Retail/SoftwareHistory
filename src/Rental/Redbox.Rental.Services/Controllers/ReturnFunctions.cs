using System;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Transactions;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ReturnFunctions
{
	public static void ShowMachineFullWarning()
	{
		Action action = delegate
		{
			ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession("machine full return");
		};
		MessagePopupViewController.ShowMachineFullWarning(action, action);
	}

	public static bool IsValidBarcode(string barcode)
	{
		bool flag = false;
		try
		{
			int.Parse(barcode);
			flag = true;
		}
		catch
		{
			flag = false;
		}
		if (flag)
		{
			return barcode.Length == 9;
		}
		return false;
	}

	public static bool CanShowReturnVisitPromoOfferView()
	{
		bool flag = false;
		if (DateTime.TryParse(Redbox.Rental.Services.Configuration.Configuration.Instance.ReturnVisitPromoOfferPromotionExpirationDate, out var result))
		{
			flag = DateTime.Now <= result;
		}
		if ((Redbox.Rental.Services.Configuration.Configuration.Instance.ReturnVisitPromoOfferEnabled & flag) && !string.IsNullOrEmpty(Redbox.Rental.Services.Configuration.Configuration.Instance.ReturnVisitPromoOfferPromotionName))
		{
			return Redbox.Rental.Services.Configuration.Configuration.Instance.ReturnVisitPromoOfferPromotionPrice > 0m;
		}
		return false;
	}

	public static void ShowReturnVisitPromoOfferView(Action closeAction)
	{
		DateTime.TryParse(Redbox.Rental.Services.Configuration.Configuration.Instance.ReturnVisitPromoOfferPromotionExpirationDate, out var result);
		OrchestrationService.ViewShow("return_visit_promo_offer_view", new ReturnVisitPromoOfferViewParameters
		{
			Name = Redbox.Rental.Services.Configuration.Configuration.Instance.ReturnVisitPromoOfferPromotionName,
			Price = Redbox.Rental.Services.Configuration.Configuration.Instance.ReturnVisitPromoOfferPromotionPrice,
			ExpirationDate = result,
			CloseAction = closeAction
		});
	}

	public static void ShowReturnCompleteViewWithPromoCheck(string barcode, Action closeAction)
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.ReturnVisitPromoOfferEnabled && Redbox.Rental.Services.Configuration.Configuration.Instance.CallReturnVisitPromoCheck)
		{
			ServiceLocator.Instance.GetService<ICallbackService>();
			ServiceLocator.Instance.GetService<IKioskClientService>().Transaction.ReturnPromoCheck(barcode, delegate(ReturnVisitPromotionInfo promo)
			{
				PromoCheckCallback(promo, closeAction);
			});
		}
		else if (CanShowReturnVisitPromoOfferView())
		{
			ShowReturnVisitPromoOfferView(closeAction);
		}
		else
		{
			MessagePopupViewController.ShowMessagePopupViewReturnSuccess(closeAction);
		}
	}

	private static void PromoCheckCallback(ReturnVisitPromotionInfo promoInfo, Action closeAction)
	{
		(ServiceLocator.Instance.GetService<IEngineApplication>() as ApplicationContext).MainForm.Invoke((Action)delegate
		{
			if (promoInfo != null && promoInfo.Success && promoInfo.IsLastDisc && promoInfo.Promo != null && DateTime.Now <= promoInfo.Promo.ExpirationDate && !string.IsNullOrEmpty(promoInfo.Promo.PromoCode) && promoInfo.Promo.Amount > 0m)
			{
				OrchestrationService.ViewShow("return_visit_promo_offer_view", new ReturnVisitPromoOfferViewParameters
				{
					Name = promoInfo.Promo.PromoCode,
					Price = promoInfo.Promo.Amount,
					ExpirationDate = promoInfo.Promo.ExpirationDate,
					CloseAction = closeAction
				});
			}
			else if (promoInfo != null && promoInfo.Success && promoInfo.IsLastDisc)
			{
				MessagePopupViewController.ShowMessagePopupViewReturnSuccessWithBrowse(delegate
				{
					closeAction?.Invoke();
				}, delegate
				{
					ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddButtonPressEvent("ReturnSuccessWithBrowseClose");
					closeAction?.Invoke();
				}, delegate
				{
					ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddButtonPressEvent("ReturnSuccessWithBrowseSeeNewMovies");
					closeAction?.Invoke();
					OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.ShowBrowseView));
					Redbox.Rental.Services.Session.Session.Instance?.BrowseState?.SetDefaultBrowseStateSet();
					OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
				});
			}
			else
			{
				MessagePopupViewController.ShowMessagePopupViewReturnSuccess(closeAction);
			}
		});
	}
}
