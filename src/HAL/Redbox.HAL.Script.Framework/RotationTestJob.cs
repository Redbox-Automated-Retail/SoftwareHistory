using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "tester-rotation-test")]
internal class RotationTestJob : NativeJobAdapter
{
	private volatile bool KeepRunning = true;

	protected override void ExecuteInner()
	{
		base.Context.RegisterHandler(delegate
		{
			KeepRunning = false;
		});
		int num = base.Context.PopTop<int>();
		IFormattedLogFactoryService service = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
		IMotionControlService service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
		for (int num2 = 0; num2 < num; num2++)
		{
			if (!KeepRunning)
			{
				break;
			}
			if (service2.MoveTo(1, 1, MoveMode.None, service.NilLog) != ErrorCodes.Success)
			{
				AddError("MOVE FAILED");
				break;
			}
			Thread.Sleep(500);
			if (service2.MoveTo(1, 90, MoveMode.None, service.NilLog) != ErrorCodes.Success)
			{
				AddError("MOVE FAILED");
				break;
			}
			Thread.Sleep(500);
		}
	}

	internal RotationTestJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
