using System.Collections.ObjectModel;
using System.Windows;
using Redbox.Core;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ReviewPopupViewController : BaseViewController
{
	public static void ConfigureReviewPopupView(ReviewPopupInfo info)
	{
		BaseViewController.StopIdleTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			ReviewsPopupViewModel reviewsPopupViewModel = GetViewModel();
			if (reviewsPopupViewModel == null)
			{
				reviewsPopupViewModel = new ReviewsPopupViewModel();
			}
			reviewsPopupViewModel.BackButtonText = Resources.back_button;
			reviewsPopupViewModel.HeaderText = Resources.reviews_title;
			reviewsPopupViewModel.BackCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), BackCommand_Execute);
			reviewsPopupViewModel.NextPagePressedCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), AddNextPagePressedAnalytics);
			reviewsPopupViewModel.PreviousPagePressedCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), AddPreviousPagePressedAnalytics);
			reviewsPopupViewModel.PageNumberPressedCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), AddPageNumberPressedAnalytics);
			UpdateReviews(reviewsPopupViewModel, info.TitleProduct);
			viewUserControl.DataContext = reviewsPopupViewModel;
			OrchestrationService.AnalyticsAddViewEvent("context", "StarRatingsDialog");
		}
		BaseViewController.StartIdleTimer();
	}

	private static void BackCommand_Execute()
	{
		BaseViewController.ProcessWPFHit();
		OrchestrationService.AnalyticsAddViewEvent("buttonPress", "star_ratings_back");
		OrchestrationService.ViewGoBackSafe("ReviewPopupView cancel", popOnly: false);
	}

	internal static void LeaveReviewPopupView()
	{
	}

	private static void UpdateReviews(ReviewsPopupViewModel viewModel, ITitleProduct titleProduct)
	{
		ObservableCollection<ReviewModel> observableCollection = new ObservableCollection<ReviewModel>();
		foreach (Review review in ServiceLocator.Instance.GetService<IDataService>().GetReviews(titleProduct.ProductId))
		{
			observableCollection.Add(new ReviewModel
			{
				NumberOfStars = review.Stars,
				ReviewerName = review.DisplayName,
				ReviewDate = review.ReviewDate.ToString("MMMM d, yyyy"),
				ReviewText = review.ReviewText
			});
		}
		viewModel.ReviewModels = observableCollection;
	}

	private static ReviewsPopupViewModel GetViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as ReviewsPopupViewModel;
		}
		return null;
	}

	private static void AddPageNumberPressedAnalytics(int pageNumber)
	{
		BaseViewController.AnalyticsAddViewEvent("pageChange", $"reviews-page-{pageNumber.ToString()}");
	}

	private static void AddPreviousPagePressedAnalytics()
	{
		BaseViewController.AnalyticsAddViewEvent("pageChange", "reviews-page-back");
	}

	private static void AddNextPagePressedAnalytics()
	{
		BaseViewController.AnalyticsAddViewEvent("pageChange", "reviews-page-next");
	}
}
