using System;
using System.ServiceProcess;

namespace HALUtilities;

internal static class ProcessHelper
{
	private static string HALServiceMoniker = "halsvc$default";

	internal static bool StopHALService()
	{
		ServiceController serviceController = new ServiceController(HALServiceMoniker);
		try
		{
			serviceController.Stop();
			TimeSpan timeout = TimeSpan.FromSeconds(30.0);
			serviceController.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
			return true;
		}
		catch
		{
			return false;
		}
	}

	internal static bool StartHALService()
	{
		ServiceController serviceController = new ServiceController(HALServiceMoniker);
		try
		{
			TimeSpan timeout = TimeSpan.FromSeconds(30.0);
			serviceController.Start();
			serviceController.WaitForStatus(ServiceControllerStatus.Running, timeout);
			return true;
		}
		catch
		{
			return false;
		}
	}
}
