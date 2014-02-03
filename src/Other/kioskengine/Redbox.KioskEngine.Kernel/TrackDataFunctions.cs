using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class TrackDataFunctions
{
	[KernelFunction(Name = "kernel.trackgetfirstname")]
	internal static string GetTrackFirstName()
	{
		return ServiceLocator.Instance.GetService<ITrackDataService>()?.GetFirstName();
	}

	[KernelFunction(Name = "kernel.trackgetlastname")]
	internal static string GetTrackLastName()
	{
		return ServiceLocator.Instance.GetService<ITrackDataService>()?.GetLastName();
	}

	[KernelFunction(Name = "kernel.trackgetexpirymonth")]
	internal static string GetTrackExpiryMonth()
	{
		return ServiceLocator.Instance.GetService<ITrackDataService>()?.GetExpiryMonth();
	}

	[KernelFunction(Name = "kernel.trackgetexpiryyear")]
	internal static string GetTrackExpiryYear()
	{
		return ServiceLocator.Instance.GetService<ITrackDataService>()?.GetExpiryYear();
	}

	[KernelFunction(Name = "kernel.trackgetlastfour")]
	internal static string GetTrackLastFour()
	{
		return ServiceLocator.Instance.GetService<ITrackDataService>()?.GetLastFour();
	}

	[KernelFunction(Name = "kernel.trackgetcardtype")]
	internal static string GetTrackCardType()
	{
		return ServiceLocator.Instance.GetService<ITrackDataService>()?.GetCardType().ToString();
	}

	[KernelFunction(Name = "kernel.trackget1")]
	internal static string GetTrack1()
	{
		return ServiceLocator.Instance.GetService<ITrackDataService>()?.GetTrack1();
	}

	[KernelFunction(Name = "kernel.trackget2")]
	internal static string GetTrack2()
	{
		return ServiceLocator.Instance.GetService<ITrackDataService>()?.GetTrack2();
	}

	[KernelFunction(Name = "kernel.trackgethashid")]
	internal static string GetCardHashId()
	{
		return ServiceLocator.Instance.GetService<ITrackDataService>()?.GenerateCardHashId();
	}

	[KernelFunction(Name = "kernel.trackdatanotify")]
	internal static void TrackDataNotifty(TrackDataNotifyHandler notifyHandler)
	{
		ITrackDataService service = ServiceLocator.Instance.GetService<ITrackDataService>();
		if (service == null)
		{
			return;
		}
		service.ClearNotify();
		service.Notify += delegate
		{
			try
			{
				notifyHandler();
			}
			catch (LuaException e2)
			{
				e2.RaiseDebuggerOrError("kernel.trackdatanotify");
			}
			catch (Exception e3)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in TrackDataNotify.", e3);
			}
		};
	}

	[KernelFunction(Name = "kernel.trackdatastart")]
	internal static void TrackDataStart()
	{
		ServiceLocator.Instance.GetService<ITrackDataService>()?.StartTrackParsing();
	}

	[KernelFunction(Name = "kernel.trackdatastop")]
	internal static void TrackDataStop()
	{
		ServiceLocator.Instance.GetService<ITrackDataService>()?.StopTrackParsing();
	}

	[KernelFunction(Name = "kernel.trackdataclear")]
	internal static void TrackDataClear()
	{
		ServiceLocator.Instance.GetService<ITrackDataService>()?.ClearData();
	}

	[KernelFunction(Name = "kernel.trackdataisvalid")]
	internal static bool TrackDataIsValid()
	{
		return ServiceLocator.Instance.GetService<ITrackDataService>()?.HasValidData() ?? false;
	}
}
