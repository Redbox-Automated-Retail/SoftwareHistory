namespace Redbox.HAL.Client;

public class RandomSyncExecutor : JobExecutor
{
	private readonly int VendTime;

	protected override string JobName => "random-sync";

	public RandomSyncExecutor(HardwareService service, int vendTime)
		: base(service)
	{
		VendTime = vendTime;
	}

	protected override void SetupJob(HardwareJob job)
	{
		job.Push(VendTime);
	}
}
