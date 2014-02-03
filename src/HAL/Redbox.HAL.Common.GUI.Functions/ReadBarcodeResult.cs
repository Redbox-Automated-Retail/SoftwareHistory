using System;
using Redbox.HAL.Client;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Common.GUI.Functions;

public class ReadBarcodeResult
{
	public bool IsDuplicate { get; private set; }

	public string ScanTime { get; private set; }

	public string Barcode { get; private set; }

	public string Count { get; private set; }

	public static ReadBarcodeResult ReadBarcodeOfDiskInPicker(HardwareService service)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		HardwareJobSchedule val = new HardwareJobSchedule();
		val.StartTime = DateTime.Now;
		val.Priority = (HardwareJobPriority)1;
		HardwareJobSchedule val2 = val;
		HardwareJob val4 = default(HardwareJob);
		HardwareCommandResult val3 = service.ScheduleJob("read-barcode", "MS read barcode", false, val2, ref val4);
		if (!((ClientCommandResult)(object)val3).Success)
		{
			return CreateErrorResult();
		}
		ClientHelper val5 = new ClientHelper();
		HardwareJobStatus val6 = default(HardwareJobStatus);
		if (!val5.WaitForJob(val4, ref val6))
		{
			return CreateErrorResult();
		}
		ProgramResult[] results = default(ProgramResult[]);
		if (!((ClientCommandResult)(object)val4.GetResults(ref results)).Success)
		{
			return CreateErrorResult();
		}
		return FromResults(results);
	}

	public static ReadBarcodeResult FromResults(ProgramResult[] results)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		ReadBarcodeResult readBarcodeResult = new ReadBarcodeResult();
		foreach (ProgramResult val in results)
		{
			if (val.Code.Equals("Read-ID", StringComparison.CurrentCultureIgnoreCase))
			{
				readBarcodeResult.Barcode = (val.ItemID.IsUnknown() ? val.ItemID.Metadata : val.ItemID.Barcode);
			}
			else if (val.Code.Equals("ReadTime", StringComparison.CurrentCultureIgnoreCase))
			{
				readBarcodeResult.ScanTime = val.Message;
			}
			else if (val.Code.Equals("NumBarcodes", StringComparison.CurrentCultureIgnoreCase))
			{
				readBarcodeResult.Count = val.Message;
			}
			else if (val.Code == "DuplicateDetected")
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
