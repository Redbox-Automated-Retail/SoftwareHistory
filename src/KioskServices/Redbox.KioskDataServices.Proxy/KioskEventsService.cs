using Redbox.Core;
using Redbox.KioskDataServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskDataServices.Proxy;

public class KioskEventsService : IKioskEventsService
{
	public static IKioskEventsService Instance => Singleton<KioskEventsService>.Instance;

	private KioskEventsService()
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			service.RegisterMessageProcessorService(new AWSKioskEventsMessageProcessorService());
		}
		else
		{
			LogHelper.Instance.Log("Error in KioskEventsService.  Unable to find QueueService.");
		}
	}
}
