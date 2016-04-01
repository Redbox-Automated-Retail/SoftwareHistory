using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

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
		ErrorCodes errorCodes = base.MotionService.MoveTo(top2, top, MoveMode.Get, base.AppLog);
		string message = $"MOVE DECK={top2} SLOT={top} MODE=GET {errorCodes.ToString().ToUpper()}";
		if (errorCodes != ErrorCodes.Success)
		{
			base.Context.ServiceLog(LogEntryType.Error, "There was an error executing MOVE: {0}", errorCodes.ToString());
			base.Context.CreateResult("ErrorMessage", message, top2, top, null, null, null);
			return;
		}
		base.Context.CreateResult("SuccessMessage", message, top2, top, null, null, null);
		IGetResult getResult = ControllerService.Instance.Get(base.AppLog);
		if (!getResult.Success)
		{
			base.Context.ServiceLog(LogEntryType.Error, "There was an error executing GET: {0}", getResult.ToString());
			base.Context.CreateResult("ErrorMessage", $"GET returned error status {getResult.ToString()}", top2, top, null, null, null);
			return;
		}
		if (top3)
		{
			base.ControlSystem.Center(CenterDiskMethod.VendDoorAndBack);
		}
		base.Context.CreateInfoResult("SuccessMessage", "GET SUCCESS");
	}

	internal GetAndCenterJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
