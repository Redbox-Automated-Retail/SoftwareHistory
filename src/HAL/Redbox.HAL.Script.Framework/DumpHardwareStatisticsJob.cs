using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "dump-hardware-statistics")]
internal sealed class DumpHardwareStatisticsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		HardwareCorrectionStatistic hardwareCorrectionStatistic = Enum<HardwareCorrectionStatistic>.ParseIgnoringCase(base.Context.PopTop<string>(), HardwareCorrectionStatistic.None);
		if (hardwareCorrectionStatistic == HardwareCorrectionStatistic.None)
		{
			return;
		}
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		string path = Path.Combine(ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().CreateSubpath("Service"), $"{hardwareCorrectionStatistic.ToString()}CorrectionStats.txt");
		service.SafeDelete(path);
		IPersistentCounterService service2 = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		using StreamWriter streamWriter = new StreamWriter(path);
		List<IHardwareCorrectionStatistic> stats = service2.GetStats(hardwareCorrectionStatistic);
		stats.Sort((IHardwareCorrectionStatistic x, IHardwareCorrectionStatistic y) => x.CorrectionTime.CompareTo(y.CorrectionTime));
		using (new DisposeableList<IHardwareCorrectionStatistic>(stats))
		{
			streamWriter.WriteLine("{0} Kiosk:{1}", DateTime.Now, service.KioskId);
			foreach (IHardwareCorrectionStatistic item in stats)
			{
				streamWriter.WriteLine("{0},{1},{2}", item.CorrectionTime.ToString(), item.ProgramName, item.CorrectionOk ? "SUCCESS" : "FAILURE");
			}
		}
	}

	internal DumpHardwareStatisticsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
