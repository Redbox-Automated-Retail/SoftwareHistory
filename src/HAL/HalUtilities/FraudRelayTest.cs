using Redbox.HAL.Client;

namespace HALUtilities;

internal sealed class FraudRelayTest : JobExecutor
{
	private readonly int Iterations;

	private readonly int Pause;

	private readonly int InitialPause;

	private readonly bool ReadSensors;

	protected override string JobName => "fraud-test-sell-relay";

	protected override void SetupJob(HardwareJob job)
	{
		job.Push(Pause);
		job.Push(Iterations);
		job.Push(InitialPause);
		job.Push(ReadSensors);
	}

	internal FraudRelayTest(HardwareService service, int iterations, int pause, int initialpause, bool read)
		: base(service)
	{
		Iterations = iterations;
		Pause = pause;
		InitialPause = initialpause;
		ReadSensors = read;
	}
}
