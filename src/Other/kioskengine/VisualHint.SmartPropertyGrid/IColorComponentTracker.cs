using System.Drawing;

namespace VisualHint.SmartPropertyGrid;

public interface IColorComponentTracker
{
	void Draw(Graphics graphics, int value, Point pt1, Point pt2, Rectangle clientRectangle);
}
