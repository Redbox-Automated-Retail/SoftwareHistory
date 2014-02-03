using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class PropertyRadioButtonLook : PropertyLook
{
	public PropertyRadioButtonLook()
	{
		mNoFeelForReadOnly = true;
	}

	public override void ControlHeightMultiplier()
	{
		base.Value.OwnerEnumerator.Property.HeightMultiplier = base.Value.GetDisplayedValues().Length;
	}

	public override void OnDraw(Graphics graphics, Rectangle valueRect, Color textColor, PropertyEnumerator propEnum, string drawAnotherString, bool multipleTexts, PropertyValue.DrawValueIn drawValueIn)
	{
		Rectangle rectangle = base.Value.DrawIcon(graphics, valueRect, drawValueIn);
		valueRect.Height++;
		int globalTextMargin = base.Value.Grid.GlobalTextMargin;
		valueRect.X += globalTextMargin - 1;
		valueRect.Width -= globalTextMargin - 1;
		if (rectangle.Width > 0)
		{
			rectangle.Width += globalTextMargin;
			valueRect.X += rectangle.Width;
			valueRect.Width -= rectangle.Width;
		}
		string[] displayedValues = base.Value.GetDisplayedValues();
		if (displayedValues.Length > 0)
		{
			Rectangle rectangle2 = valueRect;
			rectangle2.Height = valueRect.Height / displayedValues.Length;
			foreach (string text in displayedValues)
			{
				Rectangle radioRect = rectangle2;
				radioRect.Height -= 4;
				radioRect.Y += 2;
				radioRect.Width = radioRect.Height;
				propEnum.Property.ParentGrid.DrawManager.DrawRadioButton(graphics, propEnum.Property.ParentGrid.Grid, radioRect, propEnum.Property.Value.DisplayString == text, propEnum.Property.Enabled && base.Value.Grid.Enabled, PropertyValue.DrawValueIn.Grid);
				Rectangle rect = rectangle2;
				rect.X = radioRect.Right + base.Value.Grid.GlobalTextMargin;
				rect.Width = rectangle2.Right - rect.Left;
				rect = base.Value.Grid.TransformRectIfRTL(rect);
				Win32Calls.DrawText(graphics, text, ref rect, base.Value.Font, textColor, 0x824 | (((base.Value.Grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnValues) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((base.Value.Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
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
		Rectangle rectangle = base.Value.DrawIcon(null, valueRect, PropertyValue.DrawValueIn.None);
		if (rectangle.Width > 0)
		{
			rectangle.Width += base.Value.Grid.GlobalTextMargin;
			valueRect.X += rectangle.Width;
			valueRect.Width -= rectangle.Width;
		}
		Rectangle rect = Rectangle.Empty;
		string[] displayedValues = base.Value.GetDisplayedValues();
		if (displayedValues.Length > 0)
		{
			Rectangle rectangle2 = valueRect;
			rectangle2.Height = valueRect.Height / displayedValues.Length;
			foreach (string text in displayedValues)
			{
				Rectangle rectangle3 = rectangle2;
				rectangle3.Height -= 4;
				rectangle3.Y += 2;
				rectangle3.Width = rectangle3.Height;
				Rectangle rectangle4 = rectangle2;
				rectangle4.X = rectangle3.Right + globalTextMargin;
				rectangle4.Width = rectangle2.Right - rectangle4.Left;
				rect = rectangle4;
				Win32Calls.DrawText(graphics, text, ref rect, base.Value.Font, Color.Black, 3108);
				if (rect.Contains(point))
				{
					break;
				}
				rectangle2.Offset(0, rectangle2.Height);
			}
			rect.Y = rectangle2.Top;
			rect.Height = rectangle2.Bottom - rect.Y;
		}
		return rect;
	}

	public override string GetTooltipString(int line)
	{
		string[] displayedValues = base.Value.GetDisplayedValues();
		if (line < displayedValues.Length)
		{
			return displayedValues[line];
		}
		return "";
	}

	public override Size GetTooltipStringSize(Graphics graphics, Rectangle valueRect, Point point, int line)
	{
		return Win32Calls.GetTextExtent(graphics, GetTooltipString(line), base.Value.Font);
	}
}
