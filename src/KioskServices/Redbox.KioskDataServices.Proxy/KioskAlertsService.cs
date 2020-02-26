using Redbox.Core;
using Redbox.KioskDataServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskDataServices.Proxy;

public class KioskAlertsService : IKioskAlertsService
{
	public static IKioskAlertsService Instance => Singleton<KioskAlertsService>.Instance;

	private KioskAlertsService()
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			service.RegisterMessageProcessorService(new AWSKioskAlertMessageProcessorService());
		}
		else
		{
			LogHelper.Instance.Log("Error in KioskAlertsService.  Unable to find QueueService.");
		}
	}
}
