using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class PropertyPasswordLook : PropertyLook
{
	private Font _font;

	private float _fontSize;

	public override string DisplayString
	{
		get
		{
			string displayStringWithoutLook = base.Value.DisplayStringWithoutLook;
			string text = "";
			for (int i = 0; i < displayStringWithoutLook.Length; i++)
			{
				text += base.Value.Grid.DrawManager.PasswordChar;
			}
			return text;
		}
	}

	private Font Font
	{
		get
		{
			if (_font == null)
			{
				_fontSize = base.Value.Grid.Font.Size;
				_font = new Font("Microsoft Sans Serif", _fontSize);
			}
			else if (_fontSize != base.Value.Grid.Font.Size)
			{
				_font.Dispose();
				_fontSize = base.Value.Grid.Font.Size;
				_font = new Font("Microsoft Sans Serif", _fontSize);
			}
			return _font;
		}
	}

	public override void OnDraw(Graphics graphics, Rectangle valueRect, Color textColor, PropertyEnumerator propEnum, string drawAnotherString, bool multipleTexts, PropertyValue.DrawValueIn drawValueIn)
	{
		if (drawAnotherString != null && drawAnotherString.Length == 0)
		{
			return;
		}
		base.Value.DrawIcon(graphics, valueRect, drawValueIn);
		valueRect.Height++;
		Rectangle displayStringRect = GetDisplayStringRect(graphics, valueRect, Point.Empty);
		displayStringRect.X -= base.Value.Grid.GlobalTextMargin - 3;
		displayStringRect.Width = valueRect.Right - displayStringRect.Left;
		displayStringRect = base.Value.Grid.TransformRectIfRTL(displayStringRect);
		StringFormat stringFormat = (StringFormat)StringFormat.GenericDefault.Clone();
		stringFormat.FormatFlags = StringFormatFlags.NoWrap;
		if (base.Value.Grid.RightToLeft == RightToLeft.Yes)
		{
			stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
		}
		stringFormat.LineAlignment = StringAlignment.Center;
		using Brush brush = new SolidBrush(textColor);
		graphics.DrawString(DisplayString, Font, brush, displayStringRect, stringFormat);
	}

	public override Rectangle GetDisplayStringRect(Graphics graphics, Rectangle valueRect, Point point)
	{
		Size size = Size.Round(graphics.MeasureString(DisplayString, Font));
		Rectangle rectangle = base.Value.DrawIcon(null, valueRect, PropertyValue.DrawValueIn.None);
		if (rectangle.Width > 0)
		{
			rectangle.Width += base.Value.Grid.GlobalTextMargin;
		}
		Rectangle result = valueRect;
		result.X += base.Value.Grid.GlobalTextMargin + rectangle.Width;
		result.Width = size.Width;
		return result;
	}
}
