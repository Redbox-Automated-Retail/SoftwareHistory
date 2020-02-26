using System;
using System.Windows;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class TitleDetailsFullDetailsPopupViewController : BaseViewController
{
	public static void ConfigureTitleDetailsFullDetailsPopupView(TitleDetailsFullDetailsPopupViewInfo titleDetailsFullDetailsPopupViewInfo)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			TitleDetailsFullDetailsPopupViewModel titleDetailsFullDetailsPopupViewModel = new TitleDetailsFullDetailsPopupViewModel();
			titleDetailsFullDetailsPopupViewModel.Header = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.Header;
			titleDetailsFullDetailsPopupViewModel.Description = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.Description;
			titleDetailsFullDetailsPopupViewModel.Starring = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.Starring;
			titleDetailsFullDetailsPopupViewModel.StarringVisibility = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.StarringVisibility;
			titleDetailsFullDetailsPopupViewModel.Directors = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.DirectedBy;
			titleDetailsFullDetailsPopupViewModel.DirectorsVisibility = (string.IsNullOrEmpty(titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.DirectedBy) ? Visibility.Collapsed : Visibility.Visible);
			titleDetailsFullDetailsPopupViewModel.Rating = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.RatingDescription;
			titleDetailsFullDetailsPopupViewModel.RatingVisibility = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.RatingVisibility;
			titleDetailsFullDetailsPopupViewModel.FormatInfo = ((titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.CCSupported == Visibility.Visible) ? Resources.title_details_cc_text : "");
			titleDetailsFullDetailsPopupViewModel.OneNightPrices = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.OneNightPrice.Replace("\n", ", ");
			titleDetailsFullDetailsPopupViewModel.OneNightReturnTimes = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.OneNightReturnTime.Replace("\n", ", ");
			titleDetailsFullDetailsPopupViewModel.MultiNightPrices = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.MultiNightPrice.Replace("\n", ", ");
			titleDetailsFullDetailsPopupViewModel.MultiNightPricesVisibility = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.MultiNightPriceVisibility;
			titleDetailsFullDetailsPopupViewModel.MultiNightReturnTimes = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.MultiNightReturnTime.Replace("\n", ", ");
			titleDetailsFullDetailsPopupViewModel.MultiNightReturnTimesVisibility = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.MultiNightReturnTimeVisibility;
			titleDetailsFullDetailsPopupViewModel.ServiceFeeMessage = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.ServiceFeeText;
			titleDetailsFullDetailsPopupViewModel.ServiceFeeMessageVisibility = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.ServiceFeeTextVisibility;
			titleDetailsFullDetailsPopupViewModel.DealMessage = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.DealText;
			titleDetailsFullDetailsPopupViewModel.DealMessageVisibility = titleDetailsFullDetailsPopupViewInfo.DisplayProductModel.HotDealTextVisibility;
			titleDetailsFullDetailsPopupViewModel.OnFullDetailsCloseButtonClicked = (Action)Delegate.Combine(titleDetailsFullDetailsPopupViewModel.OnFullDetailsCloseButtonClicked, new Action(TitleDetailsFullDetailsPopupViewModel_OnCloseButtonClicked));
			viewUserControl.DataContext = titleDetailsFullDetailsPopupViewModel;
		}
	}

	private static void TitleDetailsFullDetailsPopupViewModel_OnCloseButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("TitleDetailsFullDetailsPopupClose");
		OrchestrationService.ViewGoBackSafe("TitleDetailsFullDetailsPopupViewController close", popOnly: false);
	}
}
