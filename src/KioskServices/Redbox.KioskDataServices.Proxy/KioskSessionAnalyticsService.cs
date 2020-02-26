using Redbox.Core;
using Redbox.KioskDataServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskDataServices.Proxy;

public class KioskSessionAnalyticsService : IKioskSessionAnalyticsService
{
	public static KioskSessionAnalyticsService Instance => Singleton<KioskSessionAnalyticsService>.Instance;

	private KioskSessionAnalyticsService()
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			service.RegisterMessageProcessorService(new AddKioskSessionMessageProcessorService());
			service.RegisterMessageProcessorService(new AddKioskSessionJsonMessageProcessorService());
		}
		else
		{
			LogHelper.Instance.Log("Error in KioskSessionAnalyticsServices.RegisterWithQueueService.  Unable to find QueueService.");
		}
	}
}
