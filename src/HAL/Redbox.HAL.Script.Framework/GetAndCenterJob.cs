using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class GetAndCenterJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ServiceLogHelper.Instance.WriteInfoMessage("Start get and center.");
		int top = GetTop<int>();
		int top2 = GetTop<int>();
		MotionResult motionResult = ControllerService.Instance.MoveTo(top2, top, MoveMode.Get, base.AppLog);
		if (motionResult != MotionResult.Success)
		{
			ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Error, "There was an error executing MOVE: {0}", motionResult.ToString().ToUpper());
			ResultHelper.Instance.CreateResult("MachineError", "There was an error executing the MOVE instruction.", top2, top, null, null, null);
			return;
		}
		GetResult getResult = ControllerService.Instance.Get(testOnEmpty: false, base.AppLog);
		if (!getResult.Success)
		{
			ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Error, "There was an error executing GET: {0}", getResult.ToString());
			ResultHelper.Instance.CreateResult("MachineError", "There was an error executing the GET instruction.", top2, top, null, null, null);
		}
		else
		{
			ControllerService.Instance.CenterItem(CenterDiskMethod.VendDoorAndBack);
		}
	}
}
