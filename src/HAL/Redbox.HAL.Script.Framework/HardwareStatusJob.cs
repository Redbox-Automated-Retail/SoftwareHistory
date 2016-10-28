using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "hardware-status", Operand = "HW-STATUS")]
internal sealed class HardwareStatusJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IDoorSensorService service = ServiceLocator.Instance.GetService<IDoorSensorService>();
		int num = 0;
		if (!base.MotionService.IsInitialized && (!ControllerConfiguration.Instance.FixArcusDuringStatusJob || !base.MotionService.Reset(base.AppLog, quick: true)))
		{
			base.Context.CreateInfoResult("MotionControllerCommunicationError", "Could not talk with the motion controller.");
			num++;
		}
		if (ControllerConfiguration.Instance.EnableDoorCheckInStatus && service.Query() != DoorSensorResult.Ok)
		{
			base.Context.CreateInfoResult("MotionControllerCommunicationError", "The door sensors are not in a safe mode.");
			num++;
		}
		using (ClearGripperOperation clearGripperOperation = new ClearGripperOperation())
		{
			if (clearGripperOperation.Execute() != ErrorCodes.Success)
			{
				num++;
				base.Context.CreateInfoResult("GripperObstructed", "The gripper is obstructed.");
			}
		}
		if (PickerBoard.IsFull())
		{
			num++;
			base.Context.CreateInfoResult("GripperObstructed", "The picker is full.");
		}
		if (!base.ControlSystem.VendDoorClose().Success)
		{
			num++;
			base.Context.CreateInfoResult("VendDoorOpen", "Unable to close the vend door.");
		}
		InventoryDatabaseTest inventoryDatabaseTest = new InventoryDatabaseTest();
		if (!inventoryDatabaseTest.Test())
		{
			num++;
			base.Context.CreateInfoResult("MachineError", "Inventory database integrity test failed.");
		}
		if (num == 0)
		{
			base.Context.CreateInfoResult("HardwareStatusOk", "The three tests passed!");
		}
		else
		{
			base.Context.CreateInfoResult("HardwareStatusInError", "One or more ops failed.");
		}
	}

	internal HardwareStatusJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
