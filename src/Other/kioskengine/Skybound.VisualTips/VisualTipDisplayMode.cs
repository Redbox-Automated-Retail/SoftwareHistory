using System.ComponentModel;
using System.Drawing.Design;
using Skybound.ComponentModel;

namespace Skybound.VisualTips;

[Editor(typeof(EnumTypeEditor), typeof(UITypeEditor))]
public enum VisualTipDisplayMode
{
	[Description("Tips are displayed when the mouse hovers over the tool area.")]
	MouseHover,
	[Description("Tips are displayed when F1 is pressed or the dialog box help button is used.")]
	HelpRequested,
	[Description("Tips must be displayed manually by calling VisualTipProvider.ShowTip.")]
	Manual
}
