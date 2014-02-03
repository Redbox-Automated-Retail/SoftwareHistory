using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using Redbox.DirectShow;
using Redbox.HAL.Core;

namespace Redbox.HAL.DirectShow.Frame.Plugin;

internal sealed class Service : ICameraPlugin
{
	private readonly AutoResetEvent ImageReceived = new AutoResetEvent(initialState: false);

	public bool IsRunning { get; private set; }

	internal int SnapTimeout { get; private set; }

	internal string ImagePath { get; private set; }

	internal Size ImageSize { get; private set; }

	private int WakeupPause { get; set; }

	private bool InError { get; set; }

	private bool WaitingOnImage { get; set; }

	private string SnapFile { get; set; }

	private VCDevice3 Device { get; set; }

	public bool Start()
	{
		if (IsRunning)
		{
			return true;
		}
		FilterInfoCollection filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
		if (filterInfoCollection == null || filterInfoCollection.Count == 0)
		{
			LogHelper.Instance.Log("DirectShow Frame: unable to find any video devices.");
			return false;
		}
		Device = new VCDevice3(filterInfoCollection[0].MonikerString);
		bool flag = false;
		VideoCapabilities[] videoCapabilities = Device.VideoCapabilities;
		foreach (VideoCapabilities videoCapabilities2 in videoCapabilities)
		{
			if (videoCapabilities2.FrameSize == ImageSize)
			{
				flag = true;
				Device.VideoResolution = videoCapabilities2;
				break;
			}
		}
		if (!flag)
		{
			LogHelper.Instance.Log("Unable to find video capability with size {0}w X {1}h", ImageSize.Width, ImageSize.Height);
		}
		Thread.Sleep(500);
		Device.SnapshotFrame += GetFrame;
		Device.PlayingFinished += PlayingFinished;
		Device.Start();
		Thread.Sleep(WakeupPause);
		LogHelper.Instance.Log("DirectShow frame control running.");
		return IsRunning = true;
	}

	public string Snap()
	{
		if (!IsRunning)
		{
			return null;
		}
		WaitingOnImage = true;
		Device.SimulateTrigger();
		if (!ImageReceived.WaitOne(SnapTimeout))
		{
			SnapFile = null;
			ImageReceived.Reset();
		}
		WaitingOnImage = false;
		if (SnapFile == null && InError)
		{
			Shutdown();
			LogHelper.Instance.Log("The player deteted an error; shut down graph.");
		}
		return SnapFile;
	}

	public bool Stop()
	{
		return Shutdown();
	}

	public void TurnOff()
	{
		Shutdown();
	}

	public void InitWithProperties(IDictionary<string, object> properties)
	{
		if (properties.ContainsKey("WorkingPath"))
		{
			ImagePath = properties["WorkingPath"] as string;
		}
		if (properties.ContainsKey("VideoFormat"))
		{
			ImageSize = (Size)properties["VideoFormat"];
		}
		if (properties.ContainsKey("SnapTimeout"))
		{
			SnapTimeout = (int)properties["SnapTimeout"];
		}
		if (properties.ContainsKey("WakeupPause"))
		{
			WakeupPause = (int)properties["WakeupPause"];
		}
	}

	internal Service()
	{
		ImageSize = new Size(640, 480);
		SnapTimeout = 5000;
		WakeupPause = 5000;
	}

	private bool Shutdown()
	{
		if (!IsRunning)
		{
			LogHelper.Instance.Log("DirectShowService::Shutdown(): camera isn't running - bypass stop.");
			return false;
		}
		IsRunning = false;
		Device.SignalToStop();
		Device.WaitForStop();
		Device.SnapshotFrame -= GetFrame;
		Device.PlayingFinished -= PlayingFinished;
		Device = null;
		LogHelper.Instance.Log(">> DirectShow Snap plugin shutdown.");
		return true;
	}

	private void GetFrame(object sender, NewFrameEventArgs eventArgs)
	{
		if (WaitingOnImage)
		{
			Bitmap frame = eventArgs.Frame;
			DateTime now = DateTime.Now;
			string path = $"m{now.Month}d{now.Day}y{now.Year}_h{now.Hour}m{now.Minute}s{now.Second}t{now.Millisecond}.jpg";
			SnapFile = Path.Combine(ImagePath, path);
			frame.Save(SnapFile);
			ImageReceived.Set();
		}
	}

	private void PlayingFinished(object sender, ReasonToFinishPlaying reason)
	{
		if (ReasonToFinishPlaying.StoppedByUser != reason)
		{
			LogHelper.Instance.Log("[CAMERA ERROR]Video capture ended with an error status {0}", reason.ToString());
			InError = true;
		}
	}
}
