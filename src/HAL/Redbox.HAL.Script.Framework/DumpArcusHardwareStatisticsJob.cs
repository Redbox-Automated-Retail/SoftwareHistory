using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "dump-arcus-hardware-statistics", Operand = "DUMP-ARCUS-HARDWARE-STATISTICS")]
internal sealed class DumpArcusHardwareStatisticsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IFormattedLogFactoryService service = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
		string path = service.CreateSubpath("Service");
		string path2 = Path.Combine(path, "ArcusCorrectionStats.txt");
		if (File.Exists(path2))
		{
			FileHelper.CreateBackup(path2, BackupAction.Move);
		}
		IPersistentCounterService service2 = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		using StreamWriter log = new StreamWriter(path2);
		service2.LogStats(HardwareCorrectionStatistic.Arcus, log);
	}

	internal DumpArcusHardwareStatisticsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
