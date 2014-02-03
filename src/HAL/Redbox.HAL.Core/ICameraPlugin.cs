using System.Collections.Generic;

namespace Redbox.HAL.Core;

public interface ICameraPlugin
{
	bool IsRunning { get; }

	bool Start();

	string Snap();

	bool Stop();

	void TurnOff();

	void InitWithProperties(IDictionary<string, object> properties);
}
