using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Rental.Model.Marketing;
using Redbox.Rental.Model.Personalization;
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
		if (viewUserControl == null)
		{
			return;
		}
		if (useIdleTimer)
		{
			BaseViewController.StopIdleTimer();
		}
		BaseViewController.SetCurrentUICulture();
		SimpleModel model = info.Model;
		model.Info = info;
		model.CancelAction = info.CancelAction;
		model.ContinueAction = info.ContinueAction;
		model.OtherAction = info.OtherAction;
		if (info.CheckBoxChangedAction != null)
		{
			model.CheckBoxChangedAction = delegate(bool isChecked)
			{
				BaseViewController.ProcessWPFHit();
				info.CheckBoxChangedAction(isChecked);
			};
		}
		if (model.CancelAction != null)
		{
			model.CancelButtonCommand = DynamicRouting.RegisterRoutedCommand(Redbox.Rental.UI.Commands.CancelButtonCommand, CancelButtonCommand_Execute);
		}
		if (model.ContinueAction != null)
		{
			model.ContinueButtonCommand = DynamicRouting.RegisterRoutedCommand(Redbox.Rental.UI.Commands.ContinueButtonCommand, ContinueButtonCommand_Execute, ContinueButtonCommand_CanExecute);
		}
		if (model.OtherAction != null)
		{
			model.OtherButtonCommand = DynamicRouting.RegisterRoutedCommand(Redbox.Rental.UI.Commands.OtherButtonCommand, OtherButtonCommand_Execute);
		}
		viewUserControl.DataContext = model;
		if (useIdleTimer)
		{
			BaseViewController.StartIdleTimer();
		}
		InvokeAnalytics(model, AnalyticsTypes.context);
	}

	public static void ConfigurePromoView(SimpleInfo info)
	{
		string arg = string.Empty;
		if (info.Obj is ILoginEmailProvider)
		{
			arg = (info.Obj as ILoginEmailProvider).LoginEmailAddress;
		}
		if (info.Model == null)
		{
			info.Model = new SimpleModel
			{
				ContinueButtonText = Resources.promo_perks_btn1_text,
				CancelButtonText = Resources.promo_perks_btn2_text,
				TitleText = Resources.promo_perks_title,
				MessageText = Resources.promo_perks_message,
				CommentText = $"{Resources.promo_perks_comment} {arg}",
				FooterText = Resources.promo_perks_footer,
				Circle1Text = Resources.promo_perks_circle1,
				Circle2Text = Resources.promo_perks_circle2,
				Circle3Text = Resources.promo_perks_circle3,
				OtherButtonText = Resources.promo_perks_btn_terms,
				CheckboxChecked = false,
				CheckboxText = Resources.promo_perks_checkbox_label
			};
		}
		ConfigureView(info, !info.UseIdleTimer.HasValue || info.UseIdleTimer.Value);
	}

	public static void ConfigurePerksView(SimpleInfo info)
	{
		if (info.Model == null)
		{
			info.Model = new SimpleModel
			{
				ContinueButtonText = Resources.joined_perks_btn_text,
				FooterText = Resources.joined_perks_footer,
				MessageText = Resources.joined_perks_message,
				TitleText = Resources.joined_perks_title
			};
		}
		ConfigureView(info, !info.UseIdleTimer.HasValue || info.UseIdleTimer.Value);
	}

	public static void ConfigureTextView(SimpleInfo info)
	{
		if (info.Model == null)
		{
			info.Model = new SimpleModel
			{
				ContinueButtonText = Resources.joined_texing_btn_text,
				MessageText = Resources.joined_texing_message,
				TitleText = Resources.joined_texing_title
			};
		}
		ConfigureView(info, !info.UseIdleTimer.HasValue || info.UseIdleTimer.Value);
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
		ConfigureView(info, info.UseIdleTimer.HasValue && info.UseIdleTimer.Value);
	}

	private static void InvokeAnalytics(SimpleModel model, AnalyticsTypes analyticsType)
	{
		model?.Info?.AnalyticsAction?.Invoke(analyticsType);
	}

	public static void OtherButtonCommand_Execute()
	{
		BaseViewController.StopIdleTimer();
		SimpleModel viewModel = BaseViewController.GetViewModel<SimpleModel>();
		InvokeAnalytics(viewModel, AnalyticsTypes.button3Press);
		if (viewModel != null && viewModel.OtherAction != null)
		{
			viewModel.OtherAction();
		}
		else
		{
			OrchestrationService.ViewGoBackSafe("Problem: SimpleModel.OtherAction was not set", popOnly: false);
		}
		BaseViewController.ProcessWPFHit();
	}

	public static void CancelButtonCommand_Execute()
	{
		BaseViewController.StopIdleTimer();
		SimpleModel viewModel = BaseViewController.GetViewModel<SimpleModel>();
		InvokeAnalytics(viewModel, AnalyticsTypes.button2Press);
		if (viewModel != null && viewModel.CancelAction != null)
		{
			viewModel.CancelAction();
		}
		else
		{
			OrchestrationService.ViewGoBackSafe("Problem: SimpleModel.CancelAction was not set", popOnly: false);
		}
		BaseViewController.ProcessWPFHit();
	}

	public static void ContinueButtonCommand_Execute()
	{
		BaseViewController.StopIdleTimer();
		SimpleModel viewModel = BaseViewController.GetViewModel<SimpleModel>();
		InvokeAnalytics(viewModel, AnalyticsTypes.button1Press);
		if (viewModel != null && viewModel.ContinueAction != null)
		{
			viewModel.ContinueAction();
		}
		else
		{
			OrchestrationService.ViewGoBackSafe("Problem: SimpleModel.ContinueAction was not set", popOnly: false);
		}
		BaseViewController.ProcessWPFHit();
	}

	private static bool ContinueButtonCommand_CanExecute()
	{
		SimpleModel viewModel = BaseViewController.GetViewModel<SimpleModel>();
		if (viewModel != null && viewModel.CheckBoxChangedAction != null)
		{
			return viewModel.CheckboxChecked;
		}
		return true;
	}
}
