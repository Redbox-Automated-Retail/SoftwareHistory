using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class DefaultGetObserver : IGetObserver
{
	internal static DefaultGetObserver Instance => Singleton<DefaultGetObserver>.Instance;

	public void OnStuck(GetResult result, Deck deck, int slot)
	{
	}

	public bool OnEmpty(GetResult result, Deck deck, int slot)
	{
		return true;
	}

	private DefaultGetObserver()
	{
	}
}
