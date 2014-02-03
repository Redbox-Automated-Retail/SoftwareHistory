using System;
using System.Drawing;

namespace VisualHint.SmartPropertyGrid;

public class RootCategory : Property
{
	private string _valueText = "";

	public string ValueText
	{
		get
		{
			return _valueText;
		}
		set
		{
			_valueText = value;
		}
	}

	public RootCategory(int propertyId, string name)
		: base(propertyId, name)
	{
		base.ExpandedInternal = true;
	}

	public override void Draw(Graphics graphics, Rectangle itemRect, PropertyVisibleDeepEnumerator enumSelf)
	{
		Color textColor = ((!base.Selected) ? base.ForeColor : ((!base.ParentGrid.GridContainsFocus) ? base.ForeColor : base.ParentGrid.HighlightedTextColor));
		if (base.ParentGrid.HasOneVisibleChild(enumSelf))
		{
			base.ParentGrid.DrawManager.DrawPlusMinusSign(graphics, base.ParentGrid.Grid, itemRect, enumSelf);
		}
		Rectangle rect = itemRect;
		rect.X += base.ParentGrid.LeftColumnWidth + 1;
		rect.Width -= base.ParentGrid.LeftColumnWidth + 1;
		base.ParentGrid.DrawManager.DrawCategoryLabelBackground(graphics, rect, enumSelf);
		Rectangle labelColumnRect = GetLabelColumnRect(itemRect, enumSelf);
		base.ParentGrid.DrawManager.DrawSeparationLines(graphics, itemRect, labelColumnRect, enumSelf);
		Rectangle manuallyDisableRect = GetManuallyDisableRect(itemRect, enumSelf);
		if (manuallyDisableRect != Rectangle.Empty)
		{
			base.ParentGrid.DrawManager.DrawCheckBox(graphics, base.ParentGrid.Grid, manuallyDisableRect, GetManuallyDisabledVariable(), drawEnabled: true, PropertyValue.DrawValueIn.Grid);
		}
		Rectangle imageRect = GetImageRect(itemRect, enumSelf);
		if (imageRect != Rectangle.Empty)
		{
			base.ParentGrid.DrawImage(base.ImageIndex, graphics, imageRect.Left, imageRect.Top);
		}
		Rectangle labelTextRect = GetLabelTextRect(itemRect, enumSelf);
		base.ParentGrid.DrawManager.DrawCategoryLabelText(graphics, itemRect, labelTextRect, textColor, enumSelf);
		if (_valueText.Length > 0)
		{
			Rectangle valueRect = itemRect;
			int num = base.ParentGrid.LeftColumnWidth + 1 + base.ParentGrid.LabelColumnWidth;
			valueRect.X += num;
			valueRect.Width -= num;
			valueRect.Inflate(-base.ParentGrid.GlobalTextMargin, 0);
			base.ParentGrid.DrawManager.DrawCategoryValue(graphics, valueRect, textColor, enumSelf);
		}
	}

	public override Rectangle GetSignRect(Rectangle itemRect, PropertyEnumerator enumSelf)
	{
		if (!base.ParentGrid.HasOneVisibleChild(enumSelf))
		{
			return Rectangle.Empty;
		}
		Rectangle result = itemRect;
		result.Y += itemRect.Height / 2 - 4;
		result.Height = 8;
		result.X = base.ParentGrid.LeftColumnWidth - 3 - 8;
		result.Width = 8;
		return result;
	}

	public override Rectangle GetManuallyDisableRect(Rectangle itemRect, PropertyEnumerator enumSelf)
	{
		if (!base.CanBeDisabledManually)
		{
			return Rectangle.Empty;
		}
		Rectangle result = itemRect;
		Rectangle signRect = GetSignRect(itemRect, enumSelf);
		if (signRect != Rectangle.Empty)
		{
			result.X = base.ParentGrid.GlobalTextMargin + signRect.Right;
		}
		else
		{
			result.X = base.ParentGrid.GlobalTextMargin + base.ParentGrid.LeftColumnWidth + 1 + (enumSelf.Depth - 1) * (base.ParentGrid.GlobalTextMargin + 8);
		}
		result.Y += 2;
		result.Height = base.ParentGrid.BasicPropertyHeight - 4;
		result.Width = result.Height;
		return result;
	}

	public override Rectangle GetLabelTextRect(Rectangle itemRect, PropertyEnumerator enumSelf)
	{
		Rectangle result = itemRect;
		Rectangle imageRect = GetImageRect(itemRect, enumSelf);
		if (imageRect != Rectangle.Empty)
		{
			result.X = imageRect.Right;
		}
		else
		{
			Rectangle manuallyDisableRect = GetManuallyDisableRect(itemRect, enumSelf);
			if (manuallyDisableRect != Rectangle.Empty)
			{
				result.X = manuallyDisableRect.Right;
			}
			else
			{
				result.X = base.ParentGrid.LeftColumnWidth + 1;
			}
		}
		if (ValueText.Length > 0 || base.ParentGrid.DrawManager.ShowCategoryVerticalLine)
		{
			result.Width = base.ParentGrid.Grid.LeftColumnWidth + 1 + base.ParentGrid.LabelColumnWidth - result.Left;
		}
		else
		{
			result.Width = itemRect.Right - result.Left;
		}
		if ((base.ParentGrid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnLabels) != PropertyGrid.EllipsisModes.None)
		{
			result.Inflate(-base.ParentGrid.GlobalTextMargin, 0);
		}
		else
		{
			result.X += base.ParentGrid.GlobalTextMargin;
			result.Width -= base.ParentGrid.GlobalTextMargin + 1;
		}
		return result;
	}

	public override Rectangle GetLabelColumnRect(Rectangle itemRect, PropertyEnumerator enumerator)
	{
		Rectangle result = itemRect;
		result.X = base.ParentGrid.LeftColumnWidth + 1;
		if (ValueText.Length > 0 || base.ParentGrid.DrawManager.ShowCategoryVerticalLine)
		{
			result.Width = base.ParentGrid.Grid.LeftColumnWidth + 1 + base.ParentGrid.LabelColumnWidth - result.Left;
		}
		else
		{
			result.Width = itemRect.Right - result.Left;
		}
		return result;
	}

	public override Rectangle GetImageRect(Rectangle itemRect, PropertyEnumerator enumSelf)
	{
		if (base.ImageIndex == -1)
		{
			return Rectangle.Empty;
		}
		Rectangle result = itemRect;
		Rectangle manuallyDisableRect = GetManuallyDisableRect(itemRect, enumSelf);
		if (manuallyDisableRect != Rectangle.Empty)
		{
			result.X = manuallyDisableRect.Right + base.ParentGrid.GlobalTextMargin;
		}
		else
		{
			result.X = base.ParentGrid.LeftColumnWidth + 1 + base.ParentGrid.GlobalTextMargin;
		}
		try
		{
			Image image = base.ParentGrid.ImageList.Images[base.ImageIndex];
			result.Width = image.Width;
			result.Height = image.Height;
		}
		catch (Exception)
		{
			return Rectangle.Empty;
		}
		result.Y = itemRect.Top + (itemRect.Height - result.Height) / 2;
		return result;
	}
}
