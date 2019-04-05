using System;
using System.Collections.Generic;
using Redbox.HAL.Client;

namespace Redbox.HAL.Common.GUI.Functions;

public sealed class ReadBarcodeResult
{
	private class ReadDiskJob : JobExecutor
	{
		protected override string JobName => "read-barcode";

		protected override string Label => "MS read barcode";

		internal ReadDiskJob(HardwareService service)
			: base(service)
		{
		}
	}

	public bool IsDuplicate { get; private set; }

	public string ScanTime { get; private set; }

	public string Barcode { get; private set; }

	public string Count { get; private set; }

	public static ReadBarcodeResult ReadBarcodeOfDiskInPicker(HardwareService service)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		ReadDiskJob readDiskJob = new ReadDiskJob(service);
		try
		{
			((JobExecutor)readDiskJob).Run();
			if ((int)((JobExecutor)readDiskJob).EndStatus == 3 || ((JobExecutor)readDiskJob).Errors.ContainsError())
			{
				return CreateErrorResult();
			}
			return FromResults(((JobExecutor)readDiskJob).Results);
		}
		finally
		{
			((IDisposable)readDiskJob)?.Dispose();
		}
	}

	public static ReadBarcodeResult FromResults(List<ProgramResult> results)
	{
		if (results.Count == 0)
		{
			return CreateErrorResult();
		}
		ReadBarcodeResult readBarcodeResult = new ReadBarcodeResult();
		foreach (ProgramResult result in results)
		{
			if (result.Code.Equals("Read-ID", StringComparison.CurrentCultureIgnoreCase))
			{
				readBarcodeResult.Barcode = (result.ItemID.IsUnknown() ? result.ItemID.Metadata : result.ItemID.Barcode);
			}
			else if (result.Code.Equals("ReadTime", StringComparison.CurrentCultureIgnoreCase))
			{
				readBarcodeResult.ScanTime = result.Message;
			}
			else if (result.Code.Equals("NumBarcodes", StringComparison.CurrentCultureIgnoreCase))
			{
				readBarcodeResult.Count = result.Message;
			}
			else if (result.Code == "DuplicateDetected")
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
