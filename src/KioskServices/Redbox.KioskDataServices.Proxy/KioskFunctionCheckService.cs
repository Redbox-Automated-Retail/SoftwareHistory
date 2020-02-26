using Redbox.Core;
using Redbox.KioskDataServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskDataServices.Proxy;

public class KioskFunctionCheckService : IKioskFunctionCheckService
{
	public static IKioskFunctionCheckService Instance => Singleton<KioskFunctionCheckService>.Instance;

	private KioskFunctionCheckService()
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			service.RegisterMessageProcessorService(new AWSKioskFunctionCheckMessageProcessorService());
		}
		else
		{
			LogHelper.Instance.Log("Error in KioskFunctionCheckService.  Unable to find QueueService.");
		}
	}
}
