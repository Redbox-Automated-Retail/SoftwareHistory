namespace Redbox.HAL.Component.Model;

public interface IDeviceDescriptor
{
	string Vendor { get; }

	string Product { get; }

	string Friendlyname { get; }

	DeviceClass DeviceClazz { get; }

	bool MatchDriver();

	bool Locate();

	DeviceStatus GetStatus();

	bool ResetDriver();
}
