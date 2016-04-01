using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class DefaultGetObserver : IGetObserver
{
	internal static DefaultGetObserver Instance => Singleton<DefaultGetObserver>.Instance;

	public void OnStuck(IGetResult result)
	{
	}

	public bool OnEmpty(IGetResult result)
	{
		return true;
	}

	private DefaultGetObserver()
	{
	}
}
