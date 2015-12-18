using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "vend-unknown", Operand = "VEND-UNKNOWN")]
internal sealed class VendUnknownJob : NativeJobAdapter, IGetObserver
{
	private const string HW_ERR_MSG = "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.";

	public void OnStuck(GetResult result, Deck deck, int slot)
	{
	}

	public bool OnEmpty(GetResult result, Deck deck, int slot)
	{
		if (!ControllerConfiguration.Instance.TestSlotDuringRemoval)
		{
			result.UpdateCode(GetResultCode.ItemStuck);
			return false;
		}
		LocationTestResult locationTestResult = Gripper.Peek();
		if (!locationTestResult.TestOk)
		{
			string msg = $"Peek Deck = {deck.Number} Slot = {slot} returned status {locationTestResult.Error.ToString()} after GET returned SLOTEMPTY";
			base.Context.AppLog.WriteFormatted(msg, LogEntryType.Error);
			result.SetHardwareError(locationTestResult.Error);
			return false;
		}
		if (GripperPeekResult.Empty == locationTestResult.PeekResult)
		{
			return true;
		}
		result.UpdateCode(GetResultCode.ItemStuck);
		return false;
	}

	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Unknowns", flushOnWrite: false, "VendUnknown");
		applicationLog.Write("Vend unknown start.");
		base.Context.Send("VendStart");
		PerfFunctions.SpinWait(30);
		base.Context.Send("VendGraphicDisplay");
		PickerStatus pickerStatus = HelperFunctions.CheckPicker(base.Result, base.Context, null);
		if (PickerStatus.Empty != pickerStatus)
		{
			AddError("Unable to clear the picker.");
			return;
		}
		int top = GetTop<int>();
		int top2 = GetTop<int>();
		Location location = new Location(top, top2);
		LogHelper.Instance.Log("Move to {0} to pick requested item.", location.ToString());
		if (CorrectiveMove(top, top2, MoveMode.Get) != MotionResult.Success)
		{
			base.Context.Send("MachineError");
			base.Context.CreateResult("MoveError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", top, top2, null, null, null);
			AddError("MACHINE ERROR");
			return;
		}
		GetResult getResult = base.Controller.Get(this, base.Context.AppLog);
		if (getResult.EmptyOrStuck)
		{
			if (!ControllerConfiguration.Instance.RotateDrumDuringUnknownRemoval2)
			{
				base.Context.CreateResult("SlotEmpty", "The slot is empty.", top, top2, null, null, null);
				VendEnd();
			}
			else if (getResult.IsSlotEmpty)
			{
				base.Context.AppLog.WriteFormatted("Slot is empty, moving to next.");
				base.Context.CreateResult("SlotEmpty", "The slot is empty.", top, top2, null, null, null);
				VendEnd();
			}
			else
			{
				LogHelper.Instance.Log("Disc stuck in slot, moving to next.");
				base.Context.CreateResult("GetAfterPeekShowsFull", "This Location was NOT updated in inventory.", top, top2, getResult.Previous, getResult.ReturnTime, null);
				ShowEmptyStuck(location);
			}
			return;
		}
		if (!getResult.Success)
		{
			base.Context.CreateResult("GetError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", top, top2, null, null, null);
			AddError("MACHINE ERROR");
			return;
		}
		base.Context.Send("MoveToVend");
		if (CorrectiveMoveVend(MoveMode.None) != MotionResult.Success)
		{
			base.Context.Send("MachineError");
			base.Context.CreateResult("MoveError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", top, top2, null, null, null);
			AddError("MACHINE ERROR");
			return;
		}
		base.Context.Send("PickupGraphicDisplay");
		VendItemResult vendItemResult = base.Controller.VendItem();
		if (vendItemResult.PickerStatus == PickerStatus.HardwareError)
		{
			base.Context.Send("MachineError");
			base.Context.CreateInfoResult("MachineError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.");
			LogHelper.Instance.Log("There was a hardware error vending the disc.", LogEntryType.Error);
			base.Context.AppLog.Write("Vend disc returned a hardware error; check the logs.");
			AddError("MACHINE ERROR");
			return;
		}
		if (vendItemResult.PickerStatus == PickerStatus.Empty)
		{
			VendDoor.Close();
			base.Context.Send("ItemVended");
			base.Context.AppLog.Write("The item was taken.");
			base.Context.CreateResult("ItemVended", "The item was successfully vended.", top, top2, getResult.Previous, getResult.ReturnTime, null);
			VendEnd();
			return;
		}
		LogHelper.Instance.Log("The iterm was not taken; pull it in and file it away.");
		PickerStatus pickerStatus2 = base.Controller.TakeDiskAtDoor();
		if (PickerStatus.DiscInPicker != pickerStatus2)
		{
			base.Context.AppLog.WriteFormatted("ACCEPT-DISK-AT-DOOR failed with error code {0}", pickerStatus2.ToString());
			base.Context.CreateResult("RollInToSensor3TimedOut", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", top, top2, null, null, null);
			AddError("MACHINE ERROR");
		}
		else if (CorrectiveMove(location, MoveMode.Put) != MotionResult.Success)
		{
			base.Context.Send("MachineError");
			base.Context.CreateResult("MoveError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", top, top2, null, null, null);
			AddError("MACHINE ERROR");
		}
		else if (!base.Controller.Put("UNKNOWN", base.Context.AppLog).Success)
		{
			base.Context.CreateResult("PutError", "Unable to remove unknown due to hardware errors. Inspect the slot after completing the process.", top, top2, null, null, null);
			AddError("MACHINE ERROR");
		}
		else
		{
			location.UpdateReturnTime(getResult.ReturnTime);
			base.Context.CreateResult("UnclaimedVend", "The vend request failed because the item was not taken.  It has been returned.", top, top2, getResult.Previous, null, null);
			VendEnd();
		}
	}

	internal VendUnknownJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private void ShowEmptyStuck(Location target)
	{
		Deck deck = DecksManager.Instance.GetDeck(target.Deck);
		int num = target.Slot - 5;
		if (num <= 0)
		{
			num = deck.NumberOfSlots - Math.Abs(num);
		}
		if (CorrectiveMove(target.Deck, num, MoveMode.None) != MotionResult.Success)
		{
			base.Context.CreateInfoResult("MoveOffsetError", "There was an error moving the offset.");
			AddError("There was an error moving to the offset position.");
			return;
		}
		Gripper.SetFinger(GripperFingerState.Rent);
		Gripper.TimedExtend(null);
		PickerBoard.SetSensors(on: true);
		base.Context.CreateInfoResult("MoveOffsetOk", "The damaged disk is located 5 slots left, starting at the disk next to the extended gripper fingers.");
	}

	private void VendEnd()
	{
		VendDoor.Close();
		Track.Close();
		CorrectiveMoveVend(MoveMode.Get);
	}
}
