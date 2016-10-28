using System;
using System.ComponentModel;
using System.Globalization;

namespace WeifenLuo.WinFormsUI.Docking;

public class DockPanelSkinConverter : ExpandableObjectConverter
{
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if ((object)destinationType == typeof(DockPanelSkin))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if ((object)destinationType == typeof(string) && value is DockPanelSkin)
		{
			return "DockPanelSkin";
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
