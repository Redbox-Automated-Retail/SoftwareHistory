using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public class PropInPlaceUnit : Control, IInPlaceControl, IMouseHookListener
{
	private PropInPlaceTextbox _edit;

	private bool _dontTransferFocus;

	private PropertyValue.ValueValidationResult _currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;

	private PropertyEnumerator _currentInvalidPropertyEnum;

	private PropertyEnumerator _ownerPropertyEnum;

	private bool _realtimeChange = true;

	private bool _pushed;

	private bool _mouseOver;

	private Control _wmFocusMissingArg;

	private PropInPlaceContainer _editorContainer = new PropInPlaceContainer();

	private string _listText;

	private bool _clickedWhileDroppedDown;

	public PropertyEnumerator OwnerPropertyEnumerator
	{
		get
		{
			return _ownerPropertyEnum;
		}
		set
		{
			_ownerPropertyEnum = (PropertyEnumerator)value.Clone();
			if (_edit != null)
			{
				_edit.OwnerPropertyEnumerator = value;
			}
			if (!_ownerPropertyEnum.Property.ParentGrid.IsInsideRefreshProperties)
			{
				if (!base.Visible)
				{
					_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
				}
				Text = _ownerPropertyEnum.Property.Value.DisplayString;
				PropertyValue value2 = _ownerPropertyEnum.Property.GetValue("unit");
				_listText = value2.DisplayString;
				_editorContainer.Visible = false;
			}
		}
	}

	public bool ReadOnly => _ownerPropertyEnum.Property.Value.IsReadOnly(_ownerPropertyEnum);

	public bool RealtimeChange
	{
		get
		{
			return _realtimeChange;
		}
		set
		{
			_realtimeChange = value;
		}
	}

	public override string Text
	{
		get
		{
			return _edit.Text;
		}
		set
		{
			_edit.Text = value;
		}
	}

	public string ListText
	{
		get
		{
			return _listText;
		}
		set
		{
			_listText = value;
			Invalidate(invalidateChildren: false);
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
			if (_edit != null)
			{
				_edit.Font = value;
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
			if (_edit != null)
			{
				_edit.BackColor = value;
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
			if (_edit != null)
			{
				_edit.ForeColor = value;
			}
		}
	}

	public TextBox TextBox => _edit;

	public PropInPlaceUnit()
	{
		_edit = new PropInPlaceTextbox(isPassword: false);
		_edit.Parent = this;
		_edit.TabStop = false;
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | Utilities.DoubleBufferStyle | ControlStyles.Opaque | ControlStyles.ResizeRedraw, value: true);
		Cursor = Cursors.Arrow;
	}

	protected int GetButtonWidth()
	{
		if (ReadOnly)
		{
			return 0;
		}
		PropertyValue value = _ownerPropertyEnum.Property.GetValue("unit");
		string[] displayedValues = value.GetDisplayedValues();
		if (displayedValues.Length <= 1)
		{
			return 0;
		}
		return SystemInformation.VerticalScrollBarWidth;
	}

	protected Rectangle GetButtonRect()
	{
		if (ReadOnly)
		{
			return Rectangle.Empty;
		}
		PropertyValue value = _ownerPropertyEnum.Property.GetValue("unit");
		string[] displayedValues = value.GetDisplayedValues();
		if (displayedValues.Length <= 1)
		{
			return Rectangle.Empty;
		}
		int verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
		return new Rectangle(base.Width - verticalScrollBarWidth, 0, verticalScrollBarWidth, base.Height);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		PropertyGrid parentGrid = _ownerPropertyEnum.Property.ParentGrid;
		PropertyValue value = _ownerPropertyEnum.Property.Value;
		parentGrid.DrawManager.DrawPropertyValueBackground(e.Graphics, base.ClientRectangle, _ownerPropertyEnum, PropertyValue.DrawValueIn.InPlaceCtrl);
		Rectangle buttonRect = GetButtonRect();
		int listboxWidth = GetListboxWidth();
		Color color = ((!_ownerPropertyEnum.Property.Enabled) ? SystemColors.GrayText : ((!Focused) ? value.ForeColor : parentGrid.HighlightedTextColor));
		if (!ReadOnly && !buttonRect.IsEmpty)
		{
			parentGrid.DrawManager.DrawDropDownButton(e.Graphics, this, drawDown: true, parentGrid.TransformRectIfRTL(buttonRect, base.ClientSize.Width), _pushed, _mouseOver, PropertyValue.DrawValueIn.InPlaceCtrl);
		}
		Rectangle clientRectangle = base.ClientRectangle;
		clientRectangle.X = clientRectangle.Right - listboxWidth;
		clientRectangle.Width = listboxWidth - GetButtonWidth();
		Rectangle rect = clientRectangle;
		rect.Inflate(0, -1);
		rect.Width--;
		rect = parentGrid.TransformRectIfRTL(rect, base.ClientSize.Width);
		if (Focused)
		{
			using (Brush brush = new SolidBrush(parentGrid.SelectedBackColor))
			{
				e.Graphics.FillRectangle(brush, rect);
			}
			ControlPaint.DrawFocusRectangle(e.Graphics, rect);
		}
		else
		{
			using Brush brush2 = new SolidBrush(parentGrid.SelectedNotFocusedBackColor);
			e.Graphics.FillRectangle(brush2, rect);
		}
		Rectangle rect2 = clientRectangle;
		int globalTextMargin = parentGrid.GlobalTextMargin;
		rect2.X += globalTextMargin;
		rect2.Width = rect2.Right - rect2.Left - globalTextMargin;
		rect2.Y++;
		rect2.Height--;
		rect2 = parentGrid.TransformRectIfRTL(rect2, base.ClientSize.Width);
		Win32Calls.DrawText(e.Graphics, ListText, ref rect2, Font, color, 0x824 | ((parentGrid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
		Rectangle clientRectangle2 = base.ClientRectangle;
		clientRectangle2.Width -= listboxWidth;
		if (value.HasMultipleTexts() && Text.Length == 0)
		{
			_ownerPropertyEnum.Property.Value.DrawValue(e.Graphics, clientRectangle2, color, _ownerPropertyEnum, "", multipleTexts: true, PropertyValue.DrawValueIn.InPlaceCtrl);
		}
		else
		{
			_ownerPropertyEnum.Property.Value.DrawValue(e.Graphics, clientRectangle2, color, _ownerPropertyEnum, Text, multipleTexts: false, PropertyValue.DrawValueIn.InPlaceCtrl);
		}
		base.OnPaint(e);
	}

	public bool OnForbidClickThrough(Point pt)
	{
		Rectangle clientRectangle = base.ClientRectangle;
		int listboxWidth = GetListboxWidth();
		clientRectangle.X = clientRectangle.Right - listboxWidth;
		clientRectangle.Width = listboxWidth;
		return _ownerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(clientRectangle, base.ClientSize.Width).Contains(pt);
	}

	public int GetListboxWidth()
	{
		PropertyValue value = _ownerPropertyEnum.Property.GetValue("unit");
		string[] displayedValues = value.GetDisplayedValues();
		Graphics graphics = CreateGraphics();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		if (displayedValues.Length > 0)
		{
			string[] array = displayedValues;
			foreach (string text in array)
			{
				Size textExtent = Win32Calls.GetTextExtent(graphics, text, Font);
				if (textExtent.Width > num)
				{
					num = textExtent.Width;
				}
			}
			if (displayedValues.Length > 1)
			{
				num3 = GetButtonWidth();
			}
			num2 = num + 2 * _ownerPropertyEnum.Property.ParentGrid.GlobalTextMargin + num3;
		}
		else
		{
			num2 = Win32Calls.GetTextExtent(graphics, ListText, Font).Width + 2 * _ownerPropertyEnum.Property.ParentGrid.GlobalTextMargin;
		}
		graphics.Dispose();
		if (num2 > base.ClientRectangle.Width - num3 - 20)
		{
			num2 = base.ClientRectangle.Width - num3 - 20;
		}
		if (num2 < num3)
		{
			num2 = num3;
		}
		return num2;
	}

	protected override void OnMove(EventArgs e)
	{
		base.OnMove(e);
		OnSizeChanged(e);
	}

	public virtual void RepositionChildren()
	{
		if (_edit != null)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (clientRectangle.Width > 0)
			{
				int doubleword = Win32Calls.LoWord(Win32Calls.SendMessage(_edit.Handle, 212, IntPtr.Zero, IntPtr.Zero));
				int num = Win32Calls.LoWord(doubleword) + Win32Calls.HiWord(doubleword);
				int listboxWidth = GetListboxWidth();
				Rectangle clientRectangle2 = base.ClientRectangle;
				clientRectangle2.Width -= listboxWidth + 1;
				clientRectangle2.Height++;
				Graphics graphics = CreateGraphics();
				clientRectangle2 = _ownerPropertyEnum.Property.Value.GetStringValueRect(graphics, clientRectangle2, Point.Empty);
				clientRectangle2.Height--;
				int globalTextMargin = _ownerPropertyEnum.Property.ParentGrid.GlobalTextMargin;
				int num2 = clientRectangle2.Left - globalTextMargin + ((_ownerPropertyEnum.Property.Value.EditboxLeftMargin != -1) ? (globalTextMargin - _ownerPropertyEnum.Property.Value.EditboxLeftMargin) : 0);
				Rectangle rect = new Rectangle(num2, 0, clientRectangle.Width - listboxWidth - num2, clientRectangle2.Height);
				rect.Y++;
				rect.Height--;
				if (rect.Width <= num)
				{
					if (_edit.Focused)
					{
						base.Parent.Focus();
					}
					_edit.Visible = false;
				}
				else
				{
					_edit.Visible = true;
					Win32Calls.TEXTMETRIC tm = default(Win32Calls.TEXTMETRIC);
					Win32Calls.GetTextMetrics(graphics, Font, ref tm);
					int num3 = rect.Height - tm.tmHeight;
					rect.Y += num3 / 2;
					rect.Height -= num3 / 2;
					rect = _ownerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(rect, base.ClientSize.Width);
					_edit.Invalidate();
					_edit.Bounds = rect;
				}
				graphics.Dispose();
			}
		}
		Invalidate(invalidateChildren: false);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		RepositionChildren();
	}

	protected override void OnGotFocus(EventArgs e)
	{
		if (_edit != null)
		{
			if (!_dontTransferFocus)
			{
				if (_wmFocusMissingArg != _edit)
				{
					if (_wmFocusMissingArg != _editorContainer.ContentControl)
					{
						_edit.Focus();
					}
				}
				else if (_ownerPropertyEnum.Property.ParentGrid.NavigationKeyMode == PropertyGrid.NavigationKeyModes.ArrowKeys || (_ownerPropertyEnum.Property.ParentGrid.TabKeyNavigationMode & PropertyGrid.TabKeyNavigationModes.TabKeyInSubControls) == 0)
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
		_mouseOver = false;
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
		else if (m.Msg == 522)
		{
			Point p = new Point(Win32Calls.LoWord((int)m.LParam), Win32Calls.HiWord((int)m.LParam));
			p = PointToClient(p);
			OnMouseWheel(new MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, Win32Calls.HiWord((int)m.WParam)));
			m.Result = IntPtr.Zero;
			return;
		}
		base.WndProc(ref m);
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		if (_edit != null)
		{
			if (Focused)
			{
				PropertyValue value = _ownerPropertyEnum.Property.GetValue("unit");
				string[] displayedValues = value.GetDisplayedValues();
				if (displayedValues.Length <= 1)
				{
					return;
				}
				char[] value2 = new char[1] { e.KeyChar };
				string strB = new string(value2);
				for (int i = 0; i < displayedValues.Length; i++)
				{
					if (!value.DisplayString.Equals(displayedValues[i]))
					{
						continue;
					}
					for (int j = 0; j < displayedValues.Length; j++)
					{
						string text = displayedValues[(i + j + 1) % displayedValues.Length];
						if (text.Length > 0 && string.Compare(text.Substring(0, 1), strB, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
						{
							ListText = text;
							if (RealtimeChange)
							{
								CommitListChanges();
							}
							break;
						}
					}
					break;
				}
			}
			else
			{
				Win32Calls.PostMessage(_edit.Handle, 258, (IntPtr)e.KeyChar, IntPtr.Zero);
			}
		}
		else
		{
			if (_edit != null)
			{
				return;
			}
			string[] displayedValues2 = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
			if (displayedValues2.Length <= 1)
			{
				return;
			}
			char[] value3 = new char[1] { e.KeyChar };
			string strB2 = new string(value3);
			for (int k = 0; k < displayedValues2.Length; k++)
			{
				if (!Text.Equals(displayedValues2[k]))
				{
					continue;
				}
				for (int l = 0; l < displayedValues2.Length; l++)
				{
					string text2 = displayedValues2[(k + l + 1) % displayedValues2.Length];
					if (text2.Length > 0 && string.Compare(text2.Substring(0, 1), strB2, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
					{
						Text = text2;
						CommitChanges(final: false);
						break;
					}
				}
				break;
			}
		}
	}

	[CLSCompliant(false)]
	public bool OnListenMouseHook(Control control, uint message, Point pt, uint hitTest)
	{
		_clickedWhileDroppedDown = true;
		return false;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			Rectangle rectangle = _ownerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(GetButtonRect(), base.ClientSize.Width);
			if (e.Clicks == 2 && !rectangle.Contains(new Point(e.X, e.Y)))
			{
				ScrollValueOnDoubleClick(Control.ModifierKeys != Keys.Shift, ScrollValueOnDoubleClickSource.FromInPlaceCtrl);
			}
			else if (!_clickedWhileDroppedDown)
			{
				if (RealtimeChange)
				{
					PropertyValue.ValueValidationResult result = CommitChanges(final: false);
					if (PropertyValue.IsErrorCode(result) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
					{
						_edit.Focus();
						_edit.SelectAll();
						return;
					}
				}
				if (rectangle.Contains(new Point(e.X, e.Y)))
				{
					_pushed = true;
					_mouseOver = true;
					base.Capture = true;
				}
				_dontTransferFocus = true;
				Focus();
				_dontTransferFocus = false;
				Invalidate(invalidateChildren: false);
				Update();
			}
		}
		_clickedWhileDroppedDown = false;
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
		_mouseOver = _ownerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(GetButtonRect(), base.ClientSize.Width).Contains(new Point(e.X, e.Y));
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
			if (_ownerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(GetButtonRect(), base.ClientSize.Width).Contains(new Point(e.X, e.Y)))
			{
				RunButton();
			}
		}
		base.OnMouseUp(e);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Space)
		{
			_mouseOver = true;
			_pushed = true;
			Invalidate(invalidateChildren: false);
		}
		else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
		{
			ProcessArrowKey(e.KeyCode, rotate: false);
			Invalidate();
		}
	}

	protected override void OnKeyUp(KeyEventArgs e)
	{
		if (e.KeyData == Keys.Space)
		{
			_mouseOver = false;
			_pushed = false;
			Invalidate(invalidateChildren: false);
			RunButton();
		}
	}

	protected void ProcessArrowKey(Keys key, bool rotate)
	{
		PropertyValue value = _ownerPropertyEnum.Property.GetValue("unit");
		object[] displayedValues = value.GetDisplayedValues();
		if (displayedValues.Length <= 1)
		{
			return;
		}
		int num = -1;
		if (value.HasMultipleTexts())
		{
			switch (key)
			{
			case Keys.Up:
				num = displayedValues.Length - 1;
				break;
			case Keys.Down:
				num = 0;
				break;
			}
		}
		else
		{
			for (int i = 0; i < displayedValues.Length; i++)
			{
				if (ListText.Equals(displayedValues[i]))
				{
					if (key == Keys.Up && i > 0)
					{
						num = i - 1;
					}
					else if (key == Keys.Up && rotate)
					{
						num = displayedValues.Length - 1;
					}
					else if (key == Keys.Down && i < displayedValues.Length - 1)
					{
						num = i + 1;
					}
					else if (key == Keys.Down && rotate)
					{
						num = 0;
					}
					break;
				}
			}
		}
		if (num != -1)
		{
			ListText = displayedValues[num].ToString();
			if (RealtimeChange)
			{
				CommitListChanges();
			}
		}
	}

	protected override bool IsInputKey(Keys keyData)
	{
		if (keyData == Keys.Up || keyData == Keys.Down)
		{
			return true;
		}
		return base.IsInputKey(keyData);
	}

	[UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
	protected override bool ProcessDialogKey(Keys keyData)
	{
		Keys keys = keyData & Keys.KeyCode;
		if (keys == Keys.Return)
		{
			if (Focused)
			{
				_pushed = false;
				_mouseOver = false;
				if (!RealtimeChange)
				{
					CommitListChanges();
				}
			}
			else
			{
				PropertyValue.ValueValidationResult result = CommitChanges(final: true);
				if (PropertyValue.IsErrorCode(result) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus && _edit != null)
				{
					_edit.SelectAll();
					return true;
				}
			}
		}
		else if (keys == Keys.Escape)
		{
			if (Focused)
			{
				_pushed = false;
				_mouseOver = false;
				PropertyValue value = _ownerPropertyEnum.Property.GetValue("unit");
				ListText = value.DisplayString;
				if (RealtimeChange)
				{
					CommitListChanges();
				}
			}
			else
			{
				Text = _ownerPropertyEnum.Property.Value.DisplayString;
				CommitChanges(final: true);
			}
		}
		else
		{
			if (keys == Keys.F4 || (keys == Keys.Down && (keyData & Keys.Alt) != Keys.None))
			{
				_pushed = false;
				_mouseOver = false;
				if (_edit != null && _edit.Focused)
				{
					PropertyValue.ValueValidationResult result2 = CommitChanges(final: false);
					if (!PropertyValue.IsErrorCode(result2) || _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.IgnoreAndRestore)
					{
						OnF4();
					}
					else
					{
						_edit.SelectAll();
					}
				}
				else
				{
					OnF4();
				}
				return true;
			}
			if (keys == Keys.Tab)
			{
				PropertyValue.ValueValidationResult result3 = CommitChanges(Focused);
				if (PropertyValue.IsErrorCode(result3) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
				{
					if (_edit != null)
					{
						_edit.SelectAll();
					}
					return true;
				}
				if (_edit != null)
				{
					bool flag = (keyData & Keys.Shift) == 0;
					if (flag && _edit.Focused)
					{
						if (_ownerPropertyEnum.Property.ParentGrid.NavigationKeyMode == PropertyGrid.NavigationKeyModes.ArrowKeys || (_ownerPropertyEnum.Property.ParentGrid.TabKeyNavigationMode & PropertyGrid.TabKeyNavigationModes.TabKeyInSubControls) != PropertyGrid.TabKeyNavigationModes.NoSubMode)
						{
							OwnerPropertyEnumerator.Property.ParentGrid.SelectAndFocusProperty(OwnerPropertyEnumerator, selectAllText: false);
							_dontTransferFocus = true;
							Focus();
							Invalidate(invalidateChildren: false);
							_dontTransferFocus = false;
							return true;
						}
					}
					else if (!flag && _edit.Focused)
					{
						if (_ownerPropertyEnum.Property.ParentGrid.NavigationKeyMode == PropertyGrid.NavigationKeyModes.ArrowKeys)
						{
							base.Parent.Focus();
							return true;
						}
					}
					else if (!flag && Focused)
					{
						_edit.Focus();
						_edit.SelectAll();
						Invalidate(invalidateChildren: false);
						return true;
					}
				}
			}
		}
		if (base.ContainsFocus)
		{
			return base.ProcessDialogKey(keyData);
		}
		return true;
	}

	protected void RunButton()
	{
		if (ReadOnly)
		{
			return;
		}
		_ownerPropertyEnum.Property.InPlaceCtrlInAction = this;
		PropertyValue value = _ownerPropertyEnum.Property.GetValue("unit");
		_editorContainer.SetContent(_ownerPropertyEnum, value.DisplayString, "unit");
		if (_editorContainer.ContentControl != null)
		{
			_editorContainer.MustBeClosed += OnDropDownContentMustBeClosed;
			Win32Calls.SetWindowLongPtr(_editorContainer.Handle, -8, base.Parent.Handle);
			Rectangle workingArea = Screen.FromControl(this).WorkingArea;
			Point point = base.Parent.PointToScreen(new Point(0, 0));
			Rectangle bounds = base.Bounds;
			if (_editorContainer.Width < base.Size.Width)
			{
				_editorContainer.Width = base.Size.Width;
			}
			Point location = new Point(bounds.Right - _editorContainer.Size.Width + point.X, bounds.Bottom + 1 + point.Y);
			if (location.X < workingArea.Left)
			{
				location.X = workingArea.Left;
			}
			if (location.Y + _editorContainer.Height > workingArea.Bottom)
			{
				location.Y = bounds.Top - _editorContainer.Height + point.Y;
			}
			_editorContainer.Location = location;
			_editorContainer.ContentControl.Dock = DockStyle.Fill;
			Win32Calls.ShowWindow(new HandleRef(_editorContainer, _editorContainer.Handle), 8);
			_editorContainer.Visible = true;
			_ownerPropertyEnum.Property.ParentGrid.SetDropDownTopWindow(_editorContainer);
			_editorContainer.DoModalLoop();
		}
	}

	private void OnDropDownContentMustBeClosed(object sender, EventArgs e)
	{
		DropDownContentClosedEventArgs e2 = e as DropDownContentClosedEventArgs;
		if (e2.ReasonLostFocus)
		{
			_wmFocusMissingArg = e2.WndTakingFocus;
		}
		object value = _editorContainer.DropDownContent.GetValue();
		if (e2.ToBeValidated && value is string)
		{
			ListText = value.ToString();
			CommitListChanges();
		}
		HideDropDownContent();
	}

	protected void HideDropDownContent()
	{
		_editorContainer.MustBeClosed -= OnDropDownContentMustBeClosed;
		_ownerPropertyEnum.Property.InPlaceCtrlInAction = null;
		_editorContainer.Visible = false;
		_editorContainer.SetContent(null, null);
		_ownerPropertyEnum.Property.ParentGrid.SetDropDownTopWindow(null);
		_ownerPropertyEnum.Property.ParentGrid.OnInPlaceCtrlFinishedEdition();
	}

	public void OnF4()
	{
		if (!ReadOnly)
		{
			_dontTransferFocus = true;
		}
		Focus();
		_dontTransferFocus = false;
		RunButton();
	}

	public bool OnParentKeyDown(KeyEventArgs e)
	{
		if (_edit != null && _edit.OnParentKeyDown(e))
		{
			return true;
		}
		return false;
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		if (e.Delta > 0)
		{
			ProcessArrowKey(Keys.Up, rotate: false);
		}
		else
		{
			ProcessArrowKey(Keys.Down, rotate: false);
		}
		Invalidate();
	}

	public virtual bool GoToNextPropertyOnTab(bool forward)
	{
		if (forward)
		{
			if (_edit == null)
			{
				return true;
			}
			return Focused;
		}
		if (_edit == null)
		{
			return true;
		}
		return _edit.Focused;
	}

	public virtual bool ScrollValueOnDoubleClick(bool forward, ScrollValueOnDoubleClickSource source)
	{
		if (ReadOnly || source != ScrollValueOnDoubleClickSource.FromInPlaceCtrl)
		{
			return false;
		}
		if (forward)
		{
			ProcessArrowKey(Keys.Down, rotate: true);
		}
		else
		{
			ProcessArrowKey(Keys.Up, rotate: true);
		}
		return true;
	}

	public void CommitListChanges()
	{
		PropertyValue value = _ownerPropertyEnum.Property.GetValue("unit");
		string displayString = value.DisplayString;
		if (!ListText.Equals(displayString))
		{
			value.PreviousValue = value.GetValue();
			object value2 = value.ConvertDisplayedStringToValue(ListText);
			value.SetValueFromInPlaceCtrl(value2, "unit");
			value = _ownerPropertyEnum.Property.GetValue("unit");
			if (value.DisplayString != displayString)
			{
				_ownerPropertyEnum.Property.ParentGrid.NotifyPropertyChanged(new PropertyChangedEventArgs(_ownerPropertyEnum, value.AdditionalValueKey));
			}
		}
	}

	public virtual PropertyValue.ValueValidationResult CommitChanges(bool final)
	{
		PropertyValue.ValueValidationResult valueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
		object obj = null;
		try
		{
			_ownerPropertyEnum.Property.ParentGrid.NotifyBeginCommitChanges(new CommitChangesEventArgs(_ownerPropertyEnum, Text, valueIsText: true));
			object obj2 = null;
			if (!_ownerPropertyEnum.Property.Value.HasMultipleValues)
			{
				obj2 = _ownerPropertyEnum.Property.Value.GetValue();
			}
			try
			{
				if (!_ownerPropertyEnum.Property.Value.HasMultipleValues || Text.Length != 0)
				{
					obj = _ownerPropertyEnum.Property.Value.ConvertDisplayedStringToValue(Text);
				}
			}
			catch (Exception e)
			{
				_currentInvalidPropertyEnum = OwnerPropertyEnumerator;
				_currentValueValidationResult = PropertyValue.ValueValidationResult.TypeConverterFailed;
				_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, Text, PropertyValue.ValueValidationResult.TypeConverterFailed, e));
				if (_ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.IgnoreAndRestore)
				{
					Text = _ownerPropertyEnum.Property.Value.DisplayString;
					_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
					_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, _ownerPropertyEnum.Property.Value.GetValue(), _currentValueValidationResult));
				}
				return _currentValueValidationResult;
			}
			if ((obj != null && (obj2 == null || !obj.Equals(obj2))) || (obj == null && obj2 != null))
			{
				_ownerPropertyEnum.Property.ParentGrid.BeginUpdate();
				_ownerPropertyEnum.Property.Value.PreviousValue = _ownerPropertyEnum.Property.Value.GetValue();
				valueValidationResult = _ownerPropertyEnum.Property.Value.SetValueFromInPlaceCtrl(obj, out var invalidPropEnum, null);
				if (PropertyValue.IsErrorCode(valueValidationResult))
				{
					_currentInvalidPropertyEnum = invalidPropEnum;
					if (valueValidationResult == PropertyValue.ValueValidationResult.ValidatorFailed)
					{
						OwnerPropertyEnumerator.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, obj, PropertyValue.ValueValidationResult.ValidatorFailed));
					}
					else
					{
						OwnerPropertyEnumerator.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, obj, valueValidationResult, _ownerPropertyEnum.Property.Value.LastExceptionByClient));
					}
				}
				if (_ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.IgnoreAndRestore || valueValidationResult == PropertyValue.ValueValidationResult.Validated)
				{
					Text = _ownerPropertyEnum.Property.Value.DisplayString;
				}
				if (_ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.IgnoreAndRestore && PropertyValue.IsErrorCode(valueValidationResult))
				{
					valueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
					_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, _ownerPropertyEnum.Property.Value.GetValue(), valueValidationResult));
				}
				if (valueValidationResult == PropertyValue.ValueValidationResult.Validated)
				{
					if (PropertyValue.IsErrorCode(_currentValueValidationResult))
					{
						_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, _ownerPropertyEnum.Property.Value.GetValue(), valueValidationResult));
					}
					_ownerPropertyEnum.Property.ParentGrid.NotifyPropertyChanged(new PropertyChangedEventArgs(_ownerPropertyEnum));
				}
				_currentValueValidationResult = valueValidationResult;
				_ownerPropertyEnum.Property.ParentGrid.EndUpdate();
			}
			else
			{
				Text = _ownerPropertyEnum.Property.Value.DisplayString;
				if (PropertyValue.IsErrorCode(_currentValueValidationResult))
				{
					_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
					_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, _ownerPropertyEnum.Property.Value.GetValue(), _currentValueValidationResult));
				}
			}
		}
		finally
		{
			_ownerPropertyEnum.Property.ParentGrid.NotifyEndCommitChanges(new CommitChangesEventArgs(_ownerPropertyEnum, obj, valueIsText: false));
		}
		return valueValidationResult;
	}

	public Control GetDropDownContainer()
	{
		return _editorContainer;
	}
}
