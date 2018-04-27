using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Rental.Model.Marketing;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class SimpleViewController : BaseViewController
{
	private static void ConfigureView(SimpleInfo info, bool useIdleTimer)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			if (useIdleTimer)
			{
				BaseViewController.StopIdleTimer();
			}
			BaseViewController.SetCurrentUICulture();
			SimpleModel model = info.Model;
			model.CancelAction = info.CancelAction;
			model.ContinueAction = info.ContinueAction;
			if (model.CancelAction != null)
			{
				model.CancelButtonCommand = DynamicRouting.RegisterRoutedCommand(Redbox.Rental.UI.Commands.CancelButtonCommand, CancelButtonCommand_Execute);
			}
			if (model.ContinueAction != null)
			{
				model.ContinueButtonCommand = DynamicRouting.RegisterRoutedCommand(Redbox.Rental.UI.Commands.ContinueButtonCommand, ContinueButtonCommand_Execute);
			}
			viewUserControl.DataContext = model;
			if (useIdleTimer)
			{
				BaseViewController.StartIdleTimer();
			}
		}
	}

	public static void ConfigurePerksView(SimpleInfo info)
	{
		if (info.Model == null)
		{
			info.Model = new SimpleModel
			{
				ContinueButtonText = Resources.joined_perks_btn_text,
				MessageText = Resources.joined_perks_message,
				TitleText = Resources.joined_perks_title
			};
		}
		ConfigureView(info, useIdleTimer: true);
	}

	public static void ConfigureTextView(SimpleInfo info)
	{
		if (info.Model == null)
		{
			info.Model = new SimpleModel
			{
				ContinueButtonText = Resources.joined_texing_btn_text,
				CommentText = Resources.joined_texing_comment,
				MessageText = Resources.joined_texing_message,
				TitleText = Resources.joined_texing_title
			};
		}
		ConfigureView(info, useIdleTimer: true);
	}

	public static void ConfigureEmailReceiptView(EmailReceiptInfo info)
	{
		if (info.Model == null)
		{
			MarketingAd marketingAd = BaseViewController.GetMarketingAd(MarketingAdLocation.EmailNotification, createImpression: true);
			BitmapImage bannerImage = ((marketingAd != null && marketingAd.Visible) ? BaseViewController.GetBundleBitmapImage(marketingAd.ImageName) : null);
			info.Model = new SimpleModel
			{
				ContinueButtonText = Resources.email_receipt_btn_text,
				CommentText = Resources.email_receipt_comment,
				TitleText = Resources.email_receipt_title,
				MessageText = info.EmailAddress,
				BannerImage = bannerImage
			};
		}
		ConfigureView(info, useIdleTimer: false);
	}

	public static void CancelButtonCommand_Execute()
	{
		BaseViewController.StopIdleTimer();
		SimpleModel viewModel = BaseViewController.GetViewModel<SimpleModel>();
		if (viewModel != null && viewModel.CancelAction != null)
		{
			viewModel.CancelAction();
		}
		else
		{
			OrchestrationService.ViewGoBackSafe("Problem: UniversalModel.CancelAction was not set", popOnly: false);
		}
		BaseViewController.ProcessWPFHit();
	}

	public static void ContinueButtonCommand_Execute()
	{
		BaseViewController.StopIdleTimer();
		SimpleModel viewModel = BaseViewController.GetViewModel<SimpleModel>();
		if (viewModel != null && viewModel.ContinueAction != null)
		{
			viewModel.ContinueAction();
		}
		else
		{
			OrchestrationService.ViewGoBackSafe("Problem: UniversalModel.ContinueAction was not set", popOnly: false);
		}
		BaseViewController.ProcessWPFHit();
	}
}
