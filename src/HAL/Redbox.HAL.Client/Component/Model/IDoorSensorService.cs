using System;

namespace Redbox.HAL.Component.Model;

internal interface IDoorSensorService : IDisposable
{
	bool SensorsEnabled { get; }

	bool SoftwareOverride { get; set; }

	DoorSensorResult Query();

	DoorSensorResult QueryStateForDisplay();
}
