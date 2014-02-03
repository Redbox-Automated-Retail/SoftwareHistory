using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal abstract class NativeJobAdapter
{
	private enum CameraIPCMethod
	{
		None,
		File,
		Message
	}

	private const string SecCamera = "c:\\Program Files\\Redbox\\SecCamera";

	private static CameraIPCMethod? m_cameraMethod = null;

	internal ExecutionResult Result { get; set; }

	internal ExecutionContext Context { get; set; }

	internal string Name { get; set; }

	protected ApplicationLog AppLog => Context.AppLog;

	protected VendDoor VendDoor => VendDoor.Instance;

	protected Track Track => Track.Instance;

	protected PickerBoard PickerBoard => PickerBoard.Instance;

	protected AuxBoard AuxBoard => AuxBoard.Instance;

	protected Gripper Gripper => Gripper.Instance;

	private static CameraIPCMethod CameraMethod
	{
		get
		{
			if (m_cameraMethod.HasValue)
			{
				return m_cameraMethod.Value;
			}
			try
			{
				m_cameraMethod = SecretOptions.Instance.GetValue("EnableSecureCamera", CameraIPCMethod.None);
			}
			catch
			{
				m_cameraMethod = CameraIPCMethod.None;
				SecretOptions.Instance.SetValue("EnableSecureCamera", CameraIPCMethod.None);
			}
			LogHelper.Instance.Log("SecureCameraMode: {0}", m_cameraMethod.Value.ToString());
			return m_cameraMethod.Value;
		}
	}

	internal void Execute()
	{
		ExecuteInner();
	}

	protected abstract void ExecuteInner();

	protected void AddError(string errMsg)
	{
		Result.Errors.Add(Error.NewError("E999", "Execution context error.", errMsg));
	}

	protected void SendMessage(string msg)
	{
		Context.Send(msg);
	}

	protected bool DiskIsUnknown(string barcode, out bool isBackward)
	{
		isBackward = false;
		if (barcode == "UNKNOWN")
		{
			return true;
		}
		if (barcode == "redbox")
		{
			isBackward = true;
			return true;
		}
		return false;
	}

	protected bool MoveAndGet(int deck, int slot)
	{
		if (ControllerService.Instance.MoveTo(deck, slot, MoveMode.Get, AppLog) != MotionResult.Success)
		{
			return false;
		}
		return ControllerService.Instance.Get(testOnEmpty: false, AppLog).Success;
	}

	protected bool MoveAndPut(int deck, int slot, string id)
	{
		if (MoveTo(deck, slot, MoveMode.Put) != MotionResult.Success)
		{
			return false;
		}
		return ControllerService.Instance.Put(id, AppLog).Success;
	}

	protected MotionResult HomeAxis(Axis axis)
	{
		return ControllerService.Instance.HomeAxis(axis, AppLog);
	}

	protected MotionResult MoveVend(MoveMode mode)
	{
		return ControllerService.Instance.MoveToVend(mode, AppLog);
	}

	protected MotionResult MoveTo(int deck, int slot, MoveMode mode)
	{
		return ControllerService.Instance.MoveTo(deck, slot, mode, AppLog);
	}

	protected MotionResult MoveTo(Location loc, MoveMode mode)
	{
		return ControllerService.Instance.MoveTo(loc, mode, AppLog);
	}

	protected T GetTop<T>()
	{
		return (T)Context.PopTop();
	}

	protected bool ContextSwitchRequested()
	{
		if (Context.DeferredStatusChangeRequested.HasValue)
		{
			LogHelper.Instance.Log($"SwitchRequested: deferred change to {Context.DeferredStatusChangeRequested.Value.ToString()}", LogEntryType.Info);
			Context.PromoteDeferredStatusChange();
			return true;
		}
		Context.FlushMessages();
		if (Context.Messages.Count != 0)
		{
			LogHelper.Instance.Log($"Context {Context.ID} {Context.ProgramName}: msg count = {Context.Messages.Count}", LogEntryType.Info);
		}
		return false;
	}

	protected void Yield(int milliseconds)
	{
		DateTime value = DateTime.Now.AddMilliseconds(milliseconds);
		Context.StartTime = value;
		ExecutionEngine.Instance.PerformContextSwitch(runtimeOnly: false);
	}
}
