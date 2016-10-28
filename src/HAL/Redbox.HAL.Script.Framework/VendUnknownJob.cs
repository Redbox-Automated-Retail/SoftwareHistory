using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "vend-unknown", Operand = "VEND-UNKNOWN")]
internal sealed class VendUnknownJob : NativeJobAdapter, IGetObserver
{
	private const string HW_ERR_MSG = "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.";

	public void OnStuck(IGetResult result)
	{
	}

	public bool OnEmpty(IGetResult result)
	{
		if (!ControllerConfiguration.Instance.TestSlotDuringRemoval)
		{
			result.Update(ErrorCodes.ItemStuck);
			return false;
		}
		using PeekOperation peekOperation = new PeekOperation();
		IPeekResult peekResult = peekOperation.Execute();
		if (!peekResult.TestOk)
		{
			string msg = $"Peek Deck = {result.Location.Deck} Slot = {result.Location.Slot} returned status {peekResult.Error.ToString()} after GET returned SLOTEMPTY";
			base.Context.AppLog.WriteFormatted(msg, LogEntryType.Error);
			result.Update(peekResult.Error);
			return false;
		}
		if (!peekResult.IsFull)
		{
			return true;
		}
		result.Update(ErrorCodes.ItemStuck);
		return false;
	}

	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Unknowns", flushOnWrite: false, "VendUnknown");
		applicationLog.Write("Vend unknown start.");
		base.Context.Send("VendStart");
		PerfFunctions.SpinWait(30);
		base.Context.Send("VendGraphicDisplay");
		using (CheckPickerOperation checkPickerOperation = new CheckPickerOperation(base.Context))
		{
			if (ErrorCodes.PickerEmpty != checkPickerOperation.CheckPicker(base.Result, null))
			{
				AddError("Unable to clear the picker.");
				return;
			}
		}
		int num = base.Context.PopTop<int>();
		int num2 = base.Context.PopTop<int>();
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		ILocation location = service.Get(num, num2);
		LogHelper.Instance.Log("Move to {0} to pick requested item.", location.ToString());
		if (CorrectiveMove(num, num2, MoveMode.Get) != ErrorCodes.Success)
		{
			base.Context.Send("MachineError");
			base.Context.CreateResult("MoveError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", num, num2, null, null, null);
			AddError("MACHINE ERROR");
			return;
		}
		IControllerService service2 = ServiceLocator.Instance.GetService<IControllerService>();
		IGetResult getResult = service2.Get(this, base.Context.AppLog);
		if (getResult.IsSlotEmpty || getResult.ItemStuck)
		{
			if (!ControllerConfiguration.Instance.RotateDrumDuringUnknownRemoval2)
			{
				base.Context.CreateResult("SlotEmpty", "The slot is empty.", num, num2, null, null, null);
				VendEnd();
			}
			else if (getResult.IsSlotEmpty)
			{
				base.Context.AppLog.WriteFormatted("Slot is empty, moving to next.");
				base.Context.CreateResult("SlotEmpty", "The slot is empty.", num, num2, null, null, null);
				VendEnd();
			}
			else
			{
				LogHelper.Instance.Log("Disc stuck in slot, moving to next.");
				base.Context.CreateResult("GetAfterPeekShowsFull", "This Location was NOT updated in inventory.", num, num2, getResult.Previous, getResult.ReturnTime, null);
				ShowEmptyStuck(location);
			}
			return;
		}
		if (!getResult.Success)
		{
			base.Context.CreateResult("GetError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", num, num2, null, null, null);
			AddError("MACHINE ERROR");
			return;
		}
		base.Context.Send("MoveToVend");
		if (CorrectiveMoveVend(MoveMode.None) != ErrorCodes.Success)
		{
			base.Context.Send("MachineError");
			base.Context.CreateResult("MoveError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", num, num2, null, null, null);
			AddError("MACHINE ERROR");
			return;
		}
		base.Context.Send("PickupGraphicDisplay");
		using (VendItemOperation vendItemOperation = new VendItemOperation())
		{
			VendItemResult vendItemResult = vendItemOperation.Execute();
			if (vendItemResult.Status == ErrorCodes.PickerEmpty)
			{
				base.ControlSystem.VendDoorClose();
				base.Context.Send("ItemVended");
				base.Context.AppLog.Write("The item was taken.");
				base.Context.CreateResult("ItemVended", "The item was successfully vended.", num, num2, getResult.Previous, getResult.ReturnTime, null);
				VendEnd();
				return;
			}
			if (vendItemResult.Status != ErrorCodes.PickerFull)
			{
				base.Context.Send("MachineError");
				base.Context.CreateInfoResult("MachineError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.");
				LogHelper.Instance.Log("There was a hardware error vending the disc.", LogEntryType.Error);
				base.Context.AppLog.Write("Vend disc returned a hardware error; check the logs.");
				AddError("MACHINE ERROR");
				return;
			}
		}
		LogHelper.Instance.Log("The iterm was not taken; pull it in and file it away.");
		using (AcceptDiskOperation acceptDiskOperation = new AcceptDiskOperation())
		{
			ErrorCodes errorCodes = acceptDiskOperation.Execute();
			if (ErrorCodes.PickerFull != errorCodes)
			{
				base.Context.AppLog.WriteFormatted("ACCEPT-DISK-AT-DOOR failed with error code {0}", errorCodes.ToString());
				base.Context.CreateResult("RollInToSensor3TimedOut", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", num, num2, null, null, null);
				AddError("MACHINE ERROR");
				return;
			}
			if (CorrectiveMove(location, MoveMode.Put) != ErrorCodes.Success)
			{
				base.Context.Send("MachineError");
				base.Context.CreateResult("MoveError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", num, num2, null, null, null);
				AddError("MACHINE ERROR");
				return;
			}
			base.ControlSystem.TrackCycle();
			if (!service2.Put("UNKNOWN", base.Context.AppLog).Success)
			{
				base.Context.CreateResult("PutError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", num, num2, null, null, null);
				AddError("MACHINE ERROR");
				return;
			}
		}
		location.ReturnDate = getResult.ReturnTime;
		service.Save(location);
		base.Context.CreateResult("UnclaimedVend", "The vend request failed because the item was not taken.  It has been returned.", num, num2, getResult.Previous, null, null);
		VendEnd();
	}

	internal VendUnknownJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private void VendEnd()
	{
		base.ControlSystem.VendDoorClose();
		base.ControlSystem.TrackClose();
		CorrectiveMoveVend(MoveMode.Get);
	}
}
