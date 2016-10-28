using System;
using System.Runtime.InteropServices;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct CameraTranslator
{
	internal CameraGeneration From(IDeviceDescriptor descriptor)
	{
		if (descriptor == null)
		{
			return CameraGeneration.Unknown;
		}
		if (descriptor.Vendor.Equals("1871") && (descriptor.Product.Equals("0d01", StringComparison.CurrentCultureIgnoreCase) || descriptor.Product.Equals("0f01", StringComparison.CurrentCultureIgnoreCase)))
		{
			return CameraGeneration.Fourth;
		}
		if (descriptor.Vendor.Equals("0c45", StringComparison.CurrentCultureIgnoreCase) && descriptor.Product.Equals("627b", StringComparison.CurrentCultureIgnoreCase))
		{
			return CameraGeneration.Third;
		}
		return CameraGeneration.Unknown;
	}
}
