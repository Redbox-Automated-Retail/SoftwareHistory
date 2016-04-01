using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal class DumpbinConfigurationObserver : IConfigurationObserver
{
	private bool IsVMZ;

	public void NotifyConfigurationChangeStart()
	{
	}

	public void NotifyConfigurationChangeEnd()
	{
		if (ControllerConfiguration.Instance.IsVMZMachine != IsVMZ)
		{
			LogHelper.Instance.Log("[DumpbinConfigurationObserver] Configuration has changed.");
			IsVMZ = ControllerConfiguration.Instance.IsVMZMachine;
			Change();
		}
	}

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[DumpbinConfigObserver] Configuration load.");
		IsVMZ = ControllerConfiguration.Instance.IsVMZMachine;
		Change();
	}

	internal DumpbinConfigurationObserver()
	{
	}

	private void Change()
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		IDumpbinService service2 = ServiceLocator.Instance.GetService<IDumpbinService>();
		if (service2 != null)
		{
			ServiceLocator.Instance.RemoveService(typeof(IDumpbinService));
			service2.Dispose();
		}
		service2 = ((!IsVMZ) ? ((IDumpbinService)new NilDumpbinService(service)) : ((IDumpbinService)new DumpbinService(service)));
		ServiceLocator.Instance.AddService(typeof(IDumpbinService), service2);
		service2.Initialize();
	}
}
