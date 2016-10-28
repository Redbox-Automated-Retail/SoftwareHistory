namespace Redbox.HAL.Component.Model;

internal interface IDoorSensorService : IMoveVeto
{
	bool SensorsEnabled { get; }

	bool SoftwareOverride { get; set; }

	DoorSensorResult Query();

	DoorSensorResult QueryStateForDisplay();
}
