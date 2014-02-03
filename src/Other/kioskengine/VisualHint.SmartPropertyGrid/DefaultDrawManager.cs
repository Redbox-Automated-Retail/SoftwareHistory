using System.Drawing;

namespace VisualHint.SmartPropertyGrid;

public class DefaultDrawManager : CustomDrawManager
{
	public override int CommentsGapHeight => 6;

	public override void DrawCommentsBackground(Graphics graphics, Rectangle commentRect)
	{
		using SolidBrush brush = new SolidBrush(Grid.CommentsBackColor);
		graphics.FillRectangle(brush, commentRect);
	}

	public override void DrawCommentsGap(Graphics graphics, Rectangle rect)
	{
		graphics.DrawLine(Pens.White, rect.Left, rect.Top, rect.Right, rect.Top);
		graphics.DrawLine(Pens.White, rect.Left, rect.Top + 2, rect.Right, rect.Top + 2);
		graphics.DrawLine(SystemPens.ControlLight, rect.Left, rect.Top + 1, rect.Right, rect.Top + 1);
		graphics.DrawLine(SystemPens.ControlLight, rect.Left, rect.Top + 3, rect.Right, rect.Top + 3);
		graphics.DrawLine(SystemPens.ControlDark, rect.Left, rect.Top + 4, rect.Right, rect.Top + 4);
		graphics.DrawLine(SystemPens.ControlDarkDark, rect.Left, rect.Top + 5, rect.Right, rect.Top + 5);
	}

	public override void DrawButtonsBackground(Graphics graphics, Rectangle rect)
	{
		graphics.FillRectangle(SystemBrushes.Control, rect);
	}

	public override void DrawButtonsGap(Graphics graphics, Rectangle rect)
	{
		graphics.DrawLine(Pens.White, rect.Left, rect.Bottom - 3, rect.Right, rect.Bottom - 3);
		graphics.DrawLine(SystemPens.ControlLight, rect.Left, rect.Bottom - 2, rect.Right, rect.Bottom - 2);
		graphics.DrawLine(SystemPens.ControlDark, rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
	}

	public override void DrawGridBackground(Graphics graphics, Rectangle rect)
	{
		using SolidBrush brush = new SolidBrush(Grid.GridBackColor);
		graphics.FillRectangle(brush, rect);
	}

	public override void DrawLeftColumnBackground(Graphics graphics, Rectangle rect, PropertyEnumerator propEnum)
	{
		using Pen pen = new Pen(Grid.GridColor);
		graphics.DrawLine(pen, Grid.TransformPointIfRTL(rect.Right - 1, rect.Top), Grid.TransformPointIfRTL(rect.Right - 1, rect.Bottom));
	}

	public override void DrawCategoryLabelBackground(Graphics graphics, Rectangle rect, PropertyEnumerator enumSelf)
	{
		rect = Grid.TransformRectIfRTL(rect);
		if (!Rectangle.Intersect(rect, Rectangle.Round(graphics.ClipBounds)).IsEmpty)
		{
			Brush brush;
			if (enumSelf.Property.Selected)
			{
				Color color = ((!Grid.GridContainsFocus) ? Grid.SelectedNotFocusedBackColor : Grid.SelectedBackColor);
				brush = new SolidBrush(color);
			}
			else
			{
				brush = new SolidBrush(Grid.GridBackColor);
			}
			graphics.FillRectangle(brush, rect);
			brush.Dispose();
		}
	}

	public override void DrawSubCategoryLabelBackground(Graphics graphics, int labelRectX, Rectangle rect, PropertyEnumerator enumSelf)
	{
		DrawCategoryLabelBackground(graphics, rect, enumSelf);
	}
}
