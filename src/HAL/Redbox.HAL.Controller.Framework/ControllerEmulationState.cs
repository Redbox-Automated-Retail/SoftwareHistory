using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Redbox.HAL.Configuration;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class ControllerEmulationState : EmulationStateConfig
{
	public readonly CircularList<bool> ExpectedIsPickerFullResult = new CircularList<bool>();

	private readonly string DataDirectory;

	private readonly string BackingFile;

	private Location m_expectedLookupResult;

	private List<string> m_vendedIds = new List<string>();

	public static ControllerEmulationState Instance => Singleton<ControllerEmulationState>.Instance;

	public string[] ExpectedVersions { get; set; }

	public GetResult ExpectedGetResult { get; set; }

	public PutResult ExpectedPutResult { get; set; }

	public QlmMotionResult ExpectedEngageQlmResult { get; set; }

	public bool ExpectedQlmHaltResult { get; set; }

	public bool ExpectedQlmLockResult { get; set; }

	public bool ExpectedQlmUnLockResult { get; set; }

	public bool ExpectedGripperPushResult { get; set; }

	public GripperPeekResult ExpectedGripperPeekResult { get; set; }

	public bool ExpectedSetVendDoorResult { get; set; }

	public bool ExpectedIsTrackOpenResult { get; set; }

	public string ExpectedPreviousIdResult { get; set; }

	public QlmMotionResult ExpectedDisengageQlmResult { get; set; }

	public MotionResult ExpectedHomeXResult { get; set; }

	public MotionResult ExpectedHomeYResult { get; set; }

	public bool ExpectedSetGripperArmResult { get; set; }

	public bool ExpectedIsTrackClosedResult { get; set; }

	public bool ExpectedIsMotorErroredResult { get; set; }

	public bool ExpectedIsGripperClearResult { get; set; }

	public MotionResult ExpectedMoveToResult { get; set; }

	public bool ExpectedSetGripperFingerResult { get; set; }

	public bool ExpectedIsVendDoorClosedResult { get; set; }

	public bool ExpectedRollerToPositionResult { get; set; }

	public bool ExpectedRollerOutResult { get; set; }

	public bool ExpectedControllerResetResult { get; set; }

	public bool ExpectedRollerInResult { get; set; }

	public bool ExpectedStopRollerResult { get; set; }

	public QlmStatus ExpectedGetQlmStatusResult { get; set; }

	public MotionResult ExpectedMoveToVendResult { get; set; }

	public MotionResult ExpectedMoveAbsoluteResult { get; set; }

	public bool ExpectedIsAtVendDoorLocationResult { get; set; }

	public bool[] ExpectedIsAuxSensorBlocked { get; private set; }

	public bool ExpectedWaitForAuxSensorsToBlockResult { get; set; }

	public bool ExpectedWaitForAuxSensorsToClearResult { get; set; }

	public bool[] ExpectedIsPickerSensorBlocked { get; private set; }

	public QlmDoorStatus ExpectedGetQlmDoorStatusResult { get; set; }

	public bool ExpectedWaitForPickerSensorsToBlockResult { get; set; }

	public bool ExpectedWaitForPickerSensorsToClearResult { get; set; }

	public bool ExpectedQlmInBayStatus { get; set; }

	public string ExpectedSetSlotIdPreviousIdResult { get; set; }

	public bool ExpectedSetSlotIdIsDuplicateInventoryResult { get; set; }

	public Location ExpectedLookupResult => m_expectedLookupResult;

	public void Reset()
	{
		ExpectedIsTrackClosedResult = true;
		ExpectedIsGripperClearResult = true;
		ExpectedIsVendDoorClosedResult = true;
		ExpectedGetResult = new GetResult();
		ExpectedPutResult = new PutResult();
		ExpectedVersions = new string[3] { "H101 Emulated controller board.", "H001 Emulated controller board.", "H002 Emulated controller board." };
		ExpectedIsAuxSensorBlocked = new bool[12];
		ExpectedHomeYResult = MotionResult.Success;
		ExpectedHomeXResult = MotionResult.Success;
		ExpectedIsPickerSensorBlocked = new bool[6];
		ExpectedMoveToResult = MotionResult.Success;
		ExpectedGetQlmStatusResult = QlmStatus.Empty;
		ExpectedMoveToVendResult = MotionResult.Success;
		ExpectedMoveAbsoluteResult = MotionResult.Success;
		ExpectedGetQlmDoorStatusResult = QlmDoorStatus.Closed;
		ExpectedIsPickerFullResult.Clear();
		ExpectedIsPickerFullResult.AddRange(new bool[2] { false, true });
		QlmMotionResult expectedDisengageQlmResult = (ExpectedEngageQlmResult = QlmMotionResult.Success);
		ExpectedDisengageQlmResult = expectedDisengageQlmResult;
		ExpectedGripperPeekResult = GripperPeekResult.Empty;
		ExpectedSetGripperArmResult = false;
		ExpectedQlmInBayStatus = true;
		m_vendedIds = Load();
	}

	protected override object GetEmulated(string name)
	{
		if (string.Compare(name, "ExpectedVendedDiscIDs", ignoreCase: true) == 0)
		{
			string result = m_vendedIds[0];
			m_vendedIds.RemoveAt(0);
			Flush();
			return result;
		}
		return null;
	}

	protected override bool ProcessOption(string name, object[] value)
	{
		if (string.Compare("ExpectedIsPickerFullResult.Set", name, ignoreCase: true) == 0)
		{
			ExpectedIsPickerFullResult.Clear();
			for (int i = 0; i < value.Length; i++)
			{
				bool item = (bool)value[i];
				ExpectedIsPickerFullResult.Add(item);
			}
			return true;
		}
		if (string.Compare("ExpectedLookupResult.Set", name, ignoreCase: true) == 0)
		{
			int deck = (int)value[0];
			int slot = (int)value[1];
			m_expectedLookupResult = new Location(deck, slot);
			return true;
		}
		if (string.Compare("ExpectedVendedDiscIDs.Set", name, ignoreCase: true) == 0)
		{
			for (int j = 0; j < value.Length; j++)
			{
				string item2 = (string)value[j];
				m_vendedIds.Add(item2);
			}
			Flush();
			return true;
		}
		return false;
	}

	private ControllerEmulationState()
	{
		string path = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "data");
		DataDirectory = Path.Combine(path, "SimulatedData");
		BackingFile = Path.Combine(DataDirectory, "SimulatedVendData.ser");
		CheckOrCreateBackingStoreDirectory(log: true);
		Reset();
	}

	private List<string> Load()
	{
		List<string> list = new List<string>();
		if (!CheckOrCreateBackingStoreDirectory(log: false))
		{
			return list;
		}
		if (!File.Exists(BackingFile))
		{
			LogHelper.Instance.Log("There is not a simulated vend stack to deserialize.");
			return list;
		}
		try
		{
			using FileStream serializationStream = File.OpenRead(BackingFile);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			list = (List<string>)binaryFormatter.Deserialize(serializationStream);
			return list;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unable to deserialize simulation vended stack.", e);
			list.Clear();
			return list;
		}
	}

	private void Flush()
	{
		if (!CheckOrCreateBackingStoreDirectory(log: false))
		{
			return;
		}
		using FileStream serializationStream = File.OpenWrite(BackingFile);
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		binaryFormatter.Serialize(serializationStream, m_vendedIds);
	}

	private bool CheckOrCreateBackingStoreDirectory(bool log)
	{
		if (!Directory.Exists(DataDirectory))
		{
			if (log)
			{
				LogHelper.Instance.Log("simvend directory doesn't exist; creating.", LogEntryType.Debug);
			}
			try
			{
				Directory.CreateDirectory(DataDirectory);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Unable to create simulatedvend directory.", e);
				return false;
			}
		}
		return true;
	}
}
