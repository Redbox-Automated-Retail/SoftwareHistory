using System;

namespace Redbox.HAL.Component.Model;

public interface IDoorSensorService : IDisposable
{
	bool SensorsEnabled { get; }

	bool SoftwareOverride { get; set; }

	DoorSensorResult Query();

	DoorSensorResult QueryStateForDisplay();
}
