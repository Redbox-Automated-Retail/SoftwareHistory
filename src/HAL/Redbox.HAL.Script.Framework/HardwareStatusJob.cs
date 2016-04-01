using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "hardware-status", Operand = "HW-STATUS")]
internal sealed class HardwareStatusJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			int num = 0;
			InventoryDatabaseTest inventoryDatabaseTest = new InventoryDatabaseTest();
			if (!inventoryDatabaseTest.Test())
			{
				num++;
				base.Context.CreateInfoResult("MachineError", "Inventory database integrity test failed.");
			}
			ProcessError(num);
			return;
		}
		IDoorSensorService service = ServiceLocator.Instance.GetService<IDoorSensorService>();
		int num2 = 0;
		if (!base.MotionService.IsInitialized && (!ControllerConfiguration.Instance.FixArcusDuringStatusJob || !base.MotionService.Reset(base.AppLog, quick: true)))
		{
			base.Context.CreateInfoResult("MotionControllerCommunicationError", "Could not talk with the motion controller.");
			num2++;
		}
		if (ControllerConfiguration.Instance.EnableDoorCheckInStatus && service.Query() != DoorSensorResult.Ok)
		{
			base.Context.CreateInfoResult("MotionControllerCommunicationError", "The door sensors are not in a safe mode.");
			num2++;
		}
		using (ClearGripperOperation clearGripperOperation = new ClearGripperOperation())
		{
			if (clearGripperOperation.Execute() != ErrorCodes.Success)
			{
				num2++;
				base.Context.CreateInfoResult("GripperObstructed", "The gripper is obstructed.");
			}
		}
		if (PickerBoard.IsFull())
		{
			num2++;
			base.Context.CreateInfoResult("GripperObstructed", "The picker is full.");
		}
		if (!base.ControlSystem.VendDoorClose().Success)
		{
			num2++;
			base.Context.CreateInfoResult("VendDoorOpen", "Unable to close the vend door.");
		}
		InventoryDatabaseTest inventoryDatabaseTest2 = new InventoryDatabaseTest();
		if (!inventoryDatabaseTest2.Test())
		{
			num2++;
			base.Context.CreateInfoResult("MachineError", "Inventory database integrity test failed.");
		}
		ProcessError(num2);
	}

	internal HardwareStatusJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}

	private void ProcessError(int count)
	{
		if (count == 0)
		{
			base.Context.CreateInfoResult("HardwareStatusOk", "The three tests passed!");
		}
		else
		{
			base.Context.CreateInfoResult("HardwareStatusInError", "One or more ops failed.");
		}
	}
}
