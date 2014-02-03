using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class PropertyFontNameLook : PropertyLook
{
	private static int fontSampleWidth = 20;

	public override void OnDraw(Graphics graphics, Rectangle valueRect, Color textColor, PropertyEnumerator propEnum, string drawAnotherString, bool multipleTexts, PropertyValue.DrawValueIn drawValueIn)
	{
		if (drawAnotherString != null && drawAnotherString.Length == 0)
		{
			return;
		}
		int num = 0;
		num = drawValueIn switch
		{
			PropertyValue.DrawValueIn.Grid => base.Value.Grid.Grid.ClientSize.Width, 
			PropertyValue.DrawValueIn.InPlaceCtrl => base.Value.Grid.InPlaceControl.ClientSize.Width, 
			_ => valueRect.Width, 
		};
		Rectangle rectangle = base.Value.DrawIcon(graphics, valueRect, drawValueIn);
		string text = ((drawAnotherString == null) ? base.Value.DisplayString : drawAnotherString);
		float emSize = (float)(((double)valueRect.Height - 2.0) / 1.2);
		Font font = GetFont(text, emSize);
		if (font != null)
		{
			int globalTextMargin = base.Value.Grid.GlobalTextMargin;
			int num2 = globalTextMargin / 2;
			valueRect.Height++;
			valueRect.X += globalTextMargin;
			valueRect.Width -= globalTextMargin;
			if (rectangle.Width > 0)
			{
				rectangle.Width += base.Value.Grid.GlobalTextMargin;
				valueRect.X += rectangle.Width;
				valueRect.Width -= rectangle.Width;
			}
			string sampleText = GetSampleText(graphics, font);
			Rectangle rect = valueRect;
			rect.Inflate(num2, -1);
			rect.Width = fontSampleWidth;
			rect.Height--;
			Rectangle rect2 = valueRect;
			rect2.X = rect.Right + base.Value.Grid.GlobalTextMargin;
			rect2.Width = valueRect.Right - rect2.Left;
			rect2 = base.Value.Grid.TransformRectIfRTL(rect2, num);
			rect = base.Value.Grid.TransformRectIfRTL(rect, num);
			graphics.FillRectangle(SystemBrushes.Highlight, rect);
			rect.Inflate(-num2, 0);
			Win32Calls.DrawText(graphics, sampleText, ref rect, font, SystemColors.HighlightText, 36);
			Win32Calls.DrawText(graphics, text, ref rect2, base.Value.Font, textColor, 0x2824 | (((base.Value.Grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnValues) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((base.Value.Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
			font.Dispose();
		}
	}

	private string GetSampleText(Graphics graphics, Font font)
	{
		int num = base.Value.Grid.GlobalTextMargin / 2;
		if (Win32Calls.GetTextExtent(graphics, "abcd", font).Width <= fontSampleWidth - 2 * num)
		{
			return "abcd";
		}
		if (Win32Calls.GetTextExtent(graphics, "abc", font).Width <= fontSampleWidth - 2 * num)
		{
			return "abc";
		}
		if (Win32Calls.GetTextExtent(graphics, "ab", font).Width <= fontSampleWidth - 2 * num)
		{
			return "ab";
		}
		if (Win32Calls.GetTextExtent(graphics, "a", font).Width <= fontSampleWidth - 2 * num)
		{
			return "a";
		}
		return "";
	}

	private Font GetFont(string fontFamily, float emSize)
	{
		FontFamily fontFamily2 = null;
		Font result = null;
		try
		{
			fontFamily2 = new FontFamily(fontFamily);
		}
		catch
		{
		}
		if (fontFamily2 != null)
		{
			try
			{
				result = new Font(fontFamily2, emSize, FontStyle.Regular, GraphicsUnit.Pixel);
			}
			catch
			{
				try
				{
					result = new Font(fontFamily2, emSize, FontStyle.Italic, GraphicsUnit.Pixel);
				}
				catch
				{
					try
					{
						result = new Font(fontFamily2, emSize, FontStyle.Bold, GraphicsUnit.Pixel);
						goto end_IL_003b;
					}
					catch
					{
						try
						{
							result = new Font(fontFamily2, emSize, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
							goto end_IL_003b;
						}
						catch
						{
							goto end_IL_003b;
						}
					}
					end_IL_003b:;
				}
			}
		}
		return result;
	}

	public override Rectangle GetDisplayStringRect(Graphics graphics, Rectangle valueRect, Point point)
	{
		float emSize = (float)(((double)valueRect.Height - 3.0) / 1.2);
		Font font = GetFont(base.Value.DisplayString, emSize);
		if (font != null)
		{
			Size textExtent = Win32Calls.GetTextExtent(graphics, base.Value.DisplayString, base.Value.Font);
			Rectangle rectangle = base.Value.DrawIcon(null, valueRect, PropertyValue.DrawValueIn.None);
			int globalTextMargin = base.Value.Grid.GlobalTextMargin;
			int num = globalTextMargin / 2;
			int num2 = 2 * globalTextMargin - num + fontSampleWidth;
			valueRect.X += num2;
			valueRect.Width = textExtent.Width;
			if (rectangle.Width > 0)
			{
				rectangle.Width += base.Value.Grid.GlobalTextMargin;
				valueRect.X += rectangle.Width;
				valueRect.Width -= rectangle.Width;
			}
			font.Dispose();
			return valueRect;
		}
		return Rectangle.Empty;
	}
}
