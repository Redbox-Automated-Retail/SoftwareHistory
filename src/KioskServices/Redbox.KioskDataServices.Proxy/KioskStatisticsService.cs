using Redbox.Core;
using Redbox.KioskDataServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskDataServices.Proxy;

public class KioskStatisticsService : IKioskStatisticsService
{
	public static IKioskStatisticsService Instance => Singleton<KioskStatisticsService>.Instance;

	private KioskStatisticsService()
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			service.RegisterMessageProcessorService(new AWSKioskStatisticsMessageProcessorService());
		}
		else
		{
			LogHelper.Instance.Log("Error in KioskStatisticsService.  Unable to find QueueService.");
		}
	}
}
