using System.Drawing;
using System.Windows.Forms;
using Skybound.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class DotnetDrawManager : CustomDrawManager
{
	private bool _drawSubCategoryBackground = true;

	private bool _drawSelectedStateAsFocusRect = true;

	public virtual bool DrawSelectedStateAsFocusRect
	{
		get
		{
			return _drawSelectedStateAsFocusRect;
		}
		set
		{
			_drawSelectedStateAsFocusRect = value;
		}
	}

	public override bool ShowCategoryVerticalLine => false;

	public override void DrawButtonsBackground(Graphics graphics, Rectangle rect)
	{
		using SolidBrush brush = new SolidBrush(Grid.BackColor);
		graphics.FillRectangle(brush, rect);
	}

	public override void DrawCommentsBackground(Graphics graphics, Rectangle commentRect)
	{
		using (Brush brush = new SolidBrush(Grid.CommentsBackColor))
		{
			graphics.FillRectangle(brush, commentRect);
		}
		commentRect.Width--;
		commentRect.Height--;
		graphics.DrawRectangle(SystemPens.GrayText, commentRect);
	}

	public override void DrawCommentsGap(Graphics graphics, Rectangle rect)
	{
		using SolidBrush brush = new SolidBrush(Grid.BackColor);
		graphics.FillRectangle(brush, rect);
	}

	public override void DrawGridBackground(Graphics graphics, Rectangle rect)
	{
		rect = Grid.TransformRectIfRTL(rect);
		Rectangle rect2 = Rectangle.Intersect(rect, Rectangle.Round(graphics.ClipBounds));
		if (rect2.IsEmpty)
		{
			return;
		}
		using Brush brush = new SolidBrush(Grid.GridBackColor);
		graphics.FillRectangle(brush, rect2);
	}

	public override void DrawLeftColumnBackground(Graphics graphics, Rectangle rect, PropertyEnumerator propEnum)
	{
		rect = Grid.TransformRectIfRTL(rect);
		Rectangle rect2 = Rectangle.Intersect(rect, Rectangle.Round(graphics.ClipBounds));
		if (rect2.IsEmpty)
		{
			return;
		}
		using SolidBrush brush = new SolidBrush(Grid.GridColor);
		graphics.FillRectangle(brush, rect2);
	}

	public override void NcCalcSize(ref Rectangle rect)
	{
		rect.Inflate(-1, -1);
	}

	public override void DrawGridNonClientArea(Graphics graphics, Rectangle rect)
	{
		rect.Width--;
		rect.Height--;
		graphics.DrawRectangle(SystemPens.GrayText, rect);
	}

	public override void DrawCategoryLabelBackground(Graphics graphics, Rectangle rect, PropertyEnumerator enumSelf)
	{
		rect = Grid.TransformRectIfRTL(rect);
		Rectangle rect2 = Rectangle.Intersect(rect, Rectangle.Round(graphics.ClipBounds));
		if (!rect2.IsEmpty)
		{
			Brush brush;
			if (DrawSelectedStateAsFocusRect)
			{
				brush = new SolidBrush(Grid.GridColor);
			}
			else if (enumSelf.Property.Selected)
			{
				Color color = ((!Grid.GridContainsFocus) ? Grid.SelectedNotFocusedBackColor : Grid.SelectedBackColor);
				brush = new SolidBrush(color);
			}
			else
			{
				brush = new SolidBrush(Grid.GridColor);
			}
			graphics.FillRectangle(brush, rect2);
			brush.Dispose();
		}
	}

	public override void DrawCategoryLabelText(Graphics graphics, Rectangle itemRect, Rectangle labelRect, Color textColor, PropertyEnumerator enumSelf)
	{
		labelRect = Grid.TransformRectIfRTL(labelRect);
		using Font font = new Font(Grid.Font, FontStyle.Bold);
		if (DrawSelectedStateAsFocusRect)
		{
			textColor = enumSelf.Property.ForeColor;
		}
		Win32Calls.DrawText(graphics, enumSelf.Property.DisplayName, ref labelRect, font, textColor, 0x824 | (((Grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnLabels) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
		if (DrawSelectedStateAsFocusRect && enumSelf.Property.Selected && Grid.GridContainsFocus)
		{
			Size textExtent = Win32Calls.GetTextExtent(graphics, enumSelf.Property.DisplayName, font);
			Rectangle rect = Grid.TransformRectIfRTL(labelRect);
			rect.Y++;
			rect.Height -= 2;
			int globalTextMargin = enumSelf.Property.ParentGrid.GlobalTextMargin;
			rect.X -= globalTextMargin / 2 + 1;
			rect.Width = globalTextMargin + textExtent.Width;
			if (rect.Right > labelRect.Right)
			{
				rect.Width = labelRect.Right - rect.X;
			}
			rect = Grid.TransformRectIfRTL(rect);
			ControlPaint.DrawFocusRectangle(graphics, rect);
		}
	}

	public override void DrawSubCategoryLabelText(Graphics graphics, Rectangle itemRect, Rectangle labelRect, Color textColor, PropertyEnumerator enumSelf)
	{
		labelRect = Grid.TransformRectIfRTL(labelRect);
		Property property = enumSelf.Property;
		if (DrawSelectedStateAsFocusRect)
		{
			textColor = enumSelf.Property.ForeColor;
		}
		Win32Calls.DrawText(graphics, property.DisplayName, ref labelRect, Grid.Font, textColor, 0x8824 | ((Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
		if (DrawSelectedStateAsFocusRect && enumSelf.Property.Selected && Grid.GridContainsFocus)
		{
			Size textExtent = Win32Calls.GetTextExtent(graphics, enumSelf.Property.DisplayName, Grid.Font);
			Rectangle rect = Grid.TransformRectIfRTL(labelRect);
			rect.Y++;
			rect.Height -= 2;
			int globalTextMargin = enumSelf.Property.ParentGrid.GlobalTextMargin;
			rect.X -= globalTextMargin / 2 + 1;
			rect.Width = globalTextMargin + textExtent.Width;
			if (rect.Right > labelRect.Right)
			{
				rect.Width = labelRect.Right - rect.X;
			}
			rect = Grid.TransformRectIfRTL(rect);
			ControlPaint.DrawFocusRectangle(graphics, rect);
		}
	}

	public override void DrawSubCategoryLabelBackground(Graphics graphics, int labelRectX, Rectangle rect, PropertyEnumerator enumSelf)
	{
		rect = Grid.TransformRectIfRTL(rect);
		Rectangle rect2 = Rectangle.Intersect(rect, Rectangle.Round(graphics.ClipBounds));
		if (rect2.IsEmpty)
		{
			return;
		}
		Brush brush;
		if (DrawSelectedStateAsFocusRect)
		{
			if (_drawSubCategoryBackground)
			{
				ColorUtils.RGBtoHSL(Grid.GridColor, out var H, out var S, out var L);
				Color color = ColorUtils.HSLtoRGB(H, S, L + (1.0 - L) / 2.0);
				brush = new SolidBrush(color);
			}
			else
			{
				brush = new SolidBrush(Grid.GridColor);
			}
		}
		else if (enumSelf.Property.Selected)
		{
			Color color2 = ((!Grid.GridContainsFocus) ? Grid.SelectedNotFocusedBackColor : Grid.SelectedBackColor);
			brush = new SolidBrush(color2);
		}
		else if (_drawSubCategoryBackground)
		{
			ColorUtils.RGBtoHSL(Grid.GridColor, out var H2, out var S2, out var _);
			Color color3 = ColorUtils.HSLtoRGB(H2, S2, 46.0 / 51.0);
			brush = new SolidBrush(color3);
		}
		else
		{
			brush = new SolidBrush(Grid.GridColor);
		}
		graphics.FillRectangle(brush, rect2);
		brush.Dispose();
	}

	public override void DrawCategoryValue(Graphics graphics, Rectangle valueRect, Color textColor, PropertyEnumerator enumSelf)
	{
		valueRect = Grid.TransformRectIfRTL(valueRect);
		Font font = ((!(enumSelf.Property is SubCategory)) ? new Font(Grid.Font, FontStyle.Bold) : Grid.Font);
		if (DrawSelectedStateAsFocusRect)
		{
			textColor = enumSelf.Property.ForeColor;
		}
		Win32Calls.DrawText(graphics, ((RootCategory)enumSelf.Property).ValueText, ref valueRect, font, textColor, 0x824 | (((Grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnValues) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
		if (!(enumSelf.Property is SubCategory))
		{
			font.Dispose();
		}
	}

	public override void DrawPlusMinusSign(Graphics graphics, Control control, Rectangle itemRect, PropertyEnumerator propEnum)
	{
		Property property = propEnum.Property;
		Rectangle signRect = property.GetSignRect(itemRect, propEnum);
		signRect = Grid.TransformRectIfRTL(signRect);
		if (Rectangle.Intersect(signRect, Rectangle.Round(graphics.ClipBounds)).IsEmpty)
		{
			return;
		}
		if (ThemeRenderer.Enabled)
		{
			base.DrawPlusMinusSign(graphics, control, itemRect, propEnum);
			return;
		}
		Color color = ((!property.Selected || propEnum.Depth <= 0) ? property.ForeColor : (((!Grid.GridContainsFocus && propEnum.Property.InPlaceCtrlInAction == null) || (DrawSelectedStateAsFocusRect && propEnum.Property is SubCategory)) ? property.ForeColor : Grid.HighlightedTextColor));
		using Pen pen = new Pen(color);
		graphics.DrawRectangle(pen, signRect);
		Point point = new Point(signRect.X + signRect.Width / 2, signRect.Y + signRect.Height / 2);
		graphics.DrawLine(pen, point.X - 2, point.Y, point.X + 2, point.Y);
		if (!property.Expanded)
		{
			graphics.DrawLine(pen, point.X, point.Y - 2, point.X, point.Y + 2);
		}
	}
}
