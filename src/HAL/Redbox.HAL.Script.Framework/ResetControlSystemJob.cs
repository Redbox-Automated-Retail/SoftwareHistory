using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "reset-controlsystem-job", Operand = "RESET-CONTROLSYSTEM-JOB")]
internal class ResetControlSystemJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ControlSystem.Instance.ClosePort(500);
		CoreResponse coreResponse = ControlSystem.Instance.Initialize();
		string message = string.Format("RESET returned {0}", coreResponse.CommunicationError ? coreResponse.CommErrorDiagnostic : "SUCCESS");
		base.Context.CreateInfoResult("ResetStatus", message);
		if (coreResponse.CommunicationError)
		{
			AddError("RESET failure.");
		}
	}

	internal ResetControlSystemJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
