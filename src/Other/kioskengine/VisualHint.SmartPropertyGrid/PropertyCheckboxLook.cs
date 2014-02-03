using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class PropertyCheckboxLook : PropertyLook
{
	private bool IsBooleanValue
	{
		get
		{
			if (!(base.Value.TypeConverter is NullableConverter nullableConverter) || nullableConverter.UnderlyingType != typeof(bool))
			{
				return base.Value.UnderlyingType == typeof(bool);
			}
			return true;
		}
	}

	public PropertyCheckboxLook()
	{
		mNoFeelForReadOnly = true;
	}

	public override void ControlHeightMultiplier()
	{
		if (IsBooleanValue)
		{
			base.Value.OwnerEnumerator.Property.HeightMultiplier = 1;
		}
		else
		{
			base.Value.OwnerEnumerator.Property.HeightMultiplier = base.Value.GetDisplayedValues().Length;
		}
		if (base.Value.OwnerEnumerator.Property.HeightMultiplier == 0)
		{
			base.Value.OwnerEnumerator.Property.HeightMultiplier = 1;
		}
	}

	public override void OnDraw(Graphics graphics, Rectangle valueRect, Color textColor, PropertyEnumerator propEnum, string drawAnotherString, bool multipleTexts, PropertyValue.DrawValueIn drawValueIn)
	{
		if (drawAnotherString != null && drawAnotherString.Length == 0)
		{
			return;
		}
		string[] displayedValues = base.Value.GetDisplayedValues();
		if (displayedValues.Length == 0)
		{
			return;
		}
		Rectangle rectangle = base.Value.DrawIcon(graphics, valueRect, drawValueIn);
		int globalTextMargin = base.Value.Grid.GlobalTextMargin;
		valueRect.Height++;
		valueRect.X += globalTextMargin - 1;
		valueRect.Width -= globalTextMargin - 1;
		if (rectangle.Width > 0)
		{
			rectangle.Width += globalTextMargin;
			valueRect.X += rectangle.Width;
			valueRect.Width -= rectangle.Width;
		}
		Rectangle rectangle2 = valueRect;
		bool hasMultipleValues = base.Value.HasMultipleValues;
		bool drawEnabled = propEnum.Property.Enabled && base.Value.Grid.Enabled;
		if (IsBooleanValue)
		{
			Rectangle buttonRect = rectangle2;
			buttonRect.Height -= 4;
			buttonRect.Y += 2;
			buttonRect.Width = buttonRect.Height;
			if (hasMultipleValues)
			{
				propEnum.Property.ParentGrid.DrawManager.DrawCheckBoxIndeterminate(graphics, propEnum.Property.ParentGrid.Grid, buttonRect, propEnum.Property.Enabled, PropertyValue.DrawValueIn.Grid);
				return;
			}
			bool flag = true;
			bool flag2 = false;
			bool? flag3 = (bool?)base.Value.GetValue();
			flag = flag3.HasValue;
			if (flag)
			{
				flag2 = flag3.Value;
			}
			if (flag)
			{
				propEnum.Property.ParentGrid.DrawManager.DrawCheckBox(graphics, propEnum.Property.ParentGrid.Grid, buttonRect, flag2, drawEnabled, PropertyValue.DrawValueIn.Grid);
			}
			else
			{
				propEnum.Property.ParentGrid.DrawManager.DrawCheckBoxIndeterminate(graphics, propEnum.Property.ParentGrid.Grid, buttonRect, drawEnabled, PropertyValue.DrawValueIn.Grid);
			}
			Rectangle rect = rectangle2;
			rect.X = buttonRect.Right + globalTextMargin;
			rect.Width = rectangle2.Right - rect.Left;
			rect = base.Value.Grid.TransformRectIfRTL(rect);
			string text = (flag ? base.Value.ConvertValueToDisplayedString(flag2) : base.Value.ConvertValueToDisplayedString(null));
			Win32Calls.DrawText(graphics, text, ref rect, base.Value.Font, textColor, 0x824 | (((base.Value.Grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnValues) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((base.Value.Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
		}
		else if (propEnum.Property.Value.UnderlyingType.IsEnum)
		{
			rectangle2.Height = valueRect.Height / displayedValues.Length;
			Enum obj = (Enum)base.Value.GetValue();
			if (!hasMultipleValues && drawAnotherString != null)
			{
				obj = (Enum)base.Value.TypeConverter.ConvertFromString(base.Value.GetTypeDescriptorContext(base.Value.OwnerEnumerator), base.Value.CultureInfo, drawAnotherString);
			}
			string[] array = displayedValues;
			foreach (string text2 in array)
			{
				Rectangle buttonRect2 = rectangle2;
				buttonRect2.Height -= 4;
				buttonRect2.Y += 2;
				buttonRect2.Width = buttonRect2.Height;
				if (hasMultipleValues)
				{
					propEnum.Property.ParentGrid.DrawManager.DrawCheckBoxIndeterminate(graphics, propEnum.Property.ParentGrid.Grid, buttonRect2, drawEnabled, PropertyValue.DrawValueIn.Grid);
				}
				else
				{
					object obj2 = base.Value.ConvertDisplayedStringToValue(text2);
					int hashCode = obj2.GetHashCode();
					int hashCode2 = obj.GetHashCode();
					bool drawChecked = ((hashCode & hashCode2) == hashCode && hashCode != 0) || (hashCode == 0 && hashCode2 == 0);
					propEnum.Property.ParentGrid.DrawManager.DrawCheckBox(graphics, propEnum.Property.ParentGrid.Grid, buttonRect2, drawChecked, drawEnabled, PropertyValue.DrawValueIn.Grid);
				}
				Rectangle rect2 = rectangle2;
				rect2.X = buttonRect2.Right + globalTextMargin;
				rect2.Width = rectangle2.Right - rect2.Left;
				rect2 = base.Value.Grid.TransformRectIfRTL(rect2);
				Win32Calls.DrawText(graphics, text2, ref rect2, base.Value.Font, textColor, 0x824 | (((base.Value.Grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnValues) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((base.Value.Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
				rectangle2.Offset(0, rectangle2.Height);
			}
		}
		else
		{
			if (!propEnum.Property.Value.UnderlyingType.Equals(typeof(ArrayList)))
			{
				return;
			}
			rectangle2.Height = valueRect.Height / displayedValues.Length;
			for (int j = 0; j < displayedValues.Length; j++)
			{
				Rectangle buttonRect3 = rectangle2;
				buttonRect3.Height -= 4;
				buttonRect3.Y += 2;
				buttonRect3.Width = buttonRect3.Height;
				if (hasMultipleValues)
				{
					propEnum.Property.ParentGrid.DrawManager.DrawCheckBoxIndeterminate(graphics, propEnum.Property.ParentGrid.Grid, buttonRect3, drawEnabled, PropertyValue.DrawValueIn.Grid);
				}
				else
				{
					bool drawChecked2 = false;
					ICollection collection = propEnum.Property.Value.UnderlyingValue as ICollection;
					foreach (object item in collection)
					{
						if (item.ToString() == displayedValues[j])
						{
							drawChecked2 = true;
						}
					}
					propEnum.Property.ParentGrid.DrawManager.DrawCheckBox(graphics, propEnum.Property.ParentGrid.Grid, buttonRect3, drawChecked2, drawEnabled, PropertyValue.DrawValueIn.Grid);
				}
				Rectangle rect3 = rectangle2;
				rect3.X = buttonRect3.Right + globalTextMargin;
				rect3.Width = rectangle2.Right - rect3.Left;
				rect3 = base.Value.Grid.TransformRectIfRTL(rect3);
				Win32Calls.DrawText(graphics, displayedValues[j], ref rect3, base.Value.Font, textColor, 0x824 | (((base.Value.Grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnValues) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((base.Value.Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
				rectangle2.Offset(0, rectangle2.Height);
			}
		}
	}

	public override Rectangle GetDisplayStringRect(Graphics graphics, Rectangle valueRect, Point point)
	{
		point = base.Value.Grid.TransformPointIfRTL(point);
		int globalTextMargin = base.Value.Grid.GlobalTextMargin;
		valueRect.X += globalTextMargin - 1;
		valueRect.Width -= globalTextMargin - 1;
		Rectangle rect = Rectangle.Empty;
		string[] displayedValues = base.Value.GetDisplayedValues();
		if (displayedValues.Length == 0)
		{
			return rect;
		}
		Rectangle rectangle = base.Value.DrawIcon(null, valueRect, PropertyValue.DrawValueIn.None);
		if (rectangle.Width > 0)
		{
			rectangle.Width += base.Value.Grid.GlobalTextMargin;
			valueRect.X += rectangle.Width;
			valueRect.Width -= rectangle.Width;
		}
		if (IsBooleanValue)
		{
			if (base.Value.HasMultipleValues)
			{
				return Rectangle.Empty;
			}
			bool flag = true;
			bool flag2 = false;
			bool? flag3 = (bool?)base.Value.GetValue();
			flag = flag3.HasValue;
			if (flag)
			{
				flag2 = flag3.Value;
			}
			string text = (flag ? base.Value.ConvertValueToDisplayedString(flag2) : base.Value.ConvertValueToDisplayedString(null));
			Rectangle rectangle2 = valueRect;
			rectangle2.Height -= 4;
			rectangle2.Y += 2;
			rectangle2.Width = rectangle2.Height;
			Rectangle rectangle3 = valueRect;
			rectangle3.X = rectangle2.Right + globalTextMargin;
			rectangle3.Width = valueRect.Right - rectangle3.Left;
			rect = rectangle3;
			Win32Calls.DrawText(graphics, text, ref rect, base.Value.Font, Color.Black, 3108);
			rect.Y = valueRect.Top;
			rect.Height = valueRect.Bottom - rect.Y;
		}
		else
		{
			Rectangle rectangle4 = valueRect;
			rectangle4.Height = valueRect.Height / displayedValues.Length;
			string[] array = displayedValues;
			foreach (string text2 in array)
			{
				Rectangle rectangle5 = rectangle4;
				rectangle5.Height -= 4;
				rectangle5.Y += 2;
				rectangle5.Width = rectangle5.Height;
				Rectangle rectangle6 = rectangle4;
				rectangle6.X = rectangle5.Right + globalTextMargin;
				rectangle6.Width = rectangle4.Right - rectangle6.Left;
				rect = rectangle6;
				Win32Calls.DrawText(graphics, text2, ref rect, base.Value.Font, Color.Black, 3108);
				if (rect.Contains(point))
				{
					break;
				}
				rectangle4.Offset(0, rectangle4.Height);
			}
			rect.Y = rectangle4.Top;
			rect.Height = rectangle4.Bottom - rect.Y;
		}
		return rect;
	}

	public override string GetTooltipString(int line)
	{
		if (IsBooleanValue)
		{
			bool flag = true;
			bool flag2 = false;
			bool? flag3 = (bool?)base.Value.GetValue();
			flag = flag3.HasValue;
			if (flag)
			{
				flag2 = flag3.Value;
			}
			if (!flag)
			{
				return base.Value.ConvertValueToDisplayedString(null);
			}
			return base.Value.ConvertValueToDisplayedString(flag2);
		}
		string[] displayedValues = base.Value.GetDisplayedValues();
		if (line >= displayedValues.Length)
		{
			return "";
		}
		return displayedValues[line];
	}

	public override Size GetTooltipStringSize(Graphics graphics, Rectangle valueRect, Point point, int line)
	{
		return Win32Calls.GetTextExtent(graphics, GetTooltipString(line), base.Value.Font);
	}
}
