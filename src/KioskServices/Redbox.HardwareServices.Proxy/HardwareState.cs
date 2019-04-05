using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Redbox.Core;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.HardwareServices.Proxy;

public class HardwareState : IHardwareState
{
	private CameraGeneration _cameraGeneration;

	private BarcodeServices _barcodeServices;

	private AirExchangerStatus _airExchangerStatus;

	private ExchangerFanStatus _exchangerFanStatus;

	private string _doorSensorStatus;

	private List<Location> _excludedSlots = new List<Location>();

	private int _maxDeck;

	private int _maxSlot;

	private int _isRunning;

	public int CurrentCameraGeneration
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return (int)_cameraGeneration;
		}
	}

	public int BarcodeDecoder
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return (int)_barcodeServices;
		}
	}

	public string ExchangerStatus
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return _airExchangerStatus.ToString();
		}
	}

	public string AirExchangerFanStatus
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return _exchangerFanStatus.ToString();
		}
	}

	public IEnumerable<Tuple<int, int>> ExcludedLocations
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return _excludedSlots.Select((Location x) => Tuple.Create(x.Deck, x.Slot));
		}
	}

	public int MaxDeck
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return _maxDeck;
		}
	}

	public int MaxSlot
	{
		get
		{
			if (!IsInitialized)
			{
				Initialize();
			}
			return _maxSlot;
		}
	}

	public bool IsInitialized { get; private set; }

	public void Initialize()
	{
		Redbox.Core.LogHelper.Instance.Log("Initialize Hardware State.", Redbox.HAL.Component.Model.LogEntryType.Info);
		Refresh();
		IsInitialized = true;
	}

	public void Refresh()
	{
		if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
		{
			Redbox.Core.LogHelper.Instance.Log("Refresh Hardware State is already running.", Redbox.HAL.Component.Model.LogEntryType.Info);
		}
		try
		{
			HardwareService.Instance.RefreshHardwareState(RefreshPropertiesCallback);
		}
		finally
		{
			_isRunning = 0;
		}
	}

	private void RefreshPropertiesCallback(MachineConfiguration mc)
	{
		_cameraGeneration = mc.CurrentCameraGeneration;
		Redbox.Core.LogHelper.Instance.Log($"Camera Generation: {_cameraGeneration}");
		_barcodeServices = mc.BarcodeDecoder;
		Redbox.Core.LogHelper.Instance.Log($"Barcode Decoder: {_barcodeServices}");
		_airExchangerStatus = mc.AirExchangerStatus;
		Redbox.Core.LogHelper.Instance.Log($"Air Exchanger Status: {_airExchangerStatus}");
		_exchangerFanStatus = mc.AirExchangerFanStatus;
		Redbox.Core.LogHelper.Instance.Log($"Air Exchanger Fan Status: {_exchangerFanStatus}");
		_doorSensorStatus = mc.DoorSensorStatus;
		Redbox.Core.LogHelper.Instance.Log($"Door Sensor Status: {_doorSensorStatus}");
		_excludedSlots = mc.ExcludedSlots.ToList() ?? new List<Location>();
		Redbox.Core.LogHelper.Instance.Log($"Excluded Slots Count: {_excludedSlots.Count()}");
		_maxDeck = mc.MaxDeck;
		Redbox.Core.LogHelper.Instance.Log($"Max Deck: {_maxDeck}");
		_maxSlot = mc.MaxSlot;
		Redbox.Core.LogHelper.Instance.Log($"Max Slot: {_maxSlot}");
		Redbox.Core.LogHelper.Instance.Log("End Processing Refresh Hardware State");
	}

	public void EnqueueProperties()
	{
		Redbox.Core.LogHelper.Instance.Log("Enqueue Hardware State properties.", Redbox.HAL.Component.Model.LogEntryType.Info);
		Redbox.Core.LogHelper.Instance.Log($"Camera: {CurrentCameraGeneration.ToString()}, Decoder: {BarcodeDecoder.ToString()}");
		IHealthServices service = Redbox.HAL.Component.Model.ServiceLocator.Instance.GetService<IHealthServices>();
		if (service == null)
		{
			Redbox.Core.LogHelper.Instance.Log("Hardware State can't locate health services to enqueue properties.");
		}
		else if (_cameraGeneration >= CameraGeneration.Third)
		{
			service.EnqueueProperty("CameraType", CurrentCameraGeneration.ToString());
		}
	}
}
