using System.ServiceProcess;
using System.Threading;

namespace Redbox.HAL.Service.Win32;

public static class Program
{
	public static void Main()
	{
		Thread.CurrentThread.Name = "HAL Service";
		ServiceBase[] services = new ServiceBase[1]
		{
			new HALService()
		};
		ServiceBase.Run(services);
	}
}
