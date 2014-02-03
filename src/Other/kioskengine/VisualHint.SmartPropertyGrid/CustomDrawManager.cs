using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Skybound.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class CustomDrawManager
{
	private PropertyGrid _grid;

	[Browsable(false)]
	public char PasswordChar
	{
		get
		{
			if (Environment.OSVersion.Version.Major <= 4)
			{
				return '*';
			}
			return '●';
		}
	}

	[Browsable(false)]
	public virtual int CommentsGapHeight => 6;

	[Browsable(false)]
	public virtual PropertyGrid Grid
	{
		get
		{
			return _grid;
		}
		set
		{
			_grid = value;
		}
	}

	[Browsable(false)]
	public virtual bool ShowCategoryVerticalLine => true;

	public virtual void DrawCommentsGap(Graphics graphics, Rectangle commentRect)
	{
	}

	public virtual void DrawCommentsBackground(Graphics graphics, Rectangle commentRect)
	{
	}

	public virtual void DrawCommentText(Graphics graphics, Rectangle rect, PropertyVisibleDeepEnumerator enumerator)
	{
		Font font = Grid.Font;
		using (Font font2 = new Font(font, FontStyle.Bold))
		{
			Win32Calls.DrawText(graphics, enumerator.Property.DisplayName, ref rect, font2, Grid.CommentsForeColor, 0x820 | ((Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
			Size textExtent = Win32Calls.GetTextExtent(graphics, enumerator.Property.DisplayName, font2);
			rect.Y += textExtent.Height + 3;
			rect.Height -= textExtent.Height + 3;
		}
		string comment = enumerator.Property.Comment;
		Win32Calls.DrawText(graphics, comment, ref rect, font, Grid.CommentsForeColor, 0x810 | ((Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
	}

	public virtual void DrawButtonsGap(Graphics graphics, Rectangle commentRect)
	{
	}

	public virtual void DrawButtonsBackground(Graphics graphics, Rectangle rect)
	{
	}

	public virtual void DrawGridBackground(Graphics graphics, Rectangle rect)
	{
	}

	public virtual Color GetValueBackgroundColor(PropertyEnumerator propEnum)
	{
		return propEnum.Property.Value.BackColor;
	}

	public virtual void DrawPropertyValueBackground(Graphics graphics, Rectangle rect, PropertyEnumerator enumerator, PropertyValue.DrawValueIn drawValueIn)
	{
		if (drawValueIn != PropertyValue.DrawValueIn.InPlaceCtrl)
		{
			rect = Grid.TransformRectIfRTL(rect);
		}
		Rectangle rect2 = Rectangle.Intersect(rect, Rectangle.Round(graphics.ClipBounds));
		if (rect2.IsEmpty)
		{
			return;
		}
		using Brush brush = new SolidBrush(GetValueBackgroundColor(enumerator));
		graphics.FillRectangle(brush, rect2);
	}

	public virtual void DrawLeftColumnBackground(Graphics graphics, Rectangle rect, PropertyEnumerator propEnum)
	{
	}

	public virtual void NcCalcSize(ref Rectangle rect)
	{
	}

	public virtual void DrawGridNonClientArea(Graphics graphics, Rectangle rect)
	{
	}

	public virtual void DrawSeparationLines(Graphics graphics, Rectangle itemRect, Rectangle labelRect, PropertyEnumerator enumSelf)
	{
		using Pen pen = new Pen(Grid.GridColor);
		Property property = enumSelf.Property;
		if (!(property is RootCategory) || ShowCategoryVerticalLine)
		{
			graphics.DrawLine(pen, Grid.TransformPointIfRTL(labelRect.Right, labelRect.Top), Grid.TransformPointIfRTL(labelRect.Right, labelRect.Bottom - 1));
		}
		graphics.DrawLine(pen, Grid.TransformPointIfRTL(itemRect.Left + Grid.LeftColumnWidth + 1, itemRect.Bottom - 1), Grid.TransformPointIfRTL(itemRect.Right, itemRect.Bottom - 1));
	}

	public virtual void DrawPlusMinusSign(Graphics graphics, Control control, Rectangle itemRect, PropertyEnumerator propEnum)
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
			signRect.Inflate(2, 2);
			int state = ((!property.Expanded) ? 1 : 2);
			ThemeTreeView.Glyph.Draw(graphics, state, signRect);
			return;
		}
		Color color = ((!property.Selected || propEnum.Depth <= 0) ? property.ForeColor : ((!Grid.GridContainsFocus && propEnum.Property.InPlaceCtrlInAction == null) ? property.ForeColor : Grid.HighlightedTextColor));
		using Pen pen = new Pen(color);
		graphics.DrawRectangle(pen, signRect);
		Point point = new Point(signRect.X + signRect.Width / 2, signRect.Y + signRect.Height / 2);
		graphics.DrawLine(pen, point.X - 2, point.Y, point.X + 2, point.Y);
		if (!property.Expanded)
		{
			graphics.DrawLine(pen, point.X, point.Y - 2, point.X, point.Y + 2);
		}
	}

	public virtual void DrawCategoryLabelBackground(Graphics graphics, Rectangle rect, PropertyEnumerator enumSelf)
	{
	}

	public virtual void DrawCategoryLabelText(Graphics graphics, Rectangle itemRect, Rectangle labelRect, Color textColor, PropertyEnumerator enumSelf)
	{
		labelRect = Grid.TransformRectIfRTL(labelRect);
		using Font font = new Font(Grid.Font, FontStyle.Bold);
		Win32Calls.DrawText(graphics, enumSelf.Property.DisplayName, ref labelRect, font, textColor, 0x824 | (((Grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnLabels) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
	}

	public virtual void DrawCategoryValue(Graphics graphics, Rectangle valueRect, Color textColor, PropertyEnumerator enumSelf)
	{
		valueRect = Grid.TransformRectIfRTL(valueRect);
		using Font font = new Font(Grid.Font, FontStyle.Bold);
		Win32Calls.DrawText(graphics, ((RootCategory)enumSelf.Property).ValueText, ref valueRect, font, textColor, 0x824 | (((_grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnValues) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
	}

	public virtual void DrawSubCategoryLabelBackground(Graphics graphics, int labelRectX, Rectangle rect, PropertyEnumerator enumSelf)
	{
	}

	public virtual void DrawSubCategoryLabelText(Graphics graphics, Rectangle itemRect, Rectangle labelRect, Color textColor, PropertyEnumerator enumSelf)
	{
		labelRect = Grid.TransformRectIfRTL(labelRect);
		Win32Calls.DrawText(graphics, enumSelf.Property.DisplayName, ref labelRect, Grid.Font, textColor, 0x824 | (((Grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnLabels) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
	}

	public virtual void DrawPropertyLabelBackground(Graphics graphics, Rectangle labelRect, PropertyEnumerator enumSelf)
	{
		labelRect = Grid.TransformRectIfRTL(labelRect);
		Rectangle rect = Rectangle.Intersect(labelRect, Rectangle.Round(graphics.ClipBounds));
		if (!rect.IsEmpty)
		{
			Brush brush;
			if (enumSelf.Property.Selected)
			{
				Color color = ((!Grid.GridContainsFocus && enumSelf.Property.InPlaceCtrlInAction == null) ? Grid.SelectedNotFocusedBackColor : Grid.SelectedBackColor);
				brush = new SolidBrush(color);
			}
			else
			{
				brush = new SolidBrush(enumSelf.Property.BackColor);
			}
			graphics.FillRectangle(brush, rect);
			brush.Dispose();
		}
	}

	public virtual void DrawPropertyLabelText(Graphics graphics, Rectangle labelColumnRect, Rectangle strRect, Color textColor, PropertyVisibleDeepEnumerator enumSelf)
	{
		strRect = Grid.TransformRectIfRTL(strRect);
		if (!Rectangle.Intersect(strRect, Rectangle.Round(graphics.ClipBounds)).IsEmpty)
		{
			strRect.Height /= enumSelf.Property.HeightMultiplier;
			Win32Calls.DrawText(graphics, enumSelf.Property.DisplayName, ref strRect, enumSelf.Property.Font, textColor, 0x824 | (((_grid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnLabels) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
		}
	}

	public virtual void DrawPropertyHyperlinkLabelText(Graphics graphics, Rectangle labelColumnRect, Color textColor, PropertyVisibleDeepEnumerator enumSelf)
	{
		labelColumnRect.X += Grid.GlobalTextMargin;
		labelColumnRect.Width -= 2 * Grid.GlobalTextMargin;
		labelColumnRect.Height--;
		labelColumnRect = Grid.TransformRectIfRTL(labelColumnRect);
		string displayName = enumSelf.Property.DisplayName;
		using Font font = new Font(enumSelf.Property.Font, FontStyle.Underline);
		if (Win32Calls.GetTextExtent(graphics, displayName, font).Width <= labelColumnRect.Width)
		{
			Win32Calls.DrawText(graphics, displayName, ref labelColumnRect, font, textColor, 0x824 | ((Grid.RightToLeft == RightToLeft.Yes) ? 131072 : 2));
		}
		else
		{
			Win32Calls.DrawText(graphics, displayName, ref labelColumnRect, font, textColor, 0x8824 | ((Grid.RightToLeft == RightToLeft.Yes) ? 131072 : 2));
		}
	}

	public virtual void DrawButton(Graphics graphics, Control control, Rectangle buttonRect, bool drawPressed, bool drawHoverEffect, PropertyValue.DrawValueIn drawValueIn)
	{
		if (drawValueIn != PropertyValue.DrawValueIn.InPlaceCtrl)
		{
			buttonRect = Grid.TransformRectIfRTL(buttonRect);
		}
		if (ThemeRenderer.Enabled)
		{
			int state = ((!drawPressed) ? ((!drawHoverEffect) ? 1 : 2) : (drawHoverEffect ? 3 : 2));
			ThemeButton.PushButton.Draw(graphics, state, buttonRect);
		}
		else
		{
			ControlPaint.DrawButton(graphics, buttonRect, (drawPressed && drawHoverEffect) ? ButtonState.Pushed : ButtonState.Normal);
		}
		if (control.Focused)
		{
			Rectangle rectangle = buttonRect;
			rectangle.Inflate(-3, -3);
			ControlPaint.DrawFocusRectangle(graphics, rectangle);
		}
	}

	public virtual Color GetButtonTextColor()
	{
		return Color.Black;
	}

	public virtual void DrawDropDownButton(Graphics graphics, Control control, bool drawDown, Rectangle buttonRect, bool drawPressed, bool drawHoverEffect, PropertyValue.DrawValueIn drawValueIn)
	{
		if (ThemeRenderer.Enabled)
		{
			int state = ((!drawPressed) ? ((!drawHoverEffect) ? 1 : 2) : (drawHoverEffect ? 3 : 2));
			ThemeComboBox.DropDownButton.Draw(graphics, state, buttonRect);
		}
		else
		{
			ControlPaint.DrawComboButton(graphics, buttonRect, (drawPressed && drawHoverEffect) ? ButtonState.Pushed : ButtonState.Normal);
		}
		if (control.Focused)
		{
			Rectangle rectangle = buttonRect;
			rectangle.Inflate(-3, -3);
			ControlPaint.DrawFocusRectangle(graphics, rectangle);
		}
	}

	public virtual void DrawUpDownButton(Graphics graphics, Control control, Rectangle buttonRect, ButtonID pushedButton, ButtonID overButton)
	{
		int num = (int)Math.Round((double)buttonRect.Height / 2.0);
		if (ThemeRenderer.Enabled)
		{
			int state = ((pushedButton == ButtonID.Up) ? 3 : ((overButton != ButtonID.Up) ? 1 : 2));
			ThemeSpin.Up.Draw(graphics, state, new Rectangle(buttonRect.Left, 0, buttonRect.Width, num));
			state = ((pushedButton == ButtonID.Down) ? 3 : ((overButton != ButtonID.Down) ? 1 : 2));
			ThemeSpin.Down.Draw(graphics, state, new Rectangle(buttonRect.Left, num, buttonRect.Width, num));
		}
		else
		{
			ControlPaint.DrawScrollButton(graphics, new Rectangle(buttonRect.Left, 0, buttonRect.Width, num), ScrollButton.Up, (pushedButton == ButtonID.Up) ? ButtonState.Pushed : ButtonState.Normal);
			ControlPaint.DrawScrollButton(graphics, new Rectangle(buttonRect.Left, num, buttonRect.Width, num), ScrollButton.Down, (pushedButton == ButtonID.Down) ? ButtonState.Pushed : ButtonState.Normal);
		}
	}

	public virtual void DrawRadioButton(Graphics graphics, Control control, Rectangle radioRect, bool drawChecked, bool drawEnabled, PropertyValue.DrawValueIn drawValueIn)
	{
		if (drawValueIn != PropertyValue.DrawValueIn.InPlaceCtrl)
		{
			radioRect = Grid.TransformRectIfRTL(radioRect);
		}
		if (ThemeRenderer.Enabled)
		{
			int state = ((!drawEnabled) ? (drawChecked ? 8 : 4) : ((!drawChecked) ? 1 : 5));
			ThemeButton.RadioButton.Draw(graphics, state, radioRect);
		}
		else
		{
			ButtonState buttonState = (drawChecked ? ButtonState.Checked : ButtonState.Normal);
			buttonState = (ButtonState)((int)buttonState | ((!drawEnabled) ? 256 : 0));
			ControlPaint.DrawRadioButton(graphics, radioRect, buttonState);
		}
	}

	public virtual void DrawCheckBox(Graphics graphics, Control control, Rectangle buttonRect, bool drawChecked, bool drawEnabled, PropertyValue.DrawValueIn drawValueIn)
	{
		if (drawValueIn != PropertyValue.DrawValueIn.InPlaceCtrl)
		{
			buttonRect = Grid.TransformRectIfRTL(buttonRect);
		}
		if (Rectangle.Intersect(buttonRect, Rectangle.Round(graphics.ClipBounds)).IsEmpty)
		{
			return;
		}
		if (ThemeRenderer.Enabled)
		{
			int state = ((!drawEnabled) ? (drawChecked ? 8 : 4) : ((!drawChecked) ? 1 : 5));
			ThemeButton.CheckBox.Draw(graphics, state, buttonRect);
			return;
		}
		ButtonState buttonState = ButtonState.Normal;
		if (drawChecked)
		{
			buttonState |= ButtonState.Checked;
		}
		if (!drawEnabled)
		{
			buttonState |= ButtonState.Inactive;
		}
		ControlPaint.DrawCheckBox(graphics, buttonRect, buttonState);
	}

	public virtual void DrawCheckBoxIndeterminate(Graphics graphics, Control control, Rectangle buttonRect, bool drawEnabled, PropertyValue.DrawValueIn drawValueIn)
	{
		if (drawValueIn != PropertyValue.DrawValueIn.InPlaceCtrl)
		{
			buttonRect = Grid.TransformRectIfRTL(buttonRect);
		}
		if (!Rectangle.Intersect(buttonRect, Rectangle.Round(graphics.ClipBounds)).IsEmpty)
		{
			if (ThemeRenderer.Enabled)
			{
				int state = (drawEnabled ? 9 : 12);
				ThemeButton.CheckBox.Draw(graphics, state, buttonRect);
			}
			else
			{
				ControlPaint.DrawCheckBox(graphics, buttonRect, ButtonState.Inactive);
			}
		}
	}

	public virtual void DrawProgressBar(Graphics graphics, Control control, Rectangle valueRect, bool drawFrame, object value, object minValue, object maxValue)
	{
		decimal num = Convert.ToDecimal(minValue);
		decimal num2 = Convert.ToDecimal(maxValue);
		decimal num3 = Convert.ToDecimal(value);
		if (ThemeRenderer.Enabled)
		{
			if (drawFrame)
			{
				Rectangle rectangle = Grid.TransformRectIfRTL(valueRect);
				ThemeRenderer bar = ThemeProgress.Bar;
				bar.Draw(graphics, 0, rectangle);
				IntPtr hdc = graphics.GetHdc();
				valueRect = bar.GetBackgroundContentRectangle(hdc, 0, Grid.TransformRectIfRTL(rectangle));
				graphics.ReleaseHdc(hdc);
			}
			valueRect.Width = (int)Math.Round((num3 - num) / (num2 - num) * (decimal)valueRect.Width);
			valueRect = Grid.TransformRectIfRTL(valueRect);
			ThemeProgress.Chunk.Draw(graphics, 0, valueRect);
			return;
		}
		Rectangle rect;
		if (drawFrame)
		{
			rect = Rectangle.Inflate(valueRect, -2, -2);
		}
		else
		{
			valueRect.Width -= 2;
			rect = Rectangle.Inflate(valueRect, 0, -2);
		}
		int num4 = rect.Right - 1;
		int num5 = (int)Math.Round((double)valueRect.Height * 0.6666);
		int num6 = (int)Math.Ceiling((decimal)rect.Width / (decimal)num5 * num3 / (num2 - num));
		Brush highlight = SystemBrushes.Highlight;
		for (int i = 0; i < num6; i++)
		{
			rect.Width = Math.Min(num4 - rect.X, num5 - 2);
			graphics.FillRectangle(highlight, Grid.TransformRectIfRTL(rect));
			rect.X += num5;
		}
		if (drawFrame)
		{
			ControlPaint.DrawBorder3D(graphics, Grid.TransformRectIfRTL(valueRect), Border3DStyle.Flat);
		}
	}

	public virtual string GetDisplayName(Property property, string displayName)
	{
		return displayName;
	}

	public virtual Font GetPropertyLabelFont(Property property)
	{
		return property.Font;
	}
}
