using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public interface IColorComponentDrawingImplementer
{
	void DrawContent(Control sender, Graphics graphics, Rectangle rect);
}
