using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "load-bin", Operand = "LOAD-BIN")]
internal sealed class LoadBinJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "KioskTest", flushOnWrite: false, "load-bin-fast");
		_ = ControllerService.Instance;
		if (!DumpBin.Instance.Active)
		{
			applicationLog.Write("The dump bin is not configured.");
			AddError("The dump bin is not configured.");
			return;
		}
		string top = GetTop<string>();
		LoadBy loadBy = Enum<LoadBy>.ParseIgnoringCase(top, LoadBy.None);
		if (loadBy == LoadBy.None)
		{
			string text = $"Unrecognized argument {top}";
			applicationLog.Write(text);
			AddError(text);
			return;
		}
		int top2 = GetTop<int>();
		for (int i = 0; i < top2; i++)
		{
			if (DumpBin.Instance.IsFull())
			{
				base.Context.AppLog.Write("The dump bin is full.");
				return;
			}
			Location location = null;
			if (LoadBy.Barcode == loadBy)
			{
				string top3 = GetTop<string>();
				location = InventoryService.Instance.Lookup(top3);
				if (location == null)
				{
					continue;
				}
			}
			else
			{
				int top4 = GetTop<int>();
				int top5 = GetTop<int>();
				location = new Location(top4, top5);
			}
			if (ControllerService.Instance.MoveTo(location, MoveMode.Get, base.AppLog) != MotionResult.Success)
			{
				AddError("There was an error during the move.");
				return;
			}
			GetResult getResult = ControllerService.Instance.Get(base.AppLog);
			if (!getResult.Success)
			{
				AddError("Unable to fetch disk from drum.");
				return;
			}
			Location location2 = DumpBin.Instance.MoveableLocation();
			if (ControllerService.Instance.MoveTo(location2.Deck, location2.Slot, MoveMode.Put, base.AppLog) != MotionResult.Success)
			{
				AddError("There was an error during the move.");
				return;
			}
			if (!ControllerService.Instance.Put(getResult.Previous, base.AppLog).Success)
			{
				AddError("Unable to place disk into drum.");
				return;
			}
		}
		if (ControllerService.Instance.MoveToVend(MoveMode.Get, base.AppLog) != MotionResult.Success)
		{
			AddError("Unable to move to vend.");
		}
	}

	internal LoadBinJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
