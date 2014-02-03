using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace Skybound.VisualTips;

internal class VisualTipConverter : ExpandableObjectConverter
{
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string) || destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		VisualTip visualTip = value as VisualTip;
		if (destinationType == typeof(string))
		{
			if (!visualTip.xe5a18e5ab4fed2d8())
			{
				return "(none)";
			}
			return "(VisualTip)";
		}
		if (destinationType == typeof(InstanceDescriptor))
		{
			return new InstanceDescriptor(typeof(VisualTip).GetConstructor(Type.EmptyTypes), null, isComplete: false);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
