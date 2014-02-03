using System.Drawing;

namespace VisualHint.SmartPropertyGrid;

public class PropertyAlphaColorLook : PropertyLook
{
	public override void OnDraw(Graphics graphics, Rectangle valueRect, Color textColor, PropertyEnumerator propEnum, string drawAnotherString, bool multipleTexts, PropertyValue.DrawValueIn drawValueIn)
	{
		Rectangle rectangle = base.Value.DrawIcon(graphics, valueRect, drawValueIn);
		if (multipleTexts)
		{
			return;
		}
		PropertyGrid parentGrid = propEnum.Property.ParentGrid;
		int containerWidth = 0;
		switch (drawValueIn)
		{
		case PropertyValue.DrawValueIn.Grid:
			containerWidth = parentGrid.InternalGrid.ClientSize.Width;
			break;
		case PropertyValue.DrawValueIn.InPlaceCtrl:
			containerWidth = parentGrid.InPlaceControl.ClientSize.Width;
			break;
		}
		if (rectangle.Width > 0)
		{
			rectangle.Width += base.Value.Grid.GlobalTextMargin;
		}
		Rectangle rect = valueRect;
		rect.Y += 2;
		rect.Height -= 5;
		int globalTextMargin = parentGrid.GlobalTextMargin;
		rect.X += globalTextMargin + rectangle.Width;
		rect.Width -= 2 * globalTextMargin + rectangle.Width;
		Brush brush = new SolidBrush(Color.White);
		Brush brush2 = new SolidBrush(Color.Gray);
		for (int i = rect.Top + 1; i < rect.Bottom; i += 4)
		{
			int height = ((i + 4 > rect.Bottom) ? (rect.Bottom - i) : 4);
			for (int j = rect.Left + 1; j < rect.Right; j += 5)
			{
				int width = ((j + 5 > rect.Right) ? (rect.Right - j) : 5);
				Rectangle rect2 = base.Value.Grid.TransformRectIfRTL(j, i, width, height, containerWidth);
				if ((j - (rect.Left + 1)) % 10 == (((i - (rect.Top + 1)) % 8 != 0) ? 5 : 0))
				{
					graphics.FillRectangle(brush, rect2);
				}
				else
				{
					graphics.FillRectangle(brush2, rect2);
				}
			}
		}
		brush.Dispose();
		brush2.Dispose();
		rect = base.Value.Grid.TransformRectIfRTL(rect, containerWidth);
		using (Brush brush3 = new SolidBrush((Color)base.Value.ConvertDisplayedStringToValue(drawAnotherString)))
		{
			graphics.FillRectangle(brush3, rect);
		}
		using Pen pen = new Pen(Color.Black);
		graphics.DrawRectangle(pen, rect);
	}
}
