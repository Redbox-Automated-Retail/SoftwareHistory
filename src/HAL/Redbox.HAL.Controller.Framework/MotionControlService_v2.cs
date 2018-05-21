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
		Controller = new ArcusMotionController2();
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

	public IMotionControlLimitResponse ReadLimits()
	{
		return Controller.ReadLimits();
	}

	public ErrorCodes MoveAbsolute(Axis axis, int? xunits, int? yunits, bool checkSensors)
	{
		if (checkSensors)
		{
			ErrorCodes errorCodes = AxesInitialized();
			if (errorCodes != ErrorCodes.Success)
			{
				return errorCodes;
			}
		}
		MoveTarget target = new MoveTarget
		{
			Axis = axis,
			XCoordinate = xunits,
			YCoordinate = yunits
		};
		if (checkSensors)
		{
			return MoveWithCheck(ref target);
		}
		return Controller.MoveToTarget(ref target);
	}

	public ErrorCodes MoveTo(int deck, int slot, MoveMode mode, IFormattedLog log, ref OffsetMoveData data)
	{
		ErrorCodes errorCodes = AxesInitialized();
		if (errorCodes != ErrorCodes.Success)
		{
			LogMoveError(deck, slot, errorCodes, log);
			return errorCodes;
		}
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
		switch (errorCodes2)
		{
		case ErrorCodes.ArcusNotResponsive:
			if (ResetMotionControllerChecked(log))
			{
				errorCodes2 = MoveWithCheck(ref target2);
			}
			break;
		case ErrorCodes.LowerLimitError:
			if (!ControllerConfiguration.Instance.LowerLimitAsError)
			{
				errorCodes2 = ErrorCodes.Timeout;
			}
			break;
		}
		LogMoveError(deck, slot, errorCodes2, log);
		if (errorCodes2 == ErrorCodes.Success)
		{
			AtVendDoor = false;
			CurrentLocation = currentLocation;
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
			LogMoveVendError(writer, errorCodes);
			return errorCodes;
		}
		ErrorCodes errorCodes2 = CheckAndReset();
		if (errorCodes2 != ErrorCodes.Success)
		{
			LogMoveVendError(writer, errorCodes2);
			return errorCodes2;
		}
		ErrorCodes errorCodes3 = Controller.MoveToVend(mode);
		if (ErrorCodes.ArcusNotResponsive == errorCodes3 && ResetMotionControllerChecked(writer))
		{
			errorCodes3 = Controller.MoveToVend(mode);
		}
		LogMoveVendError(writer, errorCodes3);
		AtVendDoor = errorCodes3 == ErrorCodes.Success;
		return errorCodes3;
	}

	public ErrorCodes InitAxes(IFormattedLog log)
	{
		return InitAxes(log, fast: false);
	}

	public ErrorCodes InitAxes(IFormattedLog log, bool fast)
	{
		if (ControllerConfiguration.Instance.RestartControllerDuringUserJobs && !CommunicationOk())
		{
			bool flag = OnReset();
			InsertCorrectionStat(flag);
			if (!flag)
			{
				return ErrorCodes.ArcusNotResponsive;
			}
		}
		using ClearPickerFrontOperation clearPickerFrontOperation = new ClearPickerFrontOperation();
		if (ControllerConfiguration.Instance.ClearPickerOnHome)
		{
			clearPickerFrontOperation.Execute();
		}
		log.WriteFormatted("Home the X motor.");
		int num = (fast ? 1 : 2);
		for (int i = 0; i < num; i++)
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
		}
		return errorCodes2;
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
		if (!OnReset())
		{
			return false;
		}
		ErrorCodes errorCodes = InitAxes(log, quick);
		LogHelper.Instance.Log("[{0}]: Restart returned {1}", GetType().Name, errorCodes.ToString().ToUpper());
		log?.WriteFormatted("RESET of motion controller returned {0}", errorCodes.ToString());
		return errorCodes == ErrorCodes.Success;
	}

	public bool TestAndReset(IFormattedLog log)
	{
		return TestAndReset(log, quick: true);
	}

	public bool TestAndReset(IFormattedLog log, bool quick)
	{
		bool flag = true;
		if (!CommunicationOk())
		{
			log.WriteFormatted("Attempt to reset Motion Controller ...", LogEntryType.Info);
			flag = Reset(log, quick: true);
			log.WriteFormatted(flag ? " ... inline reset of device interfaces succeeded; init axes." : " ... inline reset of device interfaces FAILED.", (!flag) ? LogEntryType.Error : LogEntryType.Info);
		}
		return flag;
	}

	public void Shutdown()
	{
		XAxisHomed = (YAxisHomed = false);
		Controller.OnShutdown();
	}

	internal MotionControlService_v2()
	{
		ControllerConfiguration.Instance.AddObserver(this);
	}

	private ErrorCodes MoveWithCheck(ref MoveTarget target)
	{
		ErrorCodes errorCodes = CheckAndReset();
		if (errorCodes == ErrorCodes.Success)
		{
			return Controller.MoveToTarget(ref target);
		}
		return errorCodes;
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
		CurrentLocation = null;
		AtVendDoor = false;
		return ErrorCodes.Success;
	}

	private bool ResetMotionControllerChecked(IFormattedLog log)
	{
		if (!ControllerConfiguration.Instance.RestartControllerDuringUserJobs)
		{
			return false;
		}
		IExecutionContext activeContext = ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext();
		HardwareCorrectionEventArgs e = new HardwareCorrectionEventArgs(HardwareCorrectionStatistic.Arcus);
		activeContext?.HardwareCorrectionStart(e);
		e.CorrectionOk = Reset(log, quick: true);
		activeContext?.HardwareCorrectionEnd(e);
		InsertCorrectionStat(activeContext, e.CorrectionOk);
		return e.CorrectionOk;
	}

	private void InsertCorrectionStat(bool ok)
	{
		IExecutionContext activeContext = ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext();
		InsertCorrectionStat(activeContext, ok);
	}

	private void InsertCorrectionStat(IExecutionContext activeContext, bool ok)
	{
		if (activeContext != null)
		{
			ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>().Insert(HardwareCorrectionStatistic.Arcus, activeContext, ok);
		}
	}

	private void LogMoveVendError(IFormattedLog writer, ErrorCodes error)
	{
		if (error != ErrorCodes.Success && writer != null)
		{
			string text = $"MOVEVEND returned an error status {error.ToString().ToUpper()}";
			writer.WriteFormatted(text);
			LogHelper.Instance.Log(text, LogEntryType.Error);
		}
	}

	private void LogMoveError(int deck, int slot, ErrorCodes error, IFormattedLog log)
	{
		if (log != null && error != ErrorCodes.Success)
		{
			string text = $"MOVE Deck = {deck} Slot = {slot} returned an error status {error.ToString().ToUpper()}";
			log.WriteFormatted(text);
			LogHelper.Instance.Log(text, LogEntryType.Error);
		}
	}

	private ErrorCodes OnLowerLimitError(IFormattedLog log, ref MoveTarget target)
	{
		if (!ControllerConfiguration.Instance.HomeOnLowerLimitError)
		{
			return ErrorCodes.Timeout;
		}
		LogHelper.Instance.Log("[MotionControlService] Move returned LowerLimitError; attempting correction.");
		if (InitAxes(log) != ErrorCodes.Success)
		{
			return ErrorCodes.Timeout;
		}
		ErrorCodes errorCodes = MoveWithCheck(ref target);
		if (ErrorCodes.LowerLimitError != errorCodes)
		{
			return errorCodes;
		}
		return ErrorCodes.Timeout;
	}

	private bool OnReset()
	{
		Shutdown();
		if (!Controller.OnResetDeviceDriver())
		{
			LogHelper.Instance.Log("[{0}]: ResetDevice driver returned FALSE", GetType().Name);
			return false;
		}
		Initialize();
		if (!Controller.CommunicationOk())
		{
			LogHelper.Instance.Log("[{0}]: After reset, channel still down. I'm sorry.", GetType().Name);
			return false;
		}
		return true;
	}
}
