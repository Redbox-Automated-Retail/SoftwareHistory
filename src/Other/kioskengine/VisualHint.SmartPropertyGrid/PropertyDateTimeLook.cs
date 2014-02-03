using System;
using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class PropertyDateTimeLook : PropertyLook
{
	private DateTimePickerFormat _format = DateTimePickerFormat.Time;

	private string _customFormat = "";

	public DateTimePickerFormat Format
	{
		get
		{
			return _format;
		}
		set
		{
			_format = value;
		}
	}

	public string CustomFormat
	{
		get
		{
			return _customFormat;
		}
		set
		{
			_format = DateTimePickerFormat.Custom;
			_customFormat = value;
		}
	}

	public override string DisplayString
	{
		get
		{
			GetDisplayStringInternal();
			return GetDisplayStringInternal();
		}
	}

	public PropertyDateTimeLook()
	{
	}

	public PropertyDateTimeLook(DateTimePickerFormat format)
	{
		_format = format;
	}

	public PropertyDateTimeLook(string customFormat)
	{
		_format = DateTimePickerFormat.Custom;
		_customFormat = customFormat;
	}

	public override void OnDraw(Graphics graphics, Rectangle valueRect, Color textColor, PropertyEnumerator propEnum, string drawAnotherString, bool multipleTexts, PropertyValue.DrawValueIn drawValueIn)
	{
		if (drawAnotherString == null || drawAnotherString.Length != 0)
		{
			base.Value.DrawIcon(graphics, valueRect, drawValueIn);
			valueRect.Height++;
			Rectangle displayStringRect = GetDisplayStringRect(graphics, valueRect, Point.Empty);
			displayStringRect.Width = valueRect.Right - displayStringRect.Left;
			displayStringRect = base.Value.Grid.TransformRectIfRTL(displayStringRect);
			Win32Calls.DrawText(graphics, (drawAnotherString == null) ? DisplayString : drawAnotherString, ref displayStringRect, base.Value.Font, textColor, 0x2824 | (((base.Value.Grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnValues) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((base.Value.Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
		}
	}

	protected string GetDisplayStringInternal()
	{
		if (base.Value.Feel is PropertyDateTimeFeel)
		{
			DateTime dateTime = (DateTime)base.Value.UnderlyingValue;
			if (_format == DateTimePickerFormat.Custom)
			{
				return dateTime.ToString(_customFormat);
			}
			if (_format == DateTimePickerFormat.Time)
			{
				return dateTime.ToLongTimeString();
			}
			if (_format == DateTimePickerFormat.Short)
			{
				return dateTime.ToShortDateString();
			}
			return dateTime.ToLongDateString();
		}
		return "";
	}

	public override Rectangle GetDisplayStringRect(Graphics graphics, Rectangle valueRect, Point point)
	{
		Size textExtent = Win32Calls.GetTextExtent(graphics, GetDisplayStringInternal(), base.Value.Font);
		Rectangle rectangle = base.Value.DrawIcon(null, valueRect, PropertyValue.DrawValueIn.None);
		if (rectangle.Width > 0)
		{
			rectangle.Width += base.Value.Grid.GlobalTextMargin;
		}
		Rectangle result = valueRect;
		result.X += base.Value.Grid.GlobalTextMargin + rectangle.Width;
		result.Width = textExtent.Width;
		return result;
	}
}
