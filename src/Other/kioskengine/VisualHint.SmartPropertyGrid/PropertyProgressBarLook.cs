using System;
using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class PropertyProgressBarLook : PropertyLook
{
	private bool _drawFrame;

	private string _formatStr = "";

	public string Format
	{
		get
		{
			return _formatStr;
		}
		set
		{
			_formatStr = value;
		}
	}

	public override string DisplayString => string.Format(_formatStr, base.DisplayString);

	public PropertyProgressBarLook(bool drawFrame)
	{
		_drawFrame = drawFrame;
	}

	public PropertyProgressBarLook(bool drawFrame, string formatStr)
	{
		_drawFrame = drawFrame;
		_formatStr = formatStr;
	}

	public override void AttachProperty(PropertyEnumerator propEnum)
	{
		propEnum.Property.Value.Feel = null;
	}

	public override void OnDraw(Graphics graphics, Rectangle valueRect, Color textColor, PropertyEnumerator propEnum, string drawAnotherString, bool multipleTexts, PropertyValue.DrawValueIn drawValueIn)
	{
		Rectangle rectangle = base.Value.DrawIcon(graphics, valueRect, drawValueIn);
		int globalTextMargin = base.Value.Grid.GlobalTextMargin;
		valueRect.Height++;
		if (_formatStr.Length != 0)
		{
			Size textExtent = Win32Calls.GetTextExtent(graphics, DisplayString, base.Value.Font);
			Rectangle displayStringRect = GetDisplayStringRect(graphics, valueRect, Point.Empty);
			displayStringRect.Width = Math.Min(textExtent.Width, valueRect.Width - globalTextMargin);
			displayStringRect = base.Value.Grid.TransformRectIfRTL(displayStringRect);
			Win32Calls.DrawText(graphics, DisplayString, ref displayStringRect, base.Value.Font, textColor, 0x824 | (((base.Value.Grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnValues) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((base.Value.Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
			displayStringRect = base.Value.Grid.TransformRectIfRTL(displayStringRect);
			valueRect.Width = valueRect.Right - (displayStringRect.Right + globalTextMargin);
			valueRect.X = displayStringRect.Right + globalTextMargin;
		}
		else
		{
			valueRect.X += globalTextMargin;
			valueRect.Width -= globalTextMargin;
			if (rectangle.Width > 0)
			{
				rectangle.Width += base.Value.Grid.GlobalTextMargin;
				valueRect.X += rectangle.Width;
				valueRect.Width -= rectangle.Width;
			}
		}
		if (_drawFrame)
		{
			valueRect.Width -= globalTextMargin;
		}
		else
		{
			valueRect.Width -= globalTextMargin - 2;
		}
		valueRect.Y++;
		valueRect.Height -= 3;
		if (valueRect.Width > 2)
		{
			object minValue;
			object maxValue;
			if (propEnum.Property.Value.Validator is PropertyValidatorMinMax propertyValidatorMinMax)
			{
				minValue = propertyValidatorMinMax.Min;
				maxValue = propertyValidatorMinMax.Max;
			}
			else
			{
				minValue = 0;
				maxValue = 100;
			}
			propEnum.Property.ParentGrid.DrawManager.DrawProgressBar(graphics, propEnum.Property.ParentGrid.Grid, valueRect, _drawFrame, propEnum.Property.Value.GetValue(), minValue, maxValue);
		}
	}

	public override Rectangle GetDisplayStringRect(Graphics graphics, Rectangle valueRect, Point point)
	{
		valueRect.Inflate(-base.Value.Grid.GlobalTextMargin, 0);
		Rectangle rectangle = base.Value.DrawIcon(null, valueRect, PropertyValue.DrawValueIn.None);
		if (rectangle.Width > 0)
		{
			rectangle.Width += base.Value.Grid.GlobalTextMargin;
			valueRect.X += rectangle.Width;
			valueRect.Width -= rectangle.Width;
		}
		if (_formatStr.Length != 0)
		{
			valueRect.Width = Win32Calls.GetTextExtent(graphics, DisplayString, base.Value.Font).Width;
			return valueRect;
		}
		return base.GetDisplayStringRect(graphics, valueRect, point);
	}
}
