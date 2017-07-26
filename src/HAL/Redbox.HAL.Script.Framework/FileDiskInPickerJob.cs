using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "file-disk-in-picker", Operand = "FILE-DISK-IN-PICKER")]
internal sealed class FileDiskInPickerJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		if (!PickerBoard.IsFull())
		{
			base.Context.CreateInfoResult("JobError", "Gripper is empty.");
			return;
		}
		using IEmptySearchResult emptySearchResult = ServiceLocator.Instance.GetService<IEmptySearchPatternService>().FindMultipleEmptyLocations();
		if (emptySearchResult.FoundEmpty == 0)
		{
			base.Context.CreateInfoResult("JobError", "Empty slot not found.");
			return;
		}
		string matrix = "UNKNOWN";
		using (Scanner scanner = new Scanner())
		{
			base.ControlSystem.Center(CenterDiskMethod.VendDoorAndBack);
			ScanResult scanResult = scanner.Read();
			if (!scanResult.SnapOk)
			{
				base.Context.CreateCameraCaptureErrorResult();
			}
			else
			{
				matrix = scanResult.ScannedMatrix;
			}
			if (base.InventoryService.IsBarcodeDuplicate(scanResult.ScannedMatrix, out var _))
			{
				base.Context.CreateInfoResult("DuplicateDetected", "The read ID is a duplicate.", scanResult.ScannedMatrix);
			}
		}
		ILocation location = emptySearchResult.EmptyLocations[0];
		if (!File(location, matrix))
		{
			ErrorCodes errorCodes = base.MotionService.InitAxes(base.Context.AppLog);
			base.Context.CreateInfoResult("InitStatus", string.Format("Init motors returned {0}", (errorCodes == ErrorCodes.Success) ? "SUCCESS" : "FAILURE"));
			if (errorCodes == ErrorCodes.Success)
			{
				File(location, matrix);
			}
		}
	}

	internal FileDiskInPickerJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private bool File(ILocation location, string matrix)
	{
		ErrorCodes errorCodes = base.MotionService.MoveTo(location, MoveMode.Put, base.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			base.Context.CreateInfoResult("JobError", $"MOVE {location.ToString()} MODE=PUT {errorCodes.ToString()}");
			return false;
		}
		IPutResult putResult = ServiceLocator.Instance.GetService<IControllerService>().Put(matrix, base.AppLog);
		if (putResult.Success)
		{
			base.Context.CreateInfoResult("JobSuccess", $"Filed {location.ToString()}, id={matrix}");
			return true;
		}
		base.Context.CreateInfoResult("JobError", $"PUT failed at {location.ToString()}, status={putResult.ToString()}");
		return false;
	}
}
