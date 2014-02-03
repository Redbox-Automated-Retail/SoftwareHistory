using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal struct CleanResult
{
	internal bool SourceDiskStuck { get; private set; }

	internal bool VisitedAllSlots { get; set; }

	internal int Transfers { get; private set; }

	internal bool NoEmptySlots { get; set; }

	internal bool NoThinsInZone { get; set; }

	internal bool HardwareError { get; private set; }

	internal int TransferFailures { get; private set; }

	internal void Accumulate(TransferResult result)
	{
		if (result.Transferred)
		{
			Transfers++;
			return;
		}
		TransferFailures++;
		HardwareError = result.HardwareError;
		SourceDiskStuck = result.SourceStuck || result.SourceEmpty;
	}
}
