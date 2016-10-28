namespace Redbox.HAL.Component.Model;

internal interface IDeviceSetupClassFactory
{
	IDeviceSetupClass Get(DeviceClass clazz);
}
