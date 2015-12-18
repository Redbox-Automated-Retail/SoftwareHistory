using System;

namespace Redbox.HAL.DataMatrix.Framework;

public interface IScannerDevice : IDisposable
{
	bool RequiresExternalLighting { get; }

	bool IsConnected { get; }

	bool InError { get; }

	bool Start();

	SnapResult Snap(string fileName);

	SnapResult Snap();

	bool Stop();

	void TurnOff();

	bool Restart();

	ScanResult Scan();

	string FindInstalledPort();
}
