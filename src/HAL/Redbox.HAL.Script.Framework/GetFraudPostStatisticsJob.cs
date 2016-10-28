using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-fraud-post-statistics", Operand = "GET-FRAUD-POST-STATISTICS")]
internal sealed class GetFraudPostStatisticsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IFormattedLogFactoryService service = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
		string path = service.CreateSubpath("Service");
		string path2 = Path.Combine(path, "FraudPostStatistics.txt");
		if (File.Exists(path2))
		{
			FileHelper.SafeDeleteFile(path2);
		}
		using StreamWriter streamWriter = new StreamWriter(path2);
		IDiskValidatorService service2 = ServiceLocator.Instance.GetService<IDiskValidatorService>();
		if (service2.IsConfigured)
		{
			IFraudPostStatistics statistics = service2.FraudSensor.GetStatistics();
			streamWriter.WriteLine("POST statistics sample {0}", statistics.LastSampleDate.ToString());
			streamWriter.WriteLine("  Failed post: {0}", statistics.Failures);
			streamWriter.WriteLine("  Successful post: {0}", statistics.Successes);
		}
		else
		{
			streamWriter.WriteLine("Fraud service not configured.");
		}
	}

	internal GetFraudPostStatisticsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
