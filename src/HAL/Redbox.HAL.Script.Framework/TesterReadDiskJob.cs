using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class TesterReadDiskJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode)
		{
			PushResults(TimeSpan.FromMilliseconds(0.34), 4, "999999");
			return;
		}
		using (new CameraHelper(1000))
		{
			PerfFunctions.Wait(1000);
			ReadDiskResult readDiskResult = DataMatrixHelper.Instance.ReadDiscInPicker(CenterDiskMethod.VendDoorAndBack);
			if (ReadDiscStatus.CaptureError == readDiskResult.ReadStatus)
			{
				ResultHelper.Instance.CreateCameraCaptureErrorResult();
				PushResults(default(TimeSpan), 0, "UNKNOWN");
				return;
			}
			PushResults(readDiskResult.ReadTime, readDiskResult.FoundCodes, readDiskResult.Matrix);
			if (ControllerConfiguration.Instance.IsBarcodeDuplicate(readDiskResult.Matrix, out var _))
			{
				ResultHelper.Instance.CreateInfoResult("DuplicateDetected", "The read ID is a duplicate.", readDiskResult.Matrix);
			}
		}
	}

	private void PushResults(TimeSpan ts, int readCodes, string id)
	{
		string message = $"{ts.Seconds}.{ts.Milliseconds} s";
		ResultHelper.Instance.CreateInfoResult("ReadTime", message);
		ResultHelper.Instance.CreateInfoResult("NumBarcodes", readCodes.ToString());
		ResultHelper.Instance.CreateResult("Read-ID", "Id read.", null, null, id, null, null);
	}
}
