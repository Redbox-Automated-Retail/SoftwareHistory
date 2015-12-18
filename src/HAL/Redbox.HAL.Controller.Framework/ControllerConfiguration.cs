using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;
using Redbox.HAL.Configuration;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

public sealed class ControllerConfiguration : AttributeXmlConfiguration
{
	internal readonly ESPHelper ESPHelper = new ESPHelper();

	private MerchandizingMode MerchMode;

	public static ControllerConfiguration Instance => Singleton<ControllerConfiguration>.Instance;

	[CustomEditor("(Decks Collection)", GetMethodName = "ConvertDeckConfigurationToXml", SetMethodName = "UpdateDeckConfigurationFromXml")]
	[Category("Configuration")]
	[DisplayName("Decks")]
	[Description("The physical description of the decks for the host of the connected HAL Service.")]
	public int DecksCount => DecksManager.Instance.DeckCount;

	[XmlConfiguration(DefaultValue = "350")]
	[Description("The amount of time the gripper will overrun the limit switch beforing raising a timeout.")]
	[Category("Behavior")]
	[DisplayName("Gripper Push Timeout")]
	public int PushTime { get; set; }

	[XmlConfiguration(DefaultValue = "120")]
	[DisplayName("Gripper Extend Timeout")]
	[Description("The amount of time the gripper will extend the arm before timing out; used when putting a DVD into the slot.")]
	[Category("Behavior")]
	public int RollInExtendTime { get; set; }

	[DisplayName("Test Extend Timeout")]
	[Category("Behavior")]
	[Description("The amount of time the gripper will overrun the limit switch beforing raising a timeout while testing for a DVD in the slot.")]
	[XmlConfiguration(DefaultValue = "800")]
	public int TestExtendTime { get; set; }

	[DisplayName("Vend Position Receive Offset")]
	[Description("The amount of backoff to back off when receiving an item from the vend position.")]
	[XmlConfiguration(DefaultValue = "100")]
	[Category("Behavior")]
	public int VendPositionReceiveOffset { get; set; }

	[XmlConfiguration(DefaultValue = "20")]
	[Description("Controls how long (in milliseconds) HAL waits for the Arcus controller to fill its internal buffer before reading it.")]
	[Category("Behavior")]
	[DisplayName("Arcus Write Pause")]
	public int ArcusWritePause { get; set; }

	[DisplayName("Arcus Motor Query Pause")]
	[XmlConfiguration(DefaultValue = "75")]
	[Category("Behavior")]
	[Description("Controls how long (in milliseconds) HAL waits between MST commands during a move.")]
	public int ArcusMotorQueryPause { get; set; }

	[DisplayName("Y Position for Vend")]
	[XmlConfiguration(DefaultValue = "-86200")]
	[Description("Known as the vend position, this is the point on the Y axis that is equal to the vend door, allowing the picker to vend and return disks. This must be a negative integer value.")]
	[Category("Configuration")]
	public int VendYPosition { get; set; }

	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "0")]
	[DisplayName("Y drop back for HOMEY")]
	[Description("The amount of back off movement on the Y axis during a HOMEY instruction.  This value should be used only on older machines where the upper Y axis limit switch is too close to the Y axis home switch.")]
	public int HomeYDropBack { get; set; }

	[Description("The number of attempts to try and put something in the picker away.")]
	[DisplayName("Attempts to put an item away.")]
	[XmlConfiguration(DefaultValue = "4")]
	[Category("Behavior")]
	public int PutAwayItemAttempts { get; set; }

	[DisplayName("Controller Mode")]
	[Description("The mode under which the controller driver runs.  If set to Emulated, all hardware instructions will be emulated based on the configured emulation state.")]
	[Category("Configuration")]
	[ValidValueListProvider("GetModes")]
	[XmlConfiguration(DefaultValue = "Normal")]
	public ControllerMode Mode { get; set; }

	[XmlConfiguration(DefaultValue = "2")]
	[Category("Behavior")]
	[DisplayName("Number of Pulls")]
	[Description("The number of times the picker will attempt to pull the disk from the slot.")]
	public int NumberOfPulls { get; set; }

	[ValidValueListProvider("GetComPorts")]
	[DisplayName("Motion Controller Port Name")]
	[Description("The COM port to which to which the motion controller board is connected.")]
	[XmlConfiguration(DefaultValue = "COM3")]
	[Category("Configuration")]
	public string MotionControllerPortName { get; set; }

	[XmlConfiguration(DefaultValue = "5000")]
	[DisplayName("Motion Controller Timeout (ms)")]
	[Description("The read timeout from the Motion Controller COM Port in milliseconds.")]
	[Category("Configuration")]
	public int MotionControllerTimeout { get; private set; }

	[ExcludeType]
	[Category("Configuration")]
	[DisplayName("X Motor Gear")]
	[Recurse]
	[Description("The X axis motor gear configuration.  The configuration of this property and its children affect the X Axis Speed properties.")]
	public MotorGear GearX { get; private set; }

	[DisplayName("Y Motor Gear")]
	[ExcludeType]
	[Category("Configuration")]
	[Recurse]
	[Description("The Y axis motor gear configuration.  The configuration of this property and its children affect the Y Axis Speed properties.")]
	public MotorGear GearY { get; private set; }

	[Category("Configuration")]
	[DisplayName("Controller Port Name")]
	[ValidValueListProvider("GetComPorts")]
	[Description("The COM port the main, picker, auxiliary, and alternate motion controller boards are connected to.")]
	[XmlConfiguration(DefaultValue = "COM1")]
	public string ControllerPortName { get; set; }

	[Description("The speed of the X axis for the HOMEX instruction.")]
	[ExcludeType]
	[DisplayName("X Axis Initialization Speed")]
	[Recurse]
	[Category("Configuration")]
	public MotorSpeed InitAxisXSpeed { get; private set; }

	[Description("The speed of the Y axis for the HOMEY instruction.")]
	[Recurse]
	[ExcludeType]
	[Category("Configuration")]
	[DisplayName("Y Axis Initialization Speed")]
	public MotorSpeed InitAxisYSpeed { get; private set; }

	[DisplayName("Y Axis Speed")]
	[Recurse]
	[Description("The speed of the Y axis for the MOVE instruction.")]
	[ExcludeType]
	[Category("Configuration")]
	public MotorSpeed MoveAxisYSpeed { get; private set; }

	[ExcludeType]
	[DisplayName("X Axis Speed")]
	[Description("The speed of the X axis for the MOVE instruction.")]
	[Recurse]
	[Category("Configuration")]
	public MotorSpeed MoveAxisXSpeed { get; private set; }

	[Recurse]
	[Category("Configuration")]
	[DisplayName("XY Axis Speed")]
	[Description("The speed of the combined X and Y axis for the MOVE instruction.")]
	[ExcludeType]
	public MotorSpeed MoveAxisXYSpeed { get; private set; }

	[XmlConfiguration(DefaultValue = "500")]
	[Description("If value is greater than 0, wait value ms after turning on the ringlight during sync.")]
	[Category("Configuration")]
	[DisplayName("Sync Ringlight Warmup Time")]
	public int SyncRinglightWarmupTime { get; set; }

	[Description("The timeout (in milliseconds) for an ARCUS move.")]
	[DisplayName("Move Timeout")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "30000")]
	public int MoveTimeout { get; set; }

	[Description("If set to true, will time-extend the fingers using the QlmExtendTime, ignoring the extend sensor.")]
	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	[DisplayName("Qlm Timed Extend")]
	public bool QlmTimedExtend { get; set; }

	[Category("Behavior")]
	[DisplayName("Timeout for a gripper extend/retract operation.")]
	[XmlConfiguration(DefaultValue = "2000")]
	[Description("The timeout value for a gripper extend/retract operation.")]
	public int GripperArmExtendRetractTimeout { get; set; }

	[Description("If set to Yes, validate the position after a move.")]
	[Category("Configuration")]
	[DisplayName("Validate Position After Move")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool PrintEncoderPositionAfterMove2 { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Description("If set to true ( the default ), PUT will perform an extra gripper push to seat the disk.")]
	[Category("Behavior")]
	[DisplayName("Additional PUT push.")]
	public bool AdditionalPutPush { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	[DisplayName("Reboot Kiosk During QLM")]
	[Description("If set to true ( the default ), qlm/thin/qlm-thin will reboot when detecting an Arcus problem after engage/disengage.")]
	public bool RebootKioskDuringQlm { get; set; }

	[Description("If set to true, clean/thin will dump the zone after the job.")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Dump VMZ State")]
	public bool DumpVMZState { get; set; }

	[DisplayName("Correct Motion Controller Communication")]
	[Description("If set to true ( the default ), after a QLM engage/disengage, if HAL can't communicate with the Arcus, it will try to reset the DIEs.")]
	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	public bool CorrectMotionControllerCommunication { get; set; }

	[Category("Behavior")]
	[DisplayName("Vend Disk Poll Count")]
	[Description("The number of times, during a VEND, to poll for a taken disk.")]
	[XmlConfiguration(DefaultValue = "15")]
	public int VendDiskPollCount { get; set; }

	[Category("Behavior")]
	[DisplayName("Sync Error Threshold")]
	[XmlConfiguration(DefaultValue = "20")]
	[Description("The number of errors encountered before exiting a sync.")]
	public int SyncErrorThreshold { get; set; }

	[Description("The number of attempts to reject a disk at the vend door.")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "10")]
	[DisplayName("Reject At Door Attempts")]
	public int RejectAtDoorAttempts { get; set; }

	[Category("Behavior")]
	[Description("The number of errors encountered before exiting a sync.")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Error Sync On Camera Failure")]
	public bool ErrorSyncOnCameraFailure { get; set; }

	[Description("If true, will try to move the vend door to the appropriate sensor on the AUX board.")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Move Vend Door To AUX Sensor")]
	public bool MoveVendDoorToAuxSensor { get; set; }

	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Query Position For Vend Move")]
	[Description("If true, query the current position before moving; if at the target position, wont' request a move.")]
	public bool QueryPositionForVendMove { get; set; }

	[XmlConfiguration(DefaultValue = "500")]
	[Category("Behavior")]
	[DisplayName("QLM Approach Offset")]
	[Description("If true, will try to move the vend door to the appropriate sensor on the AUX board.")]
	public int QlmApproachOffset { get; set; }

	[XmlConfiguration(DefaultValue = "50")]
	[DisplayName("QLM Y Offset")]
	[Description("If true, will try to move the vend door to the appropriate sensor on the AUX board.")]
	[Category("Behavior")]
	public int QlmYOffset { get; set; }

	[XmlConfiguration(DefaultValue = "1500")]
	[DisplayName("QLM Extend Timeout")]
	[Description("The amount of time the gripper will overrun the limit switch beforing raising a timeout while doing a grab on the QLM.")]
	[Category("Behavior")]
	public int QlmExtendTime { get; set; }

	[DisplayName("Settle Disc In Wide Slot")]
	[XmlConfiguration(DefaultValue = "true")]
	[Description("If set to true, will perform an extend, close, open and retract to help center the disc in the wide slot.")]
	[Category("Behavior")]
	public bool SettleDiscInWideSlot { get; set; }

	[DisplayName("Min Init Wakeup Time")]
	[XmlConfiguration(DefaultValue = "5000")]
	[Description("The time, in milliseconds, for the init job to wait for the ports to come up.")]
	[Browsable(false)]
	[Category("Configuration")]
	public int MinInitWakeupTime { get; set; }

	[Description("If set to true, allows the program cache to accept overwrites of programs.")]
	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Allow Program Cache Replacement")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool AllowProgramCacheReplacement2 { get; set; }

	[XmlConfiguration(DefaultValue = "true")]
	[Description("If set to true, will check both the forward and retract sensors if they're both set, and if they are, won't move.")]
	[Category("Behavior")]
	[DisplayName("Check Gripper Arm Sensors On Move")]
	public bool CheckGripperArmSensorsOnMove { get; set; }

	[Category("Behavior")]
	[Description("If set to true, will check if the gripper is in the rent position, and if not, will move it to Rent.")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Gripper Rent On Move")]
	public bool GripperRentOnMove { get; set; }

	[Description("If set to true, mark duplicate barcodes as UNKNOWN.")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Mark Duplicate Unknown")]
	[Category("Behavior")]
	public bool MarkDuplicatesUnknown { get; set; }

	[Description("If set to true, when a duplicate is recognized, will mark the original barcode as UNKNOWN.")]
	[DisplayName("Mark Original Matrix Unknown")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "true")]
	public bool MarkOriginalMatrixUnknown { get; set; }

	[Category("Behavior")]
	[Description("If set to true, will do a slot test, and if the slot is full, will rotate the drum 5 slots during an unknown removal if unable to grab a disk.")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Rotate Drum During Unknown Removal")]
	public bool RotateDrumDuringUnknownRemoval2 { get; set; }

	[XmlConfiguration(DefaultValue = "true")]
	[Description("If set to true, will leave a CAMERA CAPTURE result during a return job if duplicates are detected.")]
	[Category("Behavior")]
	[DisplayName("Leave Duplicate Result In Return")]
	public bool LeaveDuplicateResultInReturn { get; set; }

	[DisplayName("Test Slot On Empty")]
	[XmlConfiguration(DefaultValue = "true")]
	[Description("If set to true, perform a PEEK (test slot) on the slot if GET returns SLOTEMPTY.")]
	[Category("Behavior")]
	public bool TestSlotOnEmpty { get; set; }

	[Description("The count for errors before the slot is marked UNKNOWN.")]
	[XmlConfiguration(DefaultValue = "2")]
	[DisplayName("Mark Location Unknown Threshold")]
	[Category("Behavior")]
	public int MarkLocationUnknownThreshold { get; set; }

	[Description("If set to true, will fail a vend after 2 push out tries show any sensors 1-4 lit.")]
	[DisplayName("Track Push Out Failures")]
	[Browsable(false)]
	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	public bool TrackPushOutFailures { get; set; }

	[Category("Behavior")]
	[DisplayName("Track Problem Locations")]
	[Description("If set to true, will track problems with slots.")]
	[XmlConfiguration(DefaultValue = "true")]
	public bool TrackProblemLocations { get; set; }

	[Description("If set to true, will push to sensor as opposed to motor sleep.")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Gripper To Sensor On Put")]
	[Category("Behavior")]
	public bool GripperToSensorOnPut { get; set; }

	[Category("Configuration")]
	[Description("If greater than 0, will spin for the configured time in ms after clearing sensor 4 during a vend disk.")]
	[XmlConfiguration(DefaultValue = "50")]
	[DisplayName("Push Out Sleep Time")]
	public int PushOutSleepTime2 { get; set; }

	[Description("If true, try to clear the picker more aggressively during a PUT.")]
	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	[DisplayName("AggressiveClearPickerOnPut")]
	public bool AggressiveClearPickerOnPut { get; set; }

	[DisplayName("EmptyStuckAlertThreshold")]
	[XmlConfiguration(DefaultValue = "15")]
	[Category("Configuration")]
	[Description("If Empty or stuck count exceeds this number during vend in a 24 hour period, generate a result that should trigger an alert.")]
	public int EmptyStuckAlertThreshold { get; set; }

	[Description("If true, will leave a RESULT code for alert generation when the empty/stuck counter has exceeded its threshold.")]
	[DisplayName("Alert For Empty Stuck")]
	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	public bool AlertForEmptyStuck { get; set; }

	[DisplayName("Fix Camera On Init")]
	[XmlConfiguration(DefaultValue = "true")]
	[Description("If true, will test if the camera is generating images, and if not, will try and reset it.")]
	[Category("Behavior")]
	public bool FixCameraOnInit2 { get; set; }

	[DisplayName("Validate Controller Home Status")]
	[Description("If true, tells the hardware-status job to include the init status of the motion controller.")]
	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	public bool ValidateControllerHomeStatus { get; set; }

	[XmlConfiguration(DefaultValue = "10")]
	[Description("In minutes, the time before kiosk reboot that the scheduler prevents jobs from running.")]
	[Category("Configuration")]
	[DisplayName("Reboot Execution Prevention Window")]
	public int RebootExecutionPreventionWindow { get; set; }

	[Description("If true, will dump diagnostic information to the log about commands.")]
	[Category("Behavior")]
	[Browsable(false)]
	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Enable Command Trace")]
	public bool EnableCommandTrace { get; set; }

	[Description("The number of slots the kiosk must not to try and file disks to.")]
	[XmlConfiguration(DefaultValue = "1")]
	[DisplayName("Return Slot Buffer")]
	[Category("Configuration")]
	public int ReturnSlotBuffer { get; set; }

	[XmlConfiguration(DefaultValue = "0:6")]
	[Description("The number of slots the kiosk must not to try and file disks to.")]
	[Category("Configuration")]
	[DisplayName("Quick Return Power Off Window")]
	public string QuickReturnPowerOffWindow { get; set; }

	[DisplayName("Quick Return Port")]
	[Category("Configuration")]
	[XmlConfiguration(DefaultValue = "NONE")]
	[Description("The COM port on which the Quick return device is configured.")]
	public string QuickReturnComPort { get; set; }

	[Description("If true, will give the arcus a larger tolerance window.")]
	[Category("Behavior")]
	[Browsable(false)]
	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Widen Arcus Tolerance")]
	public bool WidenArcusTolerance { get; set; }

	[Category("Behavior")]
	[Description("If true, will take the disk if the barcode can't be read.")]
	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Take Invalid Disk On Return")]
	public bool TakeInvalidDiskOnReturn { get; set; }

	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Enable Door Check In Status")]
	[XmlConfiguration(DefaultValue = "true")]
	[Description("If set to true, will check the door sensors during the hardware status job.")]
	public bool EnableDoorCheckInStatus { get; set; }

	[Category("Behavior")]
	[Description("If set to true, will establish the correct driver is loaded and the device is enabled.")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Check Gen4 Camera On Init")]
	[Browsable(false)]
	public bool CheckGen4CameraOnInit { get; set; }

	[XmlConfiguration(DefaultValue = "true")]
	[Description("If set to true, will reset the camera during sync & return operations.")]
	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Enable Camera Reset")]
	public bool EnableCameraReset { get; set; }

	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Fix Camera After Return")]
	[Browsable(false)]
	[Description("If set to true, will reset the camera at the very end of return.")]
	public bool FixCameraAfterReturn { get; set; }

	[Browsable(false)]
	[Category("Configuration")]
	[DisplayName("Ringlight Connector")]
	[Description("Tells the service which ringlight connector is attached..")]
	[XmlConfiguration(DefaultValue = "2")]
	public int RinglightConnector { get; set; }

	[DisplayName("Threshold for Camera alert")]
	[XmlConfiguration(DefaultValue = "2")]
	[Browsable(false)]
	[Category("Configuration")]
	[Description("An integer describing how many consecutive failures happen before an alert is generated from return.")]
	public int ReturnCameraAlertThreshold { get; set; }

	[XmlConfiguration(DefaultValue = "true")]
	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Reset Timeout Counters Weekly")]
	[Description("If set to true, will reset the hardware timeout counters every week on Sunday morning.")]
	public bool ResetTimeoutCountersWeekly { get; set; }

	[Category("Behavior")]
	[Browsable(false)]
	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Restart Controller During User Jobs")]
	[Description("If true, will restart the controller during a user job.")]
	public bool RestartControllerDuringUserJobs { get; set; }

	[Browsable(false)]
	[XmlConfiguration(DefaultValue = "5")]
	[Category("Configuration")]
	[DisplayName("Redeploy Segment Offset")]
	[Description("An integer describing the offset from the start of a quadrant used for redeploy.")]
	public int RedeploySegmentOffset { get; set; }

	[DisplayName("Track Hardware Corrections")]
	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[Category("Behavior")]
	[Description("If true, will track hardware correction stats.")]
	public bool TrackHardwareCorrections { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Enable Hardware Healing")]
	[Description("If true, will schedule a healing test.")]
	public bool EnableHardwareHealing { get; set; }

	[DisplayName("Enable Arcus Healing")]
	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[Category("Behavior")]
	[Description("Sets the mask of options for the hardware healer.")]
	public bool EnableArcusHealing { get; set; }

	[DisplayName("Enable Arcus Keepalive")]
	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[Description("If true, will send ComStatus messages to the Arcus. The EnableHardwareHealing option must be set.")]
	[Category("Behavior")]
	public bool EnableArcusKeepalive { get; set; }

	[Browsable(false)]
	[Description("Sets the mask of options for the hardware healer.")]
	[DisplayName("Enable Camera Healing")]
	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	public bool EnableCameraHealing { get; set; }

	[Description("If true, the service will auto-schedule a clean.")]
	[XmlConfiguration(DefaultValue = "true")]
	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Enable Auto VMZ Clean")]
	public bool EnableAutoVMZClean { get; set; }

	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[DisplayName("Enable Leiwe Healing")]
	[Description("Sets the mask of options for the hardware healer.")]
	public bool EnableLeiweHealing { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	[Description("If true, will enable polling & control of Ice Qube board.")]
	[Browsable(false)]
	[DisplayName("Enable Ice Qube Polling")]
	public bool EnableIceQubePolling { get; set; }

	[DisplayName("Watchdog Image Day Age")]
	[XmlConfiguration(DefaultValue = "90")]
	[Browsable(false)]
	[Category("Configuration")]
	[Description("In days, the age of images to be deleted.")]
	public int WatchdogImageDayAge { get; set; }

	[Browsable(false)]
	[Description("In days, the age of applogs to be deleted.")]
	[XmlConfiguration(DefaultValue = "60")]
	[Category("Configuration")]
	[DisplayName("Applog Day Age")]
	public int ApplogDayAge { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	[Description("If true, will try to correct an Arcus comm error during the hardware status job.")]
	[Browsable(false)]
	[DisplayName("Fix Arcus During Status")]
	public bool FixArcusDuringStatusJob { get; set; }

	[Description("If true, will PEEK slot during vend.")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Peek During Vend")]
	[Browsable(false)]
	public bool PeekDuringVend { get; set; }

	[Description("If true, will PEEK slot during vend-unknown.")]
	[Browsable(false)]
	[DisplayName("Test Slot During Removal")]
	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	public bool TestSlotDuringRemoval { get; set; }

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
	public bool IsVMZMachine => MerchandizingMode.QLM != MerchMode;

	protected override string FileNodeName => "Controller";

	public void Initialize(ErrorList errors)
	{
		if (ControllerMode.Normal == Mode)
		{
			IPersistentMap dataStore = StoreManager.Instance.GetDataStore(PersistentMapTypes.Engine);
			int value = dataStore.GetValue("MaxSlots", -1);
			Deck deck = DecksManager.Instance.GetDeck(1);
			if (value != deck.NumberOfSlots)
			{
				errors.Add(Error.NewError("P005", "Machine configuration mismatch.", $"The store key shows {value}, but the deck shows {deck.NumberOfSlots}"));
				return;
			}
		}
		DecksManager.Instance.LoadInventory();
		ConfigureMerchandizingMode(errors);
	}

	public void DumpESP(bool dumpStore)
	{
		ESPHelper.Dump();
	}

	public void Import(TextWriter logFile, ErrorList errors)
	{
		int? num = null;
		int? num2 = null;
		List<int> list = new List<int>();
		List<bool> list2 = new List<bool>();
		IDictionary<string, int> dictionary = GampHelper.Instance.ReadLegacySystemDataFile();
		if (dictionary.Count == 0)
		{
			errors.Add(Error.NewError("H321", "The SystemData.dat file was not found or had no properties.", "Check the C:\\gamp folder to ensure the file exists."));
			return;
		}
		if (dictionary.ContainsKey("PushTime"))
		{
			PushTime = dictionary["PushTime"];
		}
		if (dictionary.ContainsKey("RollInExtendTime"))
		{
			RollInExtendTime = dictionary["RollInExtendTime"];
		}
		if (dictionary.ContainsKey("TestExtendTime"))
		{
			TestExtendTime = dictionary["TestExtendTime"];
		}
		if (dictionary.ContainsKey("PickExtendTime"))
		{
			QlmExtendTime = dictionary["PickExtendTime"];
		}
		if (dictionary.ContainsKey("QLMaproch"))
		{
			QlmApproachOffset = dictionary["QLMaproch"];
		}
		if (dictionary.ContainsKey("QLMZoffset"))
		{
			QlmYOffset = dictionary["QLMZoffset"];
		}
		if (dictionary.ContainsKey("ReceivePositionOffSet"))
		{
			VendPositionReceiveOffset = dictionary["ReceivePositionOffSet"];
		}
		if (dictionary.ContainsKey("ArcusComPort"))
		{
			MotionControllerPortName = string.Format("COM{0}", dictionary["ArcusComPort"]);
		}
		if (dictionary.ContainsKey("ControlComPort"))
		{
			ControllerPortName = string.Format("COM{0}", dictionary["ControlComPort"]);
		}
		VendYPosition = (dictionary.ContainsKey("VendPosition") ? dictionary["VendPosition"] : (-86200));
		if (dictionary.ContainsKey("ZHomeDropBack"))
		{
			HomeYDropBack = -dictionary["ZHomeDropBack"];
		}
		NumberOfPulls = (dictionary.ContainsKey("NumberOfPulls") ? dictionary["NumberOfPulls"] : 2);
		if (dictionary.ContainsKey("SellThrughOffSet"))
		{
			num2 = dictionary["SellThrughOffSet"];
		}
		if (dictionary.ContainsKey("QLMDeckNumber"))
		{
			num = dictionary["QLMDeckNumber"];
		}
		GearX = new MotorGear(dictionary.ContainsKey("RGearRatio") ? dictionary["RGearRatio"] : 10, dictionary.ContainsKey("RPulseRatio") ? dictionary["RPulseRatio"] : 398, dictionary.ContainsKey("REncoderRatio") ? dictionary["REncoderRatio"] : 114, dictionary.ContainsKey("RStepRes") ? dictionary["RStepRes"] : 8);
		GearY = new MotorGear(dictionary.ContainsKey("ZGearRatio") ? dictionary["ZGearRatio"] : 20, 0, 0, dictionary.ContainsKey("ZStepRes") ? dictionary["ZStepRes"] : 4);
		InitAxisXSpeed = new MotorSpeed(dictionary.ContainsKey("InitAxis1LS") ? dictionary["InitAxis1LS"] : 600, dictionary.ContainsKey("InitAxis1HS") ? dictionary["InitAxis1HS"] : 3000, 300, GearX);
		InitAxisYSpeed = new MotorSpeed(dictionary.ContainsKey("InitAxis2LS") ? dictionary["InitAxis2LS"] : 800, dictionary.ContainsKey("InitAxis2HS") ? dictionary["InitAxis2HS"] : 2400, 300, GearY);
		MoveAxisYSpeed = new MotorSpeed(500, dictionary.ContainsKey("MoveZHS") ? dictionary["MoveZHS"] : 15000, dictionary.ContainsKey("MoveZAcc") ? dictionary["MoveZAcc"] : 500, GearY);
		MoveAxisXSpeed = new MotorSpeed(500, dictionary.ContainsKey("MoveRHS") ? dictionary["MoveRHS"] : 15000, dictionary.ContainsKey("MoveRAcc") ? dictionary["MoveRAcc"] : 500, GearY);
		MoveAxisXYSpeed = new MotorSpeed(500, dictionary.ContainsKey("MoveZRHS") ? dictionary["MoveZRHS"] : 15000, dictionary.ContainsKey("MoveZRAcc") ? dictionary["MoveZRAcc"] : 500, GearY);
		int num3 = 1;
		while (true)
		{
			string key = $"PlatterMaxSlots{num3++}";
			if (!dictionary.ContainsKey(key))
			{
				break;
			}
			int item = (dictionary.ContainsKey(key) ? dictionary[key] : 90);
			list.Add(item);
		}
		num3 = 1;
		while (true)
		{
			string key2 = $"DeckSellStat{num3++}";
			if (!dictionary.ContainsKey(key2))
			{
				break;
			}
			list2.Add(dictionary[key2] == 1);
		}
		List<List<int>> list3 = GampHelper.Instance.ReadLegacySlotDataFile(removeZeroes: false);
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
		foreach (List<int> item2 in list3)
		{
			int num4 = ((num3 == num) ? 80 : list[num3 - 1]);
			List<Quadrant> list4 = new List<Quadrant>();
			for (int i = 1; i < item2.Count && item2[i] != 0; i++)
			{
				if (num.HasValue && num3 == num.Value && i < array.Length)
				{
					list4.Add(new Quadrant(item2[i], array[i]));
				}
				else
				{
					list4.Add(new Quadrant(item2[i]));
				}
			}
			decimal num5 = 166.6667m;
			if (num.HasValue && num3 == num.Value)
			{
				num5 = 177.7m;
			}
			bool isQlmDeck = num.HasValue && num3 == num.Value;
			DecksManager.Instance.Add(new Deck(num3, item2[0], isQlmDeck, null, num4, (num4 == 72) ? 173.3m : num5, (num4 == 72) ? new int?(6) : ((int?)null), (num4 == 72) ? num2 : ((int?)null), list4.ToArray()));
			num3++;
		}
		MerchMode = ((!num.HasValue || num.Value <= 0) ? MerchandizingMode.VMZ_Gen2 : MerchandizingMode.QLM);
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
		using (new DisposeableList<DeckData>(inventory))
		{
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
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
				if (errors.Count > 0)
				{
					LogHelper.Instance.Log("RESET INVENTORY Failed to import inventory.", LogEntryType.Error);
				}
			}
			catch (Exception e)
			{
				errors.Add(Error.NewError("P999", "An unhandled exception was raised in ControllerConfiguration.Load.", e));
				LogHelper.Instance.Log("RESET INVENTORY Failed to import inventory.", e, LogEntryType.Error);
			}
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

	protected override void StorePropertiesInner(XmlDocument document, ErrorList errors)
	{
		GearX.SaveToXml(document.DocumentElement.SelectSingleNode("Controller/GearX"));
		GearY.SaveToXml(document.DocumentElement.SelectSingleNode("Controller/GearY"));
		InitAxisXSpeed.SaveToXml(document.DocumentElement.SelectSingleNode("Controller/InitAxisXSpeed"));
		InitAxisYSpeed.SaveToXml(document.DocumentElement.SelectSingleNode("Controller/InitAxisYSpeed"));
		MoveAxisXSpeed.SaveToXml(document.DocumentElement.SelectSingleNode("Controller/MoveXAxisSpeed"));
		MoveAxisYSpeed.SaveToXml(document.DocumentElement.SelectSingleNode("Controller/MoveYAxisSpeed"));
		MoveAxisXYSpeed.SaveToXml(document.DocumentElement.SelectSingleNode("Controller/MoveXYAxisSpeed"));
		DecksManager.Instance.SaveConfiguration(document);
	}

	protected override void LoadPropertiesInner(XmlDocument document, ErrorList errors)
	{
		_ = document.DocumentElement;
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
		else
		{
			LogHelper.Instance.Log("There are no decks nodes in the configuration file.");
		}
	}

	private void ConfigureMerchandizingMode(ErrorList errors)
	{
		MerchMode = ((QLM.Deck == null) ? MerchandizingMode.VMZ_Gen2 : MerchandizingMode.QLM);
		ConfigureMerchandizingModeOptions(StreamWriter.Null);
		LogHelper.Instance.Log(LogEntryType.Info, "Kiosk is configured for merchandizing mode {0}", MerchMode.ToString().ToUpper());
		ESPHelper.Initialize();
	}

	private void ConfigureMerchandizingModeOptions(TextWriter logFile)
	{
		if (MerchandizingMode.VMZ_Gen2 == MerchMode)
		{
			DumpBin.Instance.Initialize();
		}
		DoorSensors.Instance.Initialize();
	}

	private ControllerConfiguration()
		: base("controllerConfig", typeof(ControllerConfiguration))
	{
		GearY = new MotorGear(20.0, 0, 0, 4);
		GearX = new MotorGear(10.0, 398, 114, 8);
		InitAxisXSpeed = new MotorSpeed(500, 3000, 300, GearX);
		InitAxisYSpeed = new MotorSpeed(600, 1500, 300, GearY);
		MoveAxisYSpeed = new MotorSpeed(500, 15000, 500, GearY);
		MoveAxisXSpeed = new MotorSpeed(500, 15000, 500, GearX);
		MoveAxisXYSpeed = new MotorSpeed(500, 15000, 500, GearY);
	}
}
