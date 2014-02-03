using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class ControllerConfiguration : XmlConfiguration
{
	private string m_dataDirectory;

	public static ControllerConfiguration Instance => Singleton<ControllerConfiguration>.Instance;

	[CustomEditor("(Decks Collection)", GetMethodName = "ConvertDeckConfigurationToXml", SetMethodName = "UpdateDeckConfigurationFromXml")]
	[Category("Configuration")]
	[DisplayName("Decks")]
	[Description("The physical description of the decks for the host of the connected HAL Service.")]
	public int DecksCount => DecksManager.Instance.DeckCount;

	[Description("The revision of hardware that HAL Service controls.  Revision A machines have an Arcus motion controller and original AUX and picker controllers.  Revision B machines have newer picker controllers.  Revision C machines have a replacement motion controller and updated AUX and picker controllers.")]
	[DisplayName("Machine Revision")]
	[ReadOnly(true)]
	[Category("Configuration")]
	public string Revision { get; internal set; }

	[Description("The amount of time the gripper will overrun the limit switch beforing raising a timeout.")]
	[DisplayName("Gripper Push Timeout")]
	[Category("Behavior")]
	public int PushTime { get; set; }

	[DisplayName("Gripper Extend Timeout")]
	[Category("Behavior")]
	[Description("The amount of time the gripper will extend the arm before timing out; used when putting a DVD into the slot.")]
	public int RollInExtendTime { get; set; }

	[DisplayName("Test Extend Timeout")]
	[Description("The amount of time the gripper will overrun the limit switch beforing raising a timeout while testing for a DVD in the slot.")]
	[Category("Behavior")]
	public int TestExtendTime { get; set; }

	[Category("Behavior")]
	[DisplayName("Vend Position Receive Offset")]
	[Description("The amount of backoff to back off when receiving an item from the vend position.")]
	public int? VendPositionReceiveOffset { get; set; }

	[Description("If set to Yes, external audio speakers are enabled.  The default value is No.")]
	[Category("Behavior")]
	[DisplayName("External Audio")]
	public bool AudioOn { get; set; }

	[Description("Controls how long (in milliseconds) HAL waits for the Arcus controller to fill its internal buffer before reading it.")]
	[DisplayName("Arcus pre-read wait")]
	[Category("Behavior")]
	public int ArcusReadWait { get; set; }

	[DisplayName("Y Position for Vend")]
	[Description("Known as the vend position, this is the point on the Y axis that is equal to the vend door, allowing the picker to vend and return disks. This must be a negative integer value.")]
	[Category("Configuration")]
	public int VendYPosition { get; set; }

	[Category("Behavior")]
	[DisplayName("Y drop back for HOMEY")]
	[Description("The amount of back off movement on the Y axis during a HOMEY instruction.  This value should be used only on older machines where the upper Y axis limit switch is too close to the Y axis home switch.")]
	public int? HomeYDropBack { get; set; }

	[Description("If set to Yes (default), the vend and return scripts will use the auxiliary sensors to determine if the vend door is clear before closing.")]
	[DisplayName("Use vend door sensors?")]
	[Category("Behavior")]
	public bool UseDoorSensor { get; set; }

	[Category("Behavior")]
	[DisplayName("Attempts to put an item away.")]
	[Description("The number of attempts to try and put something in the picker away.")]
	public int PutAwayItemAttempts { get; set; }

	[ValidValueListProvider("GetModes")]
	[DisplayName("Controller Mode")]
	[Category("Configuration")]
	[Description("The mode under which the controller driver runs.  If set to Emulated, all hardware instructions will be emulated based on the configured emulation state.")]
	public ControllerMode Mode { get; set; }

	[Description("The number of times the picker will attempt to pull the disk from the slot.")]
	[Category("Behavior")]
	[DisplayName("Number of Pulls")]
	public int NumberOfPulls { get; set; }

	[Description("The COM port to which to which the motion controller board is connected.")]
	[ValidValueListProvider("GetComPorts")]
	[DisplayName("Motion Controller Port Name")]
	[Category("Configuration")]
	public string MotionControllerPortName { get; set; }

	[DisplayName("Motion Controller Timeout (ms)")]
	[Description("The read timeout from the Motion Controller COM Port in milliseconds.")]
	[Category("Configuration")]
	public int MotionControllerTimeout { get; private set; }

	[Category("Configuration")]
	[DisplayName("Motion Controller Type")]
	[ValidValueListProvider("GetMotionControllerTypes")]
	[Description("Indicates the type of motion controller board installed on the host.")]
	public MotionControllerType MotionControllerType { get; set; }

	[Description("The X axis motor gear configuration.  The configuration of this property and its children affect the X Axis Speed properties.")]
	[ExcludeType]
	[Category("Configuration")]
	[DisplayName("X Motor Gear")]
	[Recurse]
	public MotorGear GearX { get; private set; }

	[Recurse]
	[Category("Configuration")]
	[Description("The Y axis motor gear configuration.  The configuration of this property and its children affect the Y Axis Speed properties.")]
	[DisplayName("Y Motor Gear")]
	[ExcludeType]
	public MotorGear GearY { get; private set; }

	[Category("Behavior")]
	[DisplayName("HOMEX Method")]
	[ValidValueListProvider("GetXHomeMethods")]
	[ExcludeType]
	[Description("The method the HOMEX instruction will use to determine if the home position has been reached.")]
	public XHomeMethod XHomeMethod { get; set; }

	[Description("The COM port the main, picker, auxiliary, and alternate motion controller boards are connected to.")]
	[DisplayName("Controller Port Name")]
	[ValidValueListProvider("GetComPorts")]
	[Category("Configuration")]
	public string ControllerPortName { get; set; }

	[Description("The speed of the X axis for the HOMEX instruction.")]
	[Category("Configuration")]
	[Recurse]
	[ExcludeType]
	[DisplayName("X Axis Initialization Speed")]
	public MotorSpeed InitAxisXSpeed { get; private set; }

	[ExcludeType]
	[DisplayName("Y Axis Initialization Speed")]
	[Description("The speed of the Y axis for the HOMEY instruction.")]
	[Category("Configuration")]
	[Recurse]
	public MotorSpeed InitAxisYSpeed { get; private set; }

	[Recurse]
	[Description("The speed of the Y axis for the MOVE instruction.")]
	[ExcludeType]
	[Category("Configuration")]
	[DisplayName("Y Axis Speed")]
	public MotorSpeed MoveAxisYSpeed { get; private set; }

	[DisplayName("X Axis Speed")]
	[Description("The speed of the X axis for the MOVE instruction.")]
	[Category("Configuration")]
	[Recurse]
	[ExcludeType]
	public MotorSpeed MoveAxisXSpeed { get; private set; }

	[Description("The speed of the combined X and Y axis for the MOVE instruction.")]
	[Recurse]
	[ExcludeType]
	[Category("Configuration")]
	[DisplayName("XY Axis Speed")]
	public MotorSpeed MoveAxisXYSpeed { get; private set; }

	[Description("If value is greater than 0, wait value ms after turning on the ringlight during sync.")]
	[Category("Configuration")]
	[DisplayName("Sync Ringlight Warmup Time")]
	public int SyncRinglightWarmupTime { get; set; }

	[DisplayName("Move Timeout")]
	[Category("Behavior")]
	[Description("The timeout (in milliseconds) for an ARCUS move.")]
	public int MoveTimeout { get; set; }

	[Category("Behavior")]
	[Description("If set to true, will time-extend the fingers using the QlmExtendTime, ignoring the extend sensor.")]
	[DisplayName("Qlm Timed Extend")]
	public bool QlmTimedExtend { get; set; }

	[Category("Behavior")]
	[DisplayName("Timeout for a gripper extend/retract operation.")]
	[Description("The timeout value for a gripper extend/retract operation.")]
	public int GripperArmExtendRetractTimeout { get; set; }

	[DisplayName("Validate Position After Move")]
	[Description("If set to Yes, validate the position after a move.")]
	[Category("Configuration")]
	public bool PrintEncoderPositionAfterMove2 { get; set; }

	[Description("If set to true ( the default ), PUT will perform an extra gripper push to seat the disk.")]
	[Category("Behavior")]
	[DisplayName("Additional PUT push.")]
	public bool AdditionalPutPush { get; set; }

	[Description("If set to true ( the default ), qlm/thin/qlm-thin will reboot when detecting an Arcus problem after engage/disengage.")]
	[DisplayName("Reboot Kiosk During QLM")]
	[Category("Behavior")]
	public bool RebootKioskDuringQlm { get; set; }

	[Description("If set to true, clean/thin will dump the zone after the job.")]
	[Category("Behavior")]
	[DisplayName("Dump VMZ State")]
	public bool DumpVMZState { get; set; }

	[Category("Behavior")]
	[Description("If set to true ( the default ), after a QLM engage/disengage, if HAL can't communicate with the Arcus, it will try to reset the DIEs.")]
	[DisplayName("Correct Motion Controller Communication")]
	public bool CorrectMotionControllerCommunication { get; set; }

	[Description("The number of times, during a VEND, to poll for a taken disk.")]
	[Category("Behavior")]
	[DisplayName("Vend Disk Poll Count")]
	public int VendDiskPollCount { get; set; }

	[Description("The number of errors encountered before exiting a sync.")]
	[Category("Behavior")]
	[DisplayName("Sync Error Threshold")]
	public int SyncErrorThreshold { get; set; }

	[Category("Behavior")]
	[DisplayName("Reject At Door Attempts")]
	[Description("The number of attempts to reject a disk at the vend door.")]
	public int RejectAtDoorAttempts { get; set; }

	[Description("The number of errors encountered before exiting a sync.")]
	[Category("Behavior")]
	[DisplayName("Error Sync On Camera Failure")]
	public bool ErrorSyncOnCameraFailure { get; set; }

	[Description("If true, will try to move the vend door to the appropriate sensor on the AUX board.")]
	[Category("Behavior")]
	[DisplayName("Move Vend Door To AUX Sensor")]
	public bool MoveVendDoorToAuxSensor { get; set; }

	[Category("Behavior")]
	[Description("If true, query the current position before moving; if at the target position, wont' request a move.")]
	[DisplayName("Query Position For Vend Move")]
	public bool QueryPositionForVendMove { get; set; }

	[Description("If true, will try to move the vend door to the appropriate sensor on the AUX board.")]
	[Category("Behavior")]
	[DisplayName("QLM Approach Offset")]
	public int QlmApproachOffset { get; set; }

	[Description("If true, will try to move the vend door to the appropriate sensor on the AUX board.")]
	[DisplayName("QLM Y Offset")]
	[Category("Behavior")]
	public int QlmYOffset { get; set; }

	[Description("The amount of time the gripper will overrun the limit switch beforing raising a timeout while doing a grab on the QLM.")]
	[Category("Behavior")]
	[DisplayName("QLM Extend Timeout")]
	public int QlmExtendTime { get; set; }

	[Category("Behavior")]
	[Description("If set to true, will perform an extend, close, open and retract to help center the disc in the wide slot.")]
	[DisplayName("Settle Disc In Wide Slot")]
	public bool SettleDiscInWideSlot { get; set; }

	[Category("Configuration")]
	[Description("The time, in milliseconds, for the init job to wait for the ports to come up.")]
	[DisplayName("COM Port Init Sleep Time")]
	public int COMPortInitSleepTime { get; set; }

	[Description("If set to true, allows the program cache to accept overwrites of programs.")]
	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Allow Program Cache Replacement")]
	public bool AllowProgramCacheReplacement2 { get; set; }

	[DisplayName("Check Gripper Arm Sensors On Move")]
	[Description("If set to true, will check both the forward and retract sensors if they're both set, and if they are, won't move.")]
	[Category("Behavior")]
	public bool CheckGripperArmSensorsOnMove { get; set; }

	[Category("Behavior")]
	[Description("If set to true, will check if the gripper is in the rent position, and if not, will move it to Rent.")]
	[DisplayName("Gripper Rent On Move")]
	public bool GripperRentOnMove { get; set; }

	[Description("If set to true, mark duplicate barcodes as UNKNOWN.")]
	[Category("Behavior")]
	[DisplayName("Mark Duplicate Unknown")]
	public bool MarkDuplicatesUnknown { get; set; }

	[Category("Behavior")]
	[Description("If set to true, when a duplicate is recognized, will mark the original barcode as UNKNOWN.")]
	[DisplayName("Mark Original Matrix Unknown")]
	public bool MarkOriginalMatrixUnknown { get; set; }

	[Description("If set to true, will do a slot test, and if the slot is full, will rotate the drum 5 slots during an unknown removal if unable to grab a disk.")]
	[Category("Behavior")]
	[DisplayName("Rotate Drum During Unknown Removal")]
	public bool RotateDrumDuringUnknownRemoval2 { get; set; }

	[Category("Behavior")]
	[Description("If set to true, will leave a CAMERA CAPTURE result during a return job if duplicates are detected.")]
	[DisplayName("Leave Duplicate Result In Return")]
	public bool LeaveDuplicateResultInReturn { get; set; }

	[Category("Behavior")]
	[Description("If set to true, perform a PEEK (test slot) on the slot if GET returns SLOTEMPTY.")]
	[DisplayName("Test Slot On Empty")]
	public bool TestSlotOnEmpty { get; set; }

	[Description("The count for errors before the slot is marked UNKNOWN.")]
	[Category("Behavior")]
	[DisplayName("Mark Location Unknown Threshold")]
	public int MarkLocationUnknownThreshold { get; set; }

	[Category("Behavior")]
	[DisplayName("Track Push Out Failures")]
	[Browsable(false)]
	[Description("If set to true, will fail a vend after 2 push out tries show any sensors 1-4 lit.")]
	public bool TrackPushOutFailures { get; set; }

	[Description("If set to true, will capture errors with certain operations.")]
	[Category("Behavior")]
	[DisplayName("Count Hardware Errors ")]
	public bool CountHardwareErrors { get; set; }

	[Category("Behavior")]
	[Description("If set to true, will track problems with slots.")]
	[DisplayName("Track Problem Locations")]
	public bool TrackProblemLocations { get; set; }

	[Category("Behavior")]
	[Description("If set to true, will push to sensor as opposed to motor sleep.")]
	[DisplayName("Gripper To Sensor On Put")]
	public bool GripperToSensorOnPut { get; set; }

	[Description("If greater than 0, will spin for the configured time in ms after clearing sensor 4 during a vend disk.")]
	[Category("Configuration")]
	[DisplayName("Push Out Sleep Time")]
	public int PushOutSleepTime2 { get; set; }

	[DisplayName("AggressiveClearPickerOnPut")]
	[Category("Behavior")]
	[Description("If true, try to clear the picker more aggressively during a PUT.")]
	public bool AggressiveClearPickerOnPut { get; set; }

	[DisplayName("EmptyStuckAlertThreshold")]
	[Category("Configuration")]
	[Description("If Empty or stuck count exceeds this number during vend in a 24 hour period, generate a result that should trigger an alert.")]
	public int EmptyStuckAlertThreshold { get; set; }

	[Category("Behavior")]
	[DisplayName("Alert For Empty Stuck")]
	[Description("If true, will leave a RESULT code for alert generation when the empty/stuck counter has exceeded its threshold.")]
	public bool AlertForEmptyStuck { get; set; }

	[Category("Behavior")]
	[Description("If true, will test if the camera is generating images, and if not, will try and reset it.")]
	[DisplayName("Fix Camera On Init")]
	public bool FixCameraOnInit2 { get; set; }

	[Description("If true, tells the hardware-status job to include the init status of the motion controller.")]
	[Category("Behavior")]
	[DisplayName("Validate Controller Home Status")]
	public bool ValidateControllerHomeStatus { get; set; }

	[Category("Configuration")]
	[Description("In minutes, the time before kiosk reboot that the scheduler prevents jobs from running.")]
	[DisplayName("Reboot Execution Prevention Window")]
	public int RebootExecutionPreventionWindow { get; set; }

	[Description("The number of slots the kiosk must not to try and file disks to.")]
	[Category("Configuration")]
	[DisplayName("Return Slot Buffer")]
	public int ReturnSlotBuffer { get; set; }

	[DisplayName("Quick Return Power Off Window")]
	[Category("Configuration")]
	[Description("The number of slots the kiosk must not to try and file disks to.")]
	public string QuickReturnPowerOffWindow { get; set; }

	[DisplayName("Quick Return Port")]
	[Description("The COM port on which the Quick return device is configured.")]
	[Category("Configuration")]
	public string QuickReturnComPort { get; set; }

	[Browsable(false)]
	[DisplayName("Widen Arcus Tolerance")]
	[Description("If true, will give the arcus a larger tolerance window.")]
	[Category("Behavior")]
	public bool WidenArcusTolerance { get; set; }

	[Category("Behavior")]
	[Description("If true, will take the disk if the barcode can't be read.")]
	[DisplayName("Take Invalid Disk On Return")]
	public bool TakeInvalidDiskOnReturn { get; private set; }

	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Enable Door Check In Status")]
	[Description("If set to true, will check the door sensors during the hardware status job.")]
	public bool EnableDoorCheckInStatus { get; set; }

	[Description("If set to true, will enable the touch screen monitor.")]
	[Category("Behavior")]
	[DisplayName("Enable Touch Screen Monitor")]
	[Browsable(false)]
	public bool EnableTouchScreenMonitor { get; set; }

	[Category("Behavior")]
	[Browsable(false)]
	[Description("If set to true, will establish the correct driver is loaded and the device is enabled.")]
	[DisplayName("Check Gen4 Camera On Init")]
	public bool CheckGen4CameraOnInit { get; set; }

	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Enable Clean VMZ Randomization")]
	[Description("If set to true, will randomize clean-vmz time from 1-3 AM.")]
	public bool EnableCleanVMZRandomization { get; set; }

	[Browsable(false)]
	public string KioskLogsPath
	{
		get
		{
			if (ControllerMode.Emulated != Mode)
			{
				return "c:\\Program Files\\Redbox\\KioskLogs";
			}
			return "c:\\bin\\KioskLogs";
		}
	}

	[Browsable(false)]
	public MerchandizingMode MerchandizingMode { get; private set; }

	[Browsable(false)]
	public string DataDirectory
	{
		get
		{
			if (string.IsNullOrEmpty(m_dataDirectory))
			{
				m_dataDirectory = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "data");
			}
			return m_dataDirectory;
		}
	}

	public void Initialize(ErrorList errors)
	{
		if (ControllerMode.Normal == Mode)
		{
			int secretValue = RegistryOptions.Instance.GetSecretValue<int>("SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store", "MaxSlots");
			Deck deck = DecksManager.Instance.GetDeck(1);
			if (secretValue != deck.NumberOfSlots)
			{
				errors.Add(Error.NewError("P005", "Machine configuration mismatch.", $"The store key shows {secretValue}, but the deck shows {deck.NumberOfSlots}"));
				return;
			}
		}
		DecksManager.Instance.LoadInventory();
		if (ControllerMode.Normal == Mode)
		{
			ConfigureMerchandizingMode(errors);
			return;
		}
		MerchandizingMode = MerchandizingMode.QLM;
		ESPHelper.Instance.Initialize();
	}

	public void Shutdown()
	{
		VMZ.Instance.Dispose();
		CountersDBManager.Instance.Close();
		SerialBoard.Instance.ClosePort(null);
		MotionController controller = MotionController.GetController();
		controller.ClosePort();
	}

	public void UpdateGamp(string backupPath)
	{
		List<List<int>> newData = new List<List<int>>();
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			List<int> list = new List<int> { deck.YOffset };
			Quadrant[] quadrants = deck.Quadrants;
			foreach (Quadrant quadrant in quadrants)
			{
				list.Add(quadrant.Offset);
			}
			newData.Add(list);
			return true;
		});
		GampBackupResult gampBackupResult = GampHelper.Instance.WriteSlotDataFile(newData, createBackup: true);
		if (!string.IsNullOrEmpty(backupPath) && Directory.Exists(backupPath))
		{
			try
			{
				string fileName = Path.GetFileName(gampBackupResult.BackupFile);
				File.Move(gampBackupResult.BackupFile, Path.Combine(backupPath, fileName));
			}
			catch
			{
			}
		}
	}

	public void DumpESP(bool dumpStore)
	{
		ESPHelper.Instance.Dump();
	}

	public List<Location> ComputeExcludedSlots()
	{
		List<Location> rv = new List<Location>();
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			for (int i = 1; i <= deck.NumberOfSlots; i++)
			{
				if (deck.IsExcluded(i))
				{
					rv.Add(new Location(deck.Number, i));
				}
			}
			return true;
		});
		return rv;
	}

	public bool IsBarcodeDuplicate(string id, out Location original)
	{
		original = null;
		if (Constants.IsKnownInventoryToken(id))
		{
			return false;
		}
		original = ControllerService.Instance.Lookup(id);
		return original != null;
	}

	public List<Location> GetUnknowns()
	{
		List<Location> rv = new List<Location>();
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			if (deck.IsQlmDeck)
			{
				return true;
			}
			List<Location> unknownLocations = deck.GetUnknownLocations();
			if (unknownLocations.Count > 0)
			{
				rv.AddRange(unknownLocations);
			}
			return true;
		});
		return rv;
	}

	public List<Location> GetEmptySlots()
	{
		List<Location> rv = new List<Location>();
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			if (deck.IsQlmDeck)
			{
				return true;
			}
			List<Location> emptyLocations = deck.GetEmptyLocations();
			if (emptyLocations.Count > 0)
			{
				rv.AddRange(emptyLocations);
			}
			return true;
		});
		return rv;
	}

	public int GetEmptyCount()
	{
		int count = 0;
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			if (!deck.IsQlmDeck)
			{
				count += deck.GetEmptyCount();
			}
			return true;
		});
		return count;
	}

	public void LogProblemLocations(TextWriter log, int threshold)
	{
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			for (int i = 1; i <= deck.NumberOfSlots; i++)
			{
				int emptyStuckCount = deck.GetEmptyStuckCount(i);
				if (emptyStuckCount >= threshold)
				{
					string value = string.Format("Empty/Stuck count for Deck = {0} Slot = {1} is {2} {3).", deck.Number, i, emptyStuckCount, (emptyStuckCount == 1) ? "instance" : "instances");
					log.WriteLine(value);
				}
			}
			return true;
		});
	}

	public static int GetSlotsFromGamp()
	{
		List<List<int>> list = ReadLegacySlotDataFile("C:\\gamp\\SlotData.dat");
		if (list.Count == 0)
		{
			return -1;
		}
		if (list[0].Count == 7)
		{
			return 90;
		}
		if (list[0][7] != 0)
		{
			return 72;
		}
		return 90;
	}

	public void Import(TextWriter logFile, ErrorList errors)
	{
		int? num = null;
		int? num2 = null;
		List<int> list = new List<int>();
		List<bool> list2 = new List<bool>();
		IDictionary<string, int?> dictionary = ReadLegacySystemDataFile("C:\\gamp\\SystemData.dat");
		if (dictionary.Count == 0)
		{
			errors.Add(Error.NewError("H321", "The SystemData.dat file was not found or had no properties.", "Check the C:\\gamp folder to ensure the file exists."));
			return;
		}
		if (dictionary.ContainsKey("PushTime"))
		{
			PushTime = dictionary["PushTime"].Value;
		}
		if (dictionary.ContainsKey("RollInExtendTime"))
		{
			RollInExtendTime = dictionary["RollInExtendTime"].Value;
		}
		if (dictionary.ContainsKey("TestExtendTime"))
		{
			TestExtendTime = dictionary["TestExtendTime"].Value;
		}
		if (dictionary.ContainsKey("PickExtendTime"))
		{
			QlmExtendTime = dictionary["PickExtendTime"].Value;
		}
		if (dictionary.ContainsKey("QLMaproch"))
		{
			QlmApproachOffset = dictionary["QLMaproch"].Value;
		}
		if (dictionary.ContainsKey("QLMZoffset"))
		{
			QlmYOffset = dictionary["QLMZoffset"].Value;
		}
		if (dictionary.ContainsKey("ReceivePositionOffSet"))
		{
			VendPositionReceiveOffset = dictionary["ReceivePositionOffSet"].Value;
		}
		if (dictionary.ContainsKey("ArcusComPort"))
		{
			MotionControllerPortName = string.Format("COM{0}", dictionary["ArcusComPort"]);
		}
		if (dictionary.ContainsKey("ControlComPort"))
		{
			ControllerPortName = string.Format("COM{0}", dictionary["ControlComPort"]);
		}
		if (dictionary.ContainsKey("VendPosition"))
		{
			VendYPosition = dictionary["VendPosition"] ?? (-86200);
		}
		if (dictionary.ContainsKey("ZHomeDropBack"))
		{
			HomeYDropBack = -dictionary["ZHomeDropBack"];
		}
		if (dictionary.ContainsKey("NumberOfPulls"))
		{
			NumberOfPulls = dictionary["NumberOfPulls"] ?? 2;
		}
		if (dictionary.ContainsKey("XhomeMethod"))
		{
			XHomeMethod = (XHomeMethod)(dictionary["XhomeMethod"] ?? 1);
		}
		if (dictionary.ContainsKey("SellThrughOffSet"))
		{
			num2 = dictionary["SellThrughOffSet"];
		}
		if (dictionary.ContainsKey("QLMDeckNumber"))
		{
			num = dictionary["QLMDeckNumber"];
		}
		if (dictionary.ContainsKey("RGearRatio") && dictionary.ContainsKey("RStepRes") && dictionary.ContainsKey("RPulseRatio") && dictionary.ContainsKey("REncoderRatio"))
		{
			GearX = new MotorGear(dictionary["RGearRatio"] ?? 10, dictionary["RPulseRatio"] ?? 398, dictionary["REncoderRatio"] ?? 114, dictionary["RStepRes"] ?? 8);
		}
		if (dictionary.ContainsKey("ZGearRatio") && dictionary.ContainsKey("ZStepRes"))
		{
			GearY = new MotorGear(dictionary["ZGearRatio"] ?? 20, 0, 0, dictionary["ZStepRes"] ?? 4);
		}
		if (dictionary.ContainsKey("InitAxis1LS") && dictionary.ContainsKey("InitAxis1HS") && dictionary.ContainsKey("MoveRAcc"))
		{
			InitAxisXSpeed = new MotorSpeed(dictionary["InitAxis1LS"] ?? 600, dictionary["InitAxis1HS"] ?? 3000, 300, GearX);
		}
		if (dictionary.ContainsKey("InitAxis2LS") && dictionary.ContainsKey("InitAxis2HS") && dictionary.ContainsKey("MoveZAcc"))
		{
			InitAxisYSpeed = new MotorSpeed(dictionary["InitAxis2LS"] ?? 800, dictionary["InitAxis2HS"] ?? 2400, 300, GearY);
		}
		if (dictionary.ContainsKey("MoveZHS") && dictionary.ContainsKey("MoveZAcc"))
		{
			MoveAxisYSpeed = new MotorSpeed(500, dictionary["MoveZHS"] ?? 15000, dictionary["MoveZAcc"] ?? 500, GearY);
		}
		if (dictionary.ContainsKey("MoveRHS") && dictionary.ContainsKey("MoveRAcc"))
		{
			MoveAxisXSpeed = new MotorSpeed(500, dictionary["MoveRHS"] ?? 15000, dictionary["MoveRAcc"] ?? 500, GearY);
		}
		if (dictionary.ContainsKey("MoveZRHS") && dictionary.ContainsKey("MoveZRAcc"))
		{
			MoveAxisXYSpeed = new MotorSpeed(500, dictionary["MoveZRHS"] ?? 15000, dictionary["MoveZRAcc"] ?? 500, GearY);
		}
		int num3 = 1;
		while (true)
		{
			string key = $"PlatterMaxSlots{num3++}";
			if (!dictionary.ContainsKey(key))
			{
				break;
			}
			list.Add(dictionary[key] ?? 90);
		}
		num3 = 1;
		while (true)
		{
			string key2 = $"DeckSellStat{num3++}";
			if (!dictionary.ContainsKey(key2))
			{
				break;
			}
			list2.Add((dictionary[key2] ?? 0) == 1);
		}
		List<List<int>> list3 = ReadLegacySlotDataFile("C:\\gamp\\SlotData.dat");
		if (list3.Count == 0)
		{
			errors.Add(Error.NewError("H321", "The SlotData.dat file was not found or had no data.", "Check the C:\\gamp folder to ensure the file exists."));
			return;
		}
		DecksManager.Instance.Clear();
		num3 = 1;
		Range[] array = new Range[5]
		{
			new Range(1, 1),
			new Range(1, 19),
			new Range(20, 40),
			new Range(41, 61),
			new Range(62, 80)
		};
		foreach (List<int> item in list3)
		{
			int num4 = ((num3 == num) ? 80 : list[num3 - 1]);
			List<Quadrant> list4 = new List<Quadrant>();
			for (int i = 1; i < item.Count && item[i] != 0; i++)
			{
				if (num.HasValue && num3 == num.Value && i < array.Length)
				{
					list4.Add(new Quadrant(item[i], array[i]));
				}
				else
				{
					list4.Add(new Quadrant(item[i]));
				}
			}
			decimal num5 = 166.6667m;
			if (num.HasValue && num3 == num.Value)
			{
				num5 = 177.7m;
			}
			bool isQlmDeck = num.HasValue && num3 == num.Value;
			DecksManager.Instance.Add(new Deck(num3, item[0], isQlmDeck, null, num4, (num4 == 72) ? 173.3m : num5, (num4 == 72) ? new int?(6) : ((int?)null), (num4 == 72) ? num2 : ((int?)null), list4.ToArray()));
			num3++;
		}
		MerchandizingMode = ((!num.HasValue || num.Value <= 0) ? MerchandizingMode.VMZ_Gen2 : MerchandizingMode.QLM);
		ConfigureMerchandizingModeOptions(logFile);
	}

	public string GetInventoryState()
	{
		using MemoryStream memoryStream = new MemoryStream();
		XmlTextWriter writer = new XmlTextWriter(memoryStream, Encoding.ASCII)
		{
			Formatting = Formatting.Indented
		};
		writer.WriteStartDocument();
		writer.WriteStartElement("inventory-items");
		DecksManager.Instance.ForAllDecksDo(delegate(Deck d)
		{
			d.WriteInventoryData(writer);
			return true;
		});
		List<DumpBinEntry> contents = DumpBin.Instance.GetContents();
		foreach (DumpBinEntry item in contents)
		{
			item.WriteElement(writer);
		}
		writer.WriteEndElement();
		writer.WriteEndDocument();
		writer.Flush();
		return Encoding.ASCII.GetString(memoryStream.ToArray());
	}

	public void ResetInventoryState(string xml, ErrorList errors)
	{
		List<DeckData> inventory = new List<DeckData>();
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			if (xmlDocument.DocumentElement == null)
			{
				errors.Add(Error.NewError("I001", "Invalid document.", "The specified document is invalid."));
				return;
			}
			XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("item");
			if (xmlNodeList == null)
			{
				errors.Add(Error.NewError("I001", "Invalid document.", "The specified document has no nodes."));
				return;
			}
			foreach (XmlNode item in xmlNodeList)
			{
				DeckData deckData = DeckData.FromXML(item, errors);
				if (deckData == null)
				{
					errors.Add(Error.NewError("I002", "Load error.", "Unable to load the deck from the XML."));
					return;
				}
				inventory.Add(deckData);
			}
			DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
			{
				List<DeckData> list = inventory.FindAll((DeckData each) => each.Deck == deck.Number);
				if (list.Count != deck.NumberOfSlots)
				{
					errors.Add(Error.NewError("P004", $"The slot count {list.Count} did not match the deck slot count for deck {deck.Number} ({deck.NumberOfSlots} slots).", "Ensure the inventory is valid."));
					LogHelper.Instance.Log($"Deck {deck.Number} expected {deck.NumberOfSlots} slots to be imported but received {list.Count}.", LogEntryType.Error);
					return true;
				}
				if (!deck.ImportInventory(list))
				{
					errors.Add(Error.NewError("P004", $"Unable to import inventory data for deck {deck.Number}.", "Ensure the inventory is valid."));
					return false;
				}
				return true;
			});
			if (errors.Count <= 0)
			{
				return;
			}
			string text = $"FailedInventoryImport-{IdHelper.GeneratePseudoUniqueId(8)}.xml";
			string arg = Path.Combine(DataDirectory, text);
			LogHelper.Instance.Log($"RESET INVENTORY Failed to import inventory; suspect inventory saved to {arg}", LogEntryType.Error);
			try
			{
				File.WriteAllText(text, xml);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Unable to write bogus inventory.", e);
			}
		}
		catch (Exception e2)
		{
			errors.Add(Error.NewError("P999", "An unhandled exception was raised in ControllerConfiguration.Load.", e2));
			LogHelper.Instance.Log("RESET INVENTORY Failed to import inventory.", e2, LogEntryType.Error);
		}
		finally
		{
			inventory.Clear();
		}
	}

	public string[] GetModes()
	{
		return new string[2]
		{
			ControllerMode.Normal.ToString(),
			ControllerMode.Emulated.ToString()
		};
	}

	public string[] GetXHomeMethods()
	{
		return new string[2]
		{
			XHomeMethod.Encoder.ToString(),
			XHomeMethod.LimitSwitch.ToString()
		};
	}

	public string[] GetMotionControllerTypes()
	{
		return new string[2]
		{
			MotionControllerType.Arcus.ToString(),
			MotionControllerType.NextGen.ToString()
		};
	}

	public void ConvertDeckConfigurationToXml(XmlWriter writer)
	{
		DecksManager.Instance.ToPropertyXml(writer);
	}

	public void UpdateDeckConfigurationFromXml(XmlNode propertyNode)
	{
		DecksManager.Instance.UpdateFromPropertyXml(propertyNode);
		ConfigureMerchandizingMode(new ErrorList());
	}

	public static bool VerifySlotData(string path)
	{
		List<List<int>> list = ReadLegacySlotDataFile(path);
		return list.Count > 0;
	}

	public int GetMachineEmptyCount()
	{
		int count = 0;
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			count += deck.GetEmptyCount();
			return true;
		});
		return count;
	}

	internal static IDictionary<string, int?> ReadLegacySystemDataFile(string path)
	{
		Dictionary<string, int?> dictionary = new Dictionary<string, int?>();
		if (!File.Exists(path))
		{
			return dictionary;
		}
		string[] array = File.ReadAllLines(path);
		string[] array2 = array;
		foreach (string text in array2)
		{
			string[] array3 = text.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if (array3.Length >= 2)
			{
				int? value = null;
				if (int.TryParse(array3[1], out var result))
				{
					value = result;
				}
				dictionary[array3[0]] = value;
			}
		}
		return dictionary;
	}

	internal static List<List<int>> ReadLegacySlotDataFile(string path)
	{
		List<List<int>> list = new List<List<int>>();
		if (!File.Exists(path))
		{
			return list;
		}
		int num = 1;
		bool flag = false;
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = File.ReadAllLines(path);
		string[] array2 = array;
		foreach (string text in array2)
		{
			List<int> list2 = new List<int>();
			int result;
			for (int j = 0; j < text.Length; j++)
			{
				if (char.IsWhiteSpace(text[j]) || text[j] == ',')
				{
					if (stringBuilder.Length > 0)
					{
						if (!int.TryParse(stringBuilder.ToString(), out result))
						{
							result = 0;
							LogHelper.Instance.Log($"The value {stringBuilder.ToString()} in slotdata.dat (line {num}) is invalid", LogEntryType.Error);
							flag = true;
						}
						else
						{
							list2.Add(result);
						}
						stringBuilder.Remove(0, stringBuilder.Length);
					}
				}
				else
				{
					stringBuilder.Append(text[j]);
				}
			}
			if (stringBuilder.Length > 0)
			{
				if (!int.TryParse(stringBuilder.ToString(), out result))
				{
					result = 0;
					LogHelper.Instance.Log($"The value {stringBuilder.ToString()} in slotdata.dat (line {num}) is invalid", LogEntryType.Error);
					flag = true;
				}
				else
				{
					list2.Add(result);
				}
				stringBuilder.Remove(0, stringBuilder.Length);
			}
			num++;
			if (list2.Count > 0)
			{
				list.Add(list2);
			}
		}
		if (flag)
		{
			list.Clear();
		}
		return list;
	}

	protected override void StoreProperties(XmlDocument document, ErrorList errors)
	{
		XmlElement documentElement = document.DocumentElement;
		SetSingleNodeValue(documentElement, "Controller/EnableCleanVMZRandomization", EnableCleanVMZRandomization);
		SetSingleNodeValue(documentElement, "Controller/CheckGen4CameraOnInit", CheckGen4CameraOnInit);
		SetSingleNodeValue(documentElement, "Controller/EnableTouchScreenMonitor", EnableTouchScreenMonitor);
		SetSingleNodeValue(documentElement, "Controller/EnableDoorCheckInStatus", EnableDoorCheckInStatus);
		SetSingleNodeValue(documentElement, "Controller/TakeInvalidDiskOnReturn", TakeInvalidDiskOnReturn);
		SetSingleNodeValue(documentElement, "Controller/QuickReturnComPort", QuickReturnComPort);
		SetSingleNodeValue(documentElement, "Controller/DumpVMZState", DumpVMZState);
		SetSingleNodeValue(documentElement, "Controller/QuickReturnPowerOffWindow", QuickReturnPowerOffWindow);
		SetSingleNodeValue(documentElement, "Controller/WidenArcusTolerance", WidenArcusTolerance);
		SetSingleNodeValue(documentElement, "Controller/SyncRinglightWarmupTime", SyncRinglightWarmupTime);
		SetSingleNodeValue(documentElement, "Controller/GripperToSensorOnPut", GripperToSensorOnPut);
		SetSingleNodeValue(documentElement, "Controller/QlmTimedExtend", QlmTimedExtend);
		SetSingleNodeValue(documentElement, "Controller/ValidateControllerHomeStatus", ValidateControllerHomeStatus);
		SetSingleNodeValue(documentElement, "Controller/RebootExecutionPreventionWindow", RebootExecutionPreventionWindow);
		SetSingleNodeValue(documentElement, "Controller/FixCameraOnInit2", FixCameraOnInit2);
		SetSingleNodeValue(documentElement, "Controller/TrackPushOutFailures", TrackPushOutFailures);
		SetSingleNodeValue(documentElement, "Controller/AlertForEmptyStuck", AlertForEmptyStuck);
		SetSingleNodeValue(documentElement, "Controller/EmptyStuckAlertThreshold", EmptyStuckAlertThreshold);
		SetSingleNodeValue(documentElement, "Controller/ReturnSlotBuffer", ReturnSlotBuffer);
		SetSingleNodeValue(documentElement, "Controller/AggressiveClearPickerOnPut", AggressiveClearPickerOnPut);
		SetSingleNodeValue(documentElement, "Controller/PushOutSleepTime2", PushOutSleepTime2);
		SetSingleNodeValue(documentElement, "Controller/TrackProblemLocations", TrackProblemLocations);
		SetSingleNodeValue(documentElement, "Controller/MarkLocationUnknownThreshold", MarkLocationUnknownThreshold);
		SetSingleNodeValue(documentElement, "Controller/TestSlotOnEmpty", TestSlotOnEmpty);
		SetSingleNodeValue(documentElement, "Controller/CaptureHardwareCounters", CountHardwareErrors);
		SetSingleNodeValue(documentElement, "Controller/LeaveDuplicateResultInReturn", LeaveDuplicateResultInReturn);
		SetSingleNodeValue(documentElement, "Controller/RotateDrumDuringUnknownRemoval2", RotateDrumDuringUnknownRemoval2);
		SetSingleNodeValue(documentElement, "Controller/MarkDuplicatesUnknown", MarkDuplicatesUnknown);
		SetSingleNodeValue(documentElement, "Controller/MarkOriginalMatrixUnknown", MarkOriginalMatrixUnknown);
		SetSingleNodeValue(documentElement, "Controller/CorrectMotionControllerCommunication", CorrectMotionControllerCommunication);
		SetSingleNodeValue(documentElement, "Controller/RebootKioskDuringQlm", RebootKioskDuringQlm);
		SetSingleNodeValue(documentElement, "Controller/COMPortInitSleepTime", COMPortInitSleepTime);
		SetSingleNodeValue(documentElement, "Controller/PrintEncoderPositionAfterMove2", PrintEncoderPositionAfterMove2);
		SetSingleNodeValue(documentElement, "Controller/GripperArmExtendRetractTimeout", GripperArmExtendRetractTimeout);
		SetSingleNodeValue(documentElement, "Controller/AdditionalPutPush", AdditionalPutPush);
		SetSingleNodeValue(documentElement, "Controller/PushTime", PushTime);
		SetSingleNodeValue(documentElement, "Controller/RollInExtendTime", RollInExtendTime);
		SetSingleNodeValue(documentElement, "Controller/TestExtendTime", TestExtendTime);
		SetSingleNodeValue(documentElement, "Controller/QlmExtendTime", QlmExtendTime);
		SetSingleNodeValue(documentElement, "Controller/MoveTimeout", MoveTimeout);
		SetSingleNodeValue(documentElement, "Controller/QlmApproachOffset", QlmApproachOffset);
		SetSingleNodeValue(documentElement, "Controller/QlmYOffset", QlmYOffset);
		if (VendPositionReceiveOffset.HasValue)
		{
			SetSingleNodeValue(documentElement, "Controller/VendPositionReceiveOffset", VendPositionReceiveOffset.Value);
		}
		SetSingleNodeValue(documentElement, "Controller/QueryPositionForVendMove", QueryPositionForVendMove);
		SetSingleNodeValue(documentElement, "Controller/ArcusReadWait", ArcusReadWait);
		SetSingleNodeValue(documentElement, "Controller/VendYPosition", VendYPosition);
		SetSingleNodeValue(documentElement, "Controller/PutAwayItemAttempts", PutAwayItemAttempts);
		SetSingleNodeValue(documentElement, "Controller/HomeYDropBack", HomeYDropBack);
		SetSingleNodeValue(documentElement, "Controller/Mode", Mode);
		SetSingleNodeValue(documentElement, "Controller/MotionControllerPortName", MotionControllerPortName);
		SetSingleNodeValue(documentElement, "Controller/ControllerPortName", ControllerPortName);
		SetSingleNodeValue(documentElement, "Controller/XHomeMethod", XHomeMethod);
		SetSingleNodeValue(documentElement, "Controller/NumberOfPulls", NumberOfPulls);
		SetSingleNodeValue(documentElement, "Controller/MotionControllerTimeout", MotionControllerTimeout);
		SetSingleNodeValue(documentElement, "Controller/SettleDiscInWideSlot", SettleDiscInWideSlot);
		SetSingleNodeValue(documentElement, "Controller/AllowProgramCacheReplacement2", AllowProgramCacheReplacement2);
		SetSingleNodeValue(documentElement, "Controller/GripperRentOnMove", GripperRentOnMove);
		SetSingleNodeValue(documentElement, "Controller/CheckGripperArmSensorsOnMove", CheckGripperArmSensorsOnMove);
		SetSingleNodeValue(documentElement, "Controller/VendDiskPollCount", VendDiskPollCount);
		SetSingleNodeValue(documentElement, "Controller/SyncErrorThreshold", SyncErrorThreshold);
		SetSingleNodeValue(documentElement, "Controller/RejectAtDoorAttempts", RejectAtDoorAttempts);
		SetSingleNodeValue(documentElement, "Controller/ErrorSyncOnCameraFailure", ErrorSyncOnCameraFailure);
		SetSingleNodeValue(documentElement, "Controller/MoveVendDoorToAuxSensor", MoveVendDoorToAuxSensor);
		GearX.SaveToXml(document.DocumentElement.SelectSingleNode("Controller/GearX"));
		GearY.SaveToXml(document.DocumentElement.SelectSingleNode("Controller/GearY"));
		InitAxisXSpeed.SaveToXml(document.DocumentElement.SelectSingleNode("Controller/InitAxisXSpeed"));
		InitAxisYSpeed.SaveToXml(document.DocumentElement.SelectSingleNode("Controller/InitAxisYSpeed"));
		MoveAxisXSpeed.SaveToXml(document.DocumentElement.SelectSingleNode("Controller/MoveXAxisSpeed"));
		MoveAxisYSpeed.SaveToXml(document.DocumentElement.SelectSingleNode("Controller/MoveYAxisSpeed"));
		MoveAxisXYSpeed.SaveToXml(document.DocumentElement.SelectSingleNode("Controller/MoveXYAxisSpeed"));
		DecksManager.Instance.SaveConfiguration(document);
	}

	protected override void LoadProperties(XmlDocument document, ErrorList errors)
	{
		XmlElement documentElement = document.DocumentElement;
		EnableCleanVMZRandomization = GetNodeValue(documentElement, "Controller/EnableCleanVMZRandomization", defaultValue: false);
		CheckGen4CameraOnInit = GetNodeValue(documentElement, "Controller/CheckGen4CameraOnInit", defaultValue: true);
		EnableTouchScreenMonitor = GetNodeValue(documentElement, "Controller/EnableTouchScreenMonitor", defaultValue: false);
		EnableDoorCheckInStatus = GetNodeValue(documentElement, "Controller/EnableDoorCheckInStatus", defaultValue: true);
		TakeInvalidDiskOnReturn = GetNodeValue(documentElement, "Controller/TakeInvalidDiskOnReturn", defaultValue: false);
		QuickReturnComPort = GetNodeValue(documentElement, "Controller/QuickReturnComPort", "NONE");
		DumpVMZState = GetNodeValue(documentElement, "Controller/DumpVMZState", defaultValue: true);
		QuickReturnPowerOffWindow = GetNodeValue(documentElement, "Controller/QuickReturnPowerOffWindow", "0:6");
		WidenArcusTolerance = GetNodeValue(documentElement, "Controller/WidenArcusTolerance", defaultValue: false);
		SyncRinglightWarmupTime = GetNodeValue(documentElement, "Controller/SyncRinglightWarmupTime", 500);
		GripperToSensorOnPut = GetNodeValue(documentElement, "Controller/GripperToSensorOnPut", defaultValue: true);
		QlmTimedExtend = GetNodeValue(documentElement, "Controller/QlmTimedExtend", defaultValue: false);
		ValidateControllerHomeStatus = GetNodeValue(documentElement, "Controller/ValidateControllerHomeStatus", defaultValue: true);
		RebootExecutionPreventionWindow = GetNodeValue(documentElement, "Controller/RebootExecutionPreventionWindow", 10);
		ReturnSlotBuffer = GetNodeValue(documentElement, "Controller/ReturnSlotBuffer", 1);
		FixCameraOnInit2 = GetNodeValue(documentElement, "Controller/FixCameraOnInit2", defaultValue: true);
		TrackPushOutFailures = GetNodeValue(documentElement, "Controller/TrackPushOutFailures", defaultValue: true);
		AlertForEmptyStuck = GetNodeValue(documentElement, "Controller/AlertForEmptyStuck", defaultValue: false);
		EmptyStuckAlertThreshold = GetNodeValue(documentElement, "Controller/EmptyStuckAlertThreshold", 15);
		AggressiveClearPickerOnPut = GetNodeValue(documentElement, "Controller/AggressiveClearPickerOnPut", defaultValue: true);
		PushOutSleepTime2 = GetNodeValue(documentElement, "Controller/PushOutSleepTime2", 50);
		TrackProblemLocations = GetNodeValue(documentElement, "Controller/TrackProblemLocations", defaultValue: true);
		MarkLocationUnknownThreshold = GetNodeValue(documentElement, "Controller/MarkLocationUnknownThreshold", 2);
		CountHardwareErrors = GetNodeValue(documentElement, "Controller/CaptureHardwareCounters", defaultValue: false);
		TestSlotOnEmpty = GetNodeValue(documentElement, "Controller/TestSlotOnEmpty", defaultValue: true);
		LeaveDuplicateResultInReturn = GetNodeValue(documentElement, "Controller/LeaveDuplicateResultInReturn", defaultValue: true);
		RotateDrumDuringUnknownRemoval2 = GetNodeValue(documentElement, "Controller/RotateDrumDuringUnknownRemoval2", defaultValue: true);
		MarkOriginalMatrixUnknown = GetNodeValue(documentElement, "Controller/MarkOriginalMatrixUnknown", defaultValue: true);
		MarkDuplicatesUnknown = GetNodeValue(documentElement, "Controller/MarkDuplicatesUnknown", defaultValue: true);
		CorrectMotionControllerCommunication = GetNodeValue(documentElement, "Controller/CorrectMotionControllerCommunication", defaultValue: true);
		RebootKioskDuringQlm = GetNodeValue(documentElement, "Controller/RebootKioskDuringQlm", defaultValue: false);
		QueryPositionForVendMove = GetNodeValue(documentElement, "Controller/QueryPositionForVendMove", defaultValue: true);
		COMPortInitSleepTime = GetNodeValue(documentElement, "Controller/COMPortInitSleepTime", 20000);
		PrintEncoderPositionAfterMove2 = GetNodeValue(documentElement, "Controller/PrintEncoderPositionAfterMove2", defaultValue: false);
		AdditionalPutPush = GetNodeValue(documentElement, "Controller/AdditionalPutPush", defaultValue: false);
		GripperArmExtendRetractTimeout = GetNodeValue(documentElement, "Controller/GripperArmExtendRetractTimeout", 2000);
		PushTime = GetNodeValue(documentElement, "Controller/PushTime", 350);
		RollInExtendTime = GetNodeValue(documentElement, "Controller/RollInExtendTime", 120);
		TestExtendTime = GetNodeValue(documentElement, "Controller/TestExtendTime", 800);
		QlmExtendTime = GetNodeValue(documentElement, "Controller/QlmExtendTime", 1500);
		QlmApproachOffset = GetNodeValue(documentElement, "Controller/QlmApproachOffset", 500);
		QlmYOffset = GetNodeValue(documentElement, "Controller/QlmYOffset", 50);
		CheckGripperArmSensorsOnMove = GetNodeValue(documentElement, "Controller/CheckGripperArmSensorsOnMove", defaultValue: true);
		GripperRentOnMove = GetNodeValue(documentElement, "Controller/GripperRentOnMove", defaultValue: true);
		VendPositionReceiveOffset = GetNodeValue<int?>(documentElement, "Controller/VendPositionReceiveOffset", null);
		MoveTimeout = GetNodeValue(documentElement, "Controller/MoveTimeout", 30000);
		SettleDiscInWideSlot = GetNodeValue(documentElement, "Controller/SettleDiscInWideSlot", defaultValue: true);
		AllowProgramCacheReplacement2 = GetNodeValue(documentElement, "Controller/AllowProgramCacheReplacement2", defaultValue: false);
		ArcusReadWait = GetNodeValue(documentElement, "Controller/ArcusReadWait", 75);
		VendYPosition = GetNodeValue(documentElement, "Controller/VendYPosition", -86200);
		HomeYDropBack = GetNodeValue<int?>(documentElement, "Controller/HomeYDropBack", null);
		PutAwayItemAttempts = GetNodeValue(documentElement, "Controller/PutAwayItemAttempts", 4);
		Mode = GetNodeValue(documentElement, "Controller/Mode", ControllerMode.Normal);
		NumberOfPulls = GetNodeValue(documentElement, "Controller/NumberOfPulls", 2);
		MotionControllerPortName = GetNodeValue(documentElement, "Controller/MotionControllerPortName", "COM3");
		ControllerPortName = GetNodeValue(documentElement, "Controller/ControllerPortName", "COM1");
		XHomeMethod = GetNodeValue(documentElement, "Controller/XHomeMethod", XHomeMethod.LimitSwitch);
		MotionControllerTimeout = GetNodeValue(documentElement, "Controller/MotionControllerTimeout", 5000);
		VendDiskPollCount = GetNodeValue(documentElement, "Controller/VendDiskPollCount", 15);
		SyncErrorThreshold = GetNodeValue(documentElement, "Controller/SyncErrorThreshold", 20);
		RejectAtDoorAttempts = GetNodeValue(documentElement, "Controller/RejectAtDoorAttempts", 10);
		ErrorSyncOnCameraFailure = GetNodeValue(documentElement, "Controller/ErrorSyncOnCameraFailure", defaultValue: true);
		MoveVendDoorToAuxSensor = GetNodeValue(documentElement, "Controller/MoveVendDoorToAuxSensor", defaultValue: true);
		XmlNode xmlNode = document.DocumentElement.SelectSingleNode("Controller/GearX");
		if (xmlNode == null)
		{
			errors.Add(Error.NewError("P002", "The required GearX node does not exist.", "Add a valid GearX node to the configuration file."));
			return;
		}
		GearX = MotorGear.FromXmlNode(xmlNode);
		XmlNode xmlNode2 = document.DocumentElement.SelectSingleNode("Controller/GearY");
		if (xmlNode2 == null)
		{
			errors.Add(Error.NewError("P002", "The required GearY node does not exist.", "Add a valid GearY node to the configuration file."));
			return;
		}
		GearY = MotorGear.FromXmlNode(xmlNode2);
		XmlNode xmlNode3 = document.DocumentElement.SelectSingleNode("Controller/InitAxisXSpeed");
		if (xmlNode3 == null)
		{
			errors.Add(Error.NewError("P003", "The required InitAxisXSpeed node does not exist.", "Add a valid InitAxisXSpeed node to the configuration file."));
			return;
		}
		InitAxisXSpeed = MotorSpeed.FromXmlNode(xmlNode3);
		InitAxisXSpeed.Gear = GearX;
		XmlNode xmlNode4 = document.DocumentElement.SelectSingleNode("Controller/InitAxisYSpeed");
		if (xmlNode4 == null)
		{
			errors.Add(Error.NewError("P003", "The required InitYAxisSpeed node does not exist.", "Add a valid InitYAxisSpeed node to the configuration file."));
			return;
		}
		InitAxisYSpeed = MotorSpeed.FromXmlNode(xmlNode4);
		InitAxisYSpeed.Gear = GearY;
		XmlNode xmlNode5 = document.DocumentElement.SelectSingleNode("Controller/MoveXAxisSpeed");
		if (xmlNode5 == null)
		{
			errors.Add(Error.NewError("P004", "The required MoveXAxisSpeed node does not exist.", "Add a valid MoveXAxisSpeed node to the configuration file."));
			return;
		}
		MoveAxisXSpeed = MotorSpeed.FromXmlNode(xmlNode5);
		MoveAxisXSpeed.Gear = GearX;
		XmlNode xmlNode6 = document.DocumentElement.SelectSingleNode("Controller/MoveYAxisSpeed");
		if (xmlNode6 == null)
		{
			errors.Add(Error.NewError("P004", "The required MoveYAxisSpeed node does not exist.", "Add a valid MoveYAxisSpeed node to the configuration file."));
			return;
		}
		MoveAxisYSpeed = MotorSpeed.FromXmlNode(xmlNode6);
		MoveAxisYSpeed.Gear = GearY;
		XmlNode xmlNode7 = document.DocumentElement.SelectSingleNode("Controller/MoveXYAxisSpeed");
		if (xmlNode7 == null)
		{
			errors.Add(Error.NewError("P004", "The required MoveXYAxisSpeed node does not exist.", "Add a valid MoveXYAxisSpeed node to the configuration file."));
			return;
		}
		MoveAxisXYSpeed = MotorSpeed.FromXmlNode(xmlNode7);
		MoveAxisXYSpeed.Gear = GearY;
		XmlNode xmlNode8 = document.DocumentElement.SelectSingleNode("Controller/Decks");
		if (xmlNode8 != null)
		{
			DecksManager.Instance.Initialize(xmlNode8);
		}
	}

	private void ConfigureMerchandizingMode(ErrorList errors)
	{
		MerchandizingMode = ((QLM.Instance.Deck == null) ? MerchandizingMode.VMZ_Gen2 : MerchandizingMode.QLM);
		ConfigureMerchandizingModeOptions(StreamWriter.Null);
		LogHelper.Instance.Log(LogEntryType.Info, "Kiosk is configured for merchandizing mode {0}", MerchandizingMode.ToString().ToUpper());
		ESPHelper.Instance.Initialize();
	}

	private void ConfigureMerchandizingModeOptions(TextWriter logFile)
	{
		if (MerchandizingMode.VMZ_Gen2 == MerchandizingMode)
		{
			logFile.WriteLine("No qlm deck number specified: configuring kiosk for VMZ.");
			VMZ.Instance.Initialize();
			DumpBin.Instance.Initialize();
			DoorSensors.Instance.SensorsEnabled = true;
		}
		else
		{
			DoorSensors.Instance.SensorsEnabled = false;
			DumpBin.Instance.Active = false;
		}
	}

	private ControllerConfiguration()
		: base("controllerConfig")
	{
		EnableCleanVMZRandomization = false;
		CheckGen4CameraOnInit = true;
		EnableTouchScreenMonitor = false;
		EnableDoorCheckInStatus = true;
		TakeInvalidDiskOnReturn = false;
		QuickReturnComPort = "NONE";
		DumpVMZState = true;
		QuickReturnPowerOffWindow = "0:6";
		WidenArcusTolerance = false;
		SyncRinglightWarmupTime = 500;
		GripperToSensorOnPut = true;
		QlmTimedExtend = false;
		ReturnSlotBuffer = 1;
		ValidateControllerHomeStatus = true;
		RebootExecutionPreventionWindow = 10;
		FixCameraOnInit2 = true;
		TrackPushOutFailures = true;
		MerchandizingMode = MerchandizingMode.QLM;
		AlertForEmptyStuck = false;
		EmptyStuckAlertThreshold = 15;
		AggressiveClearPickerOnPut = true;
		PushOutSleepTime2 = 50;
		TrackProblemLocations = true;
		MarkLocationUnknownThreshold = 2;
		CountHardwareErrors = false;
		TestSlotOnEmpty = true;
		LeaveDuplicateResultInReturn = true;
		MarkOriginalMatrixUnknown = true;
		RotateDrumDuringUnknownRemoval2 = true;
		MarkDuplicatesUnknown = true;
		CorrectMotionControllerCommunication = true;
		RebootKioskDuringQlm = false;
		MoveTimeout = 30000;
		COMPortInitSleepTime = 20000;
		PrintEncoderPositionAfterMove2 = false;
		GripperArmExtendRetractTimeout = 2000;
		AdditionalPutPush = false;
		PushTime = 350;
		RollInExtendTime = 120;
		TestExtendTime = 800;
		QlmApproachOffset = 500;
		QlmYOffset = 50;
		AudioOn = false;
		GripperRentOnMove = true;
		CheckGripperArmSensorsOnMove = true;
		NumberOfPulls = 2;
		ArcusReadWait = 75;
		PutAwayItemAttempts = 4;
		UseDoorSensor = false;
		VendYPosition = -86200;
		AllowProgramCacheReplacement2 = false;
		SettleDiscInWideSlot = true;
		ControllerPortName = "COM1";
		Mode = ControllerMode.Normal;
		MotionControllerPortName = "COM3";
		GearY = new MotorGear(20.0, 0, 0, 4);
		XHomeMethod = XHomeMethod.LimitSwitch;
		VendDiskPollCount = 15;
		SyncErrorThreshold = 20;
		RejectAtDoorAttempts = 50;
		ErrorSyncOnCameraFailure = true;
		MoveVendDoorToAuxSensor = true;
		QueryPositionForVendMove = true;
		GearX = new MotorGear(10.0, 398, 114, 8);
		MotionControllerType = MotionControllerType.Arcus;
		MotionControllerTimeout = 5000;
		InitAxisXSpeed = new MotorSpeed(500, 3000, 300, GearX);
		InitAxisYSpeed = new MotorSpeed(600, 1500, 300, GearY);
		MoveAxisYSpeed = new MotorSpeed(500, 15000, 500, GearY);
		MoveAxisXSpeed = new MotorSpeed(500, 15000, 500, GearX);
		MoveAxisXYSpeed = new MotorSpeed(500, 15000, 500, GearY);
	}
}
