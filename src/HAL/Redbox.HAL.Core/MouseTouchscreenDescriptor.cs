using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class MouseTouchscreenDescriptor : AbstractTouchscreenDescriptor
{
	protected override DeviceClass CoreDeviceClazz => DeviceClass.Mouse;

	protected override Guid DeviceGuid => DeviceClassGuids.Mouse;

	internal MouseTouchscreenDescriptor(DeviceHelper helper, string v, string p, string f, IDriverDescriptor desc)
		: base(helper, v, p, f, desc)
	{
	}
}
