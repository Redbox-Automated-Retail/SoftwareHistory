using System;

namespace Redbox.KioskEngine.ComponentModel;

public static class IMessageExtensions
{
	public static bool IsNotKioskAlertMessage(this IMessage source)
	{
		if (source == null)
		{
			return true;
		}
		if (source.Type == null)
		{
			return true;
		}
		return !string.Equals(source.Type, "KioskAlert", StringComparison.InvariantCultureIgnoreCase);
	}
}
