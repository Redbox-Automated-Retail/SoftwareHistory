using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework.Services;

namespace Redbox.HAL.Controller.Framework;

public sealed class ControllerService : IMoveVeto, IControllerService
{
	public ErrorCodes CanMove()
	{
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		if (VendDoorState.Closed != service.VendDoorState && !service.VendDoorClose().Success)
		{
			LogHelper.Instance.Log("MOVE: could not close the vend door.", LogEntryType.Error);
			return ErrorCodes.VendDoorNotClosed;
		}
		using (ClearGripperOperation clearGripperOperation = new ClearGripperOperation())
		{
			ErrorCodes errorCodes = clearGripperOperation.Execute();
			if (errorCodes != ErrorCodes.Success)
			{
				LogHelper.Instance.Log(LogEntryType.Error, "[ControllerService::CanMove] Clear picker returned error status {0}.", errorCodes);
				service.LogInputs(ControlBoards.Picker, LogEntryType.Error);
				return ErrorCodes.ObstructionDetected;
			}
		}
		string[] array = service.ReadInputs(ControlBoards.Picker);
		if (array.Length == 0)
		{
			return ErrorCodes.SensorReadError;
		}
		if (array[1] == "0")
		{
			if (ControllerConfiguration.Instance.GripperRentOnMove && array[6] == "0" && !service.SetFinger(GripperFingerState.Rent).Success)
			{
				LogHelper.Instance.Log("MOVE: could not set the gripper fingers to rent.", LogEntryType.Error);
				return ErrorCodes.GripperRentTimeout;
			}
			if (!service.RetractArm().Success)
			{
				LogHelper.Instance.Log("MOVE: could not retract the gripper arm.", LogEntryType.Error);
				return ErrorCodes.GripperRetractTimeout;
			}
		}
		if (ControllerConfiguration.Instance.CheckGripperArmSensorsOnMove && array[0] == "1" && array[1] == "1")
		{
			LogHelper.Instance.Log("MOVE: can't move because extend/retract both triggered.", LogEntryType.Error);
			service.LogInputs(ControlBoards.Picker, LogEntryType.Error);
			return ErrorCodes.SensorError;
		}
		return ErrorCodes.Success;
	}

	public void Initialize(ErrorList errors, IDictionary<string, object> initProperties)
	{
		ControllerConfiguration.Instance.Initialize();
		IDataTableService service = ServiceLocator.Instance.GetService<IDataTableService>();
		ServiceLocator.Instance.AddService(typeof(IKioskConfiguration), new KioskConfigurationService());
		ServiceLocator.Instance.AddService(typeof(IInventoryService), new InventoryService());
		ServiceLocator.Instance.AddService(typeof(IPersistentCounterService), new PersistentCounterService(service));
		ServiceLocator.Instance.AddService(typeof(IHardwareCorrectionStatisticService), new HardwareCorrectionStatisticService(service));
		ServiceLocator.Instance.AddService(typeof(IKioskFunctionCheckService), new KioskFunctionCheckService());
		ServiceLocator.Instance.AddService(typeof(IEmptySearchPatternService), new EmptySearchPatternService());
		bool flag = true;
		if (initProperties.ContainsKey("MotionControlServiceVersion") && initProperties["MotionControlServiceVersion"] is string text)
		{
			flag = text.Equals("Modern", StringComparison.CurrentCultureIgnoreCase);
		}
		if (flag)
		{
			LogHelper.Instance.Log("[ControllerService] Load modern motion controller.");
			MotionControlService_v2 motionControlService_v = new MotionControlService_v2();
			ServiceLocator.Instance.AddService(typeof(IMotionControlService), motionControlService_v);
			motionControlService_v.AddVeto(this);
		}
		else
		{
			LogHelper.Instance.Log("[ControllerService] Load legacy motion controller.");
			MotionControlService motionControlService = new MotionControlService();
			ServiceLocator.Instance.AddService(typeof(IMotionControlService), motionControlService);
			motionControlService.AddVeto(this);
		}
		ControlSystemBridge instance = new ControlSystemBridge();
		ServiceLocator.Instance.AddService(typeof(IControlSystemService), instance);
		ServiceLocator.Instance.AddService(typeof(IControlSystem), instance);
		ServiceLocator.Instance.AddService(typeof(IDiskValidatorService), new DiskValidatorService());
		ServiceLocator.Instance.AddService(typeof(IAirExchangerService), new IceQubeAirExchangerService());
		ServiceLocator.Instance.AddService(typeof(IDumpbinService), new DumpbinServiceBridge());
		ServiceLocator.Instance.AddService(typeof(IDoorSensorService), new DoorSensorService());
		IRuntimeService service2 = ServiceLocator.Instance.GetService<IRuntimeService>();
		ServiceLocator.Instance.AddService(typeof(IPowerCycleDeviceService), new PowerCycleDeviceService(service2));
	}

	public void Shutdown()
	{
		ServiceLocator.Instance.GetService<IMotionControlService>().Shutdown();
		ServiceLocator.Instance.GetService<IControlSystem>().Shutdown();
	}

	public IGetResult Get(IFormattedLog log)
	{
		return Get(DefaultGetObserver.Instance, log);
	}

	public IGetResult Get(IGetObserver observer, IFormattedLog log)
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		if (service.CurrentLocation == null)
		{
			GetResult getResult = new GetResult(service.CurrentLocation);
			getResult.Update(ErrorCodes.LocationOutOfRange);
			return getResult;
		}
		ILocation currentLocation = service.CurrentLocation;
		using GetOperation getOperation = new GetOperation(currentLocation, observer, log);
		IGetResult getResult2 = getOperation.Execute();
		if (getResult2.IsSlotEmpty)
		{
			string msg = $"GET {currentLocation.ToString()} returned SLOTEMPTY";
			log.WriteFormatted(msg);
			return getResult2;
		}
		if (!getResult2.Success)
		{
			string msg2 = $"GET {currentLocation.ToString()} returned error status {getResult2.ToString().ToUpper()}";
			log.WriteFormatted(msg2, LogEntryType.Error);
			return getResult2;
		}
		string msg3 = $"GET {currentLocation.ToString()} ID={getResult2.Previous}";
		log.WriteFormatted(msg3);
		return getResult2;
	}

	public IPutResult Put(IPutObserver observer, string id, IFormattedLog log)
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		if (!PickerBoard.IsFull())
		{
			LogHelper.Instance.Log("PUT: picker is empty - returning PickerEmpty.", LogEntryType.Error);
			IControlSystem service2 = ServiceLocator.Instance.GetService<IControlSystem>();
			service2.LogPickerSensorState();
			service2.LogInputs(ControlBoards.Picker, LogEntryType.Info);
			return new PutResult(id, service.CurrentLocation)
			{
				Code = ErrorCodes.PickerEmpty
			};
		}
		ILocation currentLocation = service.CurrentLocation;
		string arg = (ServiceLocator.Instance.GetService<IDumpbinService>().IsBin(currentLocation) ? "DUMPBIN" : $"Deck = {currentLocation.Deck} Slot = {currentLocation.Slot}");
		using PutOperation putOperation = new PutOperation(id, observer, currentLocation, log);
		IPutResult putResult = putOperation.Execute();
		if (putResult.Success)
		{
			log.WriteFormatted($"PUT {arg} ID={id}");
		}
		else
		{
			string format = $"PUT {arg} ID={id} returned error status {putResult.ToString().ToUpper()}";
			log.WriteFormatted(LogEntryType.Error, format);
		}
		return putResult;
	}

	public IPutResult Put(string id, IFormattedLog log)
	{
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
		IMotionControlService service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
		IPutObserver instance = DefaultPutObserver.Instance;
		if (service.IsBin(service2.CurrentLocation))
		{
			instance = DumpbinPutObserver.Instance;
		}
		return Put(instance, id, log);
	}

	public ITransferResult Transfer(ILocation source, ILocation destination, IFormattedLog log)
	{
		using TransferOperation transferOperation = new TransferOperation(this, source, log);
		return transferOperation.Transfer(destination);
	}

	public ITransferResult Transfer(ILocation source, IList<ILocation> destinations, IFormattedLog log)
	{
		using TransferOperation transferOperation = new TransferOperation(this, source, log);
		return transferOperation.Transfer(destinations);
	}

	public ITransferResult Transfer(ILocation source, IList<ILocation> destinations, IGetObserver observer, IFormattedLog log)
	{
		using TransferOperation transferOperation = new TransferOperation(this, source, log);
		return transferOperation.Transfer(destinations, observer);
	}
}
