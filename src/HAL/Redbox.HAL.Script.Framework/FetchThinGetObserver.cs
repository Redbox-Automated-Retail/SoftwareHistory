using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal sealed class FetchThinGetObserver : IGetObserver
{
	public void OnStuck(IGetResult result)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		service.UpdateEmptyStuck(result.Location);
	}

	public bool OnEmpty(IGetResult result)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		result.Update(ErrorCodes.ItemStuck);
		service.UpdateEmptyStuck(result.Location);
		return false;
	}

	internal FetchThinGetObserver()
	{
	}
}
