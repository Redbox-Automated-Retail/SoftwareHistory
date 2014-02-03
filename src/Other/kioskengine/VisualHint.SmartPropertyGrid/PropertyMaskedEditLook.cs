using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class PropertyMaskedEditLook : PropertyLook
{
	private MaskedTextProvider _maskedTextProvider;

	public string Mask
	{
		get
		{
			return _maskedTextProvider.Mask;
		}
		set
		{
			_maskedTextProvider = new MaskedTextProvider(value);
			_maskedTextProvider.IncludePrompt = true;
		}
	}

	public char PromptChar
	{
		get
		{
			return _maskedTextProvider.PromptChar;
		}
		set
		{
			_maskedTextProvider.PromptChar = value;
		}
	}

	public override string DisplayString
	{
		get
		{
			_maskedTextProvider.Set(base.Value.DisplayStringWithoutLook);
			return _maskedTextProvider.ToString(ignorePasswordChar: true);
		}
	}

	public PropertyMaskedEditLook(string mask)
	{
		_maskedTextProvider = new MaskedTextProvider(mask);
		_maskedTextProvider.IncludePrompt = true;
	}

	public PropertyMaskedEditLook(string mask, char promptChar)
	{
		_maskedTextProvider = new MaskedTextProvider(mask);
		_maskedTextProvider.IncludePrompt = true;
		_maskedTextProvider.PromptChar = promptChar;
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
			Win32Calls.DrawText(graphics, (drawAnotherString == null) ? DisplayString : drawAnotherString, ref displayStringRect, base.Value.Font, textColor, 0x824 | (((base.Value.Grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnValues) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((base.Value.Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
		}
	}

	public override Rectangle GetDisplayStringRect(Graphics graphics, Rectangle valueRect, Point point)
	{
		Size textExtent = Win32Calls.GetTextExtent(graphics, DisplayString, base.Value.Font);
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
