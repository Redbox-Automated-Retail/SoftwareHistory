using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class PropertyHyperLink : Property
{
	public PropertyHyperLink(int propertyId)
		: base(propertyId, "")
	{
		base.ForeColor = Color.Blue;
	}

	internal override void OnPropertyClicked(PropertyEnumerator enumSelf, Point point, Rectangle itemRect)
	{
		if ((base.ParentGrid.Enabled || base.ParentGrid.DisableMode == PropertyGrid.DisableModes.None) && GetLabelTextRect(itemRect, enumSelf).Contains(base.ParentGrid.TransformPointIfRTL(point)))
		{
			PropertyHyperLinkClickedEventArgs e = new PropertyHyperLinkClickedEventArgs(enumSelf, base.HyperLink);
			base.ParentGrid.OnHyperLinkPropertyClicked(e);
		}
	}

	public override Rectangle GetLabelColumnRect(Rectangle itemRect, PropertyEnumerator enumerator)
	{
		Rectangle result = itemRect;
		result.X = base.ParentGrid.LeftColumnWidth + 1;
		result.Width = itemRect.Right - result.Left;
		return result;
	}

	public override Rectangle GetLabelTextRect(Rectangle itemRect, PropertyEnumerator enumSelf)
	{
		Rectangle labelColumnRect = GetLabelColumnRect(itemRect, enumSelf);
		labelColumnRect.Inflate(-base.ParentGrid.GlobalTextMargin, 0);
		Rectangle result = labelColumnRect;
		using (Font font = new Font(base.Font, FontStyle.Underline))
		{
			using Graphics graphics = base.ParentGrid.CreateGraphics();
			Size textExtent = Win32Calls.GetTextExtent(graphics, base.DisplayName, font);
			if (textExtent.Width <= labelColumnRect.Width)
			{
				result.X = labelColumnRect.Right - textExtent.Width;
				result.Width = textExtent.Width;
			}
			else
			{
				result.Width = labelColumnRect.Right - result.Left;
			}
		}
		return result;
	}

	public override bool OverrideCursor(Point mouseLocation, Rectangle itemRect, PropertyEnumerator propEnum)
	{
		if (!base.ParentGrid.Enabled && base.ParentGrid.DisableMode != PropertyGrid.DisableModes.None)
		{
			return false;
		}
		if (GetLabelTextRect(itemRect, propEnum).Contains(base.ParentGrid.TransformPointIfRTL(mouseLocation)) && base.Enabled)
		{
			base.ParentGrid.Grid.Cursor = Cursors.Hand;
			return true;
		}
		return false;
	}

	public override void Draw(Graphics graphics, Rectangle itemRect, PropertyVisibleDeepEnumerator enumSelf)
	{
		Rectangle labelColumnRect = GetLabelColumnRect(itemRect, enumSelf);
		base.ParentGrid.DrawManager.DrawPropertyLabelBackground(graphics, labelColumnRect, enumSelf);
		base.ParentGrid.DrawManager.DrawSeparationLines(graphics, itemRect, labelColumnRect, enumSelf);
		Color textColor = (base.Selected ? ((!base.ParentGrid.GridContainsFocus) ? SystemColors.WindowText : SystemColors.HighlightText) : ((!base.ParentGrid.Enabled && base.ParentGrid.DisableMode != PropertyGrid.DisableModes.None && base.ParentGrid.DisableModeGrayedOut) ? base.ParentGrid.DisabledForeColor : (base.Enabled ? base.ForeColor : base.DisabledForeColor)));
		base.ParentGrid.DrawManager.DrawPropertyHyperlinkLabelText(graphics, labelColumnRect, textColor, enumSelf);
	}
}
