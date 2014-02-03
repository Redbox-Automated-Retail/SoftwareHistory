using System.Collections.Generic;

namespace Redbox.HAL.Core;

internal interface ICameraPlugin
{
	bool IsRunning { get; }

	bool Start();

	string Snap();

	bool Stop();

	string StartCapture();

	void StopCapture();

	void TurnOff();

	void ShowProperties();

	string[] GetCameraDeviceNames();

	string[] GetVideoCompressors();

	void InitWithProperties(IDictionary<string, object> properties);
}
