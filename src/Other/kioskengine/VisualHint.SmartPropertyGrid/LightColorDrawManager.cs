using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class LightColorDrawManager : DefaultDrawManager
{
	private Color _categoryBkgColor1;

	private Color _categoryBkgColor2;

	private Color _subCategoryBkgColor1;

	private Color _subCategoryBkgColor2;

	private bool _useBoldFontForCategories;

	[DisplayName("Category left BackColor")]
	public Color CategoryBkgColor1
	{
		get
		{
			return _categoryBkgColor1;
		}
		set
		{
			_categoryBkgColor1 = value;
			Grid.Refresh();
		}
	}

	[DisplayName("Category right BackColor")]
	public Color CategoryBkgColor2
	{
		get
		{
			return _categoryBkgColor2;
		}
		set
		{
			_categoryBkgColor2 = value;
			Grid.Refresh();
		}
	}

	[DisplayName("Subcategory left BackColor")]
	public Color SubCategoryBkgColor1
	{
		get
		{
			return _subCategoryBkgColor1;
		}
		set
		{
			_subCategoryBkgColor1 = value;
			Grid.Refresh();
		}
	}

	[DisplayName("Subcategory right BackColor")]
	public Color SubCategoryBkgColor2
	{
		get
		{
			return _subCategoryBkgColor2;
		}
		set
		{
			_subCategoryBkgColor2 = value;
			Grid.Refresh();
		}
	}

	public bool UseBoldFontForCategories
	{
		get
		{
			return _useBoldFontForCategories;
		}
		set
		{
			_useBoldFontForCategories = value;
			Grid.Refresh();
		}
	}

	protected Color CategoryLeftBackgroundColor => _categoryBkgColor1;

	protected Color CategoryRightBackgroundColor => _categoryBkgColor2;

	protected Color SubCategoryLeftBackgroundColor => _subCategoryBkgColor1;

	protected Color SubCategoryRightBackgroundColor => _subCategoryBkgColor2;

	public override bool ShowCategoryVerticalLine => false;

	public LightColorDrawManager()
	{
		_categoryBkgColor1 = SystemColors.ControlDark;
		_categoryBkgColor2 = Color.White;
		_subCategoryBkgColor1 = SystemColors.Control;
		_subCategoryBkgColor2 = Color.White;
	}

	public void SetCategoryBackgroundColors(Color clr1, Color clr2)
	{
		_categoryBkgColor1 = clr1;
		_categoryBkgColor2 = clr2;
		Grid.Refresh();
	}

	public void SetSubCategoryBackgroundColors(Color clr1, Color clr2)
	{
		_subCategoryBkgColor1 = clr1;
		_subCategoryBkgColor2 = clr2;
		Grid.Refresh();
	}

	public override void DrawLeftColumnBackground(Graphics graphics, Rectangle rect, PropertyEnumerator propEnum)
	{
		rect = Grid.TransformRectIfRTL(rect);
		Rectangle rect2 = Rectangle.Intersect(rect, Rectangle.Round(graphics.ClipBounds));
		if (rect2.IsEmpty)
		{
			return;
		}
		using SolidBrush brush = new SolidBrush(Grid.GridBackColor);
		graphics.FillRectangle(brush, rect2);
	}

	public override void DrawCategoryLabelBackground(Graphics graphics, Rectangle rect, PropertyEnumerator enumSelf)
	{
		if (!Rectangle.Intersect(Grid.TransformRectIfRTL(rect), Rectangle.Round(graphics.ClipBounds)).IsEmpty)
		{
			Rectangle labelColumnRect = enumSelf.Property.GetLabelColumnRect(rect, enumSelf);
			rect.Y++;
			rect.Height -= 3;
			Rectangle rect2 = rect;
			int globalTextMargin = enumSelf.Property.ParentGrid.GlobalTextMargin;
			rect2.X = labelColumnRect.Left + globalTextMargin / 2;
			rect2.Width = rect.Right - rect2.Left;
			rect2 = Grid.TransformRectIfRTL(rect2);
			Brush brush = (enumSelf.Property.Selected ? ((Brush)new SolidBrush(Grid.GridContainsFocus ? Grid.SelectedBackColor : Grid.SelectedNotFocusedBackColor)) : ((Brush)((Grid.RightToLeft != RightToLeft.Yes) ? new LinearGradientBrush(rect2, _categoryBkgColor1, _categoryBkgColor2, 0f) : new LinearGradientBrush(rect2, _categoryBkgColor2, _categoryBkgColor1, 0f))));
			graphics.FillRectangle(brush, rect2);
			brush.Dispose();
		}
	}

	public override void DrawSubCategoryLabelBackground(Graphics graphics, int labelRectX, Rectangle rect, PropertyEnumerator enumSelf)
	{
		if (!Rectangle.Intersect(Grid.TransformRectIfRTL(rect), Rectangle.Round(graphics.ClipBounds)).IsEmpty)
		{
			Rectangle labelColumnRect = enumSelf.Property.GetLabelColumnRect(rect, enumSelf);
			rect.Y++;
			rect.Height -= 3;
			int globalTextMargin = enumSelf.Property.ParentGrid.GlobalTextMargin;
			Rectangle rect2 = rect;
			Rectangle signRect = enumSelf.Property.GetSignRect(rect, enumSelf);
			if (signRect != Rectangle.Empty)
			{
				rect2.X = signRect.Right + globalTextMargin / 2;
			}
			else
			{
				rect2.X = labelColumnRect.Left + globalTextMargin / 2;
			}
			rect2.Width = rect.Right - rect2.Left;
			rect2 = Grid.TransformRectIfRTL(rect2);
			Brush brush = (enumSelf.Property.Selected ? ((Brush)new SolidBrush(Grid.GridContainsFocus ? Grid.SelectedBackColor : Grid.SelectedNotFocusedBackColor)) : ((Brush)((Grid.RightToLeft != RightToLeft.Yes) ? new LinearGradientBrush(rect2, _subCategoryBkgColor1, _subCategoryBkgColor2, 0f) : new LinearGradientBrush(rect2, _subCategoryBkgColor2, _subCategoryBkgColor1, 0f))));
			graphics.FillRectangle(brush, rect2);
			brush.Dispose();
		}
	}

	public override void DrawPlusMinusSign(Graphics graphics, Control control, Rectangle itemRect, PropertyEnumerator propEnum)
	{
		Property property = propEnum.Property;
		Rectangle rectangle = property.GetSignRect(itemRect, propEnum);
		if (Rectangle.Intersect(Grid.TransformRectIfRTL(rectangle), Rectangle.Round(graphics.ClipBounds)).IsEmpty)
		{
			return;
		}
		using (Brush brush = new SolidBrush(propEnum.Property.ParentGrid.GridBackColor))
		{
			Rectangle rect = rectangle;
			rect.X++;
			rect.Y++;
			rect.Width--;
			rect.Height--;
			rect = Grid.TransformRectIfRTL(rect);
			graphics.FillRectangle(brush, rect);
		}
		using (Pen pen = new Pen(_categoryBkgColor1))
		{
			rectangle = Grid.TransformRectIfRTL(rectangle);
			graphics.DrawRectangle(pen, rectangle);
		}
		Point point = new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);
		using Pen pen2 = new Pen(propEnum.Property.ForeColor);
		graphics.DrawLine(pen2, point.X - 2, point.Y, point.X + 2, point.Y);
		if (!property.Expanded)
		{
			graphics.DrawLine(pen2, point.X, point.Y - 2, point.X, point.Y + 2);
		}
	}

	public override void DrawCategoryLabelText(Graphics graphics, Rectangle itemRect, Rectangle labelRect, Color textColor, PropertyEnumerator enumSelf)
	{
		Font font = (_useBoldFontForCategories ? new Font(Grid.Font, FontStyle.Bold) : Grid.Font);
		labelRect = Grid.TransformRectIfRTL(labelRect);
		Win32Calls.DrawText(graphics, enumSelf.Property.DisplayName, ref labelRect, font, textColor, 0x824 | (((Grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnLabels) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
		if (_useBoldFontForCategories)
		{
			font.Dispose();
		}
	}

	public override void DrawCategoryValue(Graphics graphics, Rectangle valueRect, Color textColor, PropertyEnumerator enumSelf)
	{
		Font font = ((!(enumSelf.Property is SubCategory) && UseBoldFontForCategories) ? new Font(Grid.Font, FontStyle.Bold) : Grid.Font);
		valueRect = Grid.TransformRectIfRTL(valueRect);
		Win32Calls.DrawText(graphics, ((RootCategory)enumSelf.Property).ValueText, ref valueRect, font, textColor, 0x824 | (((Grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnValues) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
		if (!(enumSelf.Property is SubCategory) && UseBoldFontForCategories)
		{
			font.Dispose();
		}
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

	public override void DrawButtonsBackground(Graphics graphics, Rectangle rect)
	{
		using SolidBrush brush = new SolidBrush(Grid.BackColor);
		graphics.FillRectangle(brush, rect);
	}

	public override void DrawCommentsBackground(Graphics graphics, Rectangle commentRect)
	{
		using Brush brush = new SolidBrush(Grid.CommentsBackColor);
		graphics.FillRectangle(brush, commentRect);
		commentRect.Width--;
		commentRect.Height--;
		graphics.DrawRectangle(SystemPens.GrayText, commentRect);
	}

	public override void DrawCommentsGap(Graphics graphics, Rectangle rect)
	{
		using SolidBrush brush = new SolidBrush(Grid.BackColor);
		graphics.FillRectangle(brush, rect);
	}

	public override void DrawButtonsGap(Graphics graphics, Rectangle commentRect)
	{
	}

	public override void DrawPropertyLabelBackground(Graphics graphics, Rectangle labelRect, PropertyEnumerator enumSelf)
	{
		if (!Rectangle.Intersect(Grid.TransformRectIfRTL(labelRect), Rectangle.Round(graphics.ClipBounds)).IsEmpty)
		{
			using (Brush brush = new SolidBrush(Grid.GridBackColor))
			{
				graphics.FillRectangle(brush, Grid.TransformRectIfRTL(labelRect));
			}
			Rectangle labelTextRect = enumSelf.Property.GetLabelTextRect(labelRect, enumSelf);
			int globalTextMargin = enumSelf.Property.ParentGrid.GlobalTextMargin;
			labelTextRect.X -= globalTextMargin / 2;
			labelTextRect.Width = labelRect.Right - labelTextRect.Left - 1;
			labelTextRect.Y++;
			labelTextRect.Height -= 3;
			Brush brush2;
			if (enumSelf.Property.Selected)
			{
				Color color = ((!Grid.GridContainsFocus && enumSelf.Property.InPlaceCtrlInAction == null) ? Grid.SelectedNotFocusedBackColor : Grid.SelectedBackColor);
				brush2 = new SolidBrush(color);
			}
			else
			{
				brush2 = new SolidBrush(enumSelf.Property.BackColor);
			}
			labelTextRect = Grid.TransformRectIfRTL(labelTextRect);
			graphics.FillRectangle(brush2, labelTextRect);
			brush2.Dispose();
		}
	}

	public override void DrawSeparationLines(Graphics graphics, Rectangle itemRect, Rectangle labelRect, PropertyEnumerator enumSelf)
	{
		using Pen pen = new Pen(Grid.GridColor);
		Property property = enumSelf.Property;
		if (!(property is RootCategory) || ShowCategoryVerticalLine)
		{
			graphics.DrawLine(pen, Grid.TransformPointIfRTL(labelRect.Right, labelRect.Top), Grid.TransformPointIfRTL(labelRect.Right, labelRect.Bottom));
		}
		int globalTextMargin = enumSelf.Property.ParentGrid.GlobalTextMargin;
		Rectangle manuallyDisableRect = property.GetManuallyDisableRect(itemRect, enumSelf);
		if (manuallyDisableRect != Rectangle.Empty)
		{
			graphics.DrawLine(pen, Grid.TransformPointIfRTL(manuallyDisableRect.Left - globalTextMargin, itemRect.Bottom - 1), Grid.TransformPointIfRTL(itemRect.Right, itemRect.Bottom - 1));
			return;
		}
		Rectangle imageRect = property.GetImageRect(itemRect, enumSelf);
		if (imageRect != Rectangle.Empty)
		{
			graphics.DrawLine(pen, Grid.TransformPointIfRTL(imageRect.Left - globalTextMargin, itemRect.Bottom - 1), Grid.TransformPointIfRTL(itemRect.Right, itemRect.Bottom - 1));
			return;
		}
		Rectangle labelTextRect = property.GetLabelTextRect(itemRect, enumSelf);
		graphics.DrawLine(pen, Grid.TransformPointIfRTL(labelTextRect.Left - globalTextMargin, itemRect.Bottom - 1), Grid.TransformPointIfRTL(itemRect.Right, itemRect.Bottom - 1));
	}
}
