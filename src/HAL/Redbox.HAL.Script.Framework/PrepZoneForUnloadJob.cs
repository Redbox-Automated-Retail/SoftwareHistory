using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "prep-zone-for-unload", Operand = "PREP-ZONE-FOR-UNLOAD")]
internal sealed class PrepZoneForUnloadJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "VMZ", flushOnWrite: false, null);
		int top = GetTop<int>();
		int deckCount = GetTop<int>();
		int totalReset = 0;
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			AddError("Prep Zone for unload is supported only on VMZ machines. I'm sorry.");
			applicationLog.Write("Prep Zone for unload is supported only on VMZ machines. I'm sorry.");
			return;
		}
		IDoorSensorService service = ServiceLocator.Instance.GetService<IDoorSensorService>();
		DoorSensorResult doorSensorResult = service.Query();
		if (doorSensorResult != DoorSensorResult.Ok)
		{
			string text = $"Door sensor query returned error {doorSensorResult.ToString()}";
			AddError(text);
			applicationLog.Write(text);
			base.Context.CreateInfoResult("DoorOpen", text);
			return;
		}
		IDeck first = base.DecksService.First;
		if (top >= first.Quadrants.Count)
		{
			AddError("The drum as fully rotated.");
			applicationLog.Write("The drum is fully rotated.");
			base.Context.CreateInfoResult("RotationsExceeded", "There are no more rotations to be made.");
			return;
		}
		int num = ((top > 0) ? (first.Quadrants.Count - top) : 0);
		IQuadrant currentQuadrant = first.Quadrants[num];
		base.DecksService.ForAllDecksDo(delegate(IDeck deck)
		{
			if (deckCount > 0 && deck.Number > deckCount)
			{
				return false;
			}
			totalReset += base.InventoryService.SwapEmptyWith(deck, "UNKNOWN", MerchFlags.Unload, currentQuadrant.Slots).Count;
			return true;
		});
		applicationLog.WriteFormatted("{0} locations were marked for unload covering slots {1} -> {2}.", totalReset, currentQuadrant.Slots.Start, currentQuadrant.Slots.End);
		base.Context.CreateInfoResult("LocationChangedCount", totalReset.ToString());
		if (top == 0)
		{
			num = first.Quadrants.Count;
		}
		if (--num == 0)
		{
			AddError("The drum as fully rotated.");
			applicationLog.Write("The drum is fully rotated.");
			base.Context.CreateInfoResult("RotationsExceeded", "There are no more rotations to be made.");
			return;
		}
		IQuadrant quadrant = first.Quadrants[num];
		int num2 = quadrant.Slots.Start - 5;
		if (num2 <= 0)
		{
			num2 = first.NumberOfSlots - Math.Abs(num2);
		}
		applicationLog.Write($"PrepZone: Rotations = {top}, move to target Deck = {first.Number} Slot = {num2}");
		ErrorCodes errorCodes = base.MotionService.MoveTo(1, num2, MoveMode.None, base.Context.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			string text2 = $"The MOVE instruction failed with error {errorCodes.ToString()}";
			base.Context.CreateInfoResult("MachineError", text2);
			AddError(text2);
		}
	}

	internal PrepZoneForUnloadJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
