using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-and-center", Operand = "GET-AND-CENTER")]
internal sealed class GetAndCenterJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		base.Context.ServiceLog("Start get and center.");
		int top = GetTop<int>();
		int top2 = GetTop<int>();
		bool top3 = GetTop<bool>();
		MotionResult motionResult = ControllerService.Instance.MoveTo(top2, top, MoveMode.Get, base.AppLog);
		string message = $"MOVE DECK={top2} SLOT={top} MODE=GET {motionResult.ToString().ToUpper()}";
		if (motionResult != MotionResult.Success)
		{
			base.Context.ServiceLog(LogEntryType.Error, "There was an error executing MOVE: {0}", motionResult.ToString());
			base.Context.CreateResult("ErrorMessage", message, top2, top, null, null, null);
			return;
		}
		base.Context.CreateResult("SuccessMessage", message, top2, top, null, null, null);
		GetResult getResult = ControllerService.Instance.Get(base.AppLog);
		if (!getResult.Success)
		{
			base.Context.ServiceLog(LogEntryType.Error, "There was an error executing GET: {0}", getResult.ToString());
			base.Context.CreateResult("ErrorMessage", $"GET returned error status {getResult.ToString()}", top2, top, null, null, null);
			return;
		}
		if (top3)
		{
			ControllerService.Instance.CenterItem(CenterDiskMethod.VendDoorAndBack);
		}
		base.Context.CreateInfoResult("SuccessMessage", "GET SUCCESS");
	}

	internal GetAndCenterJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
