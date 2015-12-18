using System;

namespace Redbox.Core;

internal class Redbox_002EMacros1938_002ELogHelper
{
	public static Redbox_002EMacros1938_002ELogHelper Instance => Redbox_002EMacros1938_002ESingleton<Redbox_002EMacros1938_002ELogHelper>.Instance;

	public Redbox_002EMacros1938_002EILogger Logger { get; set; }

	public void Log(string message, Exception e)
	{
		if (Logger != null)
		{
			Logger.Log(message, e);
		}
	}

	public void Log(string message, Redbox_002EMacros1938_002ELogEntryType type)
	{
		if (Logger != null)
		{
			Logger.Log(message, type);
		}
	}

	public void Log(string message, params object[] parms)
	{
		if (Logger != null)
		{
			Logger.Log(message, parms);
		}
	}

	public void Log(string message, Exception e, params object[] parms)
	{
		if (Logger != null)
		{
			Logger.Log(string.Format(message, parms), e);
		}
	}

	public void Log(string message, Exception e, Redbox_002EMacros1938_002ELogEntryType type)
	{
		if (Logger != null)
		{
			Logger.Log(message, e, type);
		}
	}

	private Redbox_002EMacros1938_002ELogHelper()
	{
	}
}
