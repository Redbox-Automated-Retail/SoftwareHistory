using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-and-center", Operand = "GET-AND-CENTER")]
internal sealed class GetAndCenterJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		base.Context.ServiceLog("Start get and center.");
		int num = base.Context.PopTop<int>();
		int num2 = base.Context.PopTop<int>();
		bool flag = base.Context.PopTop<bool>();
		ErrorCodes errorCodes = base.MotionService.MoveTo(num2, num, MoveMode.Get, base.AppLog);
		string message = $"MOVE DECK={num2} SLOT={num} MODE=GET {errorCodes.ToString().ToUpper()}";
		if (errorCodes != ErrorCodes.Success)
		{
			base.Context.ServiceLog(LogEntryType.Error, "There was an error executing MOVE: {0}", errorCodes.ToString());
			base.Context.CreateResult("ErrorMessage", message, num2, num, null, null, null);
			return;
		}
		base.Context.CreateResult("SuccessMessage", message, num2, num, null, null, null);
		IGetResult getResult = ServiceLocator.Instance.GetService<IControllerService>().Get(base.AppLog);
		if (!getResult.Success)
		{
			base.Context.ServiceLog(LogEntryType.Error, "There was an error executing GET: {0}", getResult.ToString());
			base.Context.CreateResult("ErrorMessage", $"GET returned error status {getResult.ToString()}", num2, num, null, null, null);
			return;
		}
		if (flag)
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
