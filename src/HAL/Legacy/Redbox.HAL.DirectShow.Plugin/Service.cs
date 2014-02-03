using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using Redbox.HAL.Core;

namespace Redbox.HAL.DirectShow.Plugin;

internal sealed class Service : ICameraPlugin
{
	public bool IsRunning { get; private set; }

	internal int SnapTimeout { get; private set; }

	internal string ImagePath { get; private set; }

	internal Size ImageSize { get; private set; }

	private Player Worker { get; set; }

	private string SnapFile { get; set; }

	public bool Start()
	{
		if (IsRunning)
		{
			return true;
		}
		if (Worker == null)
		{
			Worker = new Player(this);
			Worker.Visible = false;
		}
		IsRunning = Worker.Connect();
		Thread.Sleep(5000);
		if (!IsRunning)
		{
			Worker.Disconnect();
			Worker.Close();
			Worker.Dispose();
			Worker = null;
		}
		return IsRunning;
	}

	public string Snap()
	{
		if (!IsRunning)
		{
			return null;
		}
		string text = Worker.Snap();
		if (text == null && Worker.InError)
		{
			Shutdown();
			LogHelper.Instance.Log("The player deteted an error; shut down graph.");
		}
		return text;
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
	}

	internal Service()
	{
		ImageSize = new Size(640, 480);
		SnapTimeout = 5000;
	}

	private bool Shutdown()
	{
		if (!IsRunning)
		{
			LogHelper.Instance.Log("DirectShowService::Shutdown(): camera isn't running - bypass stop.");
			return false;
		}
		IsRunning = false;
		Worker.Disconnect();
		Worker.Close();
		Worker.Dispose();
		Worker = null;
		LogHelper.Instance.Log(">> DirectShow plugin shutdown.");
		return true;
	}
}
