using Redbox.HAL.Client;

namespace HALUtilities.KioskTest;

internal sealed class KioskTestExecutor : JobExecutor
{
	private readonly TestLocation Source;

	private readonly TestLocation Destination;

	private readonly bool m_vend;

	protected override string JobName => "refurbish-kiosk-test";

	internal KioskTestExecutor(HardwareService service, TestLocation source, TestLocation dest, bool vend)
		: base(service)
	{
		Source = source;
		Destination = dest;
		m_vend = vend;
	}

	protected override void SetupJob(HardwareJob job)
	{
		job.Push(Destination.Slot);
		job.Push(Destination.Deck);
		job.Push(Source.Slot);
		job.Push(Source.Deck);
		job.Push(m_vend.ToString());
	}
}
