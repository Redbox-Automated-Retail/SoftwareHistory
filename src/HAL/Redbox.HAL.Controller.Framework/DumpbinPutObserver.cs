using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DumpbinPutObserver : IPutObserver
{
	internal static DumpbinPutObserver Instance => Singleton<DumpbinPutObserver>.Instance;

	public void OnSuccessfulPut(IPutResult res, IFormattedLog log)
	{
		log.WriteFormatted($"PUT DUMPBIN ID={res.StoredMatrix}");
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		service.AddBinItem(res.StoredMatrix);
		IMotionControlService service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
		IDumpbinService service3 = ServiceLocator.Instance.GetService<IDumpbinService>();
		service2.MoveTo(service3.RotationLocation, MoveMode.None, log);
		LogHelper.Instance.Log("An item was just placed to the bin: current count = {0}", service3.CurrentCount());
		service.Reset(service3.PutLocation);
	}

	public void OnFailedPut(IPutResult res, IFormattedLog log)
	{
		log.WriteFormatted($"PUT DUMPBIN ID={res.StoredMatrix} returned error status {res.ToString().ToUpper()}");
	}

	private DumpbinPutObserver()
	{
	}
}
