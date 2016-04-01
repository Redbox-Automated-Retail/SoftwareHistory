using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DefaultPutObserver : IPutObserver
{
	internal static DefaultPutObserver Instance => Singleton<DefaultPutObserver>.Instance;

	public void OnSuccessfulPut(IPutResult result, IFormattedLog log)
	{
	}

	public void OnFailedPut(IPutResult result, IFormattedLog log)
	{
	}

	private DefaultPutObserver()
	{
	}
}
