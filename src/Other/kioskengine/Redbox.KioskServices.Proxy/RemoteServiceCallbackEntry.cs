using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.Proxy;

internal sealed class RemoteServiceCallbackEntry : ICallbackEntry
{
	public RemoteServiceCallback Callback { get; set; }

	public IRemoteServiceResult Result { get; set; }

	public string Message { get; set; }

	public void Invoke()
	{
		if (Callback == null)
		{
			return;
		}
		try
		{
			if (!string.IsNullOrEmpty(Message))
			{
				LogHelper.Instance.Log(Message);
			}
			Callback(Result);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in SimpleCallback handler.", e);
		}
	}
}
