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

	[Description("The amount of time the gripper will overrun the limit switch beforing raising a timeout while testing for a DVD in the slot.")]
	[DisplayName("Test Extend Timeout")]
	[XmlConfiguration(DefaultValue = "800")]
	[Category("Behavior")]
	public int TestExtendTime { get; set; }

	[Description("The amount of backoff to back off when receiving an item from the vend position.")]
	[XmlConfiguration(DefaultValue = "100")]
	[DisplayName("Vend Position Receive Offset")]
	[Category("Behavior")]
	public int VendPositionReceiveOffset { get; set; }

	[Category("Behavior")]
	[DisplayName("Arcus Write Pause")]
	[Description("Controls how long (in milliseconds) HAL waits for the Arcus controller to fill its internal buffer before reading it.")]
	[XmlConfiguration(DefaultValue = "20")]
	public int ArcusWritePause { get; set; }

	[Description("Controls how long (in milliseconds) HAL waits between MST commands during a move.")]
	[DisplayName("Arcus Motor Query Pause")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "75")]
	public int ArcusMotorQueryPause { get; set; }

	[Category("Configuration")]
	[DisplayName("Y Position for Vend")]
	[Description("Known as the vend position, this is the point on the Y axis that is equal to the vend door, allowing the picker to vend and return disks. This must be a negative integer value.")]
	[XmlConfiguration(DefaultValue = "-86200")]
	public int VendYPosition { get; set; }

	[DisplayName("Y drop back for HOMEY")]
	[XmlConfiguration(DefaultValue = "0")]
	[Category("Behavior")]
	[Description("The amount of back off movement on the Y axis during a HOMEY instruction.  This value should be used only on older machines where the upper Y axis limit switch is too close to the Y axis home switch.")]
	public int HomeYDropBack { get; set; }

	[XmlConfiguration(DefaultValue = "4")]
	[Category("Behavior")]
	[DisplayName("Attempts to put an item away.")]
	[Description("The number of attempts to try and put something in the picker away.")]
	public int PutAwayItemAttempts { get; set; }

	[Description("The mode under which the controller driver runs.  If set to Emulated, all hardware instructions will be emulated based on the configured emulation state.")]
	[ValidValueListProvider("GetModes")]
	[XmlConfiguration(DefaultValue = "Normal")]
	[Category("Configuration")]
	[DisplayName("Controller Mode")]
	public ControllerMode Mode { get; set; }

	[XmlConfiguration(DefaultValue = "2")]
	[DisplayName("Number of Pulls")]
	[Description("The number of times the picker will attempt to pull the disk from the slot.")]
	[Category("Behavior")]
	public int NumberOfPulls { get; set; }

	[DisplayName("Motion Controller Port Name")]
	[ValidValueListProvider("GetComPorts")]
	[XmlConfiguration(DefaultValue = "COM3")]
	[Category("Configuration")]
	[Description("The COM port to which to which the motion controller board is connected.")]
	public string MotionControllerPortName { get; set; }

	[Description("The read timeout from the Motion Controller COM Port in milliseconds.")]
	[XmlConfiguration(DefaultValue = "5000")]
	[DisplayName("Motion Controller Timeout (ms)")]
	[Category("Configuration")]
	public int MotionControllerTimeout { get; private set; }

	[Category("Configuration")]
	[Recurse]
	[ExcludeType]
	[DisplayName("X Motor Gear")]
	[Description("The X axis motor gear configuration.  The configuration of this property and its children affect the X Axis Speed properties.")]
	public MotorGear GearX { get; private set; }

	[Description("The Y axis motor gear configuration.  The configuration of this property and its children affect the Y Axis Speed properties.")]
	[Category("Configuration")]
	[DisplayName("Y Motor Gear")]
	[Recurse]
	[ExcludeType]
	public MotorGear GearY { get; private set; }

	[XmlConfiguration(DefaultValue = "COM1")]
	[Category("Configuration")]
	[DisplayName("Controller Port Name")]
	[ValidValueListProvider("GetComPorts")]
	[Description("The COM port the main, picker, auxiliary, and alternate motion controller boards are connected to.")]
	public string ControllerPortName { get; set; }

	[Recurse]
	[ExcludeType]
	[Category("Configuration")]
	[DisplayName("X Axis Initialization Speed")]
	[Description("The speed of the X axis for the HOMEX instruction.")]
	public MotorSpeed InitAxisXSpeed { get; private set; }

	[Category("Configuration")]
	[DisplayName("Y Axis Initialization Speed")]
	[Description("The speed of the Y axis for the HOMEY instruction.")]
	[ExcludeType]
	[Recurse]
	public MotorSpeed InitAxisYSpeed { get; private set; }

	[Description("The speed of the Y axis for the MOVE instruction.")]
	[ExcludeType]
	[Category("Configuration")]
	[Recurse]
	[DisplayName("Y Axis Speed")]
	public MotorSpeed MoveAxisYSpeed { get; private set; }

	[Category("Configuration")]
	[Recurse]
	[DisplayName("X Axis Speed")]
	[Description("The speed of the X axis for the MOVE instruction.")]
	[ExcludeType]
	public MotorSpeed MoveAxisXSpeed { get; private set; }

	[DisplayName("XY Axis Speed")]
	[Description("The speed of the combined X and Y axis for the MOVE instruction.")]
	[ExcludeType]
	[Recurse]
	[Category("Configuration")]
	public MotorSpeed MoveAxisXYSpeed { get; private set; }

	[DisplayName("Sync Ringlight Warmup Time")]
	[Category("Configuration")]
	[XmlConfiguration(DefaultValue = "500")]
	[Description("If value is greater than 0, wait value ms after turning on the ringlight during sync.")]
	public int SyncRinglightWarmupTime { get; set; }

	[Category("Behavior")]
	[DisplayName("Move Timeout")]
	[Description("The timeout (in milliseconds) for an ARCUS move.")]
	[XmlConfiguration(DefaultValue = "30000")]
	public int MoveTimeout { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	[Description("If set to true, will time-extend the fingers using the QlmExtendTime, ignoring the extend sensor.")]
	[DisplayName("Qlm Timed Extend")]
	public bool QlmTimedExtend { get; set; }

	[Description("The timeout value for a gripper extend/retract operation.")]
	[XmlConfiguration(DefaultValue = "2000")]
	[DisplayName("Timeout for a gripper extend/retract operation.")]
	[Category("Behavior")]
	public int GripperArmExtendRetractTimeout { get; set; }

	[Category("Configuration")]
	[Description("If set to Yes, validate the position after a move.")]
	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Validate Position After Move")]
	public bool PrintEncoderPositionAfterMove2 { get; set; }

	[DisplayName("Additional PUT push.")]
	[Description("If set to true ( the default ), PUT will perform an extra gripper push to seat the disk.")]
	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	public bool AdditionalPutPush { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	[DisplayName("Reboot Kiosk During QLM")]
	[Description("If set to true ( the default ), qlm/thin/qlm-thin will reboot when detecting an Arcus problem after engage/disengage.")]
	public bool RebootKioskDuringQlm { get; set; }

	[XmlConfiguration(DefaultValue = "0")]
	[Category("Configuration")]
	[DisplayName("Move Correction Flags")]
	[Description("Set of flags describing which jobs can reset the motion controller when not responsive.")]
	[Browsable(false)]
	public int MoveCorrectionFlags { get; set; }

	[Category("Behavior")]
	[Description("If set to true, clean/thin will dump the zone after the job.")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Dump VMZ State")]
	public bool DumpVMZState { get; set; }

	[Category("Behavior")]
	[Description("If set to true ( the default ), after a QLM engage/disengage, if HAL can't communicate with the Arcus, it will try to reset the DIEs.")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Correct Motion Controller Communication")]
	public bool CorrectMotionControllerCommunication { get; set; }

	[Description("The number of times, during a VEND, to poll for a taken disk.")]
	[XmlConfiguration(DefaultValue = "15")]
	[Category("Behavior")]
	[DisplayName("Vend Disk Poll Count")]
	public int VendDiskPollCount { get; set; }

	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "20")]
	[Description("The number of errors encountered before exiting a sync.")]
	[DisplayName("Sync Error Threshold")]
	public int SyncErrorThreshold { get; set; }

	[DisplayName("Reject At Door Attempts")]
	[XmlConfiguration(DefaultValue = "10")]
	[Category("Behavior")]
	[Description("The number of attempts to reject a disk at the vend door.")]
	public int RejectAtDoorAttempts { get; set; }

	[DisplayName("Error Sync On Camera Failure")]
	[Category("Behavior")]
	[Description("The number of errors encountered before exiting a sync.")]
	[XmlConfiguration(DefaultValue = "true")]
	public bool ErrorSyncOnCameraFailure { get; set; }

	[Category("Behavior")]
	[DisplayName("Move Vend Door To AUX Sensor")]
	[Description("If true, will try to move the vend door to the appropriate sensor on the AUX board.")]
	[XmlConfiguration(DefaultValue = "true")]
	public bool MoveVendDoorToAuxSensor { get; set; }

	[XmlConfiguration(DefaultValue = "true")]
	[Description("If true, query the current position before moving; if at the target position, wont' request a move.")]
	[Category("Behavior")]
	[DisplayName("Query Position For Vend Move")]
	public bool QueryPositionForVendMove { get; set; }

	[XmlConfiguration(DefaultValue = "500")]
	[Category("Behavior")]
	[DisplayName("QLM Approach Offset")]
	[Description("If true, will try to move the vend door to the appropriate sensor on the AUX board.")]
	public int QlmApproachOffset { get; set; }

	[Category("Behavior")]
	[DisplayName("QLM Y Offset")]
	[Description("If true, will try to move the vend door to the appropriate sensor on the AUX board.")]
	[XmlConfiguration(DefaultValue = "50")]
	public int QlmYOffset { get; set; }

	[DisplayName("QLM Extend Timeout")]
	[Description("The amount of time the gripper will overrun the limit switch beforing raising a timeout while doing a grab on the QLM.")]
	[XmlConfiguration(DefaultValue = "1500")]
	[Category("Behavior")]
	public int QlmExtendTime { get; set; }

	[Description("The time, in milliseconds, for the init job to wait for the ports to come up.")]
	[DisplayName("Min Init Wakeup Time")]
	[Browsable(false)]
	[XmlConfiguration(DefaultValue = "5000")]
	[Category("Configuration")]
	public int MinInitWakeupTime { get; set; }

	[DisplayName("Allow Program Cache Replacement")]
	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	[Browsable(false)]
	[Description("If set to true, allows the program cache to accept overwrites of programs.")]
	public bool AllowProgramCacheReplacement2 { get; set; }

	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Check Gripper Arm Sensors On Move")]
	[Description("If set to true, will check both the forward and retract sensors if they're both set, and if they are, won't move.")]
	public bool CheckGripperArmSensorsOnMove { get; set; }

	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "true")]
	[Description("If set to true, will check if the gripper is in the rent position, and if not, will move it to Rent.")]
	[DisplayName("Gripper Rent On Move")]
	public bool GripperRentOnMove { get; set; }

	[Category("Behavior")]
	[DisplayName("Mark Duplicate Unknown")]
	[Description("If set to true, mark duplicate barcodes as UNKNOWN.")]
	[XmlConfiguration(DefaultValue = "true")]
	public bool MarkDuplicatesUnknown { get; set; }

	[Category("Behavior")]
	[DisplayName("Mark Original Matrix Unknown")]
	[Description("If set to true, when a duplicate is recognized, will mark the original barcode as UNKNOWN.")]
	[XmlConfiguration(DefaultValue = "true")]
	public bool MarkOriginalMatrixUnknown { get; set; }

	[Description("If set to true, will do a slot test, and if the slot is full, will rotate the drum 5 slots during an unknown removal if unable to grab a disk.")]
	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	[DisplayName("Rotate Drum During Unknown Removal")]
	public bool RotateDrumDuringUnknownRemoval2 { get; set; }

	[DisplayName("Leave Duplicate Result In Return")]
	[Description("If set to true, will leave a CAMERA CAPTURE result during a return job if duplicates are detected.")]
	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	public bool LeaveDuplicateResultInReturn { get; set; }

	[Category("Behavior")]
	[DisplayName("Test Slot On Empty")]
	[XmlConfiguration(DefaultValue = "true")]
	[Description("If set to true, perform a PEEK (test slot) on the slot if GET returns SLOTEMPTY.")]
	public bool TestSlotOnEmpty { get; set; }

	[Description("The count for errors before the slot is marked UNKNOWN.")]
	[XmlConfiguration(DefaultValue = "2")]
	[Category("Behavior")]
	[DisplayName("Mark Location Unknown Threshold")]
	public int MarkLocationUnknownThreshold { get; set; }

	[Category("Behavior")]
	[DisplayName("Track Push Out Failures")]
	[Description("If set to true, will fail a vend after 2 push out tries show any sensors 1-4 lit.")]
	[XmlConfiguration(DefaultValue = "true")]
	[Browsable(false)]
	public bool TrackPushOutFailures { get; set; }

	[Description("If set to true, will track problems with slots.")]
	[DisplayName("Track Problem Locations")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "true")]
	public bool TrackProblemLocations { get; set; }

	[DisplayName("Push Out Sleep Time")]
	[Description("If greater than 0, will spin for the configured time in ms after clearing sensor 4 during a vend disk.")]
	[Category("Configuration")]
	[XmlConfiguration(DefaultValue = "50")]
	public int PushOutSleepTime2 { get; set; }

	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("AggressiveClearPickerOnPut")]
	[Category("Behavior")]
	[Description("If true, try to clear the picker more aggressively during a PUT.")]
	public bool AggressiveClearPickerOnPut { get; set; }

	[DisplayName("EmptyStuckAlertThreshold")]
	[Description("If Empty or stuck count exceeds this number during vend in a 24 hour period, generate a result that should trigger an alert.")]
	[Category("Configuration")]
	[XmlConfiguration(DefaultValue = "15")]
	public int EmptyStuckAlertThreshold { get; set; }

	[DisplayName("Alert For Empty Stuck")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "false")]
	[Description("If true, will leave a RESULT code for alert generation when the empty/stuck counter has exceeded its threshold.")]
	public bool AlertForEmptyStuck { get; set; }

	[DisplayName("Fix Camera On Init")]
	[Description("If true, will test if the camera is generating images, and if not, will try and reset it.")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "true")]
	public bool FixCameraOnInit2 { get; set; }

	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Validate Controller Home Status")]
	[Description("If true, tells the hardware-status job to include the init status of the motion controller.")]
	[Category("Behavior")]
	public bool ValidateControllerHomeStatus { get; set; }

	[DisplayName("Reboot Execution Prevention Window")]
	[Category("Configuration")]
	[XmlConfiguration(DefaultValue = "10")]
	[Description("In minutes, the time before kiosk reboot that the scheduler prevents jobs from running.")]
	public int RebootExecutionPreventionWindow { get; set; }

	[Description("If true, will dump diagnostic information to the log about commands.")]
	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Enable Command Trace")]
	[Browsable(false)]
	[Category("Behavior")]
	public bool EnableCommandTrace { get; set; }

	[DisplayName("Return Slot Buffer")]
	[XmlConfiguration(DefaultValue = "1")]
	[Category("Configuration")]
	[Description("The number of slots the kiosk must not to try and file disks to.")]
	public int ReturnSlotBuffer { get; set; }

	[Category("Configuration")]
	[DisplayName("Quick Return Power Off Window")]
	[XmlConfiguration(DefaultValue = "0:6")]
	[Description("The number of slots the kiosk must not to try and file disks to.")]
	public string QuickReturnPowerOffWindow { get; set; }

	[DisplayName("Quick Return Port")]
	[Category("Configuration")]
	[Description("The COM port on which the Quick return device is configured.")]
	[XmlConfiguration(DefaultValue = "NONE")]
	public string QuickReturnComPort { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	[Browsable(false)]
	[DisplayName("Widen Arcus Tolerance")]
	[Description("If true, will give the arcus a larger tolerance window.")]
	public bool WidenArcusTolerance { get; set; }

	[Description("If true, will take the disk if the barcode can't be read.")]
	[DisplayName("Take Invalid Disk On Return")]
	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	public bool TakeInvalidDiskOnReturn { get; set; }

	[DisplayName("Enable Door Check In Status")]
	[Category("Behavior")]
	[Description("If set to true, will check the door sensors during the hardware status job.")]
	[XmlConfiguration(DefaultValue = "true")]
	[Browsable(false)]
	public bool EnableDoorCheckInStatus { get; set; }

	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Check Gen4 Camera On Init")]
	[Description("If set to true, will establish the correct driver is loaded and the device is enabled.")]
	[XmlConfiguration(DefaultValue = "true")]
	public bool CheckGen4CameraOnInit { get; set; }

	[Description("If set to true, will reset the camera during sync & return operations.")]
	[Browsable(false)]
	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	[DisplayName("Enable Camera Reset")]
	public bool EnableCameraReset { get; set; }

	[XmlConfiguration(DefaultValue = "true")]
	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Fix Camera After Return")]
	[Description("If set to true, will reset the camera at the very end of return.")]
	public bool FixCameraAfterReturn { get; set; }

	[DisplayName("Ringlight Connector")]
	[XmlConfiguration(DefaultValue = "2")]
	[Description("Tells the service which ringlight connector is attached..")]
	[Browsable(false)]
	[Category("Configuration")]
	public int RinglightConnector { get; set; }

	[XmlConfiguration(DefaultValue = "2")]
	[DisplayName("Threshold for Camera alert")]
	[Category("Configuration")]
	[Description("An integer describing how many consecutive failures happen before an alert is generated from return.")]
	[Browsable(false)]
	public int ReturnCameraAlertThreshold { get; set; }

	[XmlConfiguration(DefaultValue = "true")]
	[Browsable(false)]
	[Description("If set to true, will reset the hardware timeout counters every week on Sunday morning.")]
	[DisplayName("Reset Timeout Counters Weekly")]
	[Category("Behavior")]
	public bool ResetTimeoutCountersWeekly { get; set; }

	[Category("Behavior")]
	[Description("If true, will restart the controller during a user job.")]
	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Restart Controller During User Jobs")]
	[Browsable(false)]
	public bool RestartControllerDuringUserJobs { get; set; }

	[Category("Configuration")]
	[DisplayName("Redeploy Segment Offset")]
	[Description("An integer describing the offset from the start of a quadrant used for redeploy.")]
	[XmlConfiguration(DefaultValue = "5")]
	[Browsable(false)]
	public int RedeploySegmentOffset { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Description("If true, will track hardware correction stats.")]
	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Track Hardware Corrections")]
	public bool TrackHardwareCorrections { get; set; }

	[Description("If true, will schedule a healing test.")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[DisplayName("Enable Hardware Healing")]
	public bool EnableHardwareHealing { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Enable Arcus Healing")]
	[Description("Sets the mask of options for the hardware healer.")]
	public bool EnableArcusHealing { get; set; }

	[Description("Sets the mask of options for the hardware healer.")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[DisplayName("Enable Camera Healing")]
	public bool EnableCameraHealing { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[Category("Behavior")]
	[Description("Sets the mask of options for the hardware healer.")]
	[DisplayName("Enable Leiwe Healing")]
	public bool EnableLeiweHealing { get; set; }

	[Browsable(false)]
	[Description("If true, will enable polling & control of Ice Qube board.")]
	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	[DisplayName("Enable Ice Qube Polling")]
	public bool EnableIceQubePolling { get; set; }

	[DisplayName("Watchdog Image Day Age")]
	[Description("In days, the age of images to be deleted.")]
	[XmlConfiguration(DefaultValue = "90")]
	[Browsable(false)]
	[Category("Configuration")]
	public int WatchdogImageDayAge { get; set; }

	[Category("Configuration")]
	[XmlConfiguration(DefaultValue = "60")]
	[Browsable(false)]
	[DisplayName("Applog Day Age")]
	[Description("In days, the age of applogs to be deleted.")]
	public int ApplogDayAge { get; set; }

	[Browsable(false)]
	[Description("If true, will try to correct an Arcus comm error during the hardware status job.")]
	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Fix Arcus During Status")]
	[Category("Behavior")]
	public bool FixArcusDuringStatusJob { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Enable Secure Disk Validator")]
	[Description("If true, will enable secure disk check in security service.")]
	public bool EnableSecureDiskValidator { get; set; }

	[XmlConfiguration(DefaultValue = "200")]
	[Browsable(false)]
	[Category("Configuration")]
	[DisplayName("Fraud Camera Scan Pause")]
	[Description("In ms, the time the scan will pause before sensor check.")]
	public int FraudCameraScanPause { get; set; }

	[Browsable(false)]
	[DisplayName("Peek During Vend")]
	[Category("Behavior")]
	[Description("If true, will PEEK slot during vend.")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool PeekDuringVend { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Test Slot During Removal")]
	[Description("If true, will PEEK slot during vend-unknown.")]
	public bool TestSlotDuringRemoval { get; set; }

	[Browsable(false)]
	[Category("Configuration")]
	[DisplayName("Wait Sensor Pause Time")]
	[XmlConfiguration(DefaultValue = "100")]
	[Description("In ms, the time wait sensor routine will pause between reads.")]
	public int WaitSensorPauseTime { get; set; }

	[DisplayName("Clear Picker On Home")]
	[Description("If true, will clear the picker during HOME operations.")]
	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[Category("Behavior")]
	public bool ClearPickerOnHome { get; set; }

	[Browsable(false)]
	[XmlConfiguration(DefaultValue = "1000")]
	[Description("In ms, the time to wait after power cycle to read inputs.")]
	[Category("Configuration")]
	[DisplayName("Fraud Sensor POST Wait")]
	public int FraudSensorPostWait { get; set; }

	[Category("Behavior")]
	[DisplayName("Enable Fraud Sensor Check")]
	[Description("If true, will enable fraud sensor POST checks during idle time.")]
	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	public bool EnableFraudSensorCheck { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Category("Behavior")]
	[Description("If true, will enable fraud sensor diagnostic logging.")]
	[Browsable(false)]
	[DisplayName("Enable Fraud Sensor Trace")]
	public bool EnableFraudSensorTrace { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Enable Fraud Sensor Post Trace")]
	[Description("If true, will enable fraud sensor post diagnostic logging.")]
	[Browsable(false)]
	[Category("Behavior")]
	public bool EnableFraudSensorPostTrace { get; set; }

	[Browsable(false)]
	[Description("If true, will enable fraud sensor diagnostic logging.")]
	[XmlConfiguration(DefaultValue = "350")]
	[Category("Configuration")]
	[DisplayName("FraudStopScanWait")]
	public int FraudStopScanWait { get; set; }

	[Category("Configuration")]
	[XmlConfiguration(DefaultValue = "3600000")]
	[Browsable(false)]
	[DisplayName("Fraud Post Poller Pause Time")]
	[Description("In ms, the time the fraud post poller pauses between POSTs.")]
	public int FraudPostPollerPauseTime { get; set; }

	[XmlConfiguration(DefaultValue = "true")]
	[Description("If true, will pause before reading the picker inputs at the end of post.")]
	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Pause Before Post Read End")]
	public bool PauseBeforePostReadEnd { get; set; }

	[Browsable(false)]
	[Description("Time in ms that the fraud reset routine will pause between stop & initialize.")]
	[XmlConfiguration(DefaultValue = "1000")]
	[DisplayName("Fraud Reset Pause")]
	[Category("Configuration")]
	public int FraudResetPause { get; set; }

	[XmlConfiguration(DefaultValue = "0")]
	[Browsable(false)]
	[Category("Configuration")]
	[DisplayName("Fraud Scan Job Bitmask")]
	[Description("Bitmask indicating which jobs to leave scan results in.")]
	public int FraudScanJobBitmask2 { get; set; }

	[DisplayName("Lower Limit As Error")]
	[Description("If true, will treat lower limit as error during normal moves.")]
	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[Category("Behavior")]
	public bool LowerLimitAsError { get; set; }

	[Browsable(false)]
	[Description("If true, will capture metrics on fraud POST.")]
	[DisplayName("Enable Fraud POST Tracking")]
	[XmlConfiguration(DefaultValue = "true")]
	[Category("Behavior")]
	public bool EnableFraudPostTracking { get; set; }

	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Enable Unrecognized Barcode Reject")]
	[XmlConfiguration(DefaultValue = "true")]
	[Description("If true, will reject disks not recognized as Redbox barcodes during return.")]
	public bool EnableUnrecognizedBarcodeReject { get; set; }

	[Browsable(false)]
	[Category("Behavior")]
	[DisplayName("Correct Failed Fraud Post")]
	[Description("If true, try and correct a failed POST.")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool CorrectFailedFraudPost { get; set; }

	[Description("If true, will put the scanner into scan mode immediately.")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "true")]
	[Browsable(false)]
	[DisplayName("Scan Startup Immediate")]
	public bool ScanStartImmediate { get; set; }

	[DisplayName("Fraud Scan Startup Delay")]
	[Category("Configuration")]
	[XmlConfiguration(DefaultValue = "750")]
	[Description("If true, will delay the scan start for read.")]
	[Browsable(false)]
	public int FraudScanStartupDelay { get; set; }

	[Category("Behavior")]
	[Description("If true, will use smooth move motion.")]
	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[DisplayName("Arcus Smooth Move")]
	public bool ArcusSmoothMove { get; set; }

	[Browsable(false)]
	[XmlConfiguration(DefaultValue = "false")]
	[Description("If true, move below the vend door until auth signal arrives.")]
	[Category("Behavior")]
	[DisplayName("Preposition Vend Fraud Move")]
	public bool PrepositionVendFraudMove { get; set; }

	[Browsable(false)]
	[DisplayName("Fraud Scan Multi Pass")]
	[Description("If true will shuffle the disk in the picker for a secondary read if no marker was detected.")]
	[Category("Behavior")]
	[XmlConfiguration(DefaultValue = "true")]
	public bool FraudScanMultiPass { get; set; }

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
		InitAxisXSpeed = MotorSpeed.FromXmlNode(xmlNode3, GearX);
		XmlNode xmlNode4 = document.DocumentElement.SelectSingleNode("Controller/InitAxisYSpeed");
		if (xmlNode4 == null)
		{
			errors.Add(Error.NewError("P003", "The required InitYAxisSpeed node does not exist.", "Add a valid InitYAxisSpeed node to the configuration file."));
			return;
		}
		InitAxisYSpeed = MotorSpeed.FromXmlNode(xmlNode4, GearY);
		XmlNode xmlNode5 = document.DocumentElement.SelectSingleNode("Controller/MoveXAxisSpeed");
		if (xmlNode5 == null)
		{
			errors.Add(Error.NewError("P004", "The required MoveXAxisSpeed node does not exist.", "Add a valid MoveXAxisSpeed node to the configuration file."));
			return;
		}
		MoveAxisXSpeed = MotorSpeed.FromXmlNode(xmlNode5, GearX);
		XmlNode xmlNode6 = document.DocumentElement.SelectSingleNode("Controller/MoveYAxisSpeed");
		if (xmlNode6 == null)
		{
			errors.Add(Error.NewError("P004", "The required MoveYAxisSpeed node does not exist.", "Add a valid MoveYAxisSpeed node to the configuration file."));
			return;
		}
		MoveAxisYSpeed = MotorSpeed.FromXmlNode(xmlNode6, GearY);
		XmlNode xmlNode7 = document.DocumentElement.SelectSingleNode("Controller/MoveXYAxisSpeed");
		if (xmlNode7 == null)
		{
			errors.Add(Error.NewError("P004", "The required MoveXYAxisSpeed node does not exist.", "Add a valid MoveXYAxisSpeed node to the configuration file."));
			return;
		}
		MoveAxisXYSpeed = MotorSpeed.FromXmlNode(xmlNode7, GearY);
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
		IDeck qlmDeck = Manager.QlmDeck;
		MerchMode = ((qlmDeck == null) ? MerchandizingMode.VMZ_Gen2 : MerchandizingMode.QLM);
		LogHelper.Instance.Log("Kiosk is configured for merchandizing mode {0}", MerchMode.ToString().ToUpper());
	}

	internal void Initialize()
	{
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		service.RegisterConfiguration(Configurations.Controller.ToString(), this);
		ServiceLocator.Instance.AddService(typeof(IDecksService), Manager);
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
