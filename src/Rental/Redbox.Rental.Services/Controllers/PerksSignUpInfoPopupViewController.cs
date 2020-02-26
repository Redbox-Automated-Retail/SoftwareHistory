using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class PerksSignUpInfoPopupViewController : BaseViewController
{
	public static void ConfigureView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.SetCurrentUICulture();
			PerksSignUpInfoPopupViewModel perksSignUpInfoPopupViewModel = new PerksSignUpInfoPopupViewModel
			{
				OKButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
				{
					OKButtonClickedAction();
				}, (Func<bool>)null)
			};
			PopulateModelText(perksSignUpInfoPopupViewModel);
			ConfigureTTS(perksSignUpInfoPopupViewModel);
			viewUserControl.DataContext = perksSignUpInfoPopupViewModel;
			_ = ServiceLocator.Instance.GetService<ITextToSpeechService>()?.TTSEnabled;
			ServiceLocator.Instance.GetService<ITimerService>();
			BaseViewController.StartIdleTimer();
		}
	}

	private static void PopulateModelText(PerksSignUpInfoPopupViewModel viewModel)
	{
		viewModel.TitleText = Resources.perks_signup_info_title;
		viewModel.EarnHeading = Resources.perks_signup_info_earn_header;
		viewModel.EarnOption1Desc = Resources.perks_signup_info_earn_desc_1;
		viewModel.EarnOption2Desc = Resources.perks_signup_info_earn_desc_2;
		viewModel.EarnOption3Desc = Resources.perks_signup_info_earn_desc_3;
		viewModel.EarnOption4Desc = Resources.perks_signup_info_earn_desc_4;
		viewModel.EarnOption1Value = Resources.perks_signup_info_earn_points_1;
		viewModel.EarnOption2Value = Resources.perks_signup_info_earn_points_2;
		viewModel.EarnOption3Value = Resources.perks_signup_info_earn_points_3;
		viewModel.EarnOption4Value = Resources.perks_signup_info_earn_points_4;
		viewModel.UseHeading = Resources.perks_signup_info_use_header;
		viewModel.UseOption1Desc = Resources.perks_signup_info_use_desc_1;
		viewModel.UseOption2Desc = Resources.perks_signup_info_use_desc_2;
		viewModel.UseOption3Desc = Resources.perks_signup_info_use_desc_3;
		viewModel.UseOption1Value = Resources.perks_signup_info_use_points_1;
		viewModel.UseOption2Value = Resources.perks_signup_info_use_points_2;
		viewModel.UseOption3Value = Resources.perks_signup_info_use_points_3;
		viewModel.OKButtonText = Resources.okay;
	}

	private static void OKButtonClickedAction()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("PerksSignupInfoPopupOK");
		OrchestrationService.ViewGoBackSafe("PerksSignupInfoView OK", popOnly: false);
	}

	private static void ConfigureTTS(PerksSignUpInfoPopupViewModel viewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service == null || !service.TTSEnabled || viewModel == null)
		{
			return;
		}
		viewModel.ProcessGetSpeechControl = delegate
		{
			SpeechControl obj = new SpeechControl
			{
				Name = "tts_perks_sign_up_info_view"
			};
			SpeechPart part = new SpeechPart
			{
				Name = "Intro",
				Loop = true,
				StartPause = 250,
				EndPause = 10000
			};
			part.Refresh = delegate
			{
				part.Clear();
				part.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						viewModel?.OKButtonCommand.Execute(null);
					}
				});
				part.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "ENTER",
					Action = delegate
					{
						viewModel?.OKButtonCommand.Execute(null);
					}
				});
				part.Texts.Add(new TextPart
				{
					Text = "To go back the previous screen press Enter or Cancel."
				});
				part.Texts.Add(new TextPart
				{
					Text = "How it works: You can earn points by performing the following actions"
				});
				part.Texts.Add(new TextPart
				{
					Text = "Renting a DVD will earn 100 points per night"
				});
				part.Texts.Add(new TextPart
				{
					Text = "Renting a Blu-Ray will earn 125 points per night"
				});
				part.Texts.Add(new TextPart
				{
					Text = "Renting a 4K UHD disc will earn 150 points per night"
				});
				part.Texts.Add(new TextPart
				{
					Text = "Renting a Game will earn 200 points per night"
				});
				part.Texts.Add(new TextPart
				{
					Text = "Renting an On Demand movie will earn 125 points per transaction."
				});
				part.Texts.Add(new TextPart
				{
					Text = "You can use your points to get free rentals."
				});
				part.Texts.Add(new TextPart
				{
					Text = "Use 1500 points to get a free DVD rental."
				});
				part.Texts.Add(new TextPart
				{
					Text = "Use 1750 points to get a free Blu-ray rental."
				});
				part.Texts.Add(new TextPart
				{
					Text = "Use 2000 points to get a free 4K UHD rental."
				});
				part.Texts.Add(new TextPart
				{
					Text = "Use 2500 points to get a free Game rental."
				});
			};
			obj.SpeechParts.Add(part);
			int i = 0;
			obj.SpeechParts.ForEach(delegate(ISpeechPart x)
			{
				x.Sequence = i++;
			});
			return obj;
		};
	}
}
