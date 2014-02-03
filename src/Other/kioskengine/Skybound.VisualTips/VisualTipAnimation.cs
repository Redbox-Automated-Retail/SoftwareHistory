using System.ComponentModel;
using System.Drawing.Design;
using Skybound.ComponentModel;

namespace Skybound.VisualTips;

[Editor(typeof(EnumTypeEditor), typeof(UITypeEditor))]
public enum VisualTipAnimation
{
	[Description("Tips are animated when it is supported by the operating system and window animation is enabled.")]
	SystemDefault,
	[Description("Tips are not animated.")]
	Disabled,
	[Description("Tips are always animated if it is supported by the operating system.")]
	Enabled
}
