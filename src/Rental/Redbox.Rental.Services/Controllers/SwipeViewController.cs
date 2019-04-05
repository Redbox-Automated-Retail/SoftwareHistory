using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model.Marketing;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class SwipeViewController : BaseViewController
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
		if (!(BaseViewController.GetViewUserControl() is SwipeViewUserControl swipeViewUserControl))
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
		string back_button = Resources.back_button;
		BitmapImage adImage = null;
		SwipeViewType swipeViewType = _swipeViewType;
		string titleText;
		string messageText;
		if (swipeViewType != SwipeViewType.RegularCheckOut && swipeViewType == SwipeViewType.ReservationPickup)
		{
			titleText = Resources.reservation_swipe_title;
			messageText = Resources.reservation_swipe_message;
			MarketingAd marketingAd = ServiceLocator.Instance.GetService<IMarketingAdService>()?.GetScreenAd(MarketingAdLocation.Pickup, createImpression: true);
			if (marketingAd != null && marketingAd.Visible && !string.IsNullOrEmpty(marketingAd.ImageName))
			{
				Image image = ServiceLocator.Instance.GetService<IResourceBundleService>()?.GetBitmap(marketingAd.ImageName);
				adImage = ServiceLocator.Instance.GetService<IBitmapCacheService>()?.ToBitmapImage(image);
			}
		}
		else
		{
			titleText = Resources.regular_checkout_swipe_title;
			messageText = Resources.regular_checkout_swipe_message;
		}
		swipeViewUserControl.BackButtonText = back_button;
		swipeViewUserControl.TitleText = titleText;
		swipeViewUserControl.MessageText = messageText;
		swipeViewUserControl.AdImage = adImage;
		swipeViewUserControl.OnGetSpeechControl = delegate
		{
			ISpeechControl result = new SpeechControl
			{
				Name = "tts_swipe_view"
			};
			AddSpeechPartTitle(result);
			AddSpeechPartMessage(result);
			result.SpeechParts.ForEach(delegate(ISpeechPart x)
			{
				x.SpeechControl = result;
			});
			return result;
		};
		swipeViewUserControl.OnGoBack += OnBackButtonPressed;
		swipeViewUserControl.OnTimeOut += SwipeTimerTimeout;
		ITrackDataService service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
		if (service2 != null)
		{
			service2.StartTrackParsing();
			service2.ClearNotify();
			service2.Notify += TrackDataServiceNotify;
		}
		swipeViewUserControl.Timeout = 50000;
	}

	private static void AddSpeechPartTitle(ISpeechControl result)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 0,
			Name = "Title",
			Loop = false,
			StartPause = 500,
			EndPause = 1000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			AddBaseEvents(speechPart);
			SwipeViewUserControl swipeViewUserControl = BaseViewController.GetViewUserControl() as SwipeViewUserControl;
			speechPart.Texts.Add(new TextPart
			{
				Text = swipeViewUserControl?.TitleText
			});
		};
		result.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartMessage(ISpeechControl result)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "Message",
			Loop = true,
			StartPause = 0,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			AddBaseEvents(speechPart);
			SwipeViewUserControl swipeViewUserControl = BaseViewController.GetViewUserControl() as SwipeViewUserControl;
			speechPart.Texts.Add(new TextPart
			{
				Text = swipeViewUserControl?.MessageText
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "The credit card swiper is located on your right."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "If you wish to go back, press the CANCEL key."
			});
		};
		result.SpeechParts.Add(speechPart);
	}

	private static void AddBaseEvents(ISpeechPart speechPart)
	{
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "CANCEL",
			Action = delegate
			{
				OnBackButtonPressed();
			}
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "MINUS",
			Action = delegate
			{
				OnBackButtonPressed();
			}
		});
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
		(BaseViewController.GetViewUserControl() as SwipeViewUserControl)?.StopTimer();
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
		if (BaseViewController.GetViewService().Peek() == "swipe_view")
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
		else
		{
			LogHelper.Instance.Log("Back button from swipe view not processed because the swipe view is not on top of the stack.");
		}
	}

	private static void SwipeTimerTimeout()
	{
		AddAttribute("cardSwipe", "result", "timeout", ShoppingCartSessionId);
		GoBackSafe("swipe_view:timeout");
	}
}
