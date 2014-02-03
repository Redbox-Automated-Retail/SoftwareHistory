using System;
using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class DoorSensors
{
	private const int FrontDoorBit = 0;

	private const int RearDoorBit = 1;

	private DoorSensorQueryMode? m_queryMode = null;

	private bool? m_softwareOverride = null;

	private object m_queryLock = new object();

	public static DoorSensors Instance => Singleton<DoorSensors>.Instance;

	public object Barrier => m_queryLock;

	public bool SensorsEnabled
	{
		get
		{
			if (QueryMode != DoorSensorQueryMode.None)
			{
				return !SoftwareOverride;
			}
			return false;
		}
		internal set
		{
			QueryMode = (value ? DoorSensorQueryMode.AuxFrontOnly : DoorSensorQueryMode.None);
		}
	}

	public bool SoftwareOverride
	{
		get
		{
			if (m_softwareOverride.HasValue)
			{
				return m_softwareOverride.Value;
			}
			bool value = SecretOptions.Instance.GetValue("SensorSoftwareOverride", defValue: false);
			m_softwareOverride = value;
			return m_softwareOverride.Value;
		}
		set
		{
			LogHelper.Instance.Log("The door sensors have an override value: {0}", value);
			m_softwareOverride = value;
			SecretOptions.Instance.SetValue("SensorSoftwareOverride", m_softwareOverride.Value);
		}
	}

	private DoorSensorQueryMode QueryMode
	{
		get
		{
			if (m_queryMode.HasValue)
			{
				return m_queryMode.Value;
			}
			string value = SecretOptions.Instance.GetValue("DoorSensors", DoorSensorQueryMode.None.ToString());
			if (string.IsNullOrEmpty(value))
			{
				m_queryMode = DoorSensorQueryMode.None;
				return m_queryMode.Value;
			}
			try
			{
				m_queryMode = (DoorSensorQueryMode)Enum.Parse(typeof(DoorSensorQueryMode), value, ignoreCase: true);
			}
			catch (ArgumentException)
			{
				m_queryMode = DoorSensorQueryMode.None;
			}
			return m_queryMode.Value;
		}
		set
		{
			m_queryMode = value;
			SecretOptions.Instance.SetValue("DoorSensors", m_queryMode.ToString());
		}
	}

	public DoorSensorResult Query()
	{
		lock (Barrier)
		{
			if (SoftwareOverride || QueryMode == DoorSensorQueryMode.None)
			{
				return DoorSensorResult.Ok;
			}
			return QueryAux();
		}
	}

	public string[] QueryStateForDisplay()
	{
		lock (Barrier)
		{
			List<string> list = new List<string>();
			if (SoftwareOverride)
			{
				list.Add("OVERRIDE IN PLACE.");
				return list.ToArray();
			}
			CoreResponse coreResponse = CoreController.ExecuteSingleControllerCommand(CommandType.AuxSensorsRead);
			if (coreResponse.CommunicationError)
			{
				list.Add("AUX board not responsive.");
				return list.ToArray();
			}
			list.Add((!coreResponse.IsBitSet(0)) ? "FRONT DOOR OPEN" : "FRONT DOOR CLOSED");
			if (DoorSensorQueryMode.AuxFrontOnly == QueryMode)
			{
				return list.ToArray();
			}
			list.Add((!coreResponse.IsBitSet(1)) ? "REAR DOOR OPEN" : "REAR DOOR CLOSED");
			return list.ToArray();
		}
	}

	private DoorSensorResult QueryArcus()
	{
		using MotionController motionController = MotionController.GetController();
		return (motionController.ControllerErrorStatus(Axis.XY) != MotionResult.Success) ? DoorSensorResult.FrontDoor : DoorSensorResult.Ok;
	}

	private DoorSensorResult QueryAux()
	{
		CoreResponse coreResponse = CoreController.ExecuteSingleControllerCommand(CommandType.AuxSensorsRead);
		if (coreResponse.CommunicationError)
		{
			return DoorSensorResult.AuxReadError;
		}
		if (!coreResponse.IsBitSet(0))
		{
			AuxBoard.Instance.LogInputs(LogEntryType.Error);
			return DoorSensorResult.FrontDoor;
		}
		if (DoorSensorQueryMode.AuxFrontOnly == QueryMode)
		{
			return DoorSensorResult.Ok;
		}
		if (coreResponse.IsBitSet(1))
		{
			return DoorSensorResult.Ok;
		}
		return DoorSensorResult.RearDoor;
	}

	private DoorSensors()
	{
		string arg = QueryMode.ToString();
		if (SoftwareOverride)
		{
			LogHelper.Instance.Log($"** WARNING **: Door sensors are configured for mode {arg}; however, a software override in place.");
			LogHelper.Instance.Log("** WARNING **: Kiosk is operating without door sensors.");
		}
		else
		{
			LogHelper.Instance.Log($"Door sensors configured for mode {arg}");
		}
	}
}
