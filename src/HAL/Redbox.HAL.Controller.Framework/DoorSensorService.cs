using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DoorSensorService : IDoorSensorService, IDisposable
{
	private bool m_softwareOverride;

	private readonly IControlSystem ControlSystem;

	private readonly ICoreCommandExecutor Executor;

	private readonly object m_queryLock = new object();

	public bool SensorsEnabled => !SoftwareOverride;

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
		lock (m_queryLock)
		{
			return (!SoftwareOverride) ? RawQuery() : DoorSensorResult.Ok;
		}
	}

	public DoorSensorResult QueryStateForDisplay()
	{
		lock (m_queryLock)
		{
			return SoftwareOverride ? DoorSensorResult.SoftwareOverride : RawQuery();
		}
	}

	internal DoorSensorService()
	{
		IPersistentMap map = ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap();
		m_softwareOverride = map.GetValue("SensorSoftwareOverride", def: false);
		Executor = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
		ControlSystem = ServiceLocator.Instance.GetService<IControlSystem>();
	}

	private DoorSensorResult RawQuery()
	{
		CoreResponse coreResponse = Executor.ExecuteControllerCommand(CommandType.AuxSensorsRead);
		if (coreResponse.CommError)
		{
			return DoorSensorResult.AuxReadError;
		}
		if (!coreResponse.IsSensorSet(AuxSensor.QlmDown))
		{
			ControlSystem.LogInputs(ControlBoards.Aux, LogEntryType.Error);
			return DoorSensorResult.FrontDoor;
		}
		return DoorSensorResult.Ok;
	}
}
