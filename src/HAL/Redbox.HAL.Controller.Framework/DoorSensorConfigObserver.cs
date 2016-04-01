using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DoorSensorConfigObserver : IConfigurationObserver
{
	private bool IsVMZ;

	public void NotifyConfigurationChangeStart()
	{
	}

	public void NotifyConfigurationChangeEnd()
	{
		if (ControllerConfiguration.Instance.IsVMZMachine == IsVMZ)
		{
			LogHelper.Instance.Log("[DoorSensorConfigurationObserver] Configuration has not changed.");
			return;
		}
		LogHelper.Instance.Log("[DoorSensorConfigurationObserver] Configuration has changed.");
		IsVMZ = ControllerConfiguration.Instance.IsVMZMachine;
		IDoorSensorService service = ServiceLocator.Instance.GetService<IDoorSensorService>();
		if (service != null)
		{
			ServiceLocator.Instance.RemoveService(typeof(IDoorSensorService));
			service.Dispose();
		}
		Add();
	}

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[DoorSensorConfigObserver] Configuration load.");
		IsVMZ = ControllerConfiguration.Instance.IsVMZMachine;
		Add();
	}

	internal DoorSensorConfigObserver()
	{
	}

	private IDoorSensorService Add()
	{
		IDoorSensorService doorSensorService = ((!IsVMZ) ? ((IDoorSensorService)new UnconfiguredDoorSensorService()) : ((IDoorSensorService)new DoorSensorService()));
		ServiceLocator.Instance.AddService(typeof(IDoorSensorService), doorSensorService);
		if (!doorSensorService.SensorsEnabled)
		{
			LogHelper.Instance.Log("The door sensors are not configured.");
			return doorSensorService;
		}
		if (doorSensorService.SoftwareOverride)
		{
			LogHelper.Instance.Log("** WARNING **: Door sensors are configured, however a software override in place.");
			LogHelper.Instance.Log("** WARNING **: Kiosk is operating without door sensors.");
		}
		else
		{
			LogHelper.Instance.Log("Door sensors are configured.");
		}
		return doorSensorService;
	}
}
