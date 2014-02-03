using System;
using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class GroupVendResult
{
	private readonly char[] Separator = new char[1] { '|' };

	internal bool HasFoundInventoryCodes => Targets.FindAll((VendTarget each) => each.Location != null).Count > 0;

	internal string Tag { get; private set; }

	internal bool IsGroup { get; private set; }

	internal List<VendTarget> Targets { get; private set; }

	private ExecutionContext Context { get; set; }

	internal GroupVendResult(ExecutionContext ctx, string codes)
	{
		Context = ctx;
		Targets = new List<VendTarget>();
		string[] array = codes.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
		IsGroup = array.Length > 1;
		if (IsGroup)
		{
			Tag = array[0];
		}
		for (int i = (IsGroup ? 1 : 0); i < array.Length; i++)
		{
			Location location = ControllerService.Instance.Lookup(array[i]);
			if (!IsGroup)
			{
				Context.AppLog.LogLookupFailure(array[i]);
				Context.Send("ItemNotFound");
				ResultHelper.Instance.CreateInfoResult("VendError", "The LOOKUP request failed because the item was not found.", array[i]);
			}
			Targets.Add(new VendTarget(array[i], location));
		}
	}

	internal VendTarget FetchDisk()
	{
		foreach (VendTarget target in Targets)
		{
			string iD = target.ID;
			string msg = $"Move Deck = {target.Location.Deck} Slot = {target.Location.Slot} to get requested barcode = {iD}.";
			Context.AppLog.Write(msg, LogEntryType.Info);
			switch (ControllerService.Instance.MoveTo(target.Location, MoveMode.Get, Context.AppLog))
			{
			case MotionResult.Timeout:
				if (!IsGroup)
				{
					ResultHelper.Instance.CreateInfoResult("MoveFailure", "The MOVE instruction returned TIMEOUT.", iD);
				}
				target.IsStuck = true;
				break;
			default:
				Context.Send("MachineError");
				return null;
			case MotionResult.Success:
				target.GetResult = ControllerService.Instance.Get(testOnEmpty: true, Context.AppLog);
				if (target.GetResult.Success)
				{
					return target;
				}
				target.IsStuck = true;
				if (target.GetResult.IsSlotEmpty)
				{
					if (!IsGroup)
					{
						Context.Send("ItemNotFound");
						ResultHelper.Instance.CreateResult("VendError", "Get returned empty slot.", target.Location.Deck, target.Location.Slot, target.GetResult.Previous, null, iD);
					}
					break;
				}
				ControllerService.Instance.IsGripperClear();
				if (!PickerBoard.Instance.IsFull())
				{
					if (!IsGroup)
					{
						Context.Send("GripperTimedOut");
						ResultHelper.Instance.CreateResult("VendError", "Gripper timed out.", target.Location.Deck, target.Location.Slot, target.GetResult.Previous, null, iD);
					}
					break;
				}
				if (!IsGroup)
				{
					Context.Send("MachineError");
				}
				return null;
			}
		}
		return Targets[Targets.Count - 1];
	}

	internal void ComputeResults()
	{
		if (IsGroup)
		{
			VendTarget vendTarget = Targets.Find((VendTarget each) => each.Vended);
			if (vendTarget != null)
			{
				ResultHelper.Instance.CreateResult("ItemInGroupVended", Tag, vendTarget.Location.Deck, vendTarget.Location.Slot, vendTarget.ID, null, null);
			}
		}
	}
}
