using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
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
	}

	public ErrorList Initialize()
	{
		ErrorList errorList = new ErrorList();
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

	public void CloseOpenSessionAndStartNewRentalSession(string newSessionContext)
	{
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null)
		{
			OrchestrationService.CloseShoppingCartAnalytics(rentalShoppingCart.ShoppingSessionId, true.ToString());
			rentalShoppingCart.Abandon("User chose to abandon session.");
			ApplicationState.Instance.IsADAAccessible = false;
		}
		StartNewRentalSession(newSessionContext);
	}

	private void StartNewRentalSession(string context)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service == null || !service.IsSafeToChangeView())
		{
			LogHelper.Instance.Log($"Unable to start new session. view cannot be changed. Current view: {service?.Peek()}");
			return;
		}
		LogHelper.Instance.Log($"Starting new session. context: {context}");
		while ((service?.ViewStackCount ?? 0) > 0)
		{
			service.Pop();
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
		if (service != null)
		{
			service.LastHitActorName = null;
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
		OrchestrationService.ViewShow(service.GetStartOverViewName());
	}

	private void InitializeDesktop()
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string defaultCulture = ServiceLocator.Instance.GetService<IStoreManager>().GetDefaultCulture();
		if (!string.IsNullOrEmpty(defaultCulture))
		{
			service[ConfigSetting.DefaultCulture] = defaultCulture;
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

	private bool CanForceStartOfNewRentalSession()
	{
		bool result = true;
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null)
		{
			result = service.Peek() != "maintenance_view" && service.Peek() != "maintenance_sync_view" && !service.Stack.Any((IViewFrameInstance x) => x?.ViewFrame?.ViewName == "swipe_view") && !(Redbox.Rental.Services.Session.Session.Instance?.BrowseState?.InReturnFlow ?? false);
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
}
