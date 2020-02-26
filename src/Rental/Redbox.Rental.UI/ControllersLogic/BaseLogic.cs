using System.Globalization;
using System.Security;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.KioskHealth;

namespace Redbox.Rental.UI.ControllersLogic;

public class BaseLogic : DynamicRouting
{
	protected static IAnalyticsService AnalyticsService => ServiceLocator.Instance.GetService<IAnalyticsService>();

	public static SecureString CreateSecureString(string source)
	{
		if (string.IsNullOrWhiteSpace(source))
		{
			return null;
		}
		SecureString secureString = new SecureString();
		char[] array = source.ToCharArray();
		foreach (char c in array)
		{
			secureString.AppendChar(c);
		}
		return secureString;
	}

	public static void HandleWPFHit()
	{
		ServiceLocator.Instance.GetService<IRenderingService>()?.ActiveScene.PrcoessWPFHit();
		PostHealthActivity();
	}

	protected static void ProcessWPFHit()
	{
		ServiceLocator.Instance.GetService<IRenderingService>()?.ActiveScene.PrcoessWPFHit();
		PostHealthActivity();
	}

	protected static void PostHealthActivity()
	{
		ServiceLocator.Instance.GetService<ITouchScreenHealth>()?.PostActivity();
		ServiceLocator.Instance.GetService<IViewHealth>()?.PostActivity("Button Press");
	}

	protected static void SetCurrentUICulture()
	{
		string text = ServiceLocator.Instance.GetService<IResourceBundleService>()?.Filter["culture"] ?? null;
		if (text != null)
		{
			string name = Thread.CurrentThread.CurrentUICulture.Name;
			if (!text.Equals(name))
			{
				Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(text);
			}
		}
	}
}
