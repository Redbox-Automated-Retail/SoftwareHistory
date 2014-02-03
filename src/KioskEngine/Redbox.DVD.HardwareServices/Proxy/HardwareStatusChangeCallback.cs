using System;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.HardwareServices.Proxy;

internal class HardwareStatusChangeCallback : ICallbackEntry
{
	public HardwareStatusChangeHandler EventHandler { get; set; }

	public string JobId { get; set; }

	public string Status { get; set; }

	public void Invoke()
	{
		if (EventHandler == null)
		{
			return;
		}
		try
		{
			LogHelper.Instance.Log("Invoke HardwareStatusChangeCallback.EventHandler with: {0}, {1}", JobId, Status);
			EventHandler(JobId, Status);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in HardwareStatusChangeCallback handler.", e);
		}
	}
}
