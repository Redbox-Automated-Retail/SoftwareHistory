using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class PutDiskInDumpBinJob : NativeJobAdapter
{
	private readonly CenterDiskMethod[] m_returnReadMethods = new CenterDiskMethod[2]
	{
		CenterDiskMethod.VendDoorAndBack,
		CenterDiskMethod.DrumAndBack
	};

	protected override void ExecuteInner()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			ResultHelper.Instance.CreateInfoResult("PickerEmpty", "The picker is empty.");
			AddError("The picker is empty.");
			return;
		}
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "VMZ", flushOnWrite: true, "Tester-Put-In-Bin");
		if (!base.PickerBoard.IsFull())
		{
			ResultHelper.Instance.CreateInfoResult("PickerEmpty", "The picker is empty.");
			applicationLog.Write("The picker is empty.");
			AddError("The picker is empty.");
			return;
		}
		if (!DumpBin.Instance.Active)
		{
			ResultHelper.Instance.CreateInfoResult("Misconfiguration", "The dump bin is not configured.");
			applicationLog.Write("The dumpbin is not configured.");
			AddError("The bin is not configured.");
			return;
		}
		if (DumpBin.Instance.IsFull())
		{
			ResultHelper.Instance.CreateInfoResult("DumpBinFull", "The dump bin is full.");
			applicationLog.Write("The dumpbin is full.");
			AddError("The dumpbin is full.");
			return;
		}
		using (new CameraHelper(1000))
		{
			Location location = DumpBin.Instance.MoveableLocation();
			MotionResult motionResult = ControllerService.Instance.MoveTo(location, MoveMode.Put, applicationLog);
			if (motionResult != MotionResult.Success)
			{
				ResultHelper.Instance.CreateInfoResult("MoveError", $"MOVE failed with error {motionResult.ToString()}");
				AddError("The MOVE instruction failed.");
				return;
			}
			string text = "UNKNOWN";
			CenterDiskMethod[] returnReadMethods = m_returnReadMethods;
			foreach (CenterDiskMethod method in returnReadMethods)
			{
				PerfFunctions.Wait(1000);
				ReadDiskResult readDiskResult = DataMatrixHelper.Instance.ReadDiscInPicker(method);
				if (ReadDiscStatus.CaptureError == readDiskResult.ReadStatus)
				{
					ResultHelper.Instance.CreateCameraCaptureErrorResult();
					break;
				}
				if (!readDiskResult.IsDiskUnknown())
				{
					text = readDiskResult.Matrix;
					break;
				}
			}
			PutResult putResult = ControllerService.Instance.Put(text, applicationLog);
			if (!putResult.Success)
			{
				ResultHelper.Instance.CreateInfoResult("PutError", $"PUT failed with error {putResult.ToString()}");
				AddError("The PUT instruction failed.");
				return;
			}
			ResultHelper.Instance.CreateInfoResult("PutToBinOK", "The disk was successfully filed", text);
			applicationLog.WriteFormatted("The barcode {0} was put into the dump bin.", text);
			DumpBin.Instance.DumpContents(base.AppLog);
		}
	}
}
