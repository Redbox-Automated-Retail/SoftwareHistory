using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Remoting;

public class KioskEngineService : IKioskEngineService
{
	private string _kioskEnginePath;

	public static KioskEngineService Instance => Singleton<KioskEngineService>.Instance;

	public void Initialize(string kioskEnginePath, string kioskEngineUrl)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string text = service.ExpandProperties(kioskEnginePath);
		if (!Path.IsPathRooted(text))
		{
			text = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), text);
		}
		_kioskEnginePath = kioskEnginePath;
		ServiceLocator.Instance.AddService(typeof(IKioskEngineService), this);
		KioskEngineProxy.IpcHostUrl = kioskEngineUrl;
	}

	[Obsolete("Windows Messaging communication no longer supported")]
	public ErrorList ExportSessions(string path)
	{
		ErrorList errorList = new ErrorList();
		errorList.Add(Error.NewError("W999", string.Format("Service method '{0}' not implemented.", "ExportSessions"), "Windows Messaging communication no longer supported."));
		return errorList;
	}

	[Obsolete("Windows Messaging communication no longer supported")]
	public ErrorList ExportQueue(string path)
	{
		ErrorList errorList = new ErrorList();
		errorList.Add(Error.NewError("W999", string.Format("Service method '{0}' not implemented.", "ExportQueue"), "Windows Messaging communication no longer supported."));
		return errorList;
	}

	public ErrorList IsEngineRunning(out bool isRunning)
	{
		isRunning = false;
		ErrorList errorList = new ErrorList();
		try
		{
			isRunning = KioskEngineProxy.IsEngineRunning();
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("K999", "An unhandled exception was raised in KioskEngineService.IsEngineRunning.", e));
		}
		return errorList;
	}

	public ErrorList StartEngine()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("Starting engine at location: {0}", _kioskEnginePath);
			int num = ShellHelper.StartProcessAsShellUser(_kioskEnginePath);
			if (num < 0)
			{
				LogHelper.Instance.Log("Error impersonating shell user. Errors found above");
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("K999", "An unhandled exception was raised in KioskEngineService.StartEngine.", e));
		}
		return errorList;
	}

	public ErrorList StartEngine(string bundleName)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("Starting engine at location: {0}", _kioskEnginePath);
			int num = ShellHelper.StartProcessAsShellUser(_kioskEnginePath, $"--bundle:\"{bundleName}\"");
			if (num < 0)
			{
				LogHelper.Instance.Log("Error impersonating shell user. Errors found above");
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("K999", "An unhandled exception was raised in KioskEngineService.StartEngine.", e));
		}
		return errorList;
	}

	public ErrorList ReloadBundle()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			KioskEngineProxy.Send("engine reset-bundle");
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("K999", "An unhandled exception was raised in KioskEngineService.ReloadBundle.", e));
		}
		return errorList;
	}

	public ErrorList Shutdown()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			if (!KioskEngineProxy.IsEngineRunning())
			{
				return errorList;
			}
			KioskEngineProxy.Send("engine shutdown");
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("K999", "An unhandled exception was raised in KioskEngineService.Shutdown.", e));
		}
		return errorList;
	}

	public ErrorList Shutdown(int timeout, int tries)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			if (!KioskEngineProxy.IsEngineRunning())
			{
				return errorList;
			}
			KioskEngineProxy.Send("engine shutdown");
			int num = 0;
			ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
			manualResetEvent.WaitOne(timeout);
			manualResetEvent.Close();
			errorList.AddRange(IsEngineRunning(out var isRunning));
			while (isRunning && num < tries)
			{
				ManualResetEvent manualResetEvent2 = new ManualResetEvent(initialState: false);
				manualResetEvent2.WaitOne(timeout);
				manualResetEvent2.Close();
				num++;
			}
			errorList.AddRange(IsEngineRunning(out isRunning));
			if (isRunning)
			{
				errorList.Add(Error.NewError("T999", "The engine failed to shutdown in the time specified.", "Try to shutdown the engine again."));
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("K999", "An unhandled exception was raised in KioskEngineService.Shutdown.", e));
		}
		return errorList;
	}

	[Obsolete("Windows Messaging communication no longer supported")]
	public ErrorList ChangeBundle(string name)
	{
		ErrorList errorList = new ErrorList();
		errorList.Add(Error.NewError("W999", string.Format("Service method '{0}' not implemented.", "ChangeBundle"), "Windows Messaging communication no longer supported."));
		return errorList;
	}

	[Obsolete("Windows Messaging communication no longer supported")]
	public ErrorList BringToFront()
	{
		ErrorList errorList = new ErrorList();
		errorList.Add(Error.NewError("W999", string.Format("Service method '{0}' not implemented.", "BringToFront"), "Windows Messaging communication no longer supported."));
		return errorList;
	}

	[Obsolete("Windows Messaging communication no longer supported")]
	public ErrorList ExecuteScript(string path)
	{
		ErrorList errorList = new ErrorList();
		errorList.Add(Error.NewError("W999", string.Format("Service method '{0}' not implemented.", "ExecuteScript"), "Windows Messaging communication no longer supported."));
		return errorList;
	}

	[Obsolete("Windows Messaging communication no longer supported")]
	public ErrorList ShowOfflineScreen()
	{
		ErrorList errorList = new ErrorList();
		errorList.Add(Error.NewError("W999", string.Format("Service method '{0}' not implemented.", "ShowOfflineScreen"), "Windows Messaging communication no longer supported."));
		return errorList;
	}

	[Obsolete("Windows Messaging communication no longer supported")]
	public ErrorList GetMemoryUsage(out long total)
	{
		ErrorList errorList = new ErrorList();
		errorList.Add(Error.NewError("W999", string.Format("Service method '{0}' not implemented.", "GetMemoryUsage"), "Windows Messaging communication no longer supported."));
		ErrorList result = errorList;
		total = -1L;
		return result;
	}

	private KioskEngineService()
	{
	}
}
