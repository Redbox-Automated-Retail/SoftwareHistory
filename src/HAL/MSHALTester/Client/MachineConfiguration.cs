namespace Redbox.HAL.Client;

public sealed class MachineConfiguration : JobExecutor
{
	protected override string JobName => "kiosk-configuration-job";

	public MachineConfiguration(HardwareService service)
		: base(service)
	{
	}
}
