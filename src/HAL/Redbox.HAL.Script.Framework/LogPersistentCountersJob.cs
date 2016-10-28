using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "log-persistent-counters", Operand = "LOG-PERSISTENT-COUNTERS")]
internal sealed class LogPersistentCountersJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		string path = base.Context.PopTop<string>();
		IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		using StreamWriter streamWriter = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read));
		streamWriter.WriteLine("Counter sample on {0} {1}", DateTime.Now.ToShortTimeString(), DateTime.Now.ToShortDateString());
		service.LogTimeouts(streamWriter);
		service.LogCounter("EmptyOrStuckCount", streamWriter);
		streamWriter.WriteLine();
	}

	internal LogPersistentCountersJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
