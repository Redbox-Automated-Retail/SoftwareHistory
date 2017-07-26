using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class MotionControlService : IMotionControlService, IConfigurationObserver
{
	private MotionController Controller;

	private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

	private readonly List<IMoveVeto> Vetoers = new List<IMoveVeto>();

	public bool AtVendDoor { get; private set; }

	public ILocation CurrentLocation { get; private set; }

	public bool IsInitialized => Controller.IsInitialized();

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[MotionControlService] Configuration end.");
		Controller.NotifyConfigurationChangeEnd();
	}

	public void NotifyConfigurationChangeStart()
	{
		LogHelper.Instance.Log("[MotionControlService] Configuration start.");
		Controller.NotifyConfigurationChangeStart();
	}

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[MotionControlService] Notify configuration loaded.");
		if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode)
		{
			Controller = new EmulatedMotionController(this);
		}
		else
		{
			Controller = new ArcusMotionController(this);
		}
		Controller.NotifyConfigurationLoaded();
	}

	public bool Reset(IFormattedLog log, bool quick)
	{
		if (!Controller.ResetDeviceDriver())
		{
			LogHelper.Instance.Log("[Reset Hardware]: ResetDevice driver returned FALSE");
			return false;
		}
		Controller.InitializeCommunication();
		if (!Controller.CommunicationOk())
		{
			LogHelper.Instance.Log("[Reset Hardware]: After reset, channel still down. I'm sorry.");
			return false;
		}
		ErrorCodes errorCodes = InitAxes(log, quick);
		LogHelper.Instance.Log("[Reset Hardware]: Restart returned {0}", errorCodes.ToString().ToUpper());
		return errorCodes == ErrorCodes.Success;
	}

	public bool Initialize()
	{
		return Controller.InitializeCommunication();
	}

	public bool CommunicationOk()
	{
		return Controller.CommunicationOk();
	}

	public void Shutdown()
	{
		Controller.ClosePort();
	}

	public IMotionControlLimitResponse ReadLimits()
	{
		return Controller.ReadLimits();
	}

	public ErrorCodes HomeAxis(Axis axis, IFormattedLog log)
	{
		ErrorCodes errorCodes = Controller.HomeAxis(axis);
		if (errorCodes != ErrorCodes.Success)
		{
			string msg = $"HOME{axis.ToString().ToUpper()} returned an error status {errorCodes.ToString().ToUpper()}";
			log.WriteFormatted(msg, LogEntryType.Error);
		}
		return errorCodes;
	}

	public IControllerPosition ReadPositions()
	{
		return Controller.ReadPositions();
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

	public ErrorCodes InitAxes(IFormattedLog log)
	{
		return InitAxes(log, fast: false);
	}

	public ErrorCodes InitAxes(IFormattedLog log, bool fast)
	{
		using ClearPickerFrontOperation clearPickerFrontOperation = new ClearPickerFrontOperation();
		if (ControllerConfiguration.Instance.ClearPickerOnHome)
		{
			clearPickerFrontOperation.Execute();
		}
		log.WriteFormatted("Home the X motor.");
		int num = (fast ? 1 : 2);
		for (int i = 0; i < num; i++)
		{
			ErrorCodes errorCodes = Controller.HomeAxis(Axis.X);
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
				Controller.MoveToTargetWithoutSensorCheck(ref target);
			}
		}
		log.WriteFormatted("Home the Y Motor.");
		ErrorCodes errorCodes2 = Controller.HomeAxis(Axis.Y);
		if (errorCodes2 != ErrorCodes.Success)
		{
			log.WriteFormatted("HOME Y returned error status {0}", errorCodes2.ToString().ToUpper());
			return errorCodes2;
		}
		return ErrorCodes.Success;
	}

	public ErrorCodes MoveVend(MoveMode mode, IFormattedLog writer)
	{
		ResetLocation();
		ErrorCodes errorCodes = Controller.MoveToVend(mode);
		if (writer != null && errorCodes != ErrorCodes.Success)
		{
			string text = $"MOVEVEND returned an error status {errorCodes.ToString().ToUpper()}";
			writer.WriteFormatted(text);
			LogHelper.Instance.Log(text, LogEntryType.Error);
		}
		AtVendDoor = errorCodes == ErrorCodes.Success;
		return errorCodes;
	}

	public ErrorCodes MoveAbsolute(Axis axis, int? xunits, int? yunits, bool checkSensors)
	{
		MoveTarget target = new MoveTarget
		{
			Axis = axis,
			XCoordinate = xunits,
			YCoordinate = yunits
		};
		if (!checkSensors)
		{
			return Controller.MoveToTargetWithoutSensorCheck(ref target);
		}
		return Controller.MoveToTarget(ref target);
	}

	public ErrorCodes MoveTo(ILocation location, MoveMode mode, IFormattedLog log)
	{
		return MoveTo(location.Deck, location.Slot, mode, log);
	}

	public ErrorCodes MoveTo(ILocation location, MoveMode mode, IFormattedLog log, ref OffsetMoveData data)
	{
		return MoveTo(location.Deck, location.Slot, mode, log, ref data);
	}

	public ErrorCodes MoveTo(int deck, int slot, MoveMode mode, IFormattedLog log, ref OffsetMoveData data)
	{
		ResetLocation();
		ILocation currentLocation = ServiceLocator.Instance.GetService<IInventoryService>().Get(deck, slot);
		IDeck byNumber = ServiceLocator.Instance.GetService<IDecksService>().GetByNumber(deck);
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
					Controller.MoveToTarget(ref target);
					break;
				case MoveMode.Put:
					if (num3 < controllerPosition.XCoordinate.Value)
					{
						Controller.MoveToTarget(ref target);
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
		ErrorCodes errorCodes = Controller.MoveToTarget(ref target2);
		if (errorCodes == ErrorCodes.Success)
		{
			AtVendDoor = false;
			CurrentLocation = currentLocation;
			return ErrorCodes.Success;
		}
		if (log != null)
		{
			string text = $"MOVE Deck ={deck} Slot = {slot} returned an error status {errorCodes.ToString().ToUpper()}";
			log.WriteFormatted(text);
			LogHelper.Instance.Log(text, LogEntryType.Error);
		}
		return errorCodes;
	}

	public ErrorCodes MoveTo(int deck, int slot, MoveMode mode, IFormattedLog log)
	{
		OffsetMoveData data = default(OffsetMoveData);
		return MoveTo(deck, slot, mode, log, ref data);
	}

	internal MotionControlService()
	{
		ControllerConfiguration.Instance.AddObserver(this);
	}

	internal void ResetLocation()
	{
		CurrentLocation = null;
		AtVendDoor = false;
	}

	internal ErrorCodes TestMove()
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
			return ErrorCodes.Success;
		}
	}
}
