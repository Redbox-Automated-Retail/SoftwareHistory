using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

public static class EnvironmentFunctions
{
	private static bool m_safeShutdownFlagged;

	private static LuaFunction m_canShutdownHandler;

	private static LuaFunction m_canRestartHandler;

	private static LuaFunction m_showMaintenanceViewHandler;

	private static LuaFunction m_dataStoreErrorHandler;

	public static bool CanShutdown()
	{
		if (!CanSwitch())
		{
			return false;
		}
		if (m_canShutdownHandler == null)
		{
			LogHelper.Instance.Log("EnvironmentFunctions.CanShutdown() returns: The handler specified; returning true.");
			return true;
		}
		ReadOnlyCollection<object> readOnlyCollection = m_canShutdownHandler.Call();
		if (readOnlyCollection != null && readOnlyCollection.Count > 0)
		{
			bool flag = (bool)readOnlyCollection[0];
			LogHelper.Instance.Log("EnvironmentFunctions.CanShutdown() returns: {0}", flag);
			return flag;
		}
		LogHelper.Instance.Log("EnvironmentFunctions.CanShutdown() returns: The handler returned nil; returning true.");
		return true;
	}

	public static bool CanRestart()
	{
		if (!CanSwitch())
		{
			return false;
		}
		if (m_canRestartHandler == null)
		{
			LogHelper.Instance.Log("EnvironmentFunctions.CanRestart() returns: The handler specified; returning true.");
			return true;
		}
		ReadOnlyCollection<object> readOnlyCollection = m_canRestartHandler.Call();
		if (readOnlyCollection != null && readOnlyCollection.Count > 0)
		{
			bool flag = (bool)readOnlyCollection[0];
			LogHelper.Instance.Log("EnvironmentFunctions.CanRestart() returns: {0}", flag);
			return flag;
		}
		LogHelper.Instance.Log("EnvironmentFunctions.CanRestart() returns: The handler returned nil; returning true.");
		return true;
	}

	public static bool CanSwitch()
	{
		LogHelper.Instance.Log("Start CanSwitch Check");
		IKernelExtensionService service = ServiceLocator.Instance.GetService<IKernelExtensionService>();
		bool flag = ServiceLocator.Instance.GetService<IEngineApplication>()?.AppStarting ?? false;
		LogHelper.Instance.Log("Performing Startup: {0}", flag);
		bool flag2 = !flag && service.CanSwitch(out var _);
		LogHelper.Instance.Log("CanSwitch: {0}", flag2);
		return flag2;
	}

	public static bool ShowMaintenanceView()
	{
		if (m_showMaintenanceViewHandler == null)
		{
			return false;
		}
		ReadOnlyCollection<object> readOnlyCollection = m_showMaintenanceViewHandler.Call();
		if (readOnlyCollection == null || readOnlyCollection.Count <= 0)
		{
			return false;
		}
		return (bool)readOnlyCollection[0];
	}

	public static bool HandleDataStoreErrors(string dataStore)
	{
		if (m_dataStoreErrorHandler == null)
		{
			return false;
		}
		ReadOnlyCollection<object> readOnlyCollection = m_dataStoreErrorHandler.Call(dataStore);
		if (readOnlyCollection == null || readOnlyCollection.Count <= 0)
		{
			return false;
		}
		return (bool)readOnlyCollection[0];
	}

	public static void FlagForSafeShutdown()
	{
		m_safeShutdownFlagged = true;
	}

	public static void Reset()
	{
		LogHelper.Instance.Log("Reset EnvironmentFunctions");
		if (m_canShutdownHandler != null)
		{
			LogHelper.Instance.Log("...Reset shutdown handler.");
			m_canShutdownHandler.Dispose();
			m_canShutdownHandler = null;
		}
		if (m_showMaintenanceViewHandler != null)
		{
			LogHelper.Instance.Log("...Reset show maintenance view handler.");
			m_showMaintenanceViewHandler.Dispose();
			m_showMaintenanceViewHandler = null;
		}
		if (m_dataStoreErrorHandler != null)
		{
			LogHelper.Instance.Log("...Reset data store errors handler.");
			m_dataStoreErrorHandler.Dispose();
			m_dataStoreErrorHandler = null;
		}
	}

	[KernelFunction(Name = "kernel.applysafeshutdown")]
	internal static void ApplySafeShutdown()
	{
		if (!m_safeShutdownFlagged)
		{
			LogHelper.Instance.Log("kernel.applysafeshutdown is not allowed now; returning.");
			return;
		}
		m_safeShutdownFlagged = false;
		LogHelper.Instance.Log("kernel.applysafeshutdown is allowed; calling Application.Exit().");
		Application.Exit();
	}

	[KernelFunction(Name = "kernel.applyunsafeshutdown")]
	internal static void ApplyUnSafeShutdown()
	{
		LogHelper.Instance.Log("kernel.applyunsafeshutdown; calling Application.Exit().");
		Application.Exit();
	}

	[KernelFunction(Name = "kernel.registercanshutdown")]
	internal static void RegisterCanShutdownHandler(LuaFunction canShutdown)
	{
		m_canShutdownHandler = canShutdown;
	}

	[KernelFunction(Name = "kernel.registercanrestart")]
	internal static void RegisterCanRestartHandler(LuaFunction canRestart)
	{
		m_canRestartHandler = canRestart;
	}

	[KernelFunction(Name = "kernel.registershowmaintenanceview")]
	internal static void RegisterShowMaintenanceViewHandler(LuaFunction showMaintenanceView)
	{
		m_showMaintenanceViewHandler = showMaintenanceView;
	}

	[KernelFunction(Name = "kernel.registercorruptdb")]
	internal static void RegisterCorruptDbHandler(CorruptDbCallback callback)
	{
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RegisterCorruptDbCallback(callback);
	}

	[KernelFunction(Name = "kernel.unregistercorruptdb")]
	internal static void UnRegisterCorruptDbHandler()
	{
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.UnRegisterCorruptDbCallback();
	}

	[KernelFunction(Name = "kernel.registerdatastoreerrorhandler")]
	internal static void RegisterDataStoreErrorHandler(LuaFunction handleDataStoreErrors)
	{
		m_dataStoreErrorHandler = handleDataStoreErrors;
	}

	[KernelFunction(Name = "kernel.hasvalidcardreader")]
	internal static bool HasValidCardReader()
	{
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		if (service != null)
		{
			if (service.CheckCardReader)
			{
				return service.HasValidCardReader;
			}
			return true;
		}
		return false;
	}

	[KernelFunction(Name = "kernel.registerhardwarechange")]
	internal static void RegisterHardwareChangeHandler(HardwareChangeCallback callback)
	{
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.RegisterHardwareChangeCallback(callback);
	}

	[KernelFunction(Name = "kernel.unregisterhardwarechange")]
	internal static void UnregisterHardwareChangeHandler()
	{
		ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.UnregisterHardwareChangeCallback();
	}

	[KernelFunction(Name = "kernel.wait")]
	internal static void Wait(int delay)
	{
		LogHelper.Instance.Log($"Wait {delay}", LogEntryType.Info);
		new ManualResetEvent(initialState: false).WaitOne(delay);
	}
}
