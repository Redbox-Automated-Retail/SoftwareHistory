using Redbox.Core;
using Redbox.KioskDataServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskDataServices.Proxy;

public class KioskVersionsService : IKioskVersionsService
{
	public static IKioskVersionsService Instance => Singleton<KioskVersionsService>.Instance;

	private KioskVersionsService()
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			service.RegisterMessageProcessorService(new AWSKioskVersionsMessageProcessorService());
		}
		else
		{
			LogHelper.Instance.Log("Error in KioskVersionsService.  Unable to find QueueService.");
		}
	}
}
