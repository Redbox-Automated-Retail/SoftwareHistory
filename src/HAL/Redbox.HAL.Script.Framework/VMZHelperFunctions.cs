using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Script.Framework;

internal class VMZHelperFunctions
{
	internal const string ThinVMZPutFailCounter = "THINPUTFAIL";

	internal const string ThinPutSlotInUseCounter = "THINSIU";

	internal const string UnloadSlotInUseCounter = "UNLOADSIUFAIL";

	internal const string UnloadPutFailureCounter = "UNLOADPUTFAIL";

	internal const string GetFailureFromVMZCounter = "UNLOADGETFAIL";

	internal const string UnknownFromUnloadCounter = "UNKNOWNUNLOADCOUNT";

	internal const string InventoryFullVMZCounter = "INVFULLUNLOAD";

	internal const string NoVMZSlots = "NOVMZSLOTS";

	private readonly DumpBin m_dumpSlot;

	private static readonly Location ViewableLocation = new Location(3, 86);

	private static VMZHelperFunctions m_instance = new VMZHelperFunctions();

	internal static VMZHelperFunctions Instance => Singleton<VMZHelperFunctions>.Instance;

	internal ExecutionContextStatus LastCleanStatus { get; set; }

	internal static MotionResult MoveViewable(ExecutionContext context)
	{
		return ControllerService.Instance.MoveTo(ViewableLocation, MoveMode.None, context.AppLog);
	}

	internal MerchandizeResult UnloadDisc(ExecutionResult result, ExecutionContext context)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return MerchandizeResult.Success;
		}
		Location loc = null;
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			loc = deck.FindMatching(MerchFlags.Unload);
			return null == loc;
		});
		if (loc == null)
		{
			return MerchandizeResult.UnloadsCleared;
		}
		MerchandizeResult merchandizeResult = new UnloadHelper(context).UnloadDisk(result, loc.Deck, loc.Slot);
		if (merchandizeResult != MerchandizeResult.Success)
		{
			context.ServiceLog("Unload disk returned status {0}", merchandizeResult.ToString());
		}
		return merchandizeResult;
	}

	internal void ProcessFailedPut(Location loc, bool markUnknown)
	{
		if (loc == m_dumpSlot.PutLocation)
		{
			m_dumpSlot.IncrementFailureCount();
		}
		else
		{
			CounterManager.Instance.Increment("THINSIU");
		}
	}

	private VMZHelperFunctions()
	{
		m_dumpSlot = SpecialLocationLocatorService.Instance.FindDumpBin();
	}
}
