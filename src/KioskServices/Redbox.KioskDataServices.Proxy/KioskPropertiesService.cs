using Redbox.Core;
using Redbox.KioskDataServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskDataServices.Proxy;

public class KioskPropertiesService : IKioskPropertiesService
{
	public static IKioskPropertiesService Instance => Singleton<KioskPropertiesService>.Instance;

	private KioskPropertiesService()
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			service.RegisterMessageProcessorService(new AWSKioskPropertiesMessageProcessorService());
		}
		else
		{
			LogHelper.Instance.Log("Error in KioskPropertiesService.  Unable to find QueueService.");
		}
	}
}
