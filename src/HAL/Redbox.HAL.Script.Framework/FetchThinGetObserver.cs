using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal sealed class FetchThinGetObserver : IGetObserver
{
	public void OnStuck(GetResult result, Deck deck, int slot)
	{
		deck.UpdateEmptyStuck(slot);
	}

	public bool OnEmpty(GetResult result, Deck deck, int slot)
	{
		result.UpdateCode(GetResultCode.ItemStuck);
		deck.UpdateEmptyStuck(slot);
		return false;
	}

	internal FetchThinGetObserver()
	{
	}
}
