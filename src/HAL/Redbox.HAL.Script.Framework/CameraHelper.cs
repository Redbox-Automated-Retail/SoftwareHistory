using System;
using Redbox.HAL.Camera.Framework;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class CameraHelper : IDisposable
{
	public void Dispose()
	{
		CameraService.Instance.Stop();
		ControllerService.Instance.ToggleRingLight(RinglightState.Off, null);
	}

	internal CameraHelper(int? timeout)
	{
		CameraService.Instance.Start();
		ControllerService.Instance.ToggleRingLight(RinglightState.On, timeout);
	}
}
