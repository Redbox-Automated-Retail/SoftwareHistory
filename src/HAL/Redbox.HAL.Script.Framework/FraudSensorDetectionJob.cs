using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "fraud-sensor-detect-test", Operand = "FRAUD-SENSOR-DETECT-TEST", HideFromList = true)]
internal sealed class FraudSensorDetectionJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "Fraud", flushOnWrite: true, null);
		IDiskValidatorService service = ServiceLocator.Instance.GetService<IDiskValidatorService>();
		int top = GetTop<int>();
		using (new DiskValidatorHelper(service, JobsWithScan.Return))
		{
			Thread.Sleep(top);
			base.ControlSystem.StartRollerIn();
			using ExecutionTimer executionTimer = new ExecutionTimer();
			while (true)
			{
				string[] array = base.ControlSystem.ReadInputs(ControlBoards.Picker);
				if (array[6] == "1" || array[7] == "1")
				{
					break;
				}
				PerfFunctions.SpinWait(10);
			}
			executionTimer.Stop();
			base.ControlSystem.StopRoller();
			applicationLog.Write($"Taggant detected at {executionTimer.ElapsedMilliseconds}ms ");
		}
	}

	internal FraudSensorDetectionJob(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}
}
