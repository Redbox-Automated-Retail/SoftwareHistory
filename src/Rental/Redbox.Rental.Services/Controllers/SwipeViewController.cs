using System;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Marketing;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class SwipeViewController
{
	private static Action _cancelAction;

	private static Action _continueAction;

	private static SwipeViewType _swipeViewType;

	private static string ShoppingCartSessionId
	{
		get
		{
			string result = null;
			IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
			if (service != null)
			{
				IShoppingSession currentSession = service.GetCurrentSession();
				if (currentSession != null)
				{
					result = currentSession.Id.ToString();
				}
			}
			return result;
		}
	}

	public static void SetSwipeViewParameters(Action cancelAction, Action continueAction, string stringSwipeViewType)
	{
		SwipeViewType result = SwipeViewType.RegularCheckOut;
		if (Enum.TryParse<SwipeViewType>(stringSwipeViewType, out result))
		{
			SetSwipeViewParameters2(cancelAction, continueAction, result);
			return;
		}
		throw new ArgumentException("Invalid stringSwipeViewType parameter value: {0}", stringSwipeViewType);
	}

	public static void SetSwipeViewParameters2(Action cancelAction, Action continueAction, SwipeViewType swipeViewType)
	{
		_cancelAction = cancelAction;
		_continueAction = continueAction;
		_swipeViewType = swipeViewType;
	}

	public static void ConfigureSwipeView()
	{
		IActor actor = ActorFunctions.GetActor("SwipeViewUserControl");
		if (actor == null || !(actor.WPFFrameworkElement is SwipeViewUserControl swipeViewUserControl))
		{
			return;
		}
		ISessionAnalyticsService service = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
		string text = ServiceLocator.Instance.GetService<IShoppingSessionService>()?.GetCurrentSession()?.Id.ToString();
		if (service != null && service.SessionExists(text))
		{
			service.AddNode("authorizations", "cardSwipe", text);
		}
		swipeViewUserControl.SwipeType = _swipeViewType;
		string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", "back_button");
		BitmapImage adImage = null;
		string dictionaryStringValue2;
		string dictionaryStringValue3;
		switch (_swipeViewType)
		{
		case SwipeViewType.ReservationPickup:
		{
			dictionaryStringValue2 = BundleFunctions.GetDictionaryStringValue("strings", "reservation_swipe_title");
			dictionaryStringValue3 = BundleFunctions.GetDictionaryStringValue("strings", "reservation_swipe_message");
			MarketingAd marketingAd = ServiceLocator.Instance.GetService<IMarketingAdService>()?.GetScreenAd(MarketingAdLocation.Pickup, createImpression: true);
			if (marketingAd != null && marketingAd.Visible && !string.IsNullOrEmpty(marketingAd.ImageName))
			{
				adImage = ServiceLocator.Instance.GetService<IResourceBundleService>()?.GetBitmapImage(marketingAd.ImageName);
			}
			break;
		}
		case SwipeViewType.Credits:
			dictionaryStringValue2 = BundleFunctions.GetDictionaryStringValue("strings", "credits_swipe_title");
			dictionaryStringValue3 = BundleFunctions.GetDictionaryStringValue("strings", "credits_swipe_message");
			break;
		default:
			dictionaryStringValue2 = BundleFunctions.GetDictionaryStringValue("strings", "regular_checkout_swipe_title");
			dictionaryStringValue3 = BundleFunctions.GetDictionaryStringValue("strings", "regular_checkout_swipe_message");
			break;
		}
		swipeViewUserControl.BackButtonText = dictionaryStringValue;
		swipeViewUserControl.TitleText = dictionaryStringValue2;
		swipeViewUserControl.MessageText = dictionaryStringValue3;
		swipeViewUserControl.AdImage = adImage;
		IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
		if (service2 != null)
		{
			ITextToSpeechService service3 = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			if (service3 != null && service3.TTSEnabled)
			{
				service2["TTS:SwipeTitle"] = dictionaryStringValue2;
				service2["TTS:SwipeMessage"] = dictionaryStringValue3;
				service3.TTSRepeatSequence();
			}
		}
		swipeViewUserControl.OnGoBack += OnBackButtonPressed;
		swipeViewUserControl.OnTimeOut += SwipeTimerTimeout;
		ITrackDataService service4 = ServiceLocator.Instance.GetService<ITrackDataService>();
		if (service4 != null)
		{
			service4.StartTrackParsing();
			service4.ClearNotify();
			service4.Notify += TrackDataServiceNotify;
		}
		swipeViewUserControl.Timeout = 50000;
	}

	public static void LeaveSwipeView()
	{
		ServiceLocator.Instance.GetService<ITrackDataService>()?.StopTrackParsing();
		StopTimer();
	}

	private static void TrackDataServiceNotify(object sender, EventArgs e)
	{
		try
		{
			StopTimer();
			ITrackDataService service = ServiceLocator.Instance.GetService<ITrackDataService>();
			if (service == null || !service.HasValidData())
			{
				AddAttribute("cardSwipe", "result", "invalid", ShoppingCartSessionId);
				Action action = delegate
				{
					GoBackSafe("bad_swipe_action");
				};
				MessagePopupViewController.ShowMessagePopupViewCardNotRecognized(action, action);
			}
			else
			{
				AddAttribute("cardSwipe", "result", "valid", ShoppingCartSessionId);
				if (_continueAction != null)
				{
					_continueAction();
				}
				else
				{
					GoBackSafe("trackdatanotify");
				}
			}
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in TrackDataServiceNotify.", e2);
		}
	}

	private static void StopTimer()
	{
		IActor actor = ActorFunctions.GetActor("SwipeViewUserControl");
		if (actor != null && actor.WPFFrameworkElement is SwipeViewUserControl swipeViewUserControl)
		{
			swipeViewUserControl.StopTimer();
		}
	}

	private static void GoBackSafe(object content)
	{
		ServiceLocator.Instance.GetService<IViewService>()?.GoBackSafe(content, popOnly: false);
	}

	private static void AddViewEvent(string subItemType, string subItemValue)
	{
		ServiceLocator.Instance.GetService<ISessionAnalyticsService>()?.AddViewEvent(subItemType, subItemValue);
	}

	private static void AddAttribute(string parentNodeName, string attributeName, string attributeValue, string bufferedSessionID)
	{
		ServiceLocator.Instance.GetService<ISessionAnalyticsService>()?.AddAttribute(parentNodeName, attributeName, attributeValue, bufferedSessionID);
	}

	private static void OnBackButtonPressed()
	{
		AddViewEvent("buttonPress", "card-swipe-back");
		AddAttribute("cardSwipe", "result", "cancelled", ShoppingCartSessionId);
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		if (service != null)
		{
			service["MatureWarning:Header"] = string.Empty;
		}
		if (_cancelAction != null)
		{
			_cancelAction();
		}
		else
		{
			GoBackSafe("swipe_view:back_button");
		}
	}

	private static void SwipeTimerTimeout()
	{
		AddAttribute("cardSwipe", "result", "timeout", ShoppingCartSessionId);
		GoBackSafe("swipe_view:timeout");
	}
}
