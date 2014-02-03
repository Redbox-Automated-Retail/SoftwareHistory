using System.ComponentModel;
using System.Drawing.Design;
using Skybound.ComponentModel;

namespace Skybound.VisualTips;

[Editor(typeof(EnumTypeEditor), typeof(UITypeEditor))]
public enum VisualTipShadow
{
	[Description("A soft shadow is displayed when it is supported by the operating system and window shadows are enabled.")]
	SystemDefault,
	[Description("No soft shadows are displayed.")]
	Disabled,
	[Description("A soft shadow is always displayed when it is supported by the operating system.")]
	Enabled
}
