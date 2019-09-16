using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class ConfirmEmailViewController : BaseViewController
{
	private enum ItemId
	{
		No,
		Yes,
		Timeout
	}

	private class OptinItem : IOptInItem
	{
		public long ItemId { get; set; }

		public Item_Type Item_Type { get; set; }

		public OptInMethodType OptInMethodType { get; set; }

		public string OptInMethodData { get; set; }

		public string TemplateName { get; set; }

		public string CampaignName { get; set; }
	}

	private static object _lockObject = new object();

	private static Action Callback { get; set; }

	private static IIdleTimer IdleTimer { get; set; }

	private static Item_Type ItemType { get; set; }

	private static string Email { get; set; }

	public ConfirmEmailViewController()
	{
		OrchestrationService.SetCurrentUICulture();
	}

	public static void ConfigureView()
	{
		ISession instance = Redbox.Rental.Services.Session.Session.Instance;
		ItemType = ((instance != null && instance.BrowseState.NeedMarketingConfirm) ? Item_Type.MarketingConfirm : Item_Type.MarketingSubscribe);
		Email = Redbox.Rental.Services.Session.Session.Instance?.UserInfo?.Email;
		Callback = delegate
		{
			OrchestrationService.InvokeContinueVendCallback();
		};
		ConfirmEmailModel confirmEmailModel = CreateConfirmEmailModel();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			viewUserControl.DataContext = confirmEmailModel;
		}
		CommandManager.InvalidateRequerySuggested();
		ConfigureTTS(confirmEmailModel);
		ConfigureIdleTimer();
	}

	private static void ConfigureIdleTimer()
	{
		IdleTimer = ServiceLocator.Instance.GetService<IIdleTimerService>()?.CreateIdleTimer("confirm_email_timer");
		int timeout = OrchestrationService.TTSGetTimeout("Medium", 20000);
		IdleTimer.Initialize(timeout, OnTimeout);
		IdleTimer.Start();
	}

	private static ConfirmEmailModel CreateConfirmEmailModel()
	{
		ConfirmEmailModel confirmEmailModel = new ConfirmEmailModel();
		confirmEmailModel.HeaderText = Resources.confirm_email_header;
		confirmEmailModel.BodyText = Resources.confirm_email_body;
		confirmEmailModel.Button1Visibility = Visibility.Visible;
		confirmEmailModel.Button2Visibility = Visibility.Visible;
		confirmEmailModel.Button1Action += OnYesButtonPress;
		confirmEmailModel.Button2Action += OnCancelButtonPress;
		return confirmEmailModel;
	}

	private static void ConfigureTTS(ConfirmEmailModel model)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled && model != null)
		{
			model.OnGetSpeechControl = () => ConfirmEmailViewTextToSpeech.GetSpeechControl(GetModelFromView);
		}
	}

	private static ConfirmEmailModel GetModelFromView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl?.DataContext as ConfirmEmailModel;
		}
		return null;
	}

	public static void OnYesButtonPress()
	{
		ConfirmEmailModel modelFromView = GetModelFromView();
		lock (_lockObject)
		{
			if (modelFromView != null && !modelFromView.HasActionExecuted)
			{
				modelFromView.HasActionExecuted = true;
				SendKioskOptin(1L, ItemType);
				IdleTimer?.Stop();
				RemoveTimer();
				OrchestrationService.ViewShow("optin_thank_you_view");
			}
			else
			{
				LogHelper.Instance.Log("ConfirmEmailView: Yes button not pressed because it was disabled.");
			}
		}
	}

	public static void OnCancelButtonPress()
	{
		ConfirmEmailModel modelFromView = GetModelFromView();
		lock (_lockObject)
		{
			if (modelFromView != null && !modelFromView.HasActionExecuted)
			{
				modelFromView.HasActionExecuted = true;
				SendKioskOptin(0L, ItemType);
				IdleTimer?.Stop();
				RemoveTimer();
				Callback?.Invoke();
			}
			else
			{
				LogHelper.Instance.Log("ConfirmEmailView: No button not pressed because it was disabled.");
			}
		}
	}

	public static void OnTimeout()
	{
		ConfirmEmailModel modelFromView = GetModelFromView();
		lock (_lockObject)
		{
			if (modelFromView != null && !modelFromView.HasActionExecuted)
			{
				modelFromView.HasActionExecuted = true;
				SendKioskOptin(2L, ItemType);
				IdleTimer?.Stop();
				RemoveTimer();
				Callback?.Invoke();
			}
			else
			{
				LogHelper.Instance.Log("ConfirmEmailView: Timeout was not executed because it was disabled.");
			}
		}
	}

	private static void RemoveTimer()
	{
		ServiceLocator.Instance.GetService<IIdleTimerService>().RemoveIdleTimer("confirm_email_timer");
	}

	private static void SendKioskOptin(long itemId, Item_Type itemType)
	{
		IPromotionServices service = ServiceLocator.Instance.GetService<IPromotionServices>();
		IStoreManager service2 = ServiceLocator.Instance.GetService<IStoreManager>();
		ServiceLocator.Instance.GetService<IShoppingCart>();
		List<IOptInItem> obj = new List<IOptInItem>
		{
			new OptinItem
			{
				ItemId = itemId,
				Item_Type = itemType,
				OptInMethodType = OptInMethodType.EmailAddress,
				OptInMethodData = Email,
				TemplateName = "MarketingOptInTemplate",
				CampaignName = "MarketingOptInCampaign"
			}
		};
		service?.KioskOptIn(optInItems: obj, storeNumber: service2?.KioskId.ToString() ?? "Unknown", callback: null);
	}
}
