using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;
using Skybound.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public class PropInPlaceButton : Control, IInPlaceControl
{
	protected PropInPlaceTextbox mEdit;

	private bool _dontTransferFocus;

	private PropertyValue.ValueValidationResult _currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;

	protected PropertyEnumerator mOwnerPropertyEnum;

	private bool _fullWidthButton;

	private bool _pushed;

	private bool _mouseOver;

	private Control _wmFocusMissingArg;

	private string _buttonText = "...";

	public PropertyEnumerator OwnerPropertyEnumerator
	{
		get
		{
			return mOwnerPropertyEnum;
		}
		set
		{
			mOwnerPropertyEnum = (PropertyEnumerator)value.Clone();
			if (mEdit != null)
			{
				mEdit.OwnerPropertyEnumerator = value;
			}
			ButtonSettingsAttribute buttonSettingsAttribute = (ButtonSettingsAttribute)mOwnerPropertyEnum.Property.Value.GetAttribute(typeof(ButtonSettingsAttribute));
			if (buttonSettingsAttribute == null && mOwnerPropertyEnum.Parent.Property != null && mOwnerPropertyEnum.Parent.Property.Value != null)
			{
				ArrayList attributes = mOwnerPropertyEnum.Parent.Property.Value.GetAttributes(typeof(ButtonSettingsAttribute));
				foreach (ButtonSettingsAttribute item in attributes)
				{
					if (item.ChildPropertyName == mOwnerPropertyEnum.Property.Name)
					{
						buttonSettingsAttribute = item;
						break;
					}
				}
			}
			if (buttonSettingsAttribute != null)
			{
				FullWidthButton = buttonSettingsAttribute.FullWidth;
				_buttonText = buttonSettingsAttribute.ButtonText;
			}
			if (!mOwnerPropertyEnum.Property.ParentGrid.IsInsideRefreshProperties)
			{
				if (!base.Visible)
				{
					_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
				}
				Text = mOwnerPropertyEnum.Property.Value.DisplayString;
			}
		}
	}

	public bool FullWidthButton
	{
		get
		{
			if (mEdit != null)
			{
				return false;
			}
			return _fullWidthButton;
		}
		set
		{
			_fullWidthButton = value;
		}
	}

	public bool ReadOnly => mOwnerPropertyEnum.Property.Value.IsReadOnly(mOwnerPropertyEnum);

	public string ButtonText
	{
		get
		{
			return _buttonText;
		}
		set
		{
			_buttonText = value;
			RepositionChildren();
		}
	}

	public override Font Font
	{
		get
		{
			return base.Font;
		}
		set
		{
			base.Font = value;
			if (mEdit != null)
			{
				mEdit.Font = value;
			}
		}
	}

	public override Color BackColor
	{
		get
		{
			return base.BackColor;
		}
		set
		{
			base.BackColor = value;
			if (mEdit != null)
			{
				mEdit.BackColor = value;
			}
		}
	}

	public override Color ForeColor
	{
		get
		{
			return base.ForeColor;
		}
		set
		{
			base.ForeColor = value;
			if (mEdit != null)
			{
				mEdit.ForeColor = value;
			}
		}
	}

	public bool RealtimeChange
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	public TextBox TextBox => mEdit;

	public PropInPlaceButton(bool editable)
	{
		if (editable)
		{
			mEdit = new PropInPlaceTextbox(isPassword: false);
			mEdit.Parent = this;
			mEdit.Text = Text;
			mEdit.TabStop = false;
		}
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | Utilities.DoubleBufferStyle | ControlStyles.Opaque | ControlStyles.ResizeRedraw, value: true);
		Cursor = Cursors.Arrow;
	}

	protected int GetButtonWidth(Graphics graphics)
	{
		if (ReadOnly)
		{
			return 0;
		}
		if (FullWidthButton)
		{
			return base.ClientSize.Width;
		}
		Graphics graphics2 = ((graphics != null) ? graphics : CreateGraphics());
		int result = Size.Round(graphics2.MeasureString((_buttonText == null) ? mOwnerPropertyEnum.Property.Value.DisplayString : _buttonText, Font)).Width + 6;
		if (graphics == null)
		{
			graphics2.Dispose();
		}
		return result;
	}

	protected Rectangle GetButtonRect(Graphics graphics)
	{
		int buttonWidth = GetButtonWidth(graphics);
		return new Rectangle(base.Width - buttonWidth, 0, buttonWidth, base.Height);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		PropertyValue value = OwnerPropertyEnumerator.Property.Value;
		mOwnerPropertyEnum.Property.ParentGrid.DrawManager.DrawPropertyValueBackground(e.Graphics, base.ClientRectangle, mOwnerPropertyEnum, PropertyValue.DrawValueIn.InPlaceCtrl);
		Rectangle buttonRect = GetButtonRect(e.Graphics);
		buttonRect = mOwnerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(buttonRect, base.ClientSize.Width);
		if (!ReadOnly)
		{
			PropertyGrid parentGrid = mOwnerPropertyEnum.Property.ParentGrid;
			parentGrid.DrawManager.DrawButton(e.Graphics, this, buttonRect, _pushed, _mouseOver, PropertyValue.DrawValueIn.InPlaceCtrl);
			if (ThemeRenderer.Enabled)
			{
				Win32Calls.DrawText(e.Graphics, (_buttonText == null) ? value.DisplayString : _buttonText, ref buttonRect, Font, parentGrid.DrawManager.GetButtonTextColor(), 37);
			}
			else
			{
				StringFormat stringFormat = (StringFormat)StringFormat.GenericDefault.Clone();
				stringFormat.Alignment = StringAlignment.Center;
				stringFormat.LineAlignment = StringAlignment.Center;
				using Brush brush = new SolidBrush(parentGrid.DrawManager.GetButtonTextColor());
				e.Graphics.DrawString((_buttonText == null) ? value.DisplayString : _buttonText, Font, brush, buttonRect, stringFormat);
			}
		}
		if (!FullWidthButton)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Width -= buttonRect.Width + 1;
			Color textColor = (mOwnerPropertyEnum.Property.Enabled ? value.ForeColor : SystemColors.GrayText);
			if (value.HasMultipleTexts() && Text.Length == 0)
			{
				value.DrawValue(e.Graphics, clientRectangle, textColor, mOwnerPropertyEnum, "", multipleTexts: true, PropertyValue.DrawValueIn.InPlaceCtrl);
			}
			else
			{
				value.DrawValue(e.Graphics, clientRectangle, textColor, mOwnerPropertyEnum, Text, multipleTexts: false, PropertyValue.DrawValueIn.InPlaceCtrl);
			}
		}
		base.OnPaint(e);
	}

	public virtual void RepositionChildren()
	{
		if (mEdit != null)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (clientRectangle.Width > 0)
			{
				int doubleword = Win32Calls.LoWord(Win32Calls.SendMessage(mEdit.Handle, 212, IntPtr.Zero, IntPtr.Zero));
				int num = Win32Calls.LoWord(doubleword) + Win32Calls.HiWord(doubleword);
				int buttonWidth = GetButtonWidth(null);
				Rectangle clientRectangle2 = base.ClientRectangle;
				clientRectangle2.Width -= buttonWidth + 1;
				clientRectangle2.Height++;
				Graphics graphics = CreateGraphics();
				clientRectangle2 = mOwnerPropertyEnum.Property.Value.GetStringValueRect(graphics, clientRectangle2, Point.Empty);
				clientRectangle2.Height--;
				int globalTextMargin = mOwnerPropertyEnum.Property.ParentGrid.GlobalTextMargin;
				int num2 = clientRectangle2.Left - globalTextMargin + ((mOwnerPropertyEnum.Property.Value.EditboxLeftMargin != -1) ? (globalTextMargin - mOwnerPropertyEnum.Property.Value.EditboxLeftMargin) : 0);
				Rectangle rect = new Rectangle(num2, 0, clientRectangle.Width - buttonWidth - num2, clientRectangle2.Height);
				rect.Y++;
				rect.Height--;
				if (rect.Width <= num)
				{
					if (mEdit.Focused)
					{
						base.Parent.Focus();
					}
					mEdit.Visible = false;
				}
				else
				{
					mEdit.Visible = true;
					Win32Calls.TEXTMETRIC tm = default(Win32Calls.TEXTMETRIC);
					Win32Calls.GetTextMetrics(graphics, Font, ref tm);
					int num3 = rect.Height - tm.tmHeight;
					rect.Y += num3 / 2;
					rect.Height -= num3 / 2;
					mEdit.Invalidate();
					mEdit.Bounds = mOwnerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(rect, base.ClientSize.Width);
				}
				graphics.Dispose();
			}
		}
		Invalidate(invalidateChildren: false);
	}

	protected override void OnMove(EventArgs e)
	{
		base.OnMove(e);
		RepositionChildren();
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		RepositionChildren();
	}

	protected override void OnGotFocus(EventArgs e)
	{
		if (mEdit != null && mEdit.Visible)
		{
			if (!_dontTransferFocus)
			{
				if (_wmFocusMissingArg != mEdit)
				{
					mEdit.Focus();
				}
				else
				{
					base.Parent.Focus();
				}
			}
		}
		else
		{
			Invalidate();
		}
		base.OnGotFocus(e);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		_pushed = false;
		base.OnLostFocus(e);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 7)
		{
			_wmFocusMissingArg = Control.FromHandle(m.WParam);
		}
		else if (m.Msg == 8)
		{
			_wmFocusMissingArg = Control.FromHandle(m.WParam);
		}
		base.WndProc(ref m);
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		if (mEdit != null && !Focused)
		{
			Win32Calls.PostMessage(mEdit.Handle, 258, (IntPtr)e.KeyChar, IntPtr.Zero);
		}
		base.OnKeyPress(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			Graphics graphics = CreateGraphics();
			if (mOwnerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(GetButtonRect(graphics), base.ClientSize.Width).Contains(new Point(e.X, e.Y)))
			{
				_pushed = true;
				base.Capture = true;
				Invalidate(invalidateChildren: false);
			}
			graphics.Dispose();
			_dontTransferFocus = true;
			Focus();
			_dontTransferFocus = false;
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		_mouseOver = false;
		Invalidate(invalidateChildren: false);
		base.OnMouseLeave(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		bool mouseOver = _mouseOver;
		Graphics graphics = CreateGraphics();
		_mouseOver = mOwnerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(GetButtonRect(graphics), base.ClientSize.Width).Contains(new Point(e.X, e.Y));
		graphics.Dispose();
		if (mouseOver != _mouseOver)
		{
			Invalidate(invalidateChildren: false);
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (_pushed && e.Button == MouseButtons.Left)
		{
			base.Capture = false;
			_pushed = false;
			_mouseOver = false;
			Invalidate(invalidateChildren: false);
			using Graphics graphics = CreateGraphics();
			if (mOwnerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(GetButtonRect(graphics), base.ClientSize.Width).Contains(new Point(e.X, e.Y)))
			{
				if (mEdit != null)
				{
					if (!PropertyValue.IsErrorCode(mEdit.CommitChanges(final: false)))
					{
						RunButton();
					}
					else
					{
						mEdit.SelectAll();
						mEdit.Focus();
					}
				}
				else
				{
					RunButton();
				}
			}
		}
		base.OnMouseUp(e);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.KeyData == Keys.Return || e.KeyData == Keys.Space)
		{
			_mouseOver = true;
			_pushed = true;
			Invalidate(invalidateChildren: false);
		}
		base.OnKeyDown(e);
	}

	protected override void OnKeyUp(KeyEventArgs e)
	{
		if ((e.KeyData == Keys.Return || e.KeyData == Keys.Space) && _pushed)
		{
			_mouseOver = false;
			_pushed = false;
			Invalidate(invalidateChildren: false);
			RunButton();
		}
		base.OnKeyUp(e);
	}

	protected override bool IsInputKey(Keys keyData)
	{
		if (keyData == Keys.Return)
		{
			return true;
		}
		return base.IsInputKey(keyData);
	}

	[UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
	protected override bool ProcessDialogKey(Keys keyData)
	{
		switch (keyData & Keys.KeyCode)
		{
		case Keys.Return:
			if (mEdit != null && mEdit.Focused)
			{
				PropertyValue.ValueValidationResult result3 = CommitChanges(final: true);
				if (PropertyValue.IsErrorCode(result3) && mOwnerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
				{
					mEdit.SelectAll();
					return true;
				}
			}
			break;
		case Keys.F4:
			if (mEdit != null && mEdit.Focused)
			{
				PropertyValue.ValueValidationResult result2 = CommitChanges(final: false);
				if (!PropertyValue.IsErrorCode(result2) || mOwnerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.IgnoreAndRestore)
				{
					OnF4();
				}
				else
				{
					mEdit.SelectAll();
				}
			}
			else
			{
				OnF4();
			}
			return true;
		case Keys.Escape:
			if (mEdit != null && mEdit.Focused)
			{
				_pushed = false;
				PropertyValue value = mOwnerPropertyEnum.Property.Value;
				Text = value.DisplayString;
				if (PropertyValue.IsErrorCode(_currentValueValidationResult))
				{
					_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
					mOwnerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, mEdit.CurrentInvalidPropertyEnum, value.GetValue(), _currentValueValidationResult));
				}
			}
			break;
		case Keys.Tab:
		{
			if (mEdit == null || ReadOnly)
			{
				break;
			}
			bool flag = (keyData & Keys.Shift) == 0;
			bool final = !flag && mOwnerPropertyEnum.Property.ParentGrid.NavigationKeyMode == PropertyGrid.NavigationKeyModes.ArrowKeys;
			PropertyValue.ValueValidationResult result = CommitChanges(final);
			if (PropertyValue.IsErrorCode(result) && mOwnerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
			{
				mEdit.SelectAll();
				return true;
			}
			if (flag && (mEdit.Focused || !base.ContainsFocus))
			{
				if (mOwnerPropertyEnum.Property.ParentGrid.NavigationKeyMode == PropertyGrid.NavigationKeyModes.ArrowKeys || (mOwnerPropertyEnum.Property.ParentGrid.TabKeyNavigationMode & PropertyGrid.TabKeyNavigationModes.TabKeyInSubControls) != PropertyGrid.TabKeyNavigationModes.NoSubMode)
				{
					OwnerPropertyEnumerator.Property.ParentGrid.SelectAndFocusProperty(OwnerPropertyEnumerator, selectAllText: false);
					_dontTransferFocus = true;
					Focus();
					Invalidate(invalidateChildren: false);
					_dontTransferFocus = false;
					return true;
				}
			}
			else if (!flag && mEdit.Focused)
			{
				if (mOwnerPropertyEnum.Property.ParentGrid.NavigationKeyMode == PropertyGrid.NavigationKeyModes.ArrowKeys)
				{
					base.Parent.Focus();
					return true;
				}
			}
			else if (!flag && Focused)
			{
				mEdit.Focus();
				mEdit.SelectAll();
				Invalidate(invalidateChildren: false);
				return true;
			}
			break;
		}
		}
		if (base.ContainsFocus)
		{
			return base.ProcessDialogKey(keyData);
		}
		return true;
	}

	protected virtual void RunButton()
	{
		base.Parent.Focus();
		PropertyButtonClickedEventArgs e = new PropertyButtonClickedEventArgs(mOwnerPropertyEnum);
		mOwnerPropertyEnum.Property.Value.PreviousValue = mOwnerPropertyEnum.Property.Value.GetValue();
		mOwnerPropertyEnum.Property.ParentGrid.OnPropertyButtonClicked(e);
		if (e.PropertyChanged)
		{
			if (mEdit != null)
			{
				mEdit.Text = mOwnerPropertyEnum.Property.Value.DisplayString;
			}
			mOwnerPropertyEnum.Property.ParentGrid.NotifyPropertyChanged(new PropertyChangedEventArgs(mOwnerPropertyEnum));
		}
		mOwnerPropertyEnum.Property.ParentGrid.OnInPlaceCtrlFinishedEdition();
	}

	public bool OnForbidClickThrough(Point pt)
	{
		if (FullWidthButton)
		{
			return false;
		}
		Graphics graphics = CreateGraphics();
		bool result = mOwnerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(GetButtonRect(graphics), base.ClientSize.Width).Contains(pt);
		graphics.Dispose();
		return result;
	}

	public void OnF4()
	{
		if (!ReadOnly)
		{
			RunButton();
		}
	}

	public bool OnParentKeyDown(KeyEventArgs e)
	{
		if (mEdit != null && mEdit.OnParentKeyDown(e))
		{
			return true;
		}
		return false;
	}

	public virtual bool GoToNextPropertyOnTab(bool forward)
	{
		if (forward)
		{
			if (mEdit == null || ReadOnly)
			{
				return true;
			}
			return Focused;
		}
		if (mEdit == null)
		{
			return true;
		}
		return mEdit.Focused;
	}

	public virtual bool ScrollValueOnDoubleClick(bool forward, ScrollValueOnDoubleClickSource source)
	{
		return false;
	}

	public virtual PropertyValue.ValueValidationResult CommitChanges(bool final)
	{
		if (mEdit != null)
		{
			_currentValueValidationResult = mEdit.CommitChanges(final);
			return _currentValueValidationResult;
		}
		_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
		return _currentValueValidationResult;
	}

	public Control GetDropDownContainer()
	{
		return null;
	}
}
