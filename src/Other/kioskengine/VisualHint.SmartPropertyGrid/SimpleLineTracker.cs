using System.Drawing;

namespace VisualHint.SmartPropertyGrid;

internal class SimpleLineTracker : IColorComponentTracker
{
	public void Draw(Graphics graphics, int value, Point pt1, Point pt2, Rectangle clientRectangle)
	{
		using Pen pen = new Pen(Color.Black, 2f);
		graphics.DrawLine(pen, pt1, pt2);
	}
}
