using System;
using System.Collections.Generic;
using System.Drawing;
using Redbox.DirectShow;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DirectShow.Frame.Plugin;

internal abstract class AbstractDirectShowControl : ICameraPlugin
{
	protected int SnapTimeout;

	protected Size ImageSize;

	protected int WakeupPause;

	protected int GrabTime;

	protected bool Debug;

	public bool SupportsReset => true;

	public bool IsRunning { get; protected set; }

	public bool Start()
	{
		if (IsRunning)
		{
			return true;
		}
		FilterInfoCollection filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
		if (filterInfoCollection == null || filterInfoCollection.Count == 0)
		{
			LogHelper.Instance.Log("DirectShow Frame service: unable to find any video devices.");
			return false;
		}
		return IsRunning = ControlStartup(filterInfoCollection[0].MonikerString);
	}

	public void Snap(string file)
	{
		if (!IsRunning)
		{
			LogHelper.Instance.Log("Direct show control is not in the running state.");
		}
		else
		{
			CoreSnap(file);
		}
	}

	public bool Stop()
	{
		return Shutdown();
	}

	public void InitWithProperties(IDictionary<string, object> properties)
	{
		if (properties.ContainsKey("VideoFormat"))
		{
			ImageSize = (Size)properties["VideoFormat"];
		}
		if (properties.ContainsKey("WakeupPause"))
		{
			WakeupPause = GetIntProperty(properties["WakeupPause"], WakeupPause);
		}
		if (properties.ContainsKey("SnapTimeout"))
		{
			SnapTimeout = GetIntProperty(properties["SnapTimeout"], SnapTimeout);
		}
		if (properties.ContainsKey("GrabWait"))
		{
			GrabTime = GetIntProperty(properties["GrabWait"], GrabTime);
		}
		if (properties.ContainsKey("DebugControl"))
		{
			try
			{
				Debug = (bool)properties["DebugControl"];
			}
			catch
			{
				Debug = false;
			}
		}
	}

	protected int GetIntProperty(object o, int def)
	{
		try
		{
			return (int)Convert.ChangeType(o, typeof(int));
		}
		catch
		{
			return def;
		}
	}

	protected void PlayingFinished(object sender, ReasonToFinishPlaying reason)
	{
		if (ReasonToFinishPlaying.StoppedByUser != reason)
		{
			LogHelper.Instance.Log("[DirectShowService ERROR]Video capture ended with an error status {0}", reason.ToString());
		}
	}

	protected AbstractDirectShowControl()
	{
		ImageSize = new Size(640, 480);
		SnapTimeout = 5000;
		WakeupPause = 5000;
		GrabTime = 500;
	}

	protected abstract void CoreSnap(string fileName);

	protected abstract bool ControlShutdown();

	protected abstract bool ControlStartup(string moniker);

	private bool Shutdown()
	{
		if (!IsRunning)
		{
			LogHelper.Instance.Log("DirectShowService::Shutdown(): camera isn't running - bypass stop.");
			return false;
		}
		IsRunning = false;
		return ControlShutdown();
	}
}
