using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class QlmHelper
{
	private readonly CenterDiskMethod[] m_readMethods = new CenterDiskMethod[2]
	{
		CenterDiskMethod.VendDoorAndBack,
		CenterDiskMethod.DrumAndBack
	};

	internal static QlmHelper Instance => Singleton<QlmHelper>.Instance;

	private QLM QLM => QLM.Instance;

	internal void CheckAndClearPicker(ExecutionResult result)
	{
		if (!PickerBoard.Instance.IsFull())
		{
			return;
		}
		string text = "UNKNOWN";
		if (!ControllerService.Instance.IsGripperClear())
		{
			ServiceLogHelper.Instance.WriteErrorMessage("There is an item partially stuck in the gripper track.");
			ResultHelper.Instance.CreateItemStuckResult(text);
			result.Errors.Add(Error.NewError("E001", "Item stuck", "There is an item stuck in the gripper; can't continue."));
			return;
		}
		if (!PickerBoard.Instance.IsFull())
		{
			ServiceLogHelper.Instance.WriteInfoMessage("Item was cleared from picker.");
			return;
		}
		using (new CameraHelper(500))
		{
			CenterDiskMethod[] readMethods = m_readMethods;
			foreach (CenterDiskMethod method in readMethods)
			{
				ReadDiskResult readDiskResult = DataMatrixHelper.Instance.ReadDiscInPicker(method);
				text = readDiskResult.Matrix;
				if (readDiskResult.ReadStatus != ReadDiscStatus.Success)
				{
					text = "UNKNOWN";
					break;
				}
				if (!readDiskResult.IsDiskUnknown())
				{
					break;
				}
			}
			if (HelperFunctions.PutDiscAway(result, text, new DefaultHandler()) != FileDiscStatus.Success)
			{
				ResultHelper.Instance.CreateItemStuckResult(text);
				result.Errors.Add(Error.NewError("E001", "Item stuck", "There is an item stuck in the gripper; can't continue."));
			}
		}
	}

	internal bool CleanupQlmJob(ExecutionResult result, MotionResult failCode)
	{
		if (failCode != MotionResult.Success)
		{
			ResultHelper.Instance.CreateInfoResult("QlmMoveError", $"The MOVE instruction failed with code {failCode.ToString().ToUpper()}.");
		}
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		if (ControllerService.Instance.MoveToVend(MoveMode.Get, activeContext.AppLog) != MotionResult.Success)
		{
			ResultHelper.Instance.CreateInfoResult("MachineError", "There was an error executing the MOVEVEND instruction.");
			if (!ControllerService.Instance.IsGripperClear())
			{
				ResultHelper.Instance.CreateInfoResult("QlmDeckStillEngaged", "The QLM deck is still engaged.");
				result.Errors.Add(Error.NewError("E999", "Execution context error.", "The MOVEVEND instruction failed."));
				return false;
			}
		}
		ServiceLogHelper.Instance.WriteInfoMessage("Disengage the QLM.");
		activeContext.Send("DISENGAGE QLM");
		QlmMotionResult qlmMotionResult = QLM.Disengage(home: true, activeContext.AppLog);
		if (QlmMotionResult.Success != qlmMotionResult)
		{
			ResultHelper.Instance.CreateInfoResult("QlmDeckDisengageFailure", "The QLM disengage failed.");
			result.Errors.Add(Error.NewError("E999", "Execution context error.", "The QLM Case didn't disengage."));
			return false;
		}
		ResultHelper.Instance.CreateInfoResult("QlmDeckDisengageSuccess", "The QLM disengaged successfully.");
		return true;
	}

	private QlmHelper()
	{
	}
}
