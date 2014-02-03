using System;
using System.IO;
using Redbox.HAL.Core;

namespace Redbox.HAL.Camera.Framework;

public sealed class CameraService : IDisposable
{
	public bool IsRunning { get; private set; }

	public bool CameraInErroredState { get; private set; }

	public static CameraService Instance => Singleton<CameraService>.Instance;

	private ICameraPlugin Plugin { get; set; }

	public void Dispose()
	{
		Plugin.TurnOff();
		GC.SuppressFinalize(this);
	}

	public bool Start()
	{
		if (IsRunning)
		{
			return true;
		}
		try
		{
			return IsRunning = Plugin.Start();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("CameraService.Start() caught an exception.", e);
			return IsRunning = false;
		}
	}

	public string Snap()
	{
		if (CameraConfiguration.Instance.Mode == CameraMode.Emulated)
		{
			return CameraEmulationState.Instance.ExpectedStartCaptureResult;
		}
		string text = Plugin.Snap();
		try
		{
			if (string.IsNullOrEmpty(text) || !File.Exists(text))
			{
				LogHelper.Instance.Log("File doesn't exist; turn off camera.");
				ProcessHelper.DumpProcessMemoryMetrics();
				TurnOffCamera();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("CameraService.Snap() caught an exception.", e);
			CameraInErroredState = true;
		}
		return text;
	}

	public bool Stop()
	{
		if (!CameraConfiguration.Instance.CycleCameraOnUse)
		{
			return true;
		}
		if (!IsRunning)
		{
			return false;
		}
		IsRunning = false;
		try
		{
			Plugin.Stop();
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("CameraService.Stop() caught an exception.", e);
			return false;
		}
	}

	public void TurnOffCamera()
	{
		try
		{
			Plugin.TurnOff();
			IsRunning = false;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("TurnOffCamera encountered an exception.", e);
		}
	}

	private CameraService()
	{
		if (CameraConfiguration.Instance.Mode != CameraMode.Emulated)
		{
			if (!Directory.Exists(CameraConfiguration.Instance.WorkingPath))
			{
				Directory.CreateDirectory(CameraConfiguration.Instance.WorkingPath);
			}
			LoadPluginsInner();
		}
	}

	private void LoadPluginsInner()
	{
		string cameraDriver = CameraConfiguration.Instance.CameraDriver;
		if (!PluginLoader.Instance.LocatePlugin(cameraDriver, out var plugin))
		{
			LogHelper.Instance.Log($"Unable to locate plugin {cameraDriver}", LogEntryType.Error);
			Plugin = new NilCameraPlugin();
		}
		else
		{
			LogHelper.Instance.Log($"Successfully loaded camera plugin {cameraDriver}");
			Plugin = plugin;
		}
		Plugin.InitWithProperties(CameraConfiguration.Instance.Properties);
	}
}
