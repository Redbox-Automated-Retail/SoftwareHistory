using Redbox.HAL.Client;

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

	internal FraudSensorStressTest(HardwareService service, int iterations, int readPause, int pauseBetweenReads)
		: base(service)
	{
		Iterations = iterations;
		ReadPause = readPause;
		PauseBetweenReads = pauseBetweenReads;
	}
}
