using System;
using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class PropertyColorLook : PropertyLook
{
	private bool _hideValue;

	public PropertyColorLook()
	{
	}

	public PropertyColorLook(bool hideValue)
	{
		_hideValue = hideValue;
	}

	private Rectangle GetColorRect(Rectangle valueRect)
	{
		Rectangle result = valueRect;
		Rectangle rectangle = base.Value.DrawIcon(null, valueRect, PropertyValue.DrawValueIn.None);
		if (rectangle.Width > 0)
		{
			rectangle.Width += base.Value.Grid.GlobalTextMargin;
		}
		result.X += base.Value.Grid.GlobalTextMargin + rectangle.Width;
		result.Y += 2;
		result.Height -= 6;
		if (_hideValue)
		{
			result.Width = valueRect.Width - 2 * base.Value.Grid.GlobalTextMargin;
		}
		else
		{
			result.Width = 3 * result.Height / 2;
		}
		return result;
	}

	public override void OnDraw(Graphics graphics, Rectangle valueRect, Color textColor, PropertyEnumerator propEnum, string drawAnotherString, bool multipleTexts, PropertyValue.DrawValueIn drawValueIn)
	{
		valueRect.Height++;
		base.Value.DrawIcon(graphics, valueRect, drawValueIn);
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
		Rectangle colorRect = GetColorRect(valueRect);
		colorRect = base.Value.Grid.TransformRectIfRTL(colorRect, containerWidth);
		Color color = Color.Empty;
		try
		{
			if (!propEnum.Property.Value.HasMultipleTexts())
			{
				color = (Color)base.Value.ConvertDisplayedStringToValue(drawAnotherString);
			}
		}
		catch (Exception)
		{
			color = (Color)base.Value.GetValue();
		}
		Brush brush = new SolidBrush(color);
		graphics.FillRectangle(brush, colorRect);
		if (propEnum.Property.Value.HasMultipleTexts())
		{
			graphics.DrawLine(SystemPens.GrayText, base.Value.Grid.TransformPointIfRTL(colorRect.Left, colorRect.Bottom, containerWidth), base.Value.Grid.TransformPointIfRTL(colorRect.Right, colorRect.Top, containerWidth));
		}
		graphics.DrawRectangle(Pens.Black, colorRect);
		Rectangle displayStringRect = GetDisplayStringRect(graphics, valueRect, Point.Empty);
		displayStringRect.Width = valueRect.Right - displayStringRect.Left;
		displayStringRect = base.Value.Grid.TransformRectIfRTL(displayStringRect, containerWidth);
		Win32Calls.DrawText(graphics, (drawAnotherString == null) ? DisplayString : drawAnotherString, ref displayStringRect, base.Value.Font, textColor, 0x8824 | ((base.Value.Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
	}

	public override Rectangle GetDisplayStringRect(Graphics graphics, Rectangle valueRect, Point point)
	{
		if (_hideValue)
		{
			return Rectangle.Empty;
		}
		Rectangle colorRect = GetColorRect(valueRect);
		Rectangle result = valueRect;
		result.X = colorRect.Right + 3 + base.Value.Grid.GlobalTextMargin;
		result.Width = Win32Calls.GetTextExtent(graphics, DisplayString, base.Value.Font).Width;
		return result;
	}
}
