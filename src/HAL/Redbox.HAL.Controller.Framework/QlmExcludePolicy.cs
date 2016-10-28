using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class QlmExcludePolicy : IExcludeEmptySearchLocationObserver
{
	public bool ShouldExclude(ILocation location)
	{
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		IDeck deck = service.GetFrom(location);
		if (deck.IsSparse && deck.Number == 7)
		{
			return deck.IsSlotSellThru(location.Slot);
		}
		return false;
	}

	internal QlmExcludePolicy()
	{
	}
}
