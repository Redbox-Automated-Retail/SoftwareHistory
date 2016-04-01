using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class UnconfiguredDoorSensorService : IDoorSensorService, IDisposable
{
	private bool m_softwareOverride;

	public bool SensorsEnabled => false;

	public bool SoftwareOverride
	{
		get
		{
			return m_softwareOverride;
		}
		set
		{
			m_softwareOverride = value;
			IPersistentMap map = ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap();
			map.SetValue("SensorSoftwareOverride", m_softwareOverride);
		}
	}

	public void Dispose()
	{
	}

	public DoorSensorResult Query()
	{
		return DoorSensorResult.Ok;
	}

	public DoorSensorResult QueryStateForDisplay()
	{
		return DoorSensorResult.Ok;
	}

	internal UnconfiguredDoorSensorService()
	{
		IPersistentMap map = ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap();
		m_softwareOverride = map.GetValue("SensorSoftwareOverride", def: false);
	}
}
