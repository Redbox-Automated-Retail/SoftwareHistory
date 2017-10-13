using Redbox.HAL.Client;
using Redbox.HAL.Core;

namespace HALUtilities;

internal sealed class FraudSensorStressTest : JobExecutor
{
	private readonly int Iterations;

	private readonly int ReadPause;

	private readonly int PauseBetweenReads;

	protected override string JobName => "fraud-sensor-stress-test";

	protected override void SetupJob(HardwareJob job)
	{
		job.Push(PauseBetweenReads);
		job.Push(ReadPause);
		job.Push(Iterations);
	}

	internal static bool Run(string[] args, HardwareService service)
	{
		int num = 10;
		int num2 = 1500;
		int num3 = 300;
		foreach (string text in args)
		{
			if (text.StartsWith("--scanPause"))
			{
				num3 = CommandLineOption.GetOptionVal(text, num3);
			}
			else if (text.StartsWith("--iterations"))
			{
				num = CommandLineOption.GetOptionVal(text, num);
			}
			else if (text.StartsWith("--readPause"))
			{
				num2 = CommandLineOption.GetOptionVal(text, num2);
			}
		}
		using FraudSensorStressTest fraudSensorStressTest = new FraudSensorStressTest(service, num, num2, num3);
		fraudSensorStressTest.Run();
		return HardwareJobStatus.Completed == fraudSensorStressTest.EndStatus;
	}

	internal FraudSensorStressTest(HardwareService service, int iterations, int readPause, int pauseBetweenReads)
		: base(service)
	{
		Iterations = iterations;
		ReadPause = readPause;
		PauseBetweenReads = pauseBetweenReads;
	}
}
