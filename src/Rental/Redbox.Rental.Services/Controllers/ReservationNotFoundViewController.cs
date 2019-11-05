using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class ReservationNotFoundViewController : BaseViewController
{
	private static object _lockObject = new object();

	public static void ConfigureView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.StopIdleTimer("idle_timer");
			ReservationNotFoundViewModel reservationNotFoundViewModel = new ReservationNotFoundViewModel();
			ConfigureTextAndVisibility(reservationNotFoundViewModel);
			ConfigureCommands(reservationNotFoundViewModel);
			ConfigureTimeoutTimer(reservationNotFoundViewModel);
			ConfigureTTS(reservationNotFoundViewModel);
			viewUserControl.DataContext = reservationNotFoundViewModel;
		}
	}

	private static void ConfigureTTS(ReservationNotFoundViewModel viewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service == null || !service.TTSEnabled)
		{
			return;
		}
		viewModel.OnGetSpeechControl = delegate
		{
			ISpeechControl result = new SpeechControl();
			result.Name = string.Format("tts_{0}", "reservation_not_found_view");
			ISpeechPart speechPart = new SpeechPart
			{
				Name = "Main",
				Sequence = 0,
				KeySequenceDelay = 0,
				Loop = false,
				StartPause = 250,
				EndPause = 3000
			};
			speechPart.Texts.Add(new TextPart
			{
				Text = "Your reservation is waiting. Let us find it for you!"
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "This box doesn't have a reservation for you."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "That might be because: "
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "You may have reserved at a different location nearby."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "You may have used a different card to reserve than you're using now."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "To go back, press the Minus key."
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "MINUS",
				Action = delegate
				{
					viewModel?.ProcessOkButtonClick();
				}
			});
			result.SpeechParts.Add(speechPart);
			result.SpeechParts.ForEach(delegate(ISpeechPart x)
			{
				x.SpeechControl = result;
			});
			return result;
		};
	}

	private static void ConfigureTimeoutTimer(ReservationNotFoundViewModel reservationNotFoundViewModel)
	{
		if (OrchestrationService.GetIdleTimer("reservation_not_found_timer") == null)
		{
			IIdleTimer idleTimer = OrchestrationService.CreateIdleTimer("reservation_not_found_timer");
			idleTimer.Initialize(40000, CloseSession);
			idleTimer.Start();
		}
	}

	private static void ConfigureCommands(ReservationNotFoundViewModel reservationNotFoundViewModel)
	{
		if (reservationNotFoundViewModel != null)
		{
			reservationNotFoundViewModel.OnOkButtonClick = CloseSession;
		}
	}

	private static void CloseSession()
	{
		lock (_lockObject)
		{
			ReservationNotFoundViewModel viewModel = BaseViewController.GetViewModel<ReservationNotFoundViewModel>();
			if (viewModel != null && !viewModel.ActionExecuted)
			{
				viewModel.ActionExecuted = true;
				if (ServiceLocator.Instance.GetService<IViewService>()?.Peek() == "reservation_not_found_view")
				{
					ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession(null);
				}
			}
		}
	}

	private static void ConfigureTextAndVisibility(ReservationNotFoundViewModel reservationNotFoundViewModel)
	{
		OrchestrationService.SetCurrentUICulture();
		if (reservationNotFoundViewModel != null)
		{
			reservationNotFoundViewModel.TitleText = Resources.reservation_not_found_view_title_text;
			reservationNotFoundViewModel.MessageText = Resources.reservation_not_found_view_message_text;
			reservationNotFoundViewModel.OkButtonText = Resources.okay_button;
			reservationNotFoundViewModel.ImageName = Resources.reservation_not_found_view_image;
		}
	}

	public static void LeaveView()
	{
		OrchestrationService.StopIdleTimer("reservation_not_found_timer");
		OrchestrationService.RemoveIdleTimer("reservation_not_found_timer");
	}
}
