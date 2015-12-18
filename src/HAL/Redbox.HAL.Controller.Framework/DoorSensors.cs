using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

public sealed class DoorSensors
{
	private bool m_initialized;

	private bool m_softwareOverride;

	private readonly IPersistentMap DataStore;

	private readonly object m_queryLock = new object();

	public static DoorSensors Instance => Singleton<DoorSensors>.Instance;

	public bool SensorsEnabled
	{
		get
		{
			if (ControllerConfiguration.Instance.IsVMZMachine)
			{
				return !SoftwareOverride;
			}
			return false;
		}
	}

	public bool SoftwareOverride
	{
		get
		{
			return m_softwareOverride;
		}
		set
		{
			m_softwareOverride = value;
			DataStore.SetValue("SensorSoftwareOverride", m_softwareOverride);
		}
	}

	public DoorSensorResult Query()
	{
		lock (m_queryLock)
		{
			return (ControllerConfiguration.Instance.IsVMZMachine && !SoftwareOverride) ? RawQuery() : DoorSensorResult.Ok;
		}
	}

	public DoorSensorResult QueryStateForDisplay()
	{
		lock (m_queryLock)
		{
			return SoftwareOverride ? DoorSensorResult.SoftwareOverride : RawQuery();
		}
	}

	internal void Initialize()
	{
		if (!m_initialized)
		{
			m_initialized = true;
			if (!ControllerConfiguration.Instance.IsVMZMachine)
			{
				LogHelper.Instance.Log("Door sensors are not configured.");
			}
			else if (SoftwareOverride)
			{
				LogHelper.Instance.Log("** WARNING **: Door sensors are configured, however a software override in place.");
				LogHelper.Instance.Log("** WARNING **: Kiosk is operating without door sensors.");
			}
			else
			{
				LogHelper.Instance.Log("Door sensors are configured.");
			}
		}
	}

	private DoorSensorResult RawQuery()
	{
		CoreResponse coreResponse = ControlSystem.Instance.ExecuteControllerCommand(CommandType.AuxSensorsRead);
		if (coreResponse.CommunicationError)
		{
			return DoorSensorResult.AuxReadError;
		}
		if (!coreResponse.IsSensorSet(AuxSensor.QlmDown))
		{
			ControlSystem.Instance.LogInputs(ControlBoards.Aux, LogEntryType.Error);
			return DoorSensorResult.FrontDoor;
		}
		return DoorSensorResult.Ok;
	}

	private DoorSensors()
	{
		DataStore = StoreManager.Instance.GetDataStore();
		m_softwareOverride = DataStore.GetValue("SensorSoftwareOverride", def: false);
	}
}
