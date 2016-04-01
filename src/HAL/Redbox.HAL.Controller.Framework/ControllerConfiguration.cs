using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using Microsoft.Win32;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Configuration;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class ControllerConfiguration : AttributeXmlConfiguration
{
	internal readonly DecksManager Manager = new DecksManager();

	private MerchandizingMode MerchMode;

	public static ControllerConfiguration Instance => Singleton<ControllerConfiguration>.Instance;

	[DisplayName("Decks")]
	[CustomEditor("(Decks Collection)", GetMethodName = "ConvertDeckConfigurationToXml", SetMethodName = "UpdateDeckConfigurationFromXml")]
	[Description("The physical description of the decks for the host of the connected HAL Service.")]
	[Category("Configuration")]
	public int DecksCount => Manager.DeckCount;

	[Description("The amount of time the gripper will overrun the limit switch beforing raising a timeout.")]
	[XmlConfiguration(DefaultValue = "350")]
	[DisplayName("Gripper Push Timeout")]
	[Category("Behavior")]
	public int PushTime { get; set; }

	[Description("The amount of time the gripper will extend the arm before timing out; used when putting a DVD into the slot.")]
	[XmlConfiguration(DefaultValue = "120")]
	[DisplayName("Gripper Extend Timeout")]
	[Category("Behavior")]
	public int RollInExtendTime { get; set; }

	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "800")]
	[DisplayName("Test Extend Timeout")]
	[Description("The amount of time the gripper will overrun the limit switch beforing raising a timeout while testing for a DVD in the slot.")]
	public int TestExtendTime { get; set; }

	[Category("Behavior")]
	[DisplayName("Vend Position Receive Offset")]
	[XmlConfiguration(DefaultValue = "100")]
	[Description("The amount of backoff to back off when receiving an item from the vend position.")]
	public int VendPositionReceiveOffset { get; set; }

	[Description("Controls how long (in milliseconds) HAL waits for the Arcus controller to fill its internal buffer before reading it.")]
	[XmlConfiguration(DefaultValue = "20")]
	[DisplayName("Arcus Write Pause")]
	[Category("Behavior")]
	public int ArcusWritePause { get; set; }

	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "75")]
	[DisplayName("Arcus Motor Query Pause")]
	[Description("Controls how long (in milliseconds) HAL waits between MST commands during a move.")]
	public int ArcusMotorQueryPause { get; set; }

	[Category("Configuration")]
	[Description("Known as the vend position, this is the point on the Y axis that is equal to the vend door, allowing the picker to vend and return disks. This must be a negative integer value.")]
	[XmlConfiguration(DefaultValue = "-86200")]
	[DisplayName("Y Position for Vend")]
	public int VendYPosition { get; set; }

	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "0")]
	[DisplayName("Y drop back for HOMEY")]
	[Description("The amount of back off movement on the Y axis during a HOMEY instruction.  This value should be used only on older machines where the upper Y axis limit switch is too close to the Y axis home switch.")]
	public int HomeYDropBack { get; set; }

	[Description("The number of attempts to try and put something in the picker away.")]
	[DisplayName("Attempts to put an item away.")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "4")]
	public int PutAwayItemAttempts { get; set; }

	[DisplayName("Controller Mode")]
	[Category("Configuration")]
	[XmlConfiguration(DefaultValue = "Normal")]
	[ValidValueListProvider("GetModes")]
	[Description("The mode under which the controller driver runs.  If set to Emulated, all hardware instructions will be emulated based on the configured emulation state.")]
	public ControllerMode Mode { get; set; }

	[XmlConfiguration(DefaultValue = "2")]
	[Category("Behavior")]
	[DisplayName("Number of Pulls")]
	[Description("The number of times the picker will attempt to pull the disk from the slot.")]
	public int NumberOfPulls { get; set; }

	[XmlConfiguration(DefaultValue = "COM3")]
	[Category("Configuration")]
	[ValidValueListProvider("GetComPorts")]
	[Description("The COM port to which to which the motion controller board is connected.")]
	[DisplayName("Motion Controller Port Name")]
	public string MotionControllerPortName { get; set; }

	[Category("Configuration")]
	[Description("The read timeout from the Motion Controller COM Port in milliseconds.")]
	[XmlConfiguration(DefaultValue = "5000")]
	[DisplayName("Motion Controller Timeout (ms)")]
	public int MotionControllerTimeout { get; private set; }

	[Description("The X axis motor gear configuration.  The configuration of this property and its children affect the X Axis Speed properties.")]
	[Recurse]
	[ExcludeType]
	[Category("Configuration")]
	[DisplayName("X Motor Gear")]
	public MotorGear GearX { get; private set; }

	[Description("The Y axis motor gear configuration.  The configuration of this property and its children affect the Y Axis Speed properties.")]
	[Category("Configuration")]
	[Recurse]
	[DisplayName("Y Motor Gear")]
	[ExcludeType]
	public MotorGear GearY { get; private set; }

	[ValidValueListProvider("GetComPorts")]
	[Description("The COM port the main, picker, auxiliary, and alternate motion controller boards are connected to.")]
	[XmlConfiguration(DefaultValue = "COM1")]
	[DisplayName("Controller Port Name")]
	[Category("Configuration")]
	public string ControllerPortName { get; set; }

	[Recurse]
	[Description("The speed of the X axis for the HOMEX instruction.")]
	[ExcludeType]
	[Category("Configuration")]
	[DisplayName("X Axis Initialization Speed")]
	public MotorSpeed InitAxisXSpeed { get; private set; }

	[DisplayName("Y Axis Initialization Speed")]
	[ExcludeType]
	[Description("The speed of the Y axis for the HOMEY instruction.")]
	[Recurse]
	[Category("Configuration")]
	public MotorSpeed InitAxisYSpeed { get; private set; }

	[ExcludeType]
	[Description("The speed of the Y axis for the MOVE instruction.")]
	[Recurse]
	[Category("Configuration")]
	[DisplayName("Y Axis Speed")]
	public MotorSpeed MoveAxisYSpeed { get; private set; }

	[ExcludeType]
	[Recurse]
	[Category("Configuration")]
	[DisplayName("X Axis Speed")]
	[Description("The speed of the X axis for the MOVE instruction.")]
	public MotorSpeed MoveAxisXSpeed { get; private set; }

	[Category("Configuration")]
	[Description("The speed of the combined X and Y axis for the MOVE instruction.")]
	[DisplayName("XY Axis Speed")]
	[ExcludeType]
	[Recurse]
	public MotorSpeed MoveAxisXYSpeed { get; private set; }

	[Description("If value is greater than 0, wait value ms after turning on the ringlight during sync.")]
	[Category("Configuration")]
	[XmlConfiguration(DefaultValue = "500")]
	[DisplayName("Sync Ringlight Warmup Time")]
	public int SyncRinglightWarmupTime { get; set; }

	[Category("Behavior")]
	[DisplayName("Move Timeout")]
	[Description("The timeout (in milliseconds) for an ARCUS move.")]
	[XmlConfiguration(DefaultValue = "30000")]
	public int MoveTimeout { get; set; }

	[Category("Behavior")]
	[DisplayName("Qlm Timed Extend")]
	[Description("If set to true, will time-extend the fingers using the QlmExtendTime, ignoring the extend sensor.")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool QlmTimedExtend { get; set; }

	[Description("The timeout value for a gripper extend/retract operation.")]
	[DisplayName("Timeout for a gripper extend/retract operation.")]
	[XmlConfiguration(DefaultValue = "2000")]
	[Category("Behavior")]
	public int GripperArmExtendRetractTimeout { get; set; }

	[Description("If set to Yes, validate the position after a move.")]
	[DisplayName("Validate Position After Move")]
	[Category("Configuration")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool PrintEncoderPositionAfterMove2 { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Additional PUT push.")]
	[Category("Behavior")]
	[Description("If set to true ( the default ), PUT will perform an extra gripper push to seat the disk.")]
	public bool AdditionalPutPush { get; set; }

	[Description("If set to true ( the default ), qlm/thin/qlm-thin will reboot when detecting an Arcus problem after engage/disengage.")]
	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	[DisplayName("Reboot Kiosk During QLM")]
	public bool RebootKioskDuringQlm { get; set; }

	[Category("Behavior")]
	[Description("If set to true, clean/thin will dump the zone after the job.")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Dump VMZ State")]
	public bool DumpVMZState { get; set; }

	[DisplayName("Correct Motion Controller Communication")]
	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	[Description("If set to true ( the default ), after a QLM engage/disengage, if HAL can't communicate with the Arcus, it will try to reset the DIEs.")]
	public bool CorrectMotionControllerCommunication { get; set; }

	[XmlConfiguration(DefaultValue = "15")]
	[Category("Behavior")]
	[DisplayName("Vend Disk Poll Count")]
	[Description("The number of times, during a VEND, to poll for a taken disk.")]
	public int VendDiskPollCount { get; set; }

	[XmlConfiguration(DefaultValue = "20")]
	[Category("Behavior")]
	[DisplayName("Sync Error Threshold")]
	[Description("The number of errors encountered before exiting a sync.")]
	public int SyncErrorThreshold { get; set; }

	[XmlConfiguration(DefaultValue = "10")]
	[Category("Behavior")]
	[DisplayName("Reject At Door Attempts")]
	[Description("The number of attempts to reject a disk at the vend door.")]
	public int RejectAtDoorAttempts { get; set; }

	[Description("The number of errors encountered before exiting a sync.")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Error Sync On Camera Failure")]
	public bool ErrorSyncOnCameraFailure { get; set; }

	[Description("If true, will try to move the vend door to the appropriate sensor on the AUX board.")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Move Vend Door To AUX Sensor")]
	public bool MoveVendDoorToAuxSensor { get; set; }

	[Category("Behavior")]
	[Description("If true, query the current position before moving; if at the target position, wont' request a move.")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Query Position For Vend Move")]
	public bool QueryPositionForVendMove { get; set; }

	[XmlConfiguration(DefaultValue = "500")]
	[Category("Behavior")]
	[DisplayName("QLM Approach Offset")]
	[Description("If true, will try to move the vend door to the appropriate sensor on the AUX board.")]
	public int QlmApproachOffset { get; set; }

	[Description("If true, will try to move the vend door to the appropriate sensor on the AUX board.")]
	[DisplayName("QLM Y Offset")]
	[XmlConfiguration(DefaultValue = "50")]
	[Category("Behavior")]
	public int QlmYOffset { get; set; }

	[XmlConfiguration(DefaultValue = "1500")]
	[DisplayName("QLM Extend Timeout")]
	[Description("The amount of time the gripper will overrun the limit switch beforing raising a timeout while doing a grab on the QLM.")]
	[Category("Behavior")]
	public int QlmExtendTime { get; set; }

	[DisplayName("Min Init Wakeup Time")]
	[XmlConfiguration(DefaultValue = "5000")]
	[Category("Configuration")]
	[Description("The time, in milliseconds, for the init job to wait for the ports to come up.")]
	[Browsable(false)]
	public int MinInitWakeupTime { get; set; }

	[Browsable(false)]
	[DisplayName("Allow Program Cache Replacement")]
	[Description("If set to true, allows the program cache to accept overwrites of programs.")]
	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	public bool AllowProgramCacheReplacement2 { get; set; }

	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	[DisplayName("Check Gripper Arm Sensors On Move")]
	[Description("If set to true, will check both the forward and retract sensors if they're both set, and if they are, won't move.")]
	public bool CheckGripperArmSensorsOnMove { get; set; }

	[DisplayName("Gripper Rent On Move")]
	[Description("If set to true, will check if the gripper is in the rent position, and if not, will move it to Rent.")]
	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	public bool GripperRentOnMove { get; set; }

	[Description("If set to true, mark duplicate barcodes as UNKNOWN.")]
	[DisplayName("Mark Duplicate Unknown")]
	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	public bool MarkDuplicatesUnknown { get; set; }

	[DisplayName("Mark Original Matrix Unknown")]
	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	[Description("If set to true, when a duplicate is recognized, will mark the original barcode as UNKNOWN.")]
	public bool MarkOriginalMatrixUnknown { get; set; }

	[Category("Behavior")]
	[Description("If set to true, will do a slot test, and if the slot is full, will rotate the drum 5 slots during an unknown removal if unable to grab a disk.")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Rotate Drum During Unknown Removal")]
	public bool RotateDrumDuringUnknownRemoval2 { get; set; }

	[Description("If set to true, will leave a CAMERA CAPTURE result during a return job if duplicates are detected.")]
	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	[DisplayName("Leave Duplicate Result In Return")]
	public bool LeaveDuplicateResultInReturn { get; set; }

	[Category("Behavior")]
	[Description("If set to true, perform a PEEK (test slot) on the slot if GET returns SLOTEMPTY.")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Test Slot On Empty")]
	public bool TestSlotOnEmpty { get; set; }

	[XmlConfiguration(DefaultValue = "2")]
	[Category("Behavior")]
	[DisplayName("Mark Location Unknown Threshold")]
	[Description("The count for errors before the slot is marked UNKNOWN.")]
	public int MarkLocationUnknownThreshold { get; set; }

	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	[DisplayName("Track Push Out Failures")]
	[Description("If set to true, will fail a vend after 2 push out tries show any sensors 1-4 lit.")]
	[Browsable(false)]
	public bool TrackPushOutFailures { get; set; }

	[Category("Behavior")]
	[Description("If set to true, will track problems with slots.")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Track Problem Locations")]
	public bool TrackProblemLocations { get; set; }

	[Category("Configuration")]
	[Description("If greater than 0, will spin for the configured time in ms after clearing sensor 4 during a vend disk.")]
	[XmlConfiguration(DefaultValue = "50")]
	[DisplayName("Push Out Sleep Time")]
	public int PushOutSleepTime2 { get; set; }

	[Description("If true, try to clear the picker more aggressively during a PUT.")]
	[DisplayName("AggressiveClearPickerOnPut")]
	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	public bool AggressiveClearPickerOnPut { get; set; }

	[Category("Configuration")]
	[XmlConfiguration(DefaultValue = "15")]
	[Description("If Empty or stuck count exceeds this number during vend in a 24 hour period, generate a result that should trigger an alert.")]
	[DisplayName("EmptyStuckAlertThreshold")]
	public int EmptyStuckAlertThreshold { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	[DisplayName("Alert For Empty Stuck")]
	[Description("If true, will leave a RESULT code for alert generation when the empty/stuck counter has exceeded its threshold.")]
	public bool AlertForEmptyStuck { get; set; }

	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	[DisplayName("Fix Camera On Init")]
	[Description("If true, will test if the camera is generating images, and if not, will try and reset it.")]
	public bool FixCameraOnInit2 { get; set; }

	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	[Description("If true, tells the hardware-status job to include the init status of the motion controller.")]
	[DisplayName("Validate Controller Home Status")]
	public bool ValidateControllerHomeStatus { get; set; }

	[XmlConfiguration(DefaultValue = "10")]
	[DisplayName("Reboot Execution Prevention Window")]
	[Description("In minutes, the time before kiosk reboot that the scheduler prevents jobs from running.")]
	[Category("Configuration")]
	public int RebootExecutionPreventionWindow { get; set; }

	[Category("Behavior")]
	[Description("If true, will dump diagnostic information to the log about commands.")]
	[Browsable(false)]
	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Enable Command Trace")]
	public bool EnableCommandTrace { get; set; }

	[Description("The number of slots the kiosk must not to try and file disks to.")]
	[DisplayName("Return Slot Buffer")]
	[Category("Configuration")]
	[XmlConfiguration(DefaultValue = "1")]
	public int ReturnSlotBuffer { get; set; }

	[DisplayName("Quick Return Power Off Window")]
	[Category("Configuration")]
	[XmlConfiguration(DefaultValue = "0:6")]
	[Description("The number of slots the kiosk must not to try and file disks to.")]
	public string QuickReturnPowerOffWindow { get; set; }

	[Category("Configuration")]
	[XmlConfiguration(DefaultValue = "NONE")]
	[Description("The COM port on which the Quick return device is configured.")]
	[DisplayName("Quick Return Port")]
	public string QuickReturnComPort { get; set; }

	[DisplayName("Widen Arcus Tolerance")]
	[Description("If true, will give the arcus a larger tolerance window.")]
	[Browsable(false)]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool WidenArcusTolerance { get; set; }

	[Description("If true, will take the disk if the barcode can't be read.")]
	[DisplayName("Take Invalid Disk On Return")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool TakeInvalidDiskOnReturn { get; set; }

	[Category("Behavior")]
	[DisplayName("Enable Door Check In Status")]
	[Description("If set to true, will check the door sensors during the hardware status job.")]
	[XmlConfiguration(DefaultValue = "true")]
	[Browsable(false)]
	public bool EnableDoorCheckInStatus { get; set; }

	[DisplayName("Check Gen4 Camera On Init")]
	[Description("If set to true, will establish the correct driver is loaded and the device is enabled.")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "true")]
	[Browsable(false)]
	public bool CheckGen4CameraOnInit { get; set; }

	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	[Description("If set to true, will reset the camera during sync & return operations.")]
	[Browsable(false)]
	[DisplayName("Enable Camera Reset")]
	public bool EnableCameraReset { get; set; }

	[DisplayName("Fix Camera After Return")]
	[XmlConfiguration(DefaultValue = "true")]
	[Browsable(false)]
	[Category("Behavior")]
	[Description("If set to true, will reset the camera at the very end of return.")]
	public bool FixCameraAfterReturn { get; set; }

	[DisplayName("Ringlight Connector")]
	[Description("Tells the service which ringlight connector is attached..")]
	[Browsable(false)]
	[Category("Configuration")]
	[XmlConfiguration(DefaultValue = "2")]
	public int RinglightConnector { get; set; }

	[XmlConfiguration(DefaultValue = "2")]
	[Browsable(false)]
	[Category("Configuration")]
	[Description("An integer describing how many consecutive failures happen before an alert is generated from return.")]
	[DisplayName("Threshold for Camera alert")]
	public int ReturnCameraAlertThreshold { get; set; }

	[Category("Behavior")]
	[Browsable(false)]
	[DisplayName("Reset Timeout Counters Weekly")]
	[XmlConfiguration(DefaultValue = "true")]
	[Description("If set to true, will reset the hardware timeout counters every week on Sunday morning.")]
	public bool ResetTimeoutCountersWeekly { get; set; }

	[Category("Behavior")]
	[DisplayName("Restart Controller During User Jobs")]
	[Browsable(false)]
	[Description("If true, will restart the controller during a user job.")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool RestartControllerDuringUserJobs { get; set; }

	[Browsable(false)]
	[Category("Configuration")]
	[DisplayName("Redeploy Segment Offset")]
	[Description("An integer describing the offset from the start of a quadrant used for redeploy.")]
	[XmlConfiguration(DefaultValue = "5")]
	public int RedeploySegmentOffset { get; set; }

	[DisplayName("Track Hardware Corrections")]
	[XmlConfiguration(DefaultValue = "false")]
	[Description("If true, will track hardware correction stats.")]
	[Browsable(false)]
	[Category("Behavior")]
	public bool TrackHardwareCorrections { get; set; }

	[Category("Behavior")]
	[Browsable(false)]
	[Description("If true, will schedule a healing test.")]
	[DisplayName("Enable Hardware Healing")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool EnableHardwareHealing { get; set; }

	[DisplayName("Enable Arcus Healing")]
	[Browsable(false)]
	[Category("Behavior")]
	[Description("Sets the mask of options for the hardware healer.")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool EnableArcusHealing { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[Description("If true, will send ComStatus messages to the Arcus. The EnableHardwareHealing option must be set.")]
	[DisplayName("Enable Arcus Keepalive")]
	[Category("Behavior")]
	public bool EnableArcusKeepalive { get; set; }

	[Description("Sets the mask of options for the hardware healer.")]
	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Enable Camera Healing")]
	[Browsable(false)]
	[Category("Behavior")]
	public bool EnableCameraHealing { get; set; }

	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	[Description("If true, the service will auto-schedule a clean.")]
	[Browsable(false)]
	[DisplayName("Enable Auto VMZ Clean")]
	public bool EnableAutoVMZClean { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[Category("Behavior")]
	[Description("Sets the mask of options for the hardware healer.")]
	[DisplayName("Enable Leiwe Healing")]
	public bool EnableLeiweHealing { get; set; }

	[DisplayName("Enable Ice Qube Polling")]
	[Browsable(false)]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "false")]
	[Description("If true, will enable polling & control of Ice Qube board.")]
	public bool EnableIceQubePolling { get; set; }

	[XmlConfiguration(DefaultValue = "90")]
	[Category("Configuration")]
	[Browsable(false)]
	[DisplayName("Watchdog Image Day Age")]
	[Description("In days, the age of images to be deleted.")]
	public int WatchdogImageDayAge { get; set; }

	[DisplayName("Applog Day Age")]
	[XmlConfiguration(DefaultValue = "60")]
	[Category("Configuration")]
	[Browsable(false)]
	[Description("In days, the age of applogs to be deleted.")]
	public int ApplogDayAge { get; set; }

	[Category("Behavior")]
	[DisplayName("Fix Arcus During Status")]
	[Browsable(false)]
	[XmlConfiguration(DefaultValue = "false")]
	[Description("If true, will try to correct an Arcus comm error during the hardware status job.")]
	public bool FixArcusDuringStatusJob { get; set; }

	[DisplayName("Enable Secure Disk Validator")]
	[Description("If true, will enable secure disk check in security service.")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	public bool EnableSecureDiskValidator { get; set; }

	[DisplayName("Fraud Camera Scan Pause")]
	[Browsable(false)]
	[XmlConfiguration(DefaultValue = "200")]
	[Description("In ms, the time the scan will pause before sensor check.")]
	[Category("Configuration")]
	public int FraudCameraScanPause { get; set; }

	[Description("If true, will PEEK slot during vend.")]
	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Peek During Vend")]
	public bool PeekDuringVend { get; set; }

	[DisplayName("Test Slot During Removal")]
	[Category("Behavior")]
	[Browsable(false)]
	[XmlConfiguration(DefaultValue = "false")]
	[Description("If true, will PEEK slot during vend-unknown.")]
	public bool TestSlotDuringRemoval { get; set; }

	[Description("In ms, the time wait sensor routine will pause between reads.")]
	[Category("Configuration")]
	[DisplayName("Wait Sensor Pause Time")]
	[XmlConfiguration(DefaultValue = "100")]
	[Browsable(false)]
	public int WaitSensorPauseTime { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Clear Picker On Home")]
	[Category("Behavior")]
	[Browsable(false)]
	[Description("If true, will clear the picker during HOME operations.")]
	public bool ClearPickerOnHome { get; set; }

	[Category("Configuration")]
	[XmlConfiguration(DefaultValue = "300")]
	[Browsable(false)]
	[DisplayName("Fraud Sensor POST Wait")]
	[Description("In ms, the time to wait after power cycle to read inputs.")]
	public int FraudSensorPostWait { get; set; }

	[Category("Behavior")]
	[Description("If true, will leave fraud scan results for the engine.")]
	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[DisplayName("Enable Fraud Scan Results")]
	public bool EnableFraudScanResults { get; set; }

	[Description("If true, will enable fraud sensor POST checks during idle time.")]
	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Enable Fraud Sensor Check")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool EnableFraudSensorCheck { get; set; }

	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "false")]
	[Description("If true, will enable fraud sensor diagnostic logging.")]
	[Browsable(false)]
	[DisplayName("Enable Fraud Sensor Trace")]
	public bool EnableFraudSensorTrace { get; set; }

	[Browsable(false)]
	[Description("If true, will enable fraud sensor post diagnostic logging.")]
	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Enable Fraud Sensor Post Trace")]
	[Category("Behavior")]
	public bool EnableFraudSensorPostTrace { get; set; }

	[XmlConfiguration(DefaultValue = "200")]
	[Browsable(false)]
	[Category("Configuration")]
	[DisplayName("Enable Fraud Reset End Wait")]
	[Description("If true, will enable fraud sensor diagnostic logging.")]
	public int FraudResetEndWait { get; set; }

	[XmlConfiguration(DefaultValue = "3600000")]
	[Description("In ms, the time the fraud post poller pauses between POSTs.")]
	[Browsable(false)]
	[Category("Configuration")]
	[DisplayName("Fraud Post Poller Pause Time")]
	public int FraudPostPollerPauseTime { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Pause Before Post Read End")]
	[Description("If true, will pause before reading the picker inputs at the end of post.")]
	[Browsable(false)]
	[Category("Behavior")]
	public bool PauseBeforePostReadEnd { get; set; }

	[Category("Configuration")]
	[Browsable(false)]
	[DisplayName("Fraud Reset Pause")]
	[XmlConfiguration(DefaultValue = "500")]
	[Description("Time in ms that the fraud reset routine will pause between stop & initialize.")]
	public int FraudResetPause { get; set; }

	[Browsable(false)]
	[Description("Bitmask indicating which jobs to leave scan results in.")]
	[XmlConfiguration(DefaultValue = "3")]
	[DisplayName("Fraud Scan Job Bitmask")]
	[Category("Configuration")]
	public int FraudScanJobBitmask { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	[Description("If true, will treat lower limit as error during normal moves.")]
	[Browsable(false)]
	[DisplayName("Lower Limit As Error")]
	public bool LowerLimitAsError { get; set; }

	[XmlConfiguration(DefaultValue = "true")]
	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Enable Fraud POST Tracking")]
	[Description("If true, will capture metrics on fraud POST.")]
	public bool EnableFraudPostTracking { get; set; }

	[Browsable(false)]
	public bool IsVMZMachine => MerchandizingMode.QLM != MerchMode;

	protected override string FileNodeName => "Controller";

	internal int RegistrySlotCount
	{
		get
		{
			int result = 90;
			if (ControllerMode.Normal == Mode)
			{
				RegistryKey registryKey = null;
				try
				{
					registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store");
					if (registryKey != null)
					{
						object value = registryKey.GetValue("MaxSlots");
						if (value != null)
						{
							result = (int)ConversionHelper.ChangeType(value, typeof(int));
						}
					}
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("Unable to read secret option 'MaxSlots'", e);
					result = -1;
				}
				finally
				{
					registryKey?.Close();
				}
			}
			return result;
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
		return new string[1] { "Arcus" };
	}

	public void ConvertDeckConfigurationToXml(XmlWriter writer)
	{
		Manager.ToPropertyXml(writer);
	}

	public void UpdateDeckConfigurationFromXml(XmlNode propertyNode)
	{
		Manager.UpdateFromPropertyXml(propertyNode);
	}

	protected override void ImportInner(ErrorList errors)
	{
		GampHelper gampHelper = new GampHelper();
		int platterSlots = gampHelper.GetPlatterSlots();
		if (platterSlots == -1)
		{
			errors.Add(Error.NewError("H322", "The Slotdata.dat file was not found or had no entries.", "Check the C:\\gamp folder to ensure the file exists."));
			return;
		}
		int registrySlotCount = RegistrySlotCount;
		if (platterSlots != registrySlotCount)
		{
			errors.Add(Error.NewError("H323", "Configuration mismatch error.", $"The machine is configured for {registrySlotCount} slots; the gamp data says there are {platterSlots} slots."));
			return;
		}
		int? num = null;
		int? num2 = null;
		List<int> list = new List<int>();
		List<bool> list2 = new List<bool>();
		IDictionary<string, int> dictionary = gampHelper.ReadLegacySystemDataFile();
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
		List<List<int>> list3 = gampHelper.ReadLegacySlotDataFile(removeZeroes: false);
		Manager.Clear();
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
			bool isQlm = num.HasValue && num3 == num.Value;
			Manager.Add(new Deck(num3, item2[0], isQlm, num4, (num4 == 72) ? 173.3m : num5, (num4 == 72) ? new int?(6) : ((int?)null), (num4 == 72) ? num2 : ((int?)null), list4.ToArray()));
			num3++;
		}
	}

	protected override void UpgradeInner(XmlDocument document, ErrorList errors)
	{
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
		Manager.SaveConfiguration(document);
		ConfigureMerchMode();
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
		if (xmlNode8 == null)
		{
			LogHelper.Instance.Log("There are no decks nodes in the configuration file.");
			errors.Add(Error.NewError("P006", "No configured decks.", "There are no decks nodes in the configuration file."));
			return;
		}
		Manager.Initialize(xmlNode8);
		int registrySlotCount = RegistrySlotCount;
		IDeck first = Manager.First;
		if (registrySlotCount != first.NumberOfSlots)
		{
			errors.Add(Error.NewError("P005", "Machine configuration mismatch.", $"The store key shows {registrySlotCount}, but the deck shows {first.NumberOfSlots}"));
		}
		ConfigureMerchMode();
	}

	private void ConfigureMerchMode()
	{
		IDeck qlmDeck = Manager.QlmDeck;
		MerchMode = ((qlmDeck == null) ? MerchandizingMode.VMZ_Gen2 : MerchandizingMode.QLM);
		LogHelper.Instance.Log(LogEntryType.Info, "Kiosk is configured for merchandizing mode {0}", MerchMode.ToString().ToUpper());
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
