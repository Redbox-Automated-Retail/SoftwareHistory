using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class PropertyUnitLook : PropertyLook
{
	public const string UnitValue = "unit";

	public override void OnDraw(Graphics graphics, Rectangle valueRect, Color textColor, PropertyEnumerator propEnum, string drawAnotherString, bool multipleTexts, PropertyValue.DrawValueIn drawValueIn)
	{
		if (drawAnotherString == null || drawAnotherString.Length != 0)
		{
			base.Value.DrawIcon(graphics, valueRect, drawValueIn);
			valueRect.Height++;
			Rectangle displayStringRect = GetDisplayStringRect(graphics, valueRect, Point.Empty);
			displayStringRect.Width = valueRect.Right - displayStringRect.Left;
			displayStringRect = base.Value.Grid.TransformRectIfRTL(displayStringRect);
			Win32Calls.DrawText(graphics, (drawAnotherString == null) ? GetDisplayStringInternal() : drawAnotherString, ref displayStringRect, base.Value.Font, textColor, 0x824 | (((base.Value.Grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnValues) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((base.Value.Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
		}
	}

	protected string GetDisplayStringInternal()
	{
		PropertyValue value = base.Value.OwnerEnumerator.Property.GetValue("unit");
		if (base.Value.HasMultipleValues)
		{
			if (value.HasMultipleValues)
			{
				return "";
			}
			return value.DisplayString;
		}
		if (value.HasMultipleValues)
		{
			return base.Value.ConvertValueToDisplayedString(base.Value.GetValue());
		}
		return base.Value.ConvertValueToDisplayedString(base.Value.GetValue()) + " " + value.DisplayString;
	}

	public override Rectangle GetDisplayStringRect(Graphics graphics, Rectangle valueRect, Point point)
	{
		Rectangle rectangle = base.Value.DrawIcon(null, valueRect, PropertyValue.DrawValueIn.None);
		if (rectangle.Width > 0)
		{
			rectangle.Width += base.Value.Grid.GlobalTextMargin;
		}
		Size textExtent = Win32Calls.GetTextExtent(graphics, GetDisplayStringInternal(), base.Value.Font);
		Rectangle result = valueRect;
		result.X += base.Value.Grid.GlobalTextMargin + rectangle.Width;
		result.Width = Size.Round(textExtent).Width;
		return result;
	}

	public override string GetTooltipString(int line)
	{
		return GetDisplayStringInternal();
	}

	public override Size GetTooltipStringSize(Graphics graphics, Rectangle valueRect, Point point, int line)
	{
		return Win32Calls.GetTextExtent(graphics, GetTooltipString(0), base.Value.Font);
	}
}
