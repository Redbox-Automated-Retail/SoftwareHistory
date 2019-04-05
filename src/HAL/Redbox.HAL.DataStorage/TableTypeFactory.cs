using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

public sealed class TableTypeFactory : ITableTypeFactory
{
	public ILocation NewLocation(int deck, int slot)
	{
		return new Location(deck, slot);
	}

	public ILocation NewLocation(int deck, int slot, string id, DateTime? returnTime, bool excluded, int stuck, MerchFlags flags)
	{
		Location location = new Location(deck, slot);
		location.ReturnDate = returnTime;
		location.Excluded = excluded;
		location.StuckCount = stuck;
		location.Flags = flags;
		location.ID = id;
		return location;
	}

	public IPersistentCounter NewCounter(string name, int defaultValue)
	{
		PersistentCounter persistentCounter = new PersistentCounter();
		persistentCounter.Name = name;
		persistentCounter.Value = defaultValue;
		return persistentCounter;
	}

	public IDumpBinInventoryItem NewBinItem(string barcode, DateTime timestamp)
	{
		return new DumpbinItem(barcode, timestamp);
	}

	public IPersistentOption NewPersistentOption(string name, string value)
	{
		PersistentOption persistentOption = new PersistentOption();
		persistentOption.Key = name;
		persistentOption.Value = value;
		return persistentOption;
	}

	public IHardwareCorrectionStatistic NewStatistic(HardwareCorrectionStatistic stat, string programName, bool correctionOk, DateTime timestamp)
	{
		HardwareCorrectionStat hardwareCorrectionStat = new HardwareCorrectionStat();
		hardwareCorrectionStat.Statistic = stat;
		hardwareCorrectionStat.ProgramName = programName;
		hardwareCorrectionStat.CorrectionOk = correctionOk;
		hardwareCorrectionStat.CorrectionTime = timestamp;
		return hardwareCorrectionStat;
	}

	public IKioskFunctionCheckData NewCheckData(string verticalSlotTestResult, string initResult, string vendDoorTestResult, string trackTestResult, string snapDecodeTestResult, string touchscreenDriverTestResult, string cameraDriverTestResult, DateTime timestamp, string userIdentifier)
	{
		KioskFunctionCheckData kioskFunctionCheckData = new KioskFunctionCheckData();
		kioskFunctionCheckData.VerticalSlotTestResult = verticalSlotTestResult;
		kioskFunctionCheckData.InitTestResult = initResult;
		kioskFunctionCheckData.VendDoorTestResult = vendDoorTestResult;
		kioskFunctionCheckData.TrackTestResult = trackTestResult;
		kioskFunctionCheckData.SnapDecodeTestResult = snapDecodeTestResult;
		kioskFunctionCheckData.TouchscreenDriverTestResult = touchscreenDriverTestResult;
		kioskFunctionCheckData.CameraDriverTestResult = cameraDriverTestResult;
		kioskFunctionCheckData.Timestamp = timestamp;
		kioskFunctionCheckData.UserIdentifier = userIdentifier;
		return kioskFunctionCheckData;
	}
}
