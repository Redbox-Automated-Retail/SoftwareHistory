using Redbox.HAL.Client;

namespace Redbox.HAL.MSHALTester;

internal sealed class RotationTestExecutor : JobExecutor
{
	private readonly int Iterations;

	protected override string JobName => "tester-rotation-test";

	protected override void SetupJob(HardwareJob job)
	{
		job.Push(Iterations);
	}

	internal RotationTestExecutor(HardwareService service, int iterations)
		: base(service)
	{
		Iterations = iterations;
	}
}
