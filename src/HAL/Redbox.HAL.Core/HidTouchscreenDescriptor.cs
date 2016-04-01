using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class HidTouchscreenDescriptor : AbstractTouchscreenDescriptor
{
	protected override DeviceClass CoreDeviceClazz => DeviceClass.HIDClass;

	protected override Guid DeviceGuid => DeviceClassGuids.HID;

	internal HidTouchscreenDescriptor(DeviceHelper helper, string vendor, string product, string friendly, IDriverDescriptor desc)
		: base(helper, vendor, product, friendly, desc)
	{
	}
}
