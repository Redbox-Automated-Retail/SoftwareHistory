using System.ComponentModel;
using System.Drawing.Design;
using Skybound.ComponentModel;

namespace Skybound.VisualTips.Rendering;

[Editor(typeof(EnumTypeEditor), typeof(UITypeEditor))]
public enum VisualTipOfficeBackgroundEffect
{
	[Description("The background of a tip is drawn using a simple gradient.")]
	Gradient,
	[Description("The background of a tip is drawn using a \"glass\" effect.")]
	Glass
}
