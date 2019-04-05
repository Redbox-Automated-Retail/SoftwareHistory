using System.Windows;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class IdleTimeoutMessageViewController : BaseViewController
{
	public static void ConfigureView(IViewFrameInstance viewFrameInstance)
	{
		OrchestrationService.SuspendIdleTimer("idle_timer");
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			IdleTimeoutMessageViewModel idleTimeoutMessageViewModel = new IdleTimeoutMessageViewModel();
			viewFrameInstance.Parameter = idleTimeoutMessageViewModel;
			OrchestrationService.SetCurrentUICulture();
			idleTimeoutMessageViewModel.ButtonNoText = Resources.no_thanks_button;
			idleTimeoutMessageViewModel.ButtonYesText = Resources.yes_button;
			idleTimeoutMessageViewModel.MessageText = Resources.more_time_prompt_message;
			idleTimeoutMessageViewModel.OnButtonNoClicked = delegate
			{
				OrchestrationService.InvokeIdleTimerExpireSessionCallback();
			};
			idleTimeoutMessageViewModel.OnButtonYesClicked = delegate
			{
				ServiceLocator.Instance.GetService<IViewService>();
				OrchestrationService.ViewGoBackSafe("need_more_time:second_button", popOnly: false);
			};
			idleTimeoutMessageViewModel.OnGetSpeechControl = () => GetSpeechControl(idleTimeoutMessageViewModel);
			ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			int num = 15000;
			num = service?.GetTimeout("medium", num) ?? num;
			idleTimeoutMessageViewModel.Timeout = num;
			viewUserControl.DataContext = idleTimeoutMessageViewModel;
			CommandManager.InvalidateRequerySuggested();
		}
	}

	private static ISpeechControl GetSpeechControl(IdleTimeoutMessageViewModel idleTimeoutMessageViewModel)
	{
		SpeechControl obj = new SpeechControl
		{
			Name = "tts_idle_timeout_message_view"
		};
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 0,
			Name = "Main",
			StartPause = 250,
			EndPause = 5000,
			Loop = true
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Texts.Add(new TextPart
			{
				Text = idleTimeoutMessageViewModel.MessageText
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = $"Press cancel for {idleTimeoutMessageViewModel.ButtonNoText}"
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = $"Press enter for {idleTimeoutMessageViewModel.ButtonYesText}"
			});
			speechPart.Events.Add(new MapKeyPress
			{
				KeyCode = "TAB",
				Function = "TTSNextAudioSnippet"
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "CANCEL",
				Action = delegate
				{
					idleTimeoutMessageViewModel.OnButtonNoClicked();
				}
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "ENTER",
				Action = delegate
				{
					idleTimeoutMessageViewModel.OnButtonYesClicked();
				}
			});
		};
		((ISpeechControl)obj).SpeechParts.Add(speechPart);
		return obj;
	}

	public static void CleanupView(IViewFrameInstance viewFrameInstance)
	{
		if (viewFrameInstance.Parameter is IdleTimeoutMessageViewModel idleTimeoutMessageViewModel)
		{
			idleTimeoutMessageViewModel.OnButtonNoClicked = null;
		}
		if (BaseViewController.GetViewUserControl() is IdleTimeoutMessageViewUserControl idleTimeoutMessageViewUserControl)
		{
			idleTimeoutMessageViewUserControl.DataContext = null;
		}
		OrchestrationService.ResumeIdleTimer("idle_timer");
	}
}
