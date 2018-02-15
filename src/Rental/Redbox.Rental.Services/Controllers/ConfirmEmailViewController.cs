using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class ConfirmEmailViewController
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

	private static Action Callback { get; set; }

	private static IIdleTimer IdleTimer { get; set; }

	private static Item_Type ItemType { get; set; }

	private static string Email { get; set; }

	public ConfirmEmailViewController()
	{
		OrchestrationService.SetCurrentUICulture();
	}

	public static void ConfigureView(bool confirm, string email, Action callback)
	{
		ItemType = (confirm ? Item_Type.MarketingConfirm : Item_Type.MarketingSubscribe);
		Email = email;
		Callback = callback;
		FrameworkElement userControl = GetUserControl();
		if (userControl != null)
		{
			userControl.DataContext = GetModel();
		}
		CommandManager.InvalidateRequerySuggested();
		ConfigureTTS();
		ConfigureIdleTimer();
	}

	private static void ConfigureIdleTimer()
	{
		IdleTimer = ServiceLocator.Instance.GetService<IIdleTimerService>().CreateIdleTimer("confirm_email_timer");
		int timeout = OrchestrationService.TTSGetTimeout("Medium", 20000);
		IdleTimer.Initialize(timeout, OnTimeout);
		IdleTimer.Start();
	}

	private static FrameworkElement GetUserControl()
	{
		return ActorFunctions.GetActor("ConfirmEmailViewUserControl")?.WPFFrameworkElement;
	}

	private static ConfirmEmailModel GetModel()
	{
		ConfirmEmailModel confirmEmailModel = new ConfirmEmailModel();
		confirmEmailModel.HeaderText = Resources.confirm_email_header;
		confirmEmailModel.BodyText = Resources.confirm_email_body;
		confirmEmailModel.Button1Visibility = Visibility.Visible;
		confirmEmailModel.Button2Visibility = Visibility.Visible;
		confirmEmailModel.Button1Action += OnHit;
		confirmEmailModel.Button2Action += OnCancel;
		return confirmEmailModel;
	}

	private static void ConfigureTTS()
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement userControl = GetUserControl();
		ConfirmEmailModel confirmEmailModel = userControl.DataContext as ConfirmEmailModel;
		if (service != null && service.TTSEnabled && userControl != null)
		{
			confirmEmailModel.OnGetSpeechControl = () => ConfirmEmailViewTextToSpeech.GetSpeechControl(GetModelFromView);
		}
	}

	private static ConfirmEmailModel GetModelFromView()
	{
		return GetUserControl()?.DataContext as ConfirmEmailModel;
	}

	public static void OnHit()
	{
		SendKioskOptin(1L, ItemType);
		IdleTimer?.Stop();
		RemoveTimer();
		OrchestrationService.ViewShow("optin_thank_you_view");
	}

	public static void OnCancel()
	{
		SendKioskOptin(0L, ItemType);
		IdleTimer?.Stop();
		RemoveTimer();
		Callback?.Invoke();
	}

	public static void OnTimeout()
	{
		SendKioskOptin(2L, ItemType);
		IdleTimer?.Stop();
		RemoveTimer();
		Callback?.Invoke();
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
		service?.KioskOptIn(optInItems: obj, storeNumber: service2?.KioskId.ToString() ?? "Unknown", sendCardHash: false, callback: null);
	}
}
