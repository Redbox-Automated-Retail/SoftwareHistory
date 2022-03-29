using System;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Services.Configuration;
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
}
