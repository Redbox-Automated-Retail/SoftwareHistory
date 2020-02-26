using Redbox.Core;
using Redbox.KioskDataServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskDataServices.Proxy;

public class AuditEventsService : IAuditEventsService
{
	public static IAuditEventsService Instance => Singleton<AuditEventsService>.Instance;

	private AuditEventsService()
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			service.RegisterMessageProcessorService(new AWSAuditEventMessageProcessorService());
		}
		else
		{
			LogHelper.Instance.Log("Error in AuditEventsService.  Unable to find QueueService.");
		}
	}
}
