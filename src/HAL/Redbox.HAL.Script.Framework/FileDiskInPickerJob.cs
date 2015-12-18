using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.DataMatrix.Framework;

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
		using EmptySearchResult emptySearchResult = new EmptySearchResult();
		if (!emptySearchResult.FindMultipleEmptyLocations())
		{
			base.Context.CreateInfoResult("JobError", "Empty slot not found.");
			return;
		}
		string matrix = "UNKNOWN";
		using (Scanner scanner = new Scanner())
		{
			ControllerService.Instance.CenterItem(CenterDiskMethod.VendDoorAndBack);
			ScanResult scanResult = scanner.Read();
			if (!scanResult.SnapOk)
			{
				base.Context.CreateCameraCaptureErrorResult();
			}
			else
			{
				matrix = scanResult.ScannedMatrix;
			}
			if (InventoryService.Instance.IsBarcodeDuplicate(scanResult.ScannedMatrix, out var _))
			{
				base.Context.CreateInfoResult("DuplicateDetected", "The read ID is a duplicate.", scanResult.ScannedMatrix);
			}
		}
		Location location = emptySearchResult.EmptyLocations[0];
		if (!File(location, matrix))
		{
			MotionController controller = MotionController.GetController();
			HomeAxesResult homeAxesResult = controller.InitAxes(base.Context.AppLog);
			base.Context.CreateInfoResult("InitStatus", string.Format("Init motors returned {0}", homeAxesResult.Succeded ? "SUCCESS" : "FAILURE"));
			if (homeAxesResult.Succeded)
			{
				File(location, matrix);
			}
		}
	}

	internal FileDiskInPickerJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private bool File(List<Location> toTry, string matrix)
	{
		foreach (Location item in toTry)
		{
			if (File(item, matrix))
			{
				return true;
			}
		}
		return false;
	}

	private bool File(Location location, string matrix)
	{
		MotionResult motionResult = ControllerService.Instance.MoveTo(location, MoveMode.Put, base.AppLog);
		if (motionResult != MotionResult.Success)
		{
			base.Context.CreateInfoResult("JobError", $"MOVE {location.ToString()} MODE=PUT {motionResult.ToString()}");
			return false;
		}
		PutResult putResult = ControllerService.Instance.Put(matrix, base.AppLog);
		if (putResult.Success)
		{
			base.Context.CreateInfoResult("JobSuccess", $"Filed {location.ToString()}, id={matrix}");
			return true;
		}
		base.Context.CreateInfoResult("JobError", $"PUT failed at {location.ToString()}, status={putResult.ToString()}");
		return false;
	}
}
