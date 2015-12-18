using System;
using System.IO;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public abstract class MotionController
{
	internal const string SuccessResponse = "OK";

	internal const string TimeoutResponse = "TIMEOUT";

	internal const string InvalidMotorStateResponse = "INVALID MOTOR STATE";

	internal const string CommunicationErrorResponse = "COMMUNICATION ERROR";

	internal const string DoorSensorTrippedResponse = "DOOR OPEN";

	private TextWriter m_logFile;

	private static MotionController m_instance;

	private TextWriter LogFile
	{
		get
		{
			if (m_logFile == null)
			{
				string path = Path.Combine(ControllerConfiguration.Instance.KioskLogsPath, "Service");
				string path2 = Path.Combine(path, "MotionControlErrorLog.log");
				m_logFile = new StreamWriter(File.Open(path2, FileMode.Append, FileAccess.Write, FileShare.Read))
				{
					AutoFlush = true
				};
			}
			return m_logFile;
		}
	}

	public static MotionController GetController()
	{
		if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode)
		{
			if (m_instance == null)
			{
				m_instance = new EmulatedMotionController();
			}
			return m_instance;
		}
		if (m_instance == null)
		{
			m_instance = ArcusMotionController.MakeInstance();
		}
		return m_instance;
	}

	public static bool Reset(IFormattedLog log, bool quick)
	{
		MotionController controller = GetController();
		if (!controller.ResetDeviceDriver())
		{
			LogHelper.Instance.Log("[Reset Hardware]: ResetDevice driver returned FALSE");
			return false;
		}
		controller.InitializeCommunication();
		if (!controller.CommunicationOk())
		{
			LogHelper.Instance.Log("[Reset Hardware]: After reset, channel still down. I'm sorry.");
			return false;
		}
		using HomeAxesResult homeAxesResult = controller.InitAxes(log, quick);
		LogHelper.Instance.Log("[Reset Hardware]: Restart returned {0}", homeAxesResult.Succeded);
		return homeAxesResult.Succeded;
	}

	public string[] ReadLimits()
	{
		return ReadLimitsInner();
	}

	public abstract bool CommunicationOk();

	public bool IsInitialized()
	{
		if (!CommunicationOk())
		{
			return false;
		}
		return IsInitializedInner();
	}

	public bool ClosePort()
	{
		return ClosePortInner();
	}

	public MotionResult MoveToTargetWithoutSensorCheck(MoveTarget target)
	{
		return MoveToTargetInner(target);
	}

	public MotionResult MoveToTarget(MoveTarget target)
	{
		if (!IsInitializedInner())
		{
			return MotionResult.MotorNotHomed;
		}
		MotionResult motionResult = CanMove();
		if (motionResult != MotionResult.Success)
		{
			return motionResult;
		}
		ControllerService.Instance.ResetLocation();
		return MoveToTargetInner(target);
	}

	public MotionResult HomeAxis(Axis axis)
	{
		if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
		{
			LogHelper.Instance.Log($"ControllerService.HomeAxis {axis.ToString()}", LogEntryType.Debug);
		}
		MotionResult motionResult = CanMove();
		if (motionResult != MotionResult.Success)
		{
			return motionResult;
		}
		ControllerService.Instance.ResetLocation();
		MotionResult motionResult2 = HomeAxisInner(axis);
		if (motionResult2 != MotionResult.Success)
		{
			LogHelper.Instance.Log(LogEntryType.Error, "The HOME{0} failed with status {1}", axis.ToString().ToUpper(), motionResult2.ToString().ToUpper());
		}
		return motionResult2;
	}

	public abstract bool GetCurrentLocation(Axis axis, ref MoveTarget target);

	public abstract bool IsMotorErrored(Axis axis, out MotionResult status);

	public HomeAxesResult InitAxes(IFormattedLog log)
	{
		return InitAxes(log, quick: false);
	}

	public HomeAxesResult InitAxes(IFormattedLog log, bool quick)
	{
		Gripper.ClearFront();
		return InitAxesInner(log, quick);
	}

	protected abstract bool ClosePortInner();

	protected abstract HomeAxesResult InitAxesInner(IFormattedLog log, bool quick);

	protected abstract bool IsInitializedInner();

	protected abstract MotionResult MoveToTargetInner(MoveTarget target);

	protected abstract MotionResult MoveToVendInner(MoveMode mode);

	protected abstract MotionResult HomeAxisInner(Axis axis);

	protected abstract string[] ReadLimitsInner();

	protected MotionResult CheckDoorSensors()
	{
		DoorSensorResult doorSensorResult = DoorSensors.Instance.Query();
		if (doorSensorResult == DoorSensorResult.Ok)
		{
			return MotionResult.Success;
		}
		LogHelper.Instance.Log($"Door sensor query returned {doorSensorResult.ToString()}", LogEntryType.Error);
		return MotionResult.DoorOpen;
	}

	protected MotionResult CanMove()
	{
		MotionResult result = CheckDoorSensors();
		if (result != MotionResult.Success)
		{
			return result;
		}
		if (!ControllerService.Instance.CanMove(out result))
		{
			return result;
		}
		return MotionResult.Success;
	}

	internal MotionResult MoveToVend(MoveMode mode)
	{
		if (!IsInitializedInner())
		{
			return MotionResult.MotorNotHomed;
		}
		MotionResult motionResult = CanMove();
		if (motionResult != MotionResult.Success)
		{
			return motionResult;
		}
		ControllerService.Instance.ResetLocation();
		return MoveToVendInner(mode);
	}

	internal void WriteToLog(string msg)
	{
		DateTime now = DateTime.Now;
		string value = $"{now.ToShortDateString()} {now.ToShortTimeString()} {msg}";
		LogFile.WriteLine(value);
	}

	internal abstract bool ResetDeviceDriver();

	internal abstract void InitializeCommunication();
}
