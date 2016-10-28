using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal abstract class MotionController : IConfigurationObserver
{
	protected readonly MotionControlService ControlService;

	protected readonly TextWriter LogFile;

	public void NotifyConfigurationChangeEnd()
	{
		OnConfigurationChangeEnd();
	}

	public void NotifyConfigurationChangeStart()
	{
		OnConfigurationChangeStart();
	}

	public void NotifyConfigurationLoaded()
	{
		OnConfigurationLoad();
	}

	internal abstract IMotionControlLimitResponse ReadLimits();

	internal abstract bool CommunicationOk();

	internal bool IsInitialized()
	{
		if (!CommunicationOk())
		{
			return false;
		}
		return IsInitializedInner();
	}

	internal abstract bool ClosePort();

	internal ErrorCodes MoveToTargetWithoutSensorCheck(ref MoveTarget target)
	{
		return MoveToTargetInner(ref target);
	}

	internal ErrorCodes MoveToTarget(ref MoveTarget target)
	{
		if (!IsInitializedInner())
		{
			return ErrorCodes.MotorNotHomed;
		}
		ErrorCodes errorCodes = CheckAndReset();
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		return MoveToTargetInner(ref target);
	}

	internal ErrorCodes HomeAxis(Axis axis)
	{
		ErrorCodes errorCodes = CheckAndReset();
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		ErrorCodes errorCodes2 = HomeAxisInner(axis);
		if (errorCodes2 != ErrorCodes.Success)
		{
			string text = $"The HOME{axis.ToString().ToUpper()} failed with status {errorCodes2.ToString().ToUpper()}";
			LogHelper.Instance.Log(LogEntryType.Error, text);
			WriteToLog(text);
		}
		return errorCodes2;
	}

	internal abstract IControllerPosition ReadPositions();

	protected abstract void OnConfigurationLoad();

	protected abstract void OnConfigurationChangeStart();

	protected abstract void OnConfigurationChangeEnd();

	protected abstract bool IsInitializedInner();

	protected abstract ErrorCodes MoveToTargetInner(ref MoveTarget target);

	protected abstract ErrorCodes MoveToVendInner(MoveMode mode);

	protected abstract ErrorCodes HomeAxisInner(Axis axis);

	protected MotionController(MotionControlService controlService)
	{
		ControlService = controlService;
		try
		{
			IFormattedLogFactoryService service = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
			string path = service.CreateSubpath("Service");
			string path2 = Path.Combine(path, "MotionControlErrorLog.log");
			LogFile = new StreamWriter(File.Open(path2, FileMode.Append, FileAccess.Write, FileShare.Read))
			{
				AutoFlush = true
			};
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[MotionControl] Create motion control error log caught an exception.", e);
		}
	}

	internal ErrorCodes MoveToVend(MoveMode mode)
	{
		if (!IsInitializedInner())
		{
			return ErrorCodes.MotorNotHomed;
		}
		ErrorCodes errorCodes = CheckAndReset();
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		return MoveToVendInner(mode);
	}

	internal void WriteToLog(string fmt, params object[] stuff)
	{
		fmt = string.Format(fmt, stuff);
		WriteToLog(fmt);
	}

	internal void WriteToLog(string msg)
	{
		DateTime now = DateTime.Now;
		string value = $"{now.ToShortDateString()} {now.ToShortTimeString()} {msg}";
		LogFile.WriteLine(value);
	}

	internal abstract bool ResetDeviceDriver();

	internal abstract bool InitializeCommunication();

	private ErrorCodes CheckAndReset()
	{
		ErrorCodes errorCodes = ControlService.TestMove();
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		ControlService.ResetLocation();
		return ErrorCodes.Success;
	}
}
