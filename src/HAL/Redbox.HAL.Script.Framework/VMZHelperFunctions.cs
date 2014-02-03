using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

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

	private DumpBin m_dumpSlot;

	private static readonly Location ViewableLocation = new Location(3, 86);

	private static VMZHelperFunctions m_instance = new VMZHelperFunctions();

	internal static VMZHelperFunctions Instance => Singleton<VMZHelperFunctions>.Instance;

	internal ExecutionContextStatus LastCleanStatus { get; set; }

	internal static MotionResult MoveViewable()
	{
		return ControllerService.Instance.MoveTo(ViewableLocation, MoveMode.None, ExecutionEngine.Instance.ActiveContext.AppLog);
	}

	internal MerchandizeResult UnloadDisc(ExecutionResult result)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return MerchandizeResult.Success;
		}
		_ = ExecutionEngine.Instance.ActiveContext;
		Location location = VMZ.Instance.FindNextUnloadLocation();
		if (location == null)
		{
			return MerchandizeResult.UnloadsCleared;
		}
		MerchandizeResult merchandizeResult = UnloadHelper.Instance.UnloadDisk(result, location.Deck, location.Slot);
		if (merchandizeResult != MerchandizeResult.Success)
		{
			ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Info, "Unload disk returned status {0}", merchandizeResult.ToString());
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
