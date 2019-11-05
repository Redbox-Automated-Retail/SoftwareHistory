using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskEngine.Environment;
using Redbox.REDS.Framework;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Services;

public class RedboxRentalService : IRedboxRental
{
	public class Constants
	{
		public class Timers
		{
			public const string ReservationExpireTimer = "reservation-expire-timer";
		}

		public const string DisconnectedCardReaderTimer = "disconnected_usb_card_reader_timer";
	}

	private IViewService ViewService => ServiceLocator.Instance.GetService<IViewService>();

	private bool IsRentalApplicationRunning => ServiceLocator.Instance.GetService<IResourceBundleService>()?.IsRentalRunning ?? false;

	public Redbox.KioskEngine.ComponentModel.ErrorList Initialize()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		try
		{
			InitializeDesktop();
		}
		catch (Exception ex)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("EngineApplication.InitializeRedboxRental", ex.Message, ""));
		}
		return errorList;
	}

	public bool IsRentalSessionActive()
	{
		return OrchestrationService.IsRentalSessionActive();
	}

	public void StartTheRentalApplication()
	{
		IApplicationState service = ServiceLocator.Instance.GetService<IApplicationState>();
		if (service == null || !service.IsInMaintenanceMode)
		{
			OrchestrationService.SendHealthServicesMinutesInMaintenanceModeStatistics(0);
		}
		CheckForTamperedCardReader();
	}

	public void CloseOpenSessionAndStartNewRentalSession(string newSessionContext)
	{
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null)
		{
			OrchestrationService.CloseShoppingCartAnalytics(rentalShoppingCart.ShoppingSessionId, isSessionAbandoned: true);
			rentalShoppingCart.Abandon("User chose to abandon session.");
			ApplicationState.Instance.IsADAAccessible = false;
		}
		ICardReadAttemptCollection cardReadAttemptCollection = Redbox.Rental.Services.Session.Session.Instance?.CardReadAttemptCollection ?? null;
		if (cardReadAttemptCollection != null && cardReadAttemptCollection.SupportsTechnicalFallbackTracking)
		{
			ServiceLocator.Instance.GetService<IEMVReaderHealth>()?.SessionWasInTechnicalFallback(cardReadAttemptCollection.IsInTrackableTechnicalFallback);
		}
		Redbox.Rental.Services.Session.Session.Instance?.ClearTrackData();
		StartNewRentalSession(newSessionContext);
	}

	private void StartNewRentalSession(string context)
	{
		IViewService viewService = ViewService;
		if (viewService == null || !viewService.IsSafeToChangeView())
		{
			LogHelper.Instance.Log($"Unable to start new session. view cannot be changed. Current view: {ViewService?.Peek()}");
			return;
		}
		bool flag = true;
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		if (service != null && !service.HasValidCardReader)
		{
			Redbox.Rental.Services.Session.Session.Instance = new Redbox.Rental.Services.Session.Session();
			if (CanEnterMaintenanceMode())
			{
				flag = false;
				EnterMaintenanceModeBecauseOfDisconnectedCardReader();
			}
		}
		if (flag)
		{
			LogHelper.Instance.Log($"Starting new session. context: {context}");
			while ((ViewService?.ViewStackCount ?? 0) > 0)
			{
				ViewService.Pop();
			}
			OrchestrationService.StartTimer("reservation-expire-timer");
			OrchestrationService.StopIdleTimer("idle_timer");
			Redbox.Rental.Services.Configuration.Configuration.Instance.SessionReload();
			Redbox.Rental.Services.Session.Session.Instance = new Redbox.Rental.Services.Session.Session();
			OrchestrationService.ResetApplicationIdleTimerTimeout();
			ITextToSpeechService service2 = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			if ((service2 != null && service2.TTSEnabled) || ApplicationState.Instance.IsADAAccessible)
			{
				ApplicationState.Instance.IsADAAccessible = true;
			}
			if (ViewService != null)
			{
				ViewService.LastHitActorName = null;
			}
			IBrowseState browseState = Redbox.Rental.Services.Session.Session.Instance?.BrowseState;
			if (browseState != null)
			{
				browseState.BrowseMode = BrowseViewFilter.All;
				browseState.Genre = null;
				browseState.IsShoppingCartFullMessagesOpen = false;
				browseState.ShoppingCartFullMessagesShown = false;
				browseState.TitleFamily = TitleFamily.Movies;
			}
			OrchestrationService.InvokeStartNewRentalSessionCallback();
			OrchestrationService.ViewShow(ViewService.GetStartOverViewName());
		}
	}

	private void InitializeDesktop()
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string defaultCulture = ServiceLocator.Instance.GetService<IStoreManager>().GetDefaultCulture();
		if (!string.IsNullOrEmpty(defaultCulture))
		{
			service["DefaultCulture"] = defaultCulture;
		}
		ITextToSpeechService service2 = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service2 != null)
		{
			service2.ClearOnAudioDeviceConnectionChanged();
			service2.OnAudioDeviceConnectionChanged += HandleTextToSpeechAudioDeviceConnectionChanged;
			RegisterTTSWithRentalShoppingCartService();
		}
		IThemeService service3 = ServiceLocator.Instance.GetService<IThemeService>();
		if (service3 != null)
		{
			service3.RegisterThemedControls("Redbox.Rental.UI.exe");
			service3.RegisterThemedControls("Redbox.ControlPanel.UI.exe");
			ITheme theme = service3.Themes.Find((ITheme x) => x.Name == "Redbox Classic");
			if (theme != null)
			{
				theme.IsDefault = true;
				LogHelper.Instance.Log($"Set default theme to {theme.Name}", LogEntryType.Info);
				service3.CurrentTheme = theme;
				LogHelper.Instance.Log($"Set current theme to {theme.Name}", LogEntryType.Info);
			}
			else
			{
				LogHelper.Instance.Log("Unable to set default theme to Redbox Classic", LogEntryType.Info);
			}
		}
		RegisterEnvironmentServiceCallbacks();
		RegisterApplicationStateCallbacks();
		RegisterDeviceServiceCanShutDownHandler();
	}

	private void CheckForTamperedCardReader()
	{
		IDeviceServiceClientHelper service = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
		if (service != null && service.IsCardReaderTampered)
		{
			ApplicationState.Instance.AddMaintenanceModeReason(MaintenanceModeSource.CardReader, "0099CDRR");
		}
	}

	public void RegisterEnvironmentChangeCallback()
	{
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RegisterConfigurationCallback("watch-registry", delegate(string store, string action, string path, string key, string newValue)
		{
			if (store == "machine" && action == "set" && path == "Remote Services\\Merchant Services" && key == "EnableOfflineBadCardList")
			{
				bool result = true;
				if (bool.TryParse(newValue, out result))
				{
					LogHelper.Instance.Log($"Configuration value changed for EnableOfflineBadCardList: {result}");
					ApplicationState.Instance.EnableOfflineBadCardList = result;
				}
			}
		});
	}

	private void RegisterDeviceServiceCanShutDownHandler()
	{
		ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>()?.RegisterOnDeviceServiceCanShutDownHandler(delegate
		{
			bool flag = true;
			bool flag2 = Redbox.Rental.Services.Session.Session.Instance?.HasReceivedUserInteraction ?? false;
			if (IsRentalApplicationRunning & flag2)
			{
				flag = false;
			}
			LogHelper.Instance.Log($"Device Service can shut down check: {flag}   Session start trigger action: {Redbox.Rental.Services.Session.Session.Instance?.SessionStartTriggerAction}");
			return flag;
		});
	}

	private void RegisterApplicationStateCallbacks()
	{
		ApplicationState.Instance.OnAddMaintenanceModeReasonHandler += HandleOnAddMaintenanceModeReason;
		ApplicationState.Instance.OnRemoveMaintenanceModeReasonHandler += HandleOnExitMaintenanceMode;
	}

	private void HandleOnExitMaintenanceMode(IMaintenanceModeReason removedMaintenanceModeReason, List<IMaintenanceModeReason> remainingMaintenanceModeReasons)
	{
		if (remainingMaintenanceModeReasons == null || remainingMaintenanceModeReasons.Count == 0)
		{
			if (!IsRentalApplicationRunning)
			{
				return;
			}
			IViewService viewService = ViewService;
			if (viewService != null && viewService.IsMaintenanceViewShowing())
			{
				IDeviceServiceClientHelper service = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
				if (service == null || !service.IsCardReaderTampered)
				{
					ViewService?.UnwindTo(ViewService.GetStartOverViewName(), discard: true, "clear maintenance view");
					ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession($"Exit Maintenance Mode Code {removedMaintenanceModeReason?.Code}");
				}
			}
		}
		else
		{
			_ = ViewService?.IsMaintenanceViewShowing() ?? false;
		}
	}

	private void HandleOnAddMaintenanceModeReason(IMaintenanceModeReason maintenanceModeReason)
	{
		if (IsRentalApplicationRunning)
		{
			IViewService viewService = ViewService;
			if (viewService != null && !viewService.IsMaintenanceViewShowing())
			{
				if (ViewService?.UITaskScheduler != null)
				{
					Task.Factory.StartNew(delegate
					{
						OrchestrationService.ViewShow(ViewService.GetMaintenanceViewName(), maintenanceModeReason?.Code);
					}, CancellationToken.None, TaskCreationOptions.None, ViewService.UITaskScheduler);
				}
			}
			else
			{
				LogHelper.Instance.Log("Not showing maintenance view because it is already showing.");
			}
		}
		else
		{
			IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
			IManifestInfo manifestInfo = null;
			service?.GetManifest(out manifestInfo);
			LogHelper.Instance.Log($"Not showing maintenance view because the rental application is not running.  Current application: {manifestInfo?.ProductName}");
		}
	}

	private void RegisterEnvironmentServiceCallbacks()
	{
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		if (service != null)
		{
			service.RegisterCardReaderConnectedCallback(HandleCardReaderConnected);
			service.RegisterCardReaderDisconnectedCallback(HandleCardReaderDisconnected);
			service.RegisterCorruptDbCallback(HandleCorruptDb);
			service.RegisterTamperedCardReaderCallback(HandleTamperedCardReader);
		}
	}

	private void HandleTamperedCardReader()
	{
		LogHelper.Instance.Log("Tampered Card Reader detected, processing.");
		ApplicationState.Instance.AddMaintenanceModeReason(MaintenanceModeSource.CardReader, "0099CDRR");
	}

	private void HandleCorruptDb()
	{
		LogHelper.Instance.Log("Corrupt Db detected, processing.");
		ApplicationState.Instance.AddMaintenanceModeReason(MaintenanceModeSource.Database, "SOFT-FILE01");
	}

	private void HandleCardReaderDisconnected(ITrackDataService previousTrackDataService)
	{
		string arg = null;
		if (previousTrackDataService is IDeviceServiceTrackDataService)
		{
			arg = "The Device Service card reader was disconnected.";
		}
		else if (previousTrackDataService is IClassicTrackDataService)
		{
			arg = "The USB card reader was disconnected.";
		}
		if (CanEnterPleaseWaitViewBecauseOfDisconnectedCardReader())
		{
			CheckoutFunctions.ShowPleaseWaitBecauseOfDisconnectedCardReader();
		}
		else if (CanEnterMaintenanceMode())
		{
			LogHelper.Instance.Log($"{arg} Attempting to enter maintenance mode.");
			EnterMaintenanceModeBecauseOfDisconnectedCardReader();
		}
		else
		{
			LogHelper.Instance.Log($"{arg} Unable to enter maintenance mode.");
		}
	}

	public void EnterMaintenanceModeBecauseOfDisconnectedCardReader()
	{
		AddCardReaderMaintenanceModeReason("0072CDRR");
	}

	public void EnterMaintenanceModeBecauseOfTamperedCardReader()
	{
		IDeviceServiceClientHelper service = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
		if (service != null)
		{
			service.IsCardReaderTampered = true;
			service.SendTamperedCardReaderKioskAlertMessage();
		}
		AddCardReaderMaintenanceModeReason("0099CDRR");
	}

	public void HandleUnremovedChipInCardReader()
	{
		ServiceLocator.Instance.GetService<IRedboxRental>()?.CloseOpenSessionAndStartNewRentalSession("Chip not removed from Card Reader");
		string arg = DateTime.Now.ToString("yyyyMMddHHmm");
		Store obj = ServiceLocator.Instance.GetService<IDataService>()?.GetStore();
		ServiceLocator.Instance.GetService<IHardwareAlertManagerService>()?.SendAlert("HardwareError", $"KioskID: {obj?.StoreId} Credit Card Reader Error &#150; Card Not Removed from Credit Card Reader", $"{arg} - Credit Card Reader Card Not Removed.", null, null, string.Format("CardNotRemovedFromReader, {0}", "0100CDRR"));
		LogHelper.Instance.LogTo("CardReaderLog", "Card Reader chip card not removed , Alert Sent");
	}

	private ITimer CreateDisconnectedCardReaderTimer()
	{
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		int value = ((service != null && service.PreviousTrackDataServiceWasDeviceServiceTrackDataService) ? 90000 : 10000);
		ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
		ITimer timer = timerService?.GetTimer("disconnected_usb_card_reader_timer");
		if (timer == null)
		{
			timer = timerService?.CreateTimer("disconnected_usb_card_reader_timer", value, value, delegate
			{
				(timerService?.GetTimer("disconnected_usb_card_reader_timer"))?.Stop();
				LogHelper.Instance.Log(string.Format("> {0} executing", "disconnected_usb_card_reader_timer"));
				List<IMaintenanceModeReason> obj = ServiceLocator.Instance.GetService<IApplicationState>()?.GetMaintenanceModeReasons();
				if (obj == null || obj.Any((IMaintenanceModeReason x) => x.Source == MaintenanceModeSource.CardReader))
				{
					string arg = DateTime.Now.ToString("yyyyMMddHHmm");
					Store obj2 = ServiceLocator.Instance.GetService<IDataService>()?.GetStore();
					ServiceLocator.Instance.GetService<IHardwareAlertManagerService>()?.SendAlert("HardwareError", $"KioskID: {obj2?.StoreId} Credit Card Reader Communications Error &#150; Machine Down", $"{arg} - Machine in maintenance mode.", null, null, string.Format("mm:USBCardReaderOffline, {0}", "0072CDRR"));
					LogHelper.Instance.LogTo("CardReaderLog", "Card Reader Disconnected, Alert Sent");
				}
				else
				{
					LogHelper.Instance.LogTo("CardReaderLog", "Card Reader Disconnected, Replug Event Occured Within Threshold, Alert Not Sent");
				}
			});
		}
		return timer;
	}

	private void HandleCardReaderConnected()
	{
		if (ViewService?.UITaskScheduler == null)
		{
			return;
		}
		Task.Factory.StartNew(delegate
		{
			if (IsRentalApplicationRunning)
			{
				ISession instance = Redbox.Rental.Services.Session.Session.Instance;
				if (instance != null && instance.WaitingForCardReaderConnection)
				{
					LogHelper.Instance.Log(string.Format("Done waiting for card reader connection.  Popping {0}", "please_wait_view"));
					CheckoutFunctions.PopPleaseWaitViewBecauseCardReaderReconnected();
					return;
				}
			}
			ApplicationState.Instance.RemoveMaintenanceModeReason(MaintenanceModeSource.CardReader, "0072CDRR");
		}, CancellationToken.None, TaskCreationOptions.None, ViewService.UITaskScheduler).Wait();
	}

	private bool CanEnterPleaseWaitViewBecauseOfDisconnectedCardReader()
	{
		bool result = false;
		if (IsRentalApplicationRunning && ViewService.Peek() == "swipe_view")
		{
			result = true;
		}
		return result;
	}

	private bool CanEnterMaintenanceMode()
	{
		bool flag = true;
		string text = "No reason not to enter maintenance mode";
		if (IsRentalApplicationRunning)
		{
			IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
			if (service != null && !service.IsTrackDataServiceRegistered)
			{
				bool flag2 = ViewService?.Peek() == "swipe_view";
				if (service.PreviousTrackDataServiceWasDeviceServiceTrackDataService)
				{
					IDeviceServiceClientHelper service2 = ServiceLocator.Instance.GetService<IDeviceServiceClientHelper>();
					if (service2?.DeviceServiceShutDownInfo != null)
					{
						text = $"Device Service was shut down for reason {service2?.DeviceServiceShutDownInfo?.ShutDownReason}";
						if ((service2?.DeviceServiceShutDownInfo?.ShutDownReason ?? ShutDownReason.None) == ShutDownReason.SoftwareUpdate && !flag2)
						{
							flag = false;
							text += string.Format(" and current view is not {0}", "swipe_view");
						}
					}
					else
					{
						text = "Device Service card reader not connected";
						if (!flag2)
						{
							flag = false;
							text += string.Format(" and current view is not {0}", "swipe_view");
						}
					}
				}
				else if (service.PreviousTrackDataServiceWasClassicTrackDataService)
				{
					bool flag3 = Redbox.Rental.Services.Session.Session.Instance?.LastCardReadAttemptSuccess ?? false;
					flag = !flag3;
					text = $"ClassicTrackDataService last card read attempt success = {flag3}";
				}
			}
		}
		else
		{
			flag = false;
			IManifestInfo manifestInfo = null;
			ServiceLocator.Instance.GetService<IResourceBundleService>()?.GetManifest(out manifestInfo);
			text = string.Format("{0} is not running.  Current application: {1}", "Rental Application", manifestInfo?.ProductName);
		}
		LogHelper.Instance.Log($"CanEnterMaintenanceMode = {flag}   Reason: {text}");
		return flag;
	}

	private bool CanForceStartOfNewRentalSession()
	{
		bool result = true;
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null)
		{
			result = service.Peek() != service.GetMaintenanceViewName() && service.Peek() != "maintenance_sync_view" && !service.Stack.Any((IViewFrameInstance x) => x?.ViewFrame?.ViewName == "swipe_view") && !(Redbox.Rental.Services.Session.Session.Instance?.BrowseState?.InReturnFlow ?? false);
		}
		return result;
	}

	private void HandleTextToSpeechAudioDeviceConnectionChanged(ITextToSpeechService textToSpeachService, bool isConnected)
	{
		if (Redbox.Rental.Services.Session.Session.Instance != null && Redbox.Rental.Services.Session.Session.Instance.BrowseState != null)
		{
			ApplicationState.Instance.IsADAAccessible = isConnected;
		}
		if (isConnected)
		{
			ResourceBundleService.Instance.GetManifest(out var manifestInfo);
			_ = ResourceBundleService.Instance.Filter["view_name"];
			if (manifestInfo?.ProductName != "Rental Application")
			{
				LogHelper.Instance.Log($"TTS((> TextToSpeechService.AudioDeviceConnected Not in Rental Application. Currently {manifestInfo?.ProductName}");
				return;
			}
			ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
			IShoppingSessionService service2 = ServiceLocator.Instance.GetService<IShoppingSessionService>();
			if (!CanForceStartOfNewRentalSession())
			{
				return;
			}
			LogHelper.Instance.Log("TTS((> Restarting session due to headphones being connected.");
			if (service2 != null)
			{
				IShoppingSession currentSession = service2.GetCurrentSession();
				if (service != null)
				{
					service.AddNode("viewEvents", "viewEvent", new Dictionary<string, string>
					{
						{ "subItemType", "Timeout" },
						{ "subItemValue", "HeadphonesConnected" }
					}, currentSession.Id.ToString());
					service.AddNode("session", "pricing");
				}
			}
			CloseOpenSessionAndStartNewRentalSession("TTS Audio Device connected");
		}
		else if (CanForceStartOfNewRentalSession())
		{
			CloseOpenSessionAndStartNewRentalSession("TTS Audio Device disconnected");
		}
	}

	private void RegisterTTSWithRentalShoppingCartService()
	{
		ServiceLocator.Instance.GetService<IRentalShoppingCartService>().OnRentalShoppingCartAdded += delegate(IRentalShoppingCart rentalShoppingCart)
		{
			if (rentalShoppingCart != null)
			{
				rentalShoppingCart.OnChange += RespondToRentalShoppingCartChange;
				RespondToRentalShoppingCartChange(rentalShoppingCart);
			}
		};
	}

	private void RespondToRentalShoppingCartChange(IRentalShoppingCart rentalShoppingCart)
	{
		IDataService dataService = ServiceLocator.Instance.GetService(typeof(IDataService)) as IDataService;
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
		if (!service.TTSEnabled)
		{
			return;
		}
		IList<IRentalShoppingCartItem> list = rentalShoppingCart.Items;
		if (list == null)
		{
			list = new List<IRentalShoppingCartItem>();
		}
		for (int i = 1; i <= rentalShoppingCart.MaximumCapacity; i++)
		{
			string text = null;
			IRentalShoppingCartItem rentalShoppingCartItem = ((i <= list.Count) ? list[i - 1] : null);
			if (rentalShoppingCartItem != null)
			{
				dataService.GetProduct(rentalShoppingCartItem.ProductId, excludeTitleRollup: true, out var kioskProduct);
				if (kioskProduct != null && kioskProduct is ITitleProduct)
				{
					text = ((ITitleProduct)kioskProduct).LongName;
				}
			}
			string macroName = $"TTS:CartTitle_{i}_Name";
			string macroValue = text;
			SetAndLogMacroValue(service2, macroName, macroValue);
			macroName = $"TTS:CartTitle_{i}_Action";
			macroValue = ((rentalShoppingCartItem != null) ? rentalShoppingCartItem.Action.ToString() : string.Empty);
			SetAndLogMacroValue(service2, macroName, macroValue);
		}
		service2["TTS:NumberOfTitlesInCart"] = ((list.Count > 0) ? list.Count.ToString() : string.Empty);
		service2["TTS:CartTitlePlurality"] = ((list.Count == 1) ? "Title" : "Titles");
		service.TTSRepeatSequence();
	}

	private void SetAndLogMacroValue(IMacroService macroService, string macroName, string macroValue)
	{
		LogHelper.Instance.Log(string.Format("{0} = {1}", macroName, string.IsNullOrEmpty(macroValue) ? "empty" : macroValue));
		macroService[macroName] = macroValue;
	}

	private void AddCardReaderMaintenanceModeReason(string reason)
	{
		ApplicationState.Instance.AddMaintenanceModeReason(MaintenanceModeSource.CardReader, reason);
		(ServiceLocator.Instance.GetService<IIdleTimerService>()?.GetIdleTimer("idle_timer"))?.Suspend();
		CreateDisconnectedCardReaderTimer()?.Start();
	}
}
