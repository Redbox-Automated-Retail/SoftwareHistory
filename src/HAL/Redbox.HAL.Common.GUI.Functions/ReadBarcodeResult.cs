using System;
using Redbox.HAL.Client;

namespace Redbox.HAL.Common.GUI.Functions;

public class ReadBarcodeResult
{
	public bool IsDuplicate { get; private set; }

	public string ScanTime { get; private set; }

	public string Barcode { get; private set; }

	public string Count { get; private set; }

	public static ReadBarcodeResult ReadBarcodeOfDiskInPicker(HardwareService service)
	{
		HardwareJobSchedule hardwareJobSchedule = new HardwareJobSchedule();
		hardwareJobSchedule.StartTime = DateTime.Now;
		hardwareJobSchedule.Priority = HardwareJobPriority.Highest;
		HardwareJobSchedule schedule = hardwareJobSchedule;
		HardwareCommandResult hardwareCommandResult = service.ScheduleJob("read-barcode", "MS read barcode", enableDebugging: false, schedule, out var job);
		if (!hardwareCommandResult.Success)
		{
			return CreateErrorResult();
		}
		ClientHelper clientHelper = new ClientHelper();
		if (!clientHelper.WaitForJob(job, out var _))
		{
			return CreateErrorResult();
		}
		if (!job.GetResults(out var results).Success)
		{
			return CreateErrorResult();
		}
		return FromResults(results);
	}

	public static ReadBarcodeResult FromResults(ProgramResult[] results)
	{
		ReadBarcodeResult readBarcodeResult = new ReadBarcodeResult();
		foreach (ProgramResult programResult in results)
		{
			if (programResult.Code.Equals("Read-ID", StringComparison.CurrentCultureIgnoreCase))
			{
				readBarcodeResult.Barcode = (programResult.ItemID.IsUnknown() ? programResult.ItemID.Metadata : programResult.ItemID.Barcode);
			}
			else if (programResult.Code.Equals("ReadTime", StringComparison.CurrentCultureIgnoreCase))
			{
				readBarcodeResult.ScanTime = programResult.Message;
			}
			else if (programResult.Code.Equals("NumBarcodes", StringComparison.CurrentCultureIgnoreCase))
			{
				readBarcodeResult.Count = programResult.Message;
			}
			else if (programResult.Code == "DuplicateDetected")
			{
				readBarcodeResult.IsDuplicate = true;
			}
		}
		return readBarcodeResult;
	}

	public static ReadBarcodeResult CreateErrorResult()
	{
		return new ReadBarcodeResult();
	}

	private ReadBarcodeResult()
	{
		IsDuplicate = false;
		ScanTime = "0.0s";
		Barcode = "UNKNOWN";
		Count = "0";
	}
}
