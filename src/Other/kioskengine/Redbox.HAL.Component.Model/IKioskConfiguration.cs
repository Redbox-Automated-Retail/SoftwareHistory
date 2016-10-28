namespace Redbox.HAL.Component.Model;

internal interface IKioskConfiguration
{
	bool IsVmz { get; }

	int ReturnSlotBuffer { get; }
}
