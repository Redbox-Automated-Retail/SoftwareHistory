namespace Redbox.HAL.Service.Framework;

public static class Constants
{
	public static class SymbolNames
	{
		public const string Force = "force";

		public const string Bottom = "bottom";

		public const string ResumeNow = "resume-now";

		public const string NoDuplicates = "no-duplicates";
	}

	public static class ParameterNames
	{
		public const string End = "end:";

		public const string Name = "name:";

		public const string Path = "path:";

		public const string Data = "data:";

		public const string Line = "line:";

		public const string Label = "label:";

		public const string Value = "value:";

		public const string Script = "script:";

		public const string Enabled = "enabled:";

		public const string Priority = "priority:";

		public const string Debugging = "debugging:";

		public const string StartTime = "startTime:";

		public const string Statement = "statement:";

		public const string RequiresClientConnection = "requires-client-connection:";
	}

	public static class ConfigurationCommand
	{
		public const string GetForm = "get";

		public const string SetForm = "set";

		public const string LoadForm = "load";

		public const string CameraConfiguration = "camera";

		public const string GetInventoryStateForm = "get-inventory-state";

		public const string SetInventoryStateForm = "set-inventory-state";

		public const string ControllerConfiguration = "controller";
	}

	public static class DebugCommand
	{
		public const string SetForm = "set";

		public const string TraceForm = "trace";

		public const string BreakForm = "break";

		public const string ContinueForm = "continue";

		public const string StepOverForm = "step-over";

		public const string StepIntoForm = "step-into";

		public const string AddBreakpointForm = "add-breakpoint";

		public const string GetBreakpointsForm = "get-breakpoints";

		public const string RemoveBreakpointForm = "remove-breakpoint";

		public const string ClearBreakpointsForm = "clear-breakpoints";
	}

	public static class JobCommand
	{
		public const string GetForm = "get";

		public const string ListForm = "list";

		public const string PendForm = "pend";

		public const string TrashForm = "trash";

		public const string ResumeForm = "resume";

		public const string SuspendForm = "suspend";

		public const string ConnectForm = "connect";

		public const string SetLabelForm = "set-label";

		public const string ScheduleForm = "schedule";

		public const string ResumeAllForm = "resume-all";

		public const string TerminateForm = "terminate";

		public const string GetErrorsForm = "get-errors";

		public const string DisconnectForm = "disconnect";

		public const string SuspendAllForm = "suspend-all";

		public const string SetPriorityForm = "set-priority";

		public const string GetMessagesForm = "get-messages";

		public const string SetStartTimeForm = "set-start-time";

		public const string CollectGarbageForm = "collect-garbage";

		public const string SchedulerStatusForm = "scheduler-status";

		public const string ExecuteImmediateForm = "execute-immediate";

		public const string WaitForCompletionForm = "wait-for-completion";

		public const string ExecuteImmediateBase64Form = "execute-immediate-base64";
	}

	public static class ProgramCommand
	{
		public const string GetForm = "get";

		public const string SetForm = "set";

		public const string ListForm = "list";

		public const string RemoveForm = "remove";

		public const string CompileForm = "compile";

		public const string PropertiesForm = "properties";
	}

	public static class StackCommand
	{
		public const string PopForm = "pop";

		public const string ShowForm = "show";

		public const string PushForm = "push";

		public const string ClearForm = "clear";

		public const string RemoveResultsForm = "remove-results";
	}

	public static class SymbolCommand
	{
		public const string ShowForm = "show";

		public const string ClearForm = "clear";
	}

	public static class VideoCommand
	{
		public const string GetForm = "get";

		public const string ListForm = "list";

		public const string DeleteForm = "delete";
	}
}
