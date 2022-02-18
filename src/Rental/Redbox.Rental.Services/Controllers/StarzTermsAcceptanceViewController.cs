using System.Windows;
using Redbox.Rental.Services.Session;
using Redbox.Rental.Services.Starz;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class StarzTermsAcceptanceViewController : BaseViewController
{
	public static void ConfigureStarzTermsAcceptanceView(StarzTermsAcceptanceViewParameters starzTermsAcceptanceViewParameters)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.StopIdleTimer();
			StarzTermsAcceptanceViewModel starzTermsAcceptanceViewModel = new StarzTermsAcceptanceViewModel
			{
				StarzSubscriptionProduct = starzTermsAcceptanceViewParameters.StarzSubscriptionProduct
			};
			ConfigureText(starzTermsAcceptanceViewModel, starzTermsAcceptanceViewParameters.EmailAddress);
			starzTermsAcceptanceViewModel.CheckMarkCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), CheckMarkCommand_Execute);
			starzTermsAcceptanceViewModel.AcceptButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), AcceptButtonCommand_Execute);
			starzTermsAcceptanceViewModel.BackButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), BackButtonCommand_Execute);
			starzTermsAcceptanceViewModel.TermsAndPrivacyButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), TermsAndPrivacyButtonCommand_Execute);
			viewUserControl.DataContext = starzTermsAcceptanceViewModel;
			BaseViewController.StartIdleTimer();
		}
	}

	private static void ConfigureText(StarzTermsAcceptanceViewModel starzTermsAcceptanceViewModel, string emailAddress)
	{
		if (starzTermsAcceptanceViewModel != null)
		{
			OrchestrationService.SetCurrentUICulture();
			starzTermsAcceptanceViewModel.TitleText = Resources.starz_terms_acceptance_title_text;
			starzTermsAcceptanceViewModel.SubtitleText = ((Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.IsLoggedIn ?? false) ? Resources.starz_terms_acceptance_subtitle_text_existing_account : Resources.starz_terms_acceptance_subtitle_text_new_account);
			starzTermsAcceptanceViewModel.EmailText = emailAddress;
			starzTermsAcceptanceViewModel.TermsHeader = Resources.starz_terms_acceptance_terms_header;
			starzTermsAcceptanceViewModel.TermsText = starzTermsAcceptanceViewModel.StarzSubscriptionProduct.AcceptanceTerms;
			ConfigureCheckMarkAndAcceptButton(starzTermsAcceptanceViewModel);
		}
	}

	private static void ConfigureCheckMarkAndAcceptButton(StarzTermsAcceptanceViewModel starzTermsAcceptanceViewModel)
	{
		OrchestrationService.SetCurrentUICulture();
		bool isStarzTermsAcceptanceCheckMarkChecked = Redbox.Rental.Services.Session.Session.Instance.BrowseState.IsStarzTermsAcceptanceCheckMarkChecked;
		starzTermsAcceptanceViewModel.CheckMarkVisibility = ((!isStarzTermsAcceptanceCheckMarkChecked) ? Visibility.Hidden : Visibility.Visible);
		starzTermsAcceptanceViewModel.AcceptButtonText = (isStarzTermsAcceptanceCheckMarkChecked ? Resources.starz_terms_acceptance_button_enabled_text : Resources.starz_terms_acceptance_button_disabled_text);
		starzTermsAcceptanceViewModel.AcceptButtonIsEnabled = isStarzTermsAcceptanceCheckMarkChecked;
	}

	private static void CheckMarkCommand_Execute()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("StarzTermsAcceptanceCheckMark");
		StarzTermsAcceptanceViewModel viewModel = BaseViewController.GetViewModel<StarzTermsAcceptanceViewModel>();
		if (viewModel != null)
		{
			Redbox.Rental.Services.Session.Session.Instance.BrowseState.IsStarzTermsAcceptanceCheckMarkChecked = !Redbox.Rental.Services.Session.Session.Instance.BrowseState.IsStarzTermsAcceptanceCheckMarkChecked;
			ConfigureCheckMarkAndAcceptButton(viewModel);
		}
	}

	private static void AcceptButtonCommand_Execute()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("StarzTermsAcceptanceAcceptButton");
		StarzTermsAcceptanceViewModel viewModel = BaseViewController.GetViewModel<StarzTermsAcceptanceViewModel>();
		if (viewModel != null)
		{
			StarzSignupService.AddStarzSubscriptionPrerequisites(viewModel.StarzSubscriptionProduct);
		}
	}

	private static void BackButtonCommand_Execute()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("StarzTermsAcceptanceBackButton");
		OrchestrationService.UnwindToSafe("starz_offer_detail_view", discard: true, "StarzTermsAcceptanceBackButton");
	}

	private static void TermsAndPrivacyButtonCommand_Execute()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("StarzTermsAcceptanceTermsAndPrivacyButton");
		HelpViewController.ShowSubscriptions();
	}
}
