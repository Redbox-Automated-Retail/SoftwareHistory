using Redbox.HAL.Client;

namespace Redbox.HAL.MSHALTester;

internal sealed class TesterMoveToSlotExecutor : JobExecutor
{
	private readonly int Deck;

	private readonly int Slot;

	protected override string JobName => "tester-move-to-slot";

	protected override void SetupJob(HardwareJob job)
	{
		job.Push(Slot);
		job.Push(Deck);
	}

	internal TesterMoveToSlotExecutor(HardwareService service, int deck, int slot)
		: base(service)
	{
		Deck = deck;
		Slot = slot;
	}
}
