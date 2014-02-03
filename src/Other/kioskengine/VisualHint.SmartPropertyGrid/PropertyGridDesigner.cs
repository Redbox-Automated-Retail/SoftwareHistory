using System.Windows.Forms.Design;

namespace VisualHint.SmartPropertyGrid;

public class PropertyGridDesigner : ControlDesigner
{
	public override SelectionRules SelectionRules => SelectionRules.AllSizeable | SelectionRules.Moveable;
}
