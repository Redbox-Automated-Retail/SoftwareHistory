using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class ServiceLogHelper
{
	internal static ServiceLogHelper Instance => Singleton<ServiceLogHelper>.Instance;

	internal void WriteFormattedMessage(LogEntryType type, string fmt, params object[] args)
	{
		fmt = string.Format(fmt, args);
		WriteMessageInner(fmt, type);
	}

	internal void WriteInfoMessage(string msg)
	{
		WriteMessageInner(msg, LogEntryType.Info);
	}

	internal void WriteInfoMessage(string fmt, params object[] args)
	{
		fmt = string.Format(fmt, args);
		WriteMessageInner(fmt, LogEntryType.Info);
	}

	internal void WriteErrorMessage(string msg)
	{
		WriteMessageInner(msg, LogEntryType.Error);
	}

	internal void WriteMessage(string msg, LogEntryType type)
	{
		WriteMessageInner(msg, type);
	}

	private void WriteMessageInner(string msg, LogEntryType type)
	{
		string arg = ((ExecutionEngine.Instance.ActiveContext == null) ? "UNKNOWN" : ExecutionEngine.Instance.ActiveContext.ProgramName);
		string arg2 = ((ExecutionEngine.Instance.ActiveContext == null) ? "UNKNOWN" : ExecutionEngine.Instance.ActiveContext.ID);
		msg = $"[{arg}, {arg2}]: {msg}";
		LogHelper.Instance.Log(msg, type);
	}

	private ServiceLogHelper()
	{
	}
}
