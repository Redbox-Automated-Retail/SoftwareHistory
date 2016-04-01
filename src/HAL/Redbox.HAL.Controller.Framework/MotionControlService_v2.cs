using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class MotionControlService_v2 : IConfigurationObserver, IMotionControlService
{
	private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

	private readonly List<IMoveVeto> Vetoers = new List<IMoveVeto>();

	private AbstractMotionController Controller;

	private bool YAxisHomed;

	private bool XAxisHomed;

	public ILocation CurrentLocation { get; private set; }

	public bool AtVendDoor { get; private set; }

	public bool IsInitialized => AxesInitialized() == ErrorCodes.Success;

	public void NotifyConfigurationLoaded()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			Controller = new EmulatedMotionController2();
		}
		else
		{
			Controller = new ArcusMotionController2();
		}
		Controller.OnConfigurationLoad();
	}

	public void NotifyConfigurationChangeStart()
	{
		Controller.OnConfigurationChangeStart();
	}

	public void NotifyConfigurationChangeEnd()
	{
		Controller.OnConfigurationChangeEnd();
	}

	public void AddVeto(IMoveVeto veto)
	{
		using (new WithWriteLock(Lock))
		{
			Vetoers.Add(veto);
		}
	}

	public void RemoveVeto(IMoveVeto veto)
	{
		using (new WithWriteLock(Lock))
		{
			Vetoers.Remove(veto);
		}
	}

	public bool Initialize()
	{
		return Controller.OnStartup();
	}

	public bool CommunicationOk()
	{
		return Controller.CommunicationOk();
	}

	public IControllerPosition ReadPositions()
	{
		return Controller.ReadPositions();
	}

	public string[] ReadLimits()
	{
		return Controller.ReadLimits();
	}

	public ErrorCodes MoveAbsolute(Axis axis, int? xunits, int? yunits, bool checkSensors)
	{
		ErrorCodes errorCodes = AxesInitialized();
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		MoveTarget target = new MoveTarget
		{
			Axis = axis,
			XCoordinate = xunits,
			YCoordinate = yunits
		};
		if (!checkSensors)
		{
			return Controller.MoveToTarget(ref target);
		}
		return MoveWithCheck(ref target);
	}

	public ErrorCodes MoveTo(int deck, int slot, MoveMode mode, IFormattedLog log, ref OffsetMoveData data)
	{
		ErrorCodes errorCodes = AxesInitialized();
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		ResetLocation();
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		ILocation currentLocation = service.Get(deck, slot);
		IDecksService service2 = ServiceLocator.Instance.GetService<IDecksService>();
		IDeck byNumber = service2.GetByNumber(deck);
		if (byNumber == null)
		{
			return ErrorCodes.DeckOutOfRange;
		}
		if (!byNumber.IsSlotValid(slot))
		{
			return ErrorCodes.SlotOutOfRange;
		}
		int num = byNumber.YOffset;
		int num2 = (byNumber.IsQlm ? ControllerConfiguration.Instance.QlmYOffset : 50);
		switch (mode)
		{
		case MoveMode.Put:
			num += num2 * (int)ControllerConfiguration.Instance.GearY.GetStepRatio();
			break;
		case MoveMode.Get:
			num -= num2 * (int)ControllerConfiguration.Instance.GearY.GetStepRatio();
			break;
		}
		int num3 = byNumber.GetSlotOffset(slot);
		if (byNumber.IsQlm)
		{
			IControllerPosition controllerPosition = Controller.ReadPositions();
			if (controllerPosition.ReadOk)
			{
				int value = num3 - ControllerConfiguration.Instance.QlmApproachOffset;
				MoveTarget target = new MoveTarget
				{
					Axis = Axis.XY,
					XCoordinate = value,
					YCoordinate = num
				};
				switch (mode)
				{
				case MoveMode.None:
				case MoveMode.Get:
					MoveWithCheck(ref target);
					break;
				case MoveMode.Put:
					if (num3 < controllerPosition.XCoordinate.Value)
					{
						MoveWithCheck(ref target);
					}
					break;
				}
			}
		}
		if (data.XOffset.HasValue)
		{
			num3 += data.XOffset.Value;
		}
		if (data.YOffset.HasValue)
		{
			num += data.YOffset.Value;
		}
		MoveTarget target2 = new MoveTarget
		{
			Axis = Axis.XY,
			XCoordinate = num3,
			YCoordinate = num
		};
		ErrorCodes errorCodes2 = MoveWithCheck(ref target2);
		if (errorCodes2 == ErrorCodes.Success)
		{
			AtVendDoor = false;
			CurrentLocation = currentLocation;
			return ErrorCodes.Success;
		}
		if (log != null)
		{
			string text = $"MOVE Deck ={deck} Slot = {slot} returned an error status {errorCodes2.ToString().ToUpper()}";
			log.WriteFormatted(text);
			LogHelper.Instance.Log(text, LogEntryType.Error);
		}
		return errorCodes2;
	}

	public ErrorCodes MoveTo(int deck, int slot, MoveMode mode, IFormattedLog log)
	{
		OffsetMoveData data = default(OffsetMoveData);
		return MoveTo(deck, slot, mode, log, ref data);
	}

	public ErrorCodes MoveTo(ILocation location, MoveMode mode, IFormattedLog log, ref OffsetMoveData data)
	{
		return MoveTo(location.Deck, location.Slot, mode, log, ref data);
	}

	public ErrorCodes MoveTo(ILocation location, MoveMode mode, IFormattedLog log)
	{
		return MoveTo(location.Deck, location.Slot, mode, log);
	}

	public ErrorCodes MoveVend(MoveMode mode, IFormattedLog writer)
	{
		ErrorCodes errorCodes = AxesInitialized();
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		ErrorCodes errorCodes2 = CheckAndReset();
		if (errorCodes2 != ErrorCodes.Success)
		{
			return errorCodes2;
		}
		ErrorCodes errorCodes3 = Controller.MoveToVend(mode);
		if (writer != null && errorCodes3 != ErrorCodes.Success)
		{
			string text = $"MOVEVEND returned an error status {errorCodes3.ToString().ToUpper()}";
			writer.WriteFormatted(text);
			LogHelper.Instance.Log(text, LogEntryType.Error);
		}
		AtVendDoor = ErrorCodes.Success == errorCodes3;
		return errorCodes3;
	}

	public ErrorCodes InitAxes(IFormattedLog log)
	{
		return InitAxes(log, 2);
	}

	public ErrorCodes InitAxesFast(IFormattedLog log)
	{
		return InitAxes(log, 1);
	}

	public ErrorCodes HomeAxis(Axis axis, IFormattedLog log)
	{
		ErrorCodes errorCodes = HomeAxisChecked(axis, log);
		if (errorCodes != ErrorCodes.Success)
		{
			string msg = $"HOME{axis.ToString().ToUpper()} returned an error status {errorCodes.ToString().ToUpper()}";
			log.WriteFormatted(msg, LogEntryType.Error);
		}
		return errorCodes;
	}

	public bool Reset(IFormattedLog log, bool quick)
	{
		Shutdown();
		if (!Controller.OnResetDeviceDriver())
		{
			LogHelper.Instance.Log("[Reset Hardware]: ResetDevice driver returned FALSE");
			return false;
		}
		Initialize();
		if (!Controller.CommunicationOk())
		{
			LogHelper.Instance.Log("[Reset Hardware]: After reset, channel still down. I'm sorry.");
			return false;
		}
		ErrorCodes errorCodes = (quick ? InitAxesFast(log) : InitAxes(log));
		LogHelper.Instance.Log("[Reset Hardware]: Restart returned {0}", errorCodes.ToString().ToUpper());
		return ErrorCodes.Success == errorCodes;
	}

	public void Shutdown()
	{
		XAxisHomed = (YAxisHomed = false);
		Controller.OnShutdown();
	}

	internal MotionControlService_v2()
	{
	}

	private void ResetLocation()
	{
		CurrentLocation = null;
		AtVendDoor = false;
	}

	private ErrorCodes MoveWithCheck(ref MoveTarget target)
	{
		ErrorCodes errorCodes = CheckAndReset();
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		return Controller.MoveToTarget(ref target);
	}

	private ErrorCodes HomeAxisChecked(Axis axis, IFormattedLog log)
	{
		ErrorCodes errorCodes = CheckAndReset();
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		if (Axis.Y == axis)
		{
			YAxisHomed = false;
		}
		else if (axis == Axis.X)
		{
			XAxisHomed = false;
		}
		ErrorCodes errorCodes2 = Controller.HomeAxis(axis);
		if (errorCodes2 != ErrorCodes.Success)
		{
			string text = $"The HOME{axis.ToString().ToUpper()} failed with status {errorCodes2.ToString().ToUpper()}";
			LogHelper.Instance.Log(LogEntryType.Error, text);
			log.WriteFormatted(text);
		}
		else
		{
			switch (axis)
			{
			case Axis.X:
				XAxisHomed = true;
				break;
			case Axis.Y:
				YAxisHomed = true;
				break;
			}
		}
		return errorCodes2;
	}

	private ErrorCodes InitAxes(IFormattedLog log, int xtries)
	{
		using ClearPickerFrontOperation clearPickerFrontOperation = new ClearPickerFrontOperation();
		if (ControllerConfiguration.Instance.ClearPickerOnHome)
		{
			clearPickerFrontOperation.Execute();
		}
		log.WriteFormatted("Home the X motor.");
		for (int i = 0; i < xtries; i++)
		{
			ErrorCodes errorCodes = HomeAxisChecked(Axis.X, log);
			if (errorCodes != ErrorCodes.Success)
			{
				log.WriteFormatted("HOME X returned error status {0}", errorCodes.ToString().ToUpper());
				return errorCodes;
			}
			if (i == 0)
			{
				MoveTarget target = new MoveTarget
				{
					Axis = Axis.X,
					XCoordinate = -200,
					YCoordinate = null
				};
				Controller.MoveToTarget(ref target);
			}
		}
		log.WriteFormatted("Home the Y Motor.");
		ErrorCodes errorCodes2 = HomeAxisChecked(Axis.Y, log);
		if (errorCodes2 != ErrorCodes.Success)
		{
			log.WriteFormatted("HOME Y returned error status {0}", errorCodes2.ToString().ToUpper());
			return errorCodes2;
		}
		return ErrorCodes.Success;
	}

	private ErrorCodes AxesInitialized()
	{
		if (!ControllerConfiguration.Instance.ValidateControllerHomeStatus)
		{
			return ErrorCodes.Success;
		}
		int num = 0;
		if (!XAxisHomed)
		{
			num++;
			LogHelper.Instance.Log("The X axis did not init.");
		}
		if (!YAxisHomed)
		{
			num++;
			LogHelper.Instance.Log("The Y axis did not init.");
		}
		if (num != 0)
		{
			return ErrorCodes.MotorNotHomed;
		}
		return ErrorCodes.Success;
	}

	private ErrorCodes CheckAndReset()
	{
		using (new WithReadLock(Lock))
		{
			foreach (IMoveVeto vetoer in Vetoers)
			{
				ErrorCodes errorCodes = vetoer.CanMove();
				if (errorCodes != ErrorCodes.Success)
				{
					LogHelper.Instance.Log("[MotionService] Move veto returned code {0}", errorCodes.ToString());
					return errorCodes;
				}
			}
		}
		ResetLocation();
		return ErrorCodes.Success;
	}
}
