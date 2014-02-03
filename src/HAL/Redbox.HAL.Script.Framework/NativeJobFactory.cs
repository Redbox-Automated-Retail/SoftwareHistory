using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal sealed class NativeJobFactory
{
	private Dictionary<string, string> m_jobToOperandMap = new Dictionary<string, string>();

	internal static NativeJobFactory Instance => Singleton<NativeJobFactory>.Instance;

	internal Dictionary<string, string> NativeJobs
	{
		get
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (string key in m_jobToOperandMap.Keys)
			{
				dictionary[key] = $"NATIVEJOB {m_jobToOperandMap[key]}";
			}
			return dictionary;
		}
	}

	internal NativeJobAdapter MakeJobFromOperand(ExecutionContext ctx, ExecutionResult result, string key)
	{
		NativeJobAdapter nativeJobAdapter = null;
		switch (key)
		{
		case "RETURN":
			nativeJobAdapter = new ReturnJob();
			break;
		case "QUICKRETURN":
		{
			ReturnJob returnJob = new ReturnJob();
			returnJob.IsQuick = true;
			nativeJobAdapter = returnJob;
			break;
		}
		case "VEND":
			nativeJobAdapter = new VendJob();
			break;
		case "VERTSYNC":
			nativeJobAdapter = new VerticalSyncJob();
			break;
		case "INIT":
			nativeJobAdapter = new InitJob();
			break;
		case "HW-STATUS":
			nativeJobAdapter = new HardwareStatusJob();
			break;
		case "GET-MACH-INFO":
			nativeJobAdapter = new GetMachineInfoJob();
			break;
		case "SOFT-SYNC":
			nativeJobAdapter = new SoftSyncJob();
			break;
		case "GET-EXCLUDED-LOCS":
			nativeJobAdapter = new GetExcludedLocationsJob();
			break;
		case "READ-FRAUD-DISK":
			nativeJobAdapter = new ReadFraudDiskJob();
			break;
		case "READ-BARCODE":
			nativeJobAdapter = new TesterReadDiskJob();
			break;
		case "GET-AND-CENTER":
			nativeJobAdapter = new GetAndCenterJob();
			break;
		case "RESET-MOTIONCONTROL":
			nativeJobAdapter = new ResetArcusJob();
			break;
		case "PUSH-IN-DVD":
			nativeJobAdapter = new PushInDVDJob();
			break;
		case "CHECK-ICEQUBE-BOARD":
			nativeJobAdapter = new TestIceQubeExchangerJob();
			break;
		case "QLM-TEST-SYNC":
			nativeJobAdapter = new TestQlmJob();
			break;
		case "KIOSK-TEST":
			nativeJobAdapter = new KioskTestJob();
			break;
		case "PUT-DISK-IN-PICKER-IN-BIN":
			nativeJobAdapter = new PutDiskInDumpBinJob();
			break;
		case "VMZ-REMOVED-THINS":
			nativeJobAdapter = new VMZRemovedThinsJob();
			break;
		case "TEST-RETROFIT-DECK":
			nativeJobAdapter = new VMZRetrofitTest();
			break;
		case "LOAD-BIN":
			nativeJobAdapter = new LoadBinJob();
			break;
		case "ADJACENT-BIN-TEST":
			nativeJobAdapter = new AdjacentDumpSlotTestJob();
			break;
		case "QUICK-DECK-TEST":
			nativeJobAdapter = new QuickSlotTestJob();
			break;
		case "ONE-DISK-QUICK-DECK-TEST":
			nativeJobAdapter = new OneDiskQuickTestJob();
			break;
		case "GET-INVENTORY-STATS":
			nativeJobAdapter = new InventoryStatisticsJob();
			break;
		case "GET-NON-BARCODE-INVENTORY":
			nativeJobAdapter = new GetNonBarcodeInventory();
			break;
		case "DISK-GAP-TEST":
			nativeJobAdapter = new DiskGapTestJob();
			break;
		case "CLEAR-MERCH-STATUS":
			nativeJobAdapter = new ClearMerchStatusJob();
			break;
		case "GET-BARCODE-VMZ-LOCATION":
			nativeJobAdapter = new GetDiskLocationJob();
			break;
		case "GET-NONTHINS-IN-VMZ":
			nativeJobAdapter = new GetNonThinsInZoneJob();
			break;
		case "GET-BARCODES-IN-BIN":
			nativeJobAdapter = new GetBarcodesInBinJob();
			break;
		case "VMZ-MERCH-SUMMARY":
			nativeJobAdapter = new GetVMZInfoJob();
			break;
		case "FILE-MARKER-DISK":
			nativeJobAdapter = new FileMarkerDiskJob();
			break;
		case "MS-TAKE-DISK-AT-DOOR":
			nativeJobAdapter = new TakeDiskAtDoorJob();
			break;
		case "PREP-DECK-FOR-REDEPLOY":
			nativeJobAdapter = new PrepDeckForRedeployJob();
			break;
		case "AIR-EXCHANGER-STATUS":
			nativeJobAdapter = new AirExchangerStatusJob();
			break;
		case "RESET-LOCATIONS":
			nativeJobAdapter = new ResetLocationsJob();
			break;
		case "QUICK-SYNC-TEST":
			nativeJobAdapter = new FullFastTest();
			break;
		case "QUICK-SINGLE-SYNC":
			nativeJobAdapter = new FastSyncSingleSlotJob();
			break;
		}
		if (nativeJobAdapter == null)
		{
			return null;
		}
		nativeJobAdapter.Context = ctx;
		nativeJobAdapter.Result = result;
		return nativeJobAdapter;
	}

	private NativeJobFactory()
	{
		m_jobToOperandMap["check-iceqube-board"] = "CHECK-ICEQUBE-BOARD";
		m_jobToOperandMap["get-and-center"] = "GET-AND-CENTER";
		m_jobToOperandMap["get-excluded-locations"] = "GET-EXCLUDED-LOCS";
		m_jobToOperandMap["get-machine-info"] = "GET-MACH-INFO";
		m_jobToOperandMap["hardware-status"] = "HW-STATUS";
		m_jobToOperandMap["init"] = "INIT";
		m_jobToOperandMap["push-in-dvd"] = "PUSH-IN-DVD";
		m_jobToOperandMap["quick-return"] = "QUICKRETURN";
		m_jobToOperandMap["read-fraud-disc"] = "READ-FRAUD-DISK";
		m_jobToOperandMap["reset-motioncontrol"] = "RESET-MOTIONCONTROL";
		m_jobToOperandMap["return"] = "RETURN";
		m_jobToOperandMap["soft-sync"] = "SOFT-SYNC";
		m_jobToOperandMap["vend"] = "VEND";
		m_jobToOperandMap["vertical-sync"] = "VERTSYNC";
		m_jobToOperandMap["read-barcode"] = "READ-BARCODE";
		m_jobToOperandMap["qlm-test-sync"] = "QLM-TEST-SYNC";
		m_jobToOperandMap["KioskTest"] = "KIOSK-TEST";
		m_jobToOperandMap["put-disk-in-picker-in-bin"] = "PUT-DISK-IN-PICKER-IN-BIN";
		m_jobToOperandMap["vmz-removed-thins"] = "VMZ-REMOVED-THINS";
		m_jobToOperandMap["test-retrofit-deck"] = "TEST-RETROFIT-DECK";
		m_jobToOperandMap["load-bin"] = "LOAD-BIN";
		m_jobToOperandMap["adjacent-bin-test"] = "ADJACENT-BIN-TEST";
		m_jobToOperandMap["quick-deck-test"] = "QUICK-DECK-TEST";
		m_jobToOperandMap["one-disk-quick-deck-test"] = "ONE-DISK-QUICK-DECK-TEST";
		m_jobToOperandMap["get-inventory-stats"] = "GET-INVENTORY-STATS";
		m_jobToOperandMap["get-non-barcode-inventory"] = "GET-NON-BARCODE-INVENTORY";
		m_jobToOperandMap["disk-gap-test"] = "DISK-GAP-TEST";
		m_jobToOperandMap["clear-merch-status"] = "CLEAR-MERCH-STATUS";
		m_jobToOperandMap["get-barcode-vmz-location"] = "GET-BARCODE-VMZ-LOCATION";
		m_jobToOperandMap["get-nonthins-in-vmz"] = "GET-NONTHINS-IN-VMZ";
		m_jobToOperandMap["get-barcodes-in-bin"] = "GET-BARCODES-IN-BIN";
		m_jobToOperandMap["vmz-merch-summary"] = "VMZ-MERCH-SUMMARY";
		m_jobToOperandMap["file-marker-disk"] = "FILE-MARKER-DISK";
		m_jobToOperandMap["ms-take-disk-at-door"] = "MS-TAKE-DISK-AT-DOOR";
		m_jobToOperandMap["prep-deck-for-redeploy"] = "PREP-DECK-FOR-REDEPLOY";
		m_jobToOperandMap["air-exchanger-status"] = "AIR-EXCHANGER-STATUS";
		m_jobToOperandMap["reset-locations"] = "RESET-LOCATIONS";
		m_jobToOperandMap["quick-sync-test"] = "QUICK-SYNC-TEST";
		m_jobToOperandMap["quick-single-sync"] = "QUICK-SINGLE-SYNC";
	}
}
