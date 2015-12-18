using System.Collections.Generic;

namespace Redbox.HAL.Core;

internal interface ICameraPlugin
{
	bool IsRunning { get; }

	bool SupportsReset { get; }

	bool Start();

	void Snap(string file);

	bool Stop();

	void TurnOff();

	void InitWithProperties(IDictionary<string, object> properties);
}
