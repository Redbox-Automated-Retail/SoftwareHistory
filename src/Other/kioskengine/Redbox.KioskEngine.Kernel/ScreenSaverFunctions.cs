using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class ScreenSaverFunctions
{
	[KernelFunction(Name = "kernel.screensaverisactive")]
	internal static bool IsScreenSaverActive()
	{
		IScreenSaverService service = ServiceLocator.Instance.GetService<IScreenSaverService>();
		return service.IsScreenSaverActive();
	}

	[KernelFunction(Name = "kernel.screensaverisrunning")]
	internal static bool IsScreenSaverRunning()
	{
		IScreenSaverService service = ServiceLocator.Instance.GetService<IScreenSaverService>();
		return service.IsScreenSaverRunning();
	}

	[KernelFunction(Name = "kernel.screensavergettimeout")]
	internal static int GetScreenSaverTimeout()
	{
		IScreenSaverService service = ServiceLocator.Instance.GetService<IScreenSaverService>();
		return service.GetScreenSaverTimeout();
	}

	[KernelFunction(Name = "kernel.screensaversettimeout")]
	internal static void SetScreenSaverTimeout(int timeout)
	{
		IScreenSaverService service = ServiceLocator.Instance.GetService<IScreenSaverService>();
		service.SetScreenSaverTimeout(timeout);
	}

	[KernelFunction(Name = "kernel.screensaverdisable")]
	internal static void DisableScreenSaver()
	{
		IScreenSaverService service = ServiceLocator.Instance.GetService<IScreenSaverService>();
		service.DisableScreenSaver();
	}

	[KernelFunction(Name = "kernel.screensaverenable")]
	internal static void EnableScreenSaver()
	{
		IScreenSaverService service = ServiceLocator.Instance.GetService<IScreenSaverService>();
		service.EnableScreenSaver();
	}
}
