using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class ControlSystemBridge : IConfigurationObserver, IControlSystemService, IControlSystem
{
	private IControlSystem Implementor;

	private readonly List<IControlSystemObserver> Observers = new List<IControlSystemObserver>();

	private readonly ReaderWriterLockSlim ObserverLock = new ReaderWriterLockSlim();

	public bool IsInitialized { get; private set; }

	public VendDoorState VendDoorState => Implementor.VendDoorState;

	public TrackState TrackState => Implementor.TrackState;

	public void NotifyConfigurationChangeStart()
	{
	}

	public void NotifyConfigurationChangeEnd()
	{
	}

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[ControlSystemBridge] Configuration load.");
		ControlSystem instance = (ControlSystem)(Implementor = new ControlSystem());
		ServiceLocator.Instance.AddService(typeof(ICoreCommandExecutor), instance);
	}

	public void AddHandler(IControlSystemObserver observer)
	{
		using (new WithWriteLock(ObserverLock))
		{
			if (!Observers.Contains(observer))
			{
				Observers.Add(observer);
			}
		}
	}

	public void RemoveHandler(IControlSystemObserver observer)
	{
		using (new WithWriteLock(ObserverLock))
		{
			Observers.Remove(observer);
		}
	}

	public bool Restart()
	{
		if (!Shutdown())
		{
			return false;
		}
		Thread.Sleep(1200);
		return Initialize().Success;
	}

	public IControlResponse Initialize()
	{
		IControlResponse controlResponse = Implementor.Initialize();
		IsInitialized = controlResponse.Success;
		if (controlResponse.Success)
		{
			using (new WithReadLock(ObserverLock))
			{
				foreach (IControlSystemObserver observer in Observers)
				{
					observer.OnSystemInitialize(ErrorCodes.Success);
				}
			}
		}
		return controlResponse;
	}

	public bool Shutdown()
	{
		using (new WithReadLock(ObserverLock))
		{
			foreach (IControlSystemObserver observer in Observers)
			{
				observer.OnSystemShutdown();
			}
		}
		IsInitialized = false;
		return Implementor.Shutdown();
	}

	public IControlResponse SetAudio(AudioChannelState newState)
	{
		return Implementor.SetAudio(newState);
	}

	public IControlResponse ToggleRingLight(bool on, int? sleepAfter)
	{
		return Implementor.ToggleRingLight(on, sleepAfter);
	}

	public IControlResponse VendDoorRent()
	{
		return Implementor.VendDoorRent();
	}

	public IControlResponse VendDoorClose()
	{
		return Implementor.VendDoorClose();
	}

	public VendDoorState ReadVendDoorPosition()
	{
		return Implementor.ReadVendDoorPosition();
	}

	public IControlResponse TrackOpen()
	{
		return Implementor.TrackOpen();
	}

	public IControlResponse TrackClose()
	{
		return Implementor.TrackClose();
	}

	public IControlResponse TrackCycle()
	{
		return Implementor.TrackCycle();
	}

	public void TimedExtend()
	{
		Implementor.TimedExtend();
	}

	public void TimedExtend(int timeout)
	{
		Implementor.TimedExtend(timeout);
	}

	public IControlResponse ExtendArm()
	{
		return Implementor.ExtendArm();
	}

	public IControlResponse ExtendArm(int timeout)
	{
		return Implementor.ExtendArm(timeout);
	}

	public IControlResponse RetractArm()
	{
		return Implementor.RetractArm();
	}

	public IControlResponse SetFinger(GripperFingerState state)
	{
		return Implementor.SetFinger(state);
	}

	public ErrorCodes Center(CenterDiskMethod method)
	{
		return Implementor.Center(method);
	}

	public string GetBoardVersion(ControlBoards board)
	{
		return Implementor.GetBoardVersion(board);
	}

	public string[] GetBoardVersions()
	{
		return Implementor.GetBoardVersions();
	}

	public string[] ReadInputs(ControlBoards board)
	{
		return Implementor.ReadInputs(board);
	}

	public void LogPickerSensorState()
	{
		Implementor.LogPickerSensorState();
	}

	public void LogPickerSensorState(LogEntryType type)
	{
		Implementor.LogPickerSensorState(type);
	}

	public void LogInputs(ControlBoards board)
	{
		Implementor.LogInputs(board);
	}

	public void LogInputs(ControlBoards board, LogEntryType type)
	{
		Implementor.LogInputs(board, type);
	}

	public IControlResponse SetSensors(bool on)
	{
		return Implementor.SetSensors(on);
	}

	public IPickerSensorReadResult ReadSensors()
	{
		return Implementor.ReadSensors();
	}

	public IPickerSensorReadResult ReadImmediateSensors()
	{
		return Implementor.ReadImmediateSensors();
	}

	public IControlResponse StopRoller()
	{
		return Implementor.StopRoller();
	}

	public IControlResponse StartRollerIn()
	{
		return Implementor.StartRollerIn();
	}

	public IControlResponse StartRollerOut()
	{
		return Implementor.StartRollerOut();
	}

	public IControlResponse RollerToPosition(RollerPosition position)
	{
		return Implementor.RollerToPosition(position);
	}

	public IControlResponse RollerToPosition(RollerPosition position, int opTimeout)
	{
		return Implementor.RollerToPosition(position, opTimeout);
	}

	public IControlResponse RollerToPosition(RollerPosition position, int opTimeout, bool logSensors)
	{
		return Implementor.RollerToPosition(position, opTimeout, logSensors);
	}

	public QlmStatus GetQlmStatus()
	{
		return Implementor.GetQlmStatus();
	}

	public ErrorCodes EngageQlm(IFormattedLog log)
	{
		return EngageQlm(home: true, log);
	}

	public ErrorCodes EngageQlm(bool home, IFormattedLog log)
	{
		return Implementor.EngageQlm(home, log);
	}

	public ErrorCodes DisengageQlm(IFormattedLog log)
	{
		return DisengageQlm(home: true, log);
	}

	public ErrorCodes DisengageQlm(bool home, IFormattedLog log)
	{
		return Implementor.DisengageQlm(home, log);
	}

	public IControlResponse DropQlm()
	{
		return Implementor.DropQlm();
	}

	public IControlResponse LiftQlm()
	{
		return Implementor.LiftQlm();
	}

	public IControlResponse HaltQlmOperation()
	{
		return Implementor.HaltQlmOperation();
	}

	internal ControlSystemBridge()
	{
		ControllerConfiguration.Instance.AddObserver(this);
		IsInitialized = false;
	}
}
