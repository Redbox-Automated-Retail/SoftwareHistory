using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

internal class QlmHelper
{
	private readonly CenterDiskMethod[] m_readMethods = new CenterDiskMethod[2]
	{
		CenterDiskMethod.VendDoorAndBack,
		CenterDiskMethod.DrumAndBack
	};

	internal static QlmHelper Instance => Singleton<QlmHelper>.Instance;

	internal void CheckAndClearPicker(ExecutionResult result, ExecutionContext context)
	{
		if (!PickerBoard.IsFull())
		{
			return;
		}
		string text = "UNKNOWN";
		if (!Gripper.IsClear())
		{
			context.ServiceLog("There is an item partially stuck in the gripper track.");
			context.CreateItemStuckResult(text);
			result.Errors.Add(Error.NewError("E001", "Item stuck", "There is an item stuck in the gripper; can't continue."));
			return;
		}
		if (!PickerBoard.IsFull())
		{
			context.ServiceLog("Item was cleared from picker.");
			return;
		}
		using Scanner scanner = new Scanner();
		CenterDiskMethod[] readMethods = m_readMethods;
		foreach (CenterDiskMethod method in readMethods)
		{
			ControllerService.Instance.CenterItem(method);
			ScanResult scanResult = scanner.Read();
			text = scanResult.ScannedMatrix;
			if (!scanResult.SnapOk || scanResult.ReadCode)
			{
				break;
			}
		}
		if (HelperFunctions.PutDiscAway(result, context, text, new DefaultHandler()) != FileDiscStatus.Success)
		{
			context.CreateItemStuckResult(text);
			result.Errors.Add(Error.NewError("E001", "Item stuck", "There is an item stuck in the gripper; can't continue."));
		}
	}

	internal bool CleanupQlmJob(ExecutionResult result, ExecutionContext context, MotionResult failCode)
	{
		if (failCode != MotionResult.Success)
		{
			context.CreateInfoResult("QlmMoveError", $"The MOVE instruction failed with code {failCode.ToString().ToUpper()}.");
		}
		if (ControllerService.Instance.MoveToVend(MoveMode.Get, context.AppLog) != MotionResult.Success)
		{
			context.CreateInfoResult("MachineError", "There was an error executing the MOVEVEND instruction.");
			if (!Gripper.IsClear())
			{
				context.CreateInfoResult("QlmDeckStillEngaged", "The QLM deck is still engaged.");
				result.Errors.Add(Error.NewError("E999", "Execution context error.", "The MOVEVEND instruction failed."));
				return false;
			}
		}
		context.ServiceLog("Disengage the QLM.");
		context.Send("DISENGAGE QLM");
		QlmMotionResult qlmMotionResult = QLM.Disengage(home: true, context.AppLog);
		if (QlmMotionResult.Success != qlmMotionResult)
		{
			context.CreateInfoResult("QlmDeckDisengageFailure", "The QLM disengage failed.");
			result.Errors.Add(Error.NewError("E999", "Execution context error.", "The QLM Case didn't disengage."));
			return false;
		}
		context.CreateInfoResult("QlmDeckDisengageSuccess", "The QLM disengaged successfully.");
		return true;
	}

	private QlmHelper()
	{
	}
}
