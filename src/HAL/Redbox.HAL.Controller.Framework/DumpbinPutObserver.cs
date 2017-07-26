using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DumpbinPutObserver : IPutObserver
{
	internal static DumpbinPutObserver Instance => Singleton<DumpbinPutObserver>.Instance;

	public void OnSuccessfulPut(IPutResult res, IFormattedLog log)
	{
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
		IInventoryService service2 = ServiceLocator.Instance.GetService<IInventoryService>();
		service.AddBinItem(res.StoredMatrix);
		ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(service.RotationLocation, MoveMode.None, log);
		service2.Reset(service.PutLocation);
	}

	public void OnFailedPut(IPutResult res, IFormattedLog log)
	{
	}

	private DumpbinPutObserver()
	{
	}
}
