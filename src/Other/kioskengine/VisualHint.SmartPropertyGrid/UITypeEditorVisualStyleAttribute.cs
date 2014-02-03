using System;
using System.Drawing.Design;

namespace VisualHint.SmartPropertyGrid;

public class UITypeEditorVisualStyleAttribute : Attribute
{
	private UITypeEditorEditStyle _style;

	public UITypeEditorEditStyle VisualStyle => _style;

	public UITypeEditorVisualStyleAttribute(UITypeEditorEditStyle style)
	{
		_style = style;
	}
}
