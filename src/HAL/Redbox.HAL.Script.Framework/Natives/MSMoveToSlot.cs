using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "tester-move-to-slot")]
internal sealed class MSMoveToSlot : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int num = base.Context.PopTop<int>();
		int num2 = base.Context.PopTop<int>();
		int num3 = 0;
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		if ("TIMEOUT" == service.GetBoardVersion(ControlBoards.Aux))
		{
			base.Context.CreateInfoResult("BoardVersionTimeout", "Move: AUX board not responsive.");
			num3++;
		}
		if ("TIMEOUT" == service.GetBoardVersion(ControlBoards.Picker))
		{
			base.Context.CreateInfoResult("BoardVersionTimeout", "Move: PCB not responsive.");
			num3++;
		}
		IMotionControlService service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
		IMotionControlLimitResponse motionControlLimitResponse = service2.ReadLimits();
		if (!motionControlLimitResponse.ReadOk)
		{
			base.Context.CreateInfoResult(ErrorCodes.ArcusNotResponsive.ToString().ToUpper(), "Unable to read limits.");
			num3++;
		}
		else
		{
			IMotionControlLimit[] limits = motionControlLimitResponse.Limits;
			foreach (IMotionControlLimit motionControlLimit in limits)
			{
				if (motionControlLimit.Blocked)
				{
					base.Context.CreateInfoResult("LimitBlocked", $"MOVE: {motionControlLimit.Limit.ToString().ToUpper()} LIMIT BLOCKED");
					num3++;
				}
			}
		}
		if (num3 > 0)
		{
			AddError("Job failure");
			return;
		}
		IFormattedLogFactoryService service3 = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
		ErrorCodes errorCodes = service2.MoveTo(num, num2, MoveMode.None, service3.NilLog);
		base.Context.CreateInfoResult("MoveResult", $"MOVE DECK={num} SLOT={num2} {errorCodes.ToString().ToUpper()}");
	}

	internal MSMoveToSlot(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
