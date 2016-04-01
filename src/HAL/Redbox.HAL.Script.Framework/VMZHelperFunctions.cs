using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class VMZHelperFunctions
{
	internal static VMZHelperFunctions Instance => Singleton<VMZHelperFunctions>.Instance;

	internal static ErrorCodes MoveViewable(ExecutionContext context)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		ILocation location = service.Get(3, 86);
		IMotionControlService service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
		return service2.MoveTo(location, MoveMode.None, context.AppLog);
	}

	internal MerchandizeResult UnloadDisc(ExecutionResult result, ExecutionContext context)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return MerchandizeResult.Success;
		}
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		ILocation loc = null;
		IDecksService service2 = ServiceLocator.Instance.GetService<IDecksService>();
		service2.ForAllDecksDo(delegate(IDeck deck)
		{
			for (int i = 1; i <= deck.NumberOfSlots; i++)
			{
				ILocation location = service.Get(deck.Number, i);
				if (location.Flags == MerchFlags.Unload)
				{
					loc = location;
					return false;
				}
			}
			return true;
		});
		if (loc == null)
		{
			return MerchandizeResult.UnloadsCleared;
		}
		MerchandizeResult merchandizeResult = new UnloadHelper(context, result).UnloadDisk(loc.Deck, loc.Slot);
		if (merchandizeResult != MerchandizeResult.Success)
		{
			context.ServiceLog("Unload disk returned status {0}", merchandizeResult.ToString());
		}
		return merchandizeResult;
	}

	private VMZHelperFunctions()
	{
	}
}
