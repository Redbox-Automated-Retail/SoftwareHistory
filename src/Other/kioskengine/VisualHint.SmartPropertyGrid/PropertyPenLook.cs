using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class PropertyPenLook : PropertyLook
{
	public override string DisplayString
	{
		get
		{
			if (base.Value.TargetInstances.Length == 1)
			{
				return "";
			}
			return "(multiple pens)";
		}
	}

	public override void OnDraw(Graphics graphics, Rectangle valueRect, Color textColor, PropertyEnumerator propEnum, string drawAnotherString, bool multipleTexts, PropertyValue.DrawValueIn drawValueIn)
	{
		if ((drawAnotherString != null && drawAnotherString.Length == 0) || multipleTexts)
		{
			return;
		}
		Rectangle rectangle = base.Value.DrawIcon(graphics, valueRect, drawValueIn);
		if (rectangle.Width > 0)
		{
			rectangle.Width += base.Value.Grid.GlobalTextMargin;
			valueRect.X += rectangle.Width;
			valueRect.Width -= rectangle.Width;
		}
		Pen pen = null;
		if (base.Value.TargetInstances.Length == 1)
		{
			valueRect = base.Value.Grid.TransformRectIfRTL(valueRect);
			pen = (Pen)base.Value.GetValue();
			if (pen.Width > (float)(valueRect.Height - 2))
			{
				pen = (Pen)pen.Clone();
				pen.Width = valueRect.Height - 2;
			}
			int globalTextMargin = base.Value.Grid.GlobalTextMargin;
			graphics.DrawLine(pen, valueRect.Left + globalTextMargin, valueRect.Top + valueRect.Height / 2, valueRect.Right - globalTextMargin, valueRect.Top + valueRect.Height / 2);
		}
		else
		{
			valueRect.Height++;
			Rectangle displayStringRect = GetDisplayStringRect(graphics, valueRect, Point.Empty);
			displayStringRect.Width = valueRect.Right - displayStringRect.Left;
			displayStringRect = base.Value.Grid.TransformRectIfRTL(displayStringRect);
			Win32Calls.DrawText(graphics, (drawAnotherString == null) ? DisplayString : drawAnotherString, ref displayStringRect, base.Value.Font, textColor, 0x824 | (((base.Value.Grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnValues) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((base.Value.Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
		}
	}
}
