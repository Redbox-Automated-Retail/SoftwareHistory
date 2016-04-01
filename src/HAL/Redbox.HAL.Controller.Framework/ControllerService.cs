using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class ControllerService : IConfigurationObserver, IMoveVeto
{
	private InventoryService InventoryService;

	private DiskValidatorService DiskValidator;

	private IControlSystem ControlSystem;

	private ICoreCommandExecutor Executor;

	private readonly IStoreDescriptor CountersDesc;

	private readonly IStoreDescriptor InventoryDesc;

	private readonly EmptySearchPatternService SearchService = new EmptySearchPatternService();

	private readonly List<IConfigurationObserver> OrderedObservers = new List<IConfigurationObserver>();

	public static ControllerService Instance => Singleton<ControllerService>.Instance;

	public ErrorCodes CanMove()
	{
		if (VendDoorState.Closed != ControlSystem.VendDoorState && !ControlSystem.VendDoorClose().Success)
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
				return ErrorCodes.ObstructionDetected;
			}
		}
		string[] array = ControlSystem.ReadInputs(ControlBoards.Picker);
		if (array.Length == 0)
		{
			return ErrorCodes.SensorReadError;
		}
		if (array[1] == "0")
		{
			if (ControllerConfiguration.Instance.GripperRentOnMove && array[6] == "0" && !ControlSystem.SetFinger(GripperFingerState.Rent).Success)
			{
				LogHelper.Instance.Log("MOVE: could not set the gripper fingers to rent.", LogEntryType.Error);
				return ErrorCodes.GripperRentTimeout;
			}
			if (!ControlSystem.RetractArm().Success)
			{
				LogHelper.Instance.Log("MOVE: could not retract the gripper arm.", LogEntryType.Error);
				return ErrorCodes.GripperRetractTimeout;
			}
		}
		if (ControllerConfiguration.Instance.CheckGripperArmSensorsOnMove && array[0] == "1" && array[1] == "1")
		{
			LogHelper.Instance.Log("MOVE: can't move because extend/retract both triggered.", LogEntryType.Error);
			ControlSystem.LogInputs(ControlBoards.Picker, LogEntryType.Error);
			return ErrorCodes.SensorError;
		}
		return ErrorCodes.Success;
	}

	public void NotifyConfigurationChangeStart()
	{
		foreach (IConfigurationObserver orderedObserver in OrderedObservers)
		{
			orderedObserver.NotifyConfigurationChangeStart();
		}
	}

	public void NotifyConfigurationChangeEnd()
	{
		foreach (IConfigurationObserver orderedObserver in OrderedObservers)
		{
			orderedObserver.NotifyConfigurationChangeEnd();
		}
	}

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[ControllerService] NotifyConfigurationLoaded");
		OrderedObservers.Add(InventoryService);
		OrderedObservers.Add(new DumpbinConfigurationObserver());
		OrderedObservers.Add(new DoorSensorConfigObserver());
		OrderedObservers.Add(SearchService);
		if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode)
		{
			EmulatedControlSystem instance = (EmulatedControlSystem)(Executor = (ICoreCommandExecutor)(ControlSystem = new EmulatedControlSystem()));
			ServiceLocator.Instance.AddService(typeof(IControlSystem), instance);
			ServiceLocator.Instance.AddService(typeof(ICoreCommandExecutor), instance);
		}
		else
		{
			ControlSystem instance2 = (ControlSystem)(Executor = (ICoreCommandExecutor)(ControlSystem = new ControlSystem()));
			ServiceLocator.Instance.AddService(typeof(IControlSystem), instance2);
			ServiceLocator.Instance.AddService(typeof(ICoreCommandExecutor), instance2);
		}
		ControlSystem.AddHandler(DiskValidator);
		foreach (IConfigurationObserver orderedObserver in OrderedObservers)
		{
			orderedObserver.NotifyConfigurationLoaded();
		}
	}

	public void Initialize(ErrorList errors)
	{
		InventoryService = new InventoryService(InventoryDesc.Store);
		ServiceLocator.Instance.AddService(typeof(IPersistentCounterService), new PersistentCounterService(CountersDesc.Store));
		ServiceLocator.Instance.AddService(typeof(IPersistentMapService), new SecretOptions(CountersDesc.Store));
		ServiceLocator.Instance.AddService(typeof(IKioskFunctionCheckService), new KioskFunctionCheckService(CountersDesc.Store));
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		service.RegisterConfiguration(Configurations.Controller.ToString(), ControllerConfiguration.Instance);
		ControllerConfiguration.Instance.AddObserver(this);
		ServiceLocator.Instance.AddService(typeof(IDecksService), ControllerConfiguration.Instance.Manager);
		ServiceLocator.Instance.AddService(typeof(IEmptySearchPatternService), SearchService);
		MotionControlService motionControlService = new MotionControlService();
		ServiceLocator.Instance.AddService(typeof(IMotionControlService), motionControlService);
		motionControlService.AddVeto(this);
		ControllerConfiguration.Instance.AddObserver(motionControlService);
		FraudSensor sensor = new FraudSensor();
		DiskValidator = new DiskValidatorService(sensor);
		ServiceLocator.Instance.AddService(typeof(IDiskValidatorService), DiskValidator);
		ControllerConfiguration.Instance.AddObserver(DiskValidator);
		IceQubeBoard iceQubeBoard = new IceQubeBoard();
		ServiceLocator.Instance.AddService(typeof(IAirExchangerService), iceQubeBoard);
		ControllerConfiguration.Instance.AddObserver(iceQubeBoard);
	}

	public void ParkHardware()
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		service.Shutdown();
		ControlSystem.Shutdown();
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
			ControlSystem.LogPickerSensorState();
			ControlSystem.LogInputs(ControlBoards.Picker, LogEntryType.Info);
			PutResult putResult = new PutResult(id, service.CurrentLocation);
			putResult.Code = ErrorCodes.PickerEmpty;
			return putResult;
		}
		using PutOperation putOperation = new PutOperation(id, observer, service.CurrentLocation, log);
		IPutResult putResult2 = putOperation.Execute();
		if (putResult2.Success)
		{
			string msg = $"PUT {service.CurrentLocation.ToString()} ID={id}";
			log.WriteFormatted(msg);
		}
		else
		{
			string format = $"PUT {service.CurrentLocation.ToString()} ID={id} returned error status {putResult2.ToString().ToUpper()}";
			log.WriteFormatted(LogEntryType.Error, format);
		}
		return putResult2;
	}

	public IPutResult Put(string id, IFormattedLog log)
	{
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
		ServiceLocator.Instance.GetService<IInventoryService>();
		IMotionControlService service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
		IPutObserver instance = DefaultPutObserver.Instance;
		if (service.IsBin(service2.CurrentLocation))
		{
			instance = DumpbinPutObserver.Instance;
		}
		return Put(instance, id, log);
	}

	public TransferResult Transfer(ILocation source, ILocation destination, IFormattedLog log)
	{
		using TransferOperation transferOperation = new TransferOperation(source, log);
		return transferOperation.Transfer(destination);
	}

	public TransferResult Transfer(ILocation source, IList<ILocation> destinations, IFormattedLog log)
	{
		using TransferOperation transferOperation = new TransferOperation(source, log);
		return transferOperation.Transfer(destinations);
	}

	public TransferResult Transfer(ILocation source, IList<ILocation> destinations, IGetObserver observer, IFormattedLog log)
	{
		using TransferOperation transferOperation = new TransferOperation(source, log);
		return transferOperation.Transfer(destinations, observer);
	}

	private ControllerService()
	{
		CountersDesc = new CountersStore();
		InventoryDesc = new InventoryStore();
	}
}
