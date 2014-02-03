using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class PropertyMultilineEditLook : PropertyLook
{
	public override void OnDraw(Graphics graphics, Rectangle valueRect, Color textColor, PropertyEnumerator propEnum, string drawAnotherString, bool multipleTexts, PropertyValue.DrawValueIn drawValueIn)
	{
		if (drawAnotherString == null || drawAnotherString.Length != 0)
		{
			base.Value.DrawIcon(graphics, valueRect, drawValueIn);
			Rectangle displayStringRect = GetDisplayStringRect(graphics, valueRect, Point.Empty);
			displayStringRect.Y += 2;
			displayStringRect.Height = valueRect.Bottom - 2 - displayStringRect.Y;
			displayStringRect = base.Value.Grid.TransformRectIfRTL(displayStringRect);
			Win32Calls.DrawText(graphics, DisplayString, ref displayStringRect, base.Value.Font, textColor, 0x2810 | ((base.Value.Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
		}
	}

	public override Rectangle GetDisplayStringRect(Graphics graphics, Rectangle valueRect, Point point)
	{
		Rectangle rectangle = base.Value.DrawIcon(null, valueRect, PropertyValue.DrawValueIn.None);
		if (rectangle.Width > 0)
		{
			rectangle.Width += base.Value.Grid.GlobalTextMargin;
		}
		Rectangle rect = valueRect;
		rect.Inflate(-base.Value.Grid.GlobalTextMargin, 0);
		rect.X += rectangle.Width;
		rect.Width -= rectangle.Width;
		Win32Calls.DrawText(graphics, DisplayString, ref rect, base.Value.Font, Color.Black, 11280);
		return rect;
	}

	public override Size GetTooltipStringSize(Graphics graphics, Rectangle valueRect, Point point, int line)
	{
		Rectangle displayStringRect = GetDisplayStringRect(graphics, valueRect, point);
		return new Size(displayStringRect.Width, displayStringRect.Height - 4);
	}
}
