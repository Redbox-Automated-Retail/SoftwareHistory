using System;
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
		DoorSensorResult doorSensorResult = DoorSensors.Instance.Query();
		if (doorSensorResult != DoorSensorResult.Ok)
		{
			string text = $"Door sensor query returned error {doorSensorResult.ToString()}";
			AddError(text);
			applicationLog.Write(text);
			base.Context.CreateInfoResult("DoorOpen", text);
			return;
		}
		Deck deck = DecksManager.Instance.GetDeck(1);
		if (top >= deck.Quadrants.Length)
		{
			AddError("The drum as fully rotated.");
			applicationLog.Write("The drum is fully rotated.");
			base.Context.CreateInfoResult("RotationsExceeded", "There are no more rotations to be made.");
			return;
		}
		int num = ((top > 0) ? (deck.Quadrants.Length - top) : 0);
		Quadrant currentQuadrant = deck.Quadrants[num];
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck2)
		{
			if (deckCount > 0 && deck2.Number > deckCount)
			{
				return false;
			}
			totalReset += deck2.SwapEmptyWith("UNKNOWN", MerchFlags.Unload, currentQuadrant.Slots).Count;
			return true;
		});
		applicationLog.WriteFormatted("{0} locations were marked for unload covering slots {1} -> {2}.", totalReset, currentQuadrant.Slots.Start, currentQuadrant.Slots.End);
		base.Context.CreateInfoResult("LocationChangedCount", totalReset.ToString());
		if (top == 0)
		{
			num = deck.Quadrants.Length;
		}
		if (--num == 0)
		{
			AddError("The drum as fully rotated.");
			applicationLog.Write("The drum is fully rotated.");
			base.Context.CreateInfoResult("RotationsExceeded", "There are no more rotations to be made.");
			return;
		}
		Quadrant quadrant = deck.Quadrants[num];
		int num2 = quadrant.Slots.Start - 5;
		if (num2 <= 0)
		{
			num2 = deck.NumberOfSlots - Math.Abs(num2);
		}
		applicationLog.Write($"PrepZone: Rotations = {top}, move to target Deck = {deck.Number} Slot = {num2}");
		MotionResult motionResult = ControllerService.Instance.MoveTo(1, num2, MoveMode.None, base.Context.AppLog);
		if (motionResult != MotionResult.Success)
		{
			string text2 = $"The MOVE instruction failed with error {motionResult.ToString()}";
			base.Context.CreateInfoResult("MachineError", text2);
			AddError(text2);
		}
	}

	internal PrepZoneForUnloadJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
