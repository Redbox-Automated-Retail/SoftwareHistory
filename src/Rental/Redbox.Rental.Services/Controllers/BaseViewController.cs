using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Model.Marketing;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.Controllers;

public class BaseViewController
{
	private static string IdleTimerName = "idle_timer";

	protected static IRentalShoppingCartService RentalShoppingCartService => ServiceLocator.Instance.GetService<IRentalShoppingCartService>();

	protected static ISessionAnalyticsService SessionAnalyticsService => ServiceLocator.Instance.GetService<ISessionAnalyticsService>();

	protected static IApplicationState ApplicationState => ServiceLocator.Instance.GetService<IApplicationState>();

	protected static IDataService DataService => ServiceLocator.Instance.GetService<IDataService>();

	protected static bool IsADAMode => ApplicationState?.IsADAAccessible ?? false;

	protected static FrameworkElement GetViewUserControl()
	{
		IViewFrameInstance obj = ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame();
		return ServiceLocator.Instance.GetService<IRenderingService>().GetScene("Default")?.GetWPFGridChild(obj?.Id);
	}

	protected static T GetViewModel<T>() where T : class
	{
		return GetViewUserControl()?.DataContext as T;
	}

	protected static void AnalyticsAddCurrentView()
	{
		OrchestrationService.AnalyticsAddCurrentView();
	}

	protected static void AnalyticsAddViewEvent(string subItemType, string subItemValue)
	{
		SessionAnalyticsService?.AddViewEvent(subItemType, subItemValue);
	}

	protected static void ProcessWPFHit()
	{
		ServiceLocator.Instance.GetService<IRenderingService>()?.ActiveScene.PrcoessWPFHit();
		PostTouchScreenHealthActivity();
	}

	protected static void PostTouchScreenHealthActivity()
	{
		ServiceLocator.Instance.GetService<ITouchScreenHealth>()?.PostActivity();
	}

	protected static Visibility BoolToVisibility(bool show)
	{
		if (!show)
		{
			return Visibility.Collapsed;
		}
		return Visibility.Visible;
	}

	protected static void Log(string message, string prefix = "")
	{
		LogHelper.Instance.Log($"{prefix}: {message}");
	}

	protected static IViewService GetViewService()
	{
		return ServiceLocator.Instance.GetService<IViewService>();
	}

	protected static IMacroService GetMacroService()
	{
		return ServiceLocator.Instance.GetService<IMacroService>();
	}

	protected static void SetCurrentUICulture()
	{
		OrchestrationService.SetCurrentUICulture();
	}

	protected static void StartIdleTimer(bool reset = false)
	{
		OrchestrationService.StartIdleTimer(IdleTimerName, reset);
	}

	protected static void StopIdleTimer()
	{
		OrchestrationService.StopIdleTimer(IdleTimerName);
	}

	protected static void ResetIdleTimer()
	{
		OrchestrationService.ResetIdleTimer(IdleTimerName);
	}

	protected static bool GetPickupStatus()
	{
		return (RentalShoppingCartService?.CurrentRentalShoppingCart)?.PickupRecommendationsShown ?? false;
	}

	protected static BitmapImage GetBundleBitmapImage(string resourceName)
	{
		IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		BitmapImage result = null;
		Image image = null;
		image = ((!resourceName.StartsWith("cache:")) ? ServiceLocator.Instance.GetService<IResourceBundleService>().GetBitmap(resourceName) : service?.GetImage(resourceName.Substring(6)));
		if (image != null)
		{
			result = service?.ToBitmapImage(image);
		}
		return result;
	}

	protected static MarketingAd GetMarketingAd(MarketingAdLocation location, bool createImpression)
	{
		return ServiceLocator.Instance.GetService<IMarketingAdService>().GetScreenAd(location, createImpression);
	}

	protected static void InvokeActionOnIdle(Action action)
	{
		Application.Current.Dispatcher.BeginInvoke(action, DispatcherPriority.SystemIdle);
	}

	protected static SecureString StringToSecureString(string source)
	{
		if (string.IsNullOrWhiteSpace(source))
		{
			return null;
		}
		SecureString secureString = new SecureString();
		foreach (char c in source)
		{
			secureString.AppendChar(c);
		}
		secureString.MakeReadOnly();
		return secureString;
	}

	protected static string SecureStringToString(SecureString source)
	{
		if (source == null)
		{
			return null;
		}
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = Marshal.SecureStringToGlobalAllocUnicode(source);
			return Marshal.PtrToStringUni(intPtr);
		}
		finally
		{
			Marshal.ZeroFreeGlobalAllocUnicode(intPtr);
		}
	}
}
