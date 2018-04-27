using System.Security;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskHealth;

namespace Redbox.Rental.UI.ControllersLogic;

public class BaseLogic : DynamicRouting
{
	protected static ISessionAnalyticsService SessionAnalyticsService => ServiceLocator.Instance.GetService<ISessionAnalyticsService>();

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
		ServiceLocator.Instance.GetService<ITouchScreenHealth>()?.PostActivity();
	}

	public static void AnalyticsAddViewEvent(string subItemType, string subItemValue)
	{
		SessionAnalyticsService?.AddViewEvent(subItemType, subItemValue);
	}
}
