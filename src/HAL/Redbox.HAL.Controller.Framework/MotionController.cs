using System;
using System.IO;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public abstract class MotionController : IDisposable
{
	public const string NopCommand = "NOP";

	public const string WaitCommand = "WAIT";

	public const string WaitAfterHomeCommand = "WAITH";

	public const string WaitAfterHomeYCommand = "WAITHY";

	public const string WaitAfterHomeXCommand = "WAITHX";

	public const string HomeXCommand = "HOMEX";

	public const string WaitXCommand = "WAITX";

	public const string WaitYCommand = "WAITY";

	public const string WaitYDropBackCommand = "WAITYDB";

	public const string WaitXYCommand = "WAITXY";

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
		if (ControllerConfiguration.Instance.MotionControllerType == MotionControllerType.Arcus)
		{
			if (m_instance == null)
			{
				m_instance = ArcusMotionController.MakeInstance();
			}
			return m_instance;
		}
		return null;
	}

	public string[] ReadLimits()
	{
		return ReadLimitsInner();
	}

	public void Dispose()
	{
	}

	public abstract bool ResetDeviceDriver();

	public abstract bool CommunicationOk();

	public abstract bool CheckAndCorrect(IFormattedLog log);

	public abstract bool Restart(IFormattedLog log);

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
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedMoveAbsoluteResult;
		}
		ControllerService.Instance.ResetLocation();
		MotionResult motionResult2 = MoveToTargetInner(target);
		if (MotionResult.ArcusNotResponsive == motionResult2)
		{
			IncrementCommunicationErrorCtr();
		}
		return motionResult2;
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
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			if (axis != Axis.X)
			{
				return ControllerEmulationState.Instance.ExpectedHomeYResult;
			}
			return ControllerEmulationState.Instance.ExpectedHomeXResult;
		}
		ControllerService.Instance.ResetLocation();
		MotionResult motionResult2 = HomeAxisInner(axis);
		if (motionResult2 != MotionResult.Success)
		{
			LogHelper.Instance.Log(LogEntryType.Error, "The HOME{0} failed with status {1}", axis.ToString().ToUpper(), motionResult2.ToString().ToUpper());
		}
		if (MotionResult.ArcusNotResponsive == motionResult2)
		{
			IncrementCommunicationErrorCtr();
		}
		return motionResult2;
	}

	public MotionResult MoveToVend(MoveMode mode)
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
		MotionResult motionResult2 = MoveToVendInner(mode);
		if (MotionResult.ArcusNotResponsive == motionResult2)
		{
			IncrementCommunicationErrorCtr();
		}
		return motionResult2;
	}

	public bool GetCurrentLocation(Axis axis, ref MoveTarget target)
	{
		return GetCurrentLocationInner(axis, ref target);
	}

	public bool IsMotorErrored(Axis axis, out MotionResult status)
	{
		return IsMotorErroredInner(axis, out status);
	}

	public void InitializeCommunication()
	{
		InitializeCommandInner();
	}

	public HomeAxesResult InitAxes(IFormattedLog log)
	{
		return InitAxesInner(log);
	}

	internal MotionResult ControllerErrorStatus(Axis axis)
	{
		if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode)
		{
			return MotionResult.Success;
		}
		return ControllerErrorStatusInner(axis);
	}

	internal void WriteToLog(string msg)
	{
		DateTime now = DateTime.Now;
		string value = $"{now.ToShortDateString()} {now.ToShortTimeString()} {msg}";
		LogFile.WriteLine(value);
	}

	protected abstract bool ClosePortInner();

	protected abstract HomeAxesResult InitAxesInner(IFormattedLog log);

	protected abstract void InitializeCommandInner();

	protected abstract bool IsInitializedInner();

	protected abstract MotionResult MoveToTargetInner(MoveTarget target);

	protected abstract MotionResult MoveToVendInner(MoveMode mode);

	protected abstract bool IsMotorErroredInner(Axis axis, out MotionResult status);

	protected abstract MotionResult ControllerErrorStatusInner(Axis axis);

	protected abstract MotionResult HomeAxisInner(Axis axis);

	protected abstract bool GetCurrentLocationInner(Axis axis, ref MoveTarget target);

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

	private void IncrementCommunicationErrorCtr()
	{
		if (ControllerConfiguration.Instance.CountHardwareErrors)
		{
			CounterManager.Instance.Increment("MCCommFailures");
		}
	}
}
