using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public class PropInPlaceList : Control, IInPlaceControl, IMouseHookListener
{
	private PropInPlaceTextbox _edit;

	private bool _dontTransferFocus;

	private int _oldSelectedIndex;

	private PropertyValue.ValueValidationResult _currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;

	private PropertyEnumerator _currentInvalidPropertyEnum;

	private PropertyEnumerator _ownerPropertyEnum;

	private int _selectedIndex = -1;

	private bool _realtimeChange = true;

	private bool _preventResizable;

	private bool _pushed;

	private bool _mouseOver;

	private Control _wmFocusMissingArg;

	private PropInPlaceContainer _editorContainer;

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
				if (_edit == null)
				{
					Text = _ownerPropertyEnum.Property.Value.DisplayString;
				}
				_selectedIndex = _ownerPropertyEnum.Property.Value.ImageIndex;
				_oldSelectedIndex = _selectedIndex;
			}
		}
	}

	public int SelectedIndex
	{
		get
		{
			return _selectedIndex;
		}
		set
		{
			_selectedIndex = value;
		}
	}

	public int ImageIndex
	{
		set
		{
			if (_ownerPropertyEnum.Property.Value.ImageSource == PropertyGrid.ImageSources.Value)
			{
				_ownerPropertyEnum.Property.Value.ImageIndex = value;
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

	public bool PreventResizable
	{
		get
		{
			return _preventResizable;
		}
		set
		{
			_preventResizable = value;
		}
	}

	public override string Text
	{
		get
		{
			if (_edit == null)
			{
				return base.Text;
			}
			return _edit.Text;
		}
		set
		{
			if (_edit == null)
			{
				base.Text = value;
				Invalidate();
			}
			else
			{
				_edit.Text = value;
			}
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

	private bool ShowIcon
	{
		get
		{
			PropertyValue value = _ownerPropertyEnum.Property.Value;
			ImageList imageList = null;
			if (value.ImageSource == PropertyGrid.ImageSources.Value)
			{
				imageList = value.ImageList;
			}
			else if (value.ImageSource == PropertyGrid.ImageSources.Grid)
			{
				imageList = value.Grid.ImageList;
			}
			if (imageList != null)
			{
				return _ownerPropertyEnum.Property.Value.ImageIndex != -1;
			}
			return false;
		}
	}

	public TextBox TextBox => _edit;

	public PropInPlaceList(bool editable)
	{
		if (editable)
		{
			_edit = new PropInPlaceTextbox(isPassword: false);
			_edit.LostFocus += OnChildLostFocus;
			_edit.Parent = this;
			_edit.TabStop = false;
			_edit.TextChanged += OnTextboxTextChanged;
		}
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | Utilities.DoubleBufferStyle | ControlStyles.Opaque | ControlStyles.ResizeRedraw, value: true);
		Cursor = Cursors.Arrow;
	}

	protected int GetButtonWidth()
	{
		if (ReadOnly)
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
		int buttonWidth = GetButtonWidth();
		return new Rectangle(base.Width - buttonWidth, 0, buttonWidth, base.Height);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		PropertyGrid parentGrid = _ownerPropertyEnum.Property.ParentGrid;
		_ownerPropertyEnum.Property.ParentGrid.DrawManager.DrawPropertyValueBackground(e.Graphics, base.ClientRectangle, _ownerPropertyEnum, PropertyValue.DrawValueIn.InPlaceCtrl);
		Rectangle buttonRect = GetButtonRect();
		PropertyValue value = _ownerPropertyEnum.Property.Value;
		Rectangle clientRectangle = base.ClientRectangle;
		Color textColor = ((!_ownerPropertyEnum.Property.Enabled) ? SystemColors.GrayText : ((!Focused) ? value.ForeColor : parentGrid.HighlightedTextColor));
		if (_edit == null && Focused)
		{
			Rectangle stringValueRect = value.GetStringValueRect(e.Graphics, clientRectangle, Point.Empty);
			int globalTextMargin = _ownerPropertyEnum.Property.ParentGrid.GlobalTextMargin;
			stringValueRect.X -= (ShowIcon ? (globalTextMargin / 2) : globalTextMargin) - 1;
			stringValueRect.Width = buttonRect.Left - 2 - stringValueRect.Left;
			stringValueRect.Y++;
			stringValueRect.Height -= 2;
			stringValueRect = OwnerPropertyEnumerator.Property.ParentGrid.TransformRectIfRTL(stringValueRect, base.ClientSize.Width);
			using (Brush brush = new SolidBrush(parentGrid.SelectedBackColor))
			{
				e.Graphics.FillRectangle(brush, stringValueRect);
			}
			ControlPaint.DrawFocusRectangle(e.Graphics, stringValueRect);
		}
		clientRectangle.Width -= buttonRect.Width + 1;
		if (value.HasMultipleTexts() && Text.Length == 0)
		{
			value.DrawValue(e.Graphics, clientRectangle, textColor, _ownerPropertyEnum, "", multipleTexts: true, PropertyValue.DrawValueIn.InPlaceCtrl);
		}
		else
		{
			value.DrawValue(e.Graphics, clientRectangle, textColor, _ownerPropertyEnum, Text, multipleTexts: false, PropertyValue.DrawValueIn.InPlaceCtrl);
		}
		if (!ReadOnly)
		{
			buttonRect = OwnerPropertyEnumerator.Property.ParentGrid.TransformRectIfRTL(buttonRect, base.ClientSize.Width);
			parentGrid.DrawManager.DrawDropDownButton(e.Graphics, this, drawDown: true, buttonRect, _pushed, _mouseOver, PropertyValue.DrawValueIn.InPlaceCtrl);
		}
		base.OnPaint(e);
	}

	public bool OnForbidClickThrough(Point pt)
	{
		return _ownerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(GetButtonRect(), base.ClientSize.Width).Contains(pt);
	}

	protected override void OnMove(EventArgs e)
	{
		base.OnMove(e);
		OnSizeChanged(e);
	}

	private void OnTextboxTextChanged(object sender, EventArgs e)
	{
		Invalidate();
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
				int buttonWidth = GetButtonWidth();
				Rectangle valueRect = clientRectangle;
				valueRect.Width -= buttonWidth + 1;
				valueRect.Height++;
				Graphics graphics = CreateGraphics();
				valueRect = _ownerPropertyEnum.Property.Value.GetStringValueRect(graphics, valueRect, Point.Empty);
				valueRect.Height--;
				int globalTextMargin = _ownerPropertyEnum.Property.ParentGrid.GlobalTextMargin;
				int num2 = valueRect.Left - globalTextMargin + ((_ownerPropertyEnum.Property.Value.EditboxLeftMargin != -1) ? (globalTextMargin - _ownerPropertyEnum.Property.Value.EditboxLeftMargin) : 0);
				Rectangle rect = new Rectangle(num2, 0, clientRectangle.Width - buttonWidth - num2, valueRect.Height);
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
					rect = OwnerPropertyEnumerator.Property.ParentGrid.TransformRectIfRTL(rect, base.ClientSize.Width);
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
					if ((_editorContainer == null && _wmFocusMissingArg != null) || (_editorContainer != null && _wmFocusMissingArg != _editorContainer.ContentControl))
					{
						_edit.Focus();
					}
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
		_mouseOver = false;
		base.OnLostFocus(e);
	}

	private void OnChildLostFocus(object source, EventArgs e)
	{
		if (_ownerPropertyEnum.Property.Value.ImageList == null)
		{
			return;
		}
		int num = 0;
		string[] displayedValues = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
		ImageIndex = -1;
		string[] array = displayedValues;
		foreach (string text in array)
		{
			if (text == _ownerPropertyEnum.Property.Value.DisplayString)
			{
				ImageIndex = num;
				break;
			}
			num++;
		}
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
		if (_edit != null && !Focused)
		{
			Win32Calls.PostMessage(_edit.Handle, 258, (IntPtr)e.KeyChar, IntPtr.Zero);
		}
		else if (_edit == null)
		{
			PropertyValue value = OwnerPropertyEnumerator.Property.Value;
			if (value.UseAutoCompletion)
			{
				int currentIndex = SelectedIndex;
				string text = value.IncrementalFind(e.KeyChar, ref currentIndex);
				if (currentIndex != -1 && SelectedIndex != currentIndex)
				{
					SelectedIndex = currentIndex;
					ImageIndex = currentIndex;
					Text = text;
					if (RealtimeChange)
					{
						CommitChanges(final: false);
					}
				}
			}
		}
		base.OnKeyPress(e);
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
				if (_edit == null || _edit.Focused)
				{
					PropertyValue.ValueValidationResult result = CommitChanges(final: false);
					if (PropertyValue.IsErrorCode(result) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
					{
						if (_edit != null)
						{
							_edit.Focus();
							_edit.SelectAll();
						}
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
		else if ((e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) && _edit == null)
		{
			ProcessArrowKey(e.KeyCode, ScrollValueOnDoubleClickSource.FromInPlaceCtrl, rotate: false);
			Invalidate();
		}
		base.OnKeyDown(e);
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
		base.OnKeyUp(e);
	}

	protected bool ProcessArrowKey(Keys key, ScrollValueOnDoubleClickSource source, bool rotate)
	{
		if (ReadOnly)
		{
			return false;
		}
		string[] displayedValues = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
		if (displayedValues.Length > 0)
		{
			int num = -1;
			if (_ownerPropertyEnum.Property.Value.HasMultipleTexts())
			{
				num = ((key != Keys.Down) ? (displayedValues.Length - 1) : 0);
			}
			else
			{
				for (int i = 0; i < displayedValues.Length; i++)
				{
					if (Text.Equals(displayedValues[i]))
					{
						num = ((key != Keys.Down) ? ((i > 0) ? (i - 1) : (rotate ? (displayedValues.Length - 1) : (-1))) : ((i < displayedValues.Length - 1) ? (i + 1) : ((!rotate) ? (-1) : 0)));
						break;
					}
				}
			}
			if (num != -1)
			{
				SelectedIndex = num;
				ImageIndex = num;
				Text = displayedValues[num];
				if (source == ScrollValueOnDoubleClickSource.FromLabel || RealtimeChange)
				{
					CommitChanges(source == ScrollValueOnDoubleClickSource.FromLabel);
				}
				if (source != ScrollValueOnDoubleClickSource.FromLabel)
				{
					OwnerPropertyEnumerator.Property.ParentGrid.SelectAndFocusProperty(OwnerPropertyEnumerator, selectAllText: true);
				}
				return true;
			}
		}
		return false;
	}

	public virtual bool ScrollValueOnDoubleClick(bool forward, ScrollValueOnDoubleClickSource source)
	{
		if (forward)
		{
			return ProcessArrowKey(Keys.Down, source, rotate: true);
		}
		return ProcessArrowKey(Keys.Up, source, rotate: true);
	}

	protected override bool IsInputKey(Keys keyData)
	{
		if (!ReadOnly && (keyData == Keys.Up || keyData == Keys.Down))
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
			_pushed = false;
			_mouseOver = false;
			PropertyValue.ValueValidationResult result = CommitChanges(final: true);
			if (PropertyValue.IsErrorCode(result) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus && _edit != null)
			{
				_edit.SelectAll();
				return true;
			}
		}
		else if (keys == Keys.Escape)
		{
			_pushed = false;
			_mouseOver = false;
			if (!RealtimeChange)
			{
				Text = _ownerPropertyEnum.Property.Value.DisplayString;
				SelectedIndex = _oldSelectedIndex;
				ImageIndex = _oldSelectedIndex;
			}
			CommitChanges(final: true);
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
				bool flag = (keyData & Keys.Shift) == 0;
				bool final = _edit == null || (!flag && _edit.Focused);
				PropertyValue.ValueValidationResult result3 = CommitChanges(final);
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
		_editorContainer = new PropInPlaceContainer(_preventResizable);
		_editorContainer.RightToLeft = _ownerPropertyEnum.Property.ParentGrid.RightToLeft;
		_editorContainer.SetContent(_ownerPropertyEnum, Text);
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
			Point location = ((RightToLeft != RightToLeft.Yes) ? new Point(bounds.Right - _editorContainer.Size.Width + point.X, bounds.Bottom + 1 + point.Y) : new Point(bounds.Left + point.X, bounds.Bottom + 1 + point.Y));
			if (location.X < workingArea.Left)
			{
				location.X = workingArea.Left;
			}
			else if (location.X + _editorContainer.Size.Width > workingArea.Right)
			{
				location.X = workingArea.Right - _editorContainer.Size.Width;
			}
			if (location.Y + _editorContainer.Height > workingArea.Bottom)
			{
				_editorContainer.ResizeTop = true;
				location.Y = bounds.Top - _editorContainer.Height + point.Y;
			}
			else
			{
				_editorContainer.ResizeTop = false;
			}
			_editorContainer.Location = location;
			_editorContainer.ContentControl.Dock = DockStyle.Fill;
			Win32Calls.ShowWindow(new HandleRef(_editorContainer, _editorContainer.Handle), 8);
			_editorContainer.Visible = true;
			_ownerPropertyEnum.Property.ParentGrid.SetDropDownTopWindow(_editorContainer);
			_editorContainer.DoModalLoop();
		}
	}

	public void CommitChangesFromLiveDropDown()
	{
		object value = _editorContainer.DropDownContent.GetValue();
		if (value != null)
		{
			if (value is string)
			{
				Text = value as string;
			}
			else
			{
				string text = _ownerPropertyEnum.Property.Value.TypeConverter.ConvertToString(_ownerPropertyEnum.Property.Value.GetTypeDescriptorContext(_ownerPropertyEnum), _ownerPropertyEnum.Property.Value.CultureInfo, value);
				Text = ((text == null) ? "" : text);
			}
			SelectedIndex = _editorContainer.SelectedIndex;
			ImageIndex = SelectedIndex;
			_ownerPropertyEnum.Property.Value.PreventRefreshProperties = true;
			CommitChanges(final: true);
			_ownerPropertyEnum.Property.Value.PreventRefreshProperties = false;
		}
	}

	private void OnDropDownContentMustBeClosed(object sender, EventArgs e)
	{
		_ownerPropertyEnum.Property.ParentGrid.BeginUpdate();
		object value = _editorContainer.DropDownContent.GetValue();
		int selectedIndex = _editorContainer.SelectedIndex;
		DropDownContentClosedEventArgs e2 = e as DropDownContentClosedEventArgs;
		if (e2.ToBeValidated)
		{
			if (value != null)
			{
				if (value is string)
				{
					Text = value as string;
				}
				else
				{
					string text = _ownerPropertyEnum.Property.Value.TypeConverter.ConvertToString(_ownerPropertyEnum.Property.Value.GetTypeDescriptorContext(_ownerPropertyEnum), _ownerPropertyEnum.Property.Value.CultureInfo, value);
					Text = ((text == null) ? "" : text);
				}
				SelectedIndex = selectedIndex;
				ImageIndex = selectedIndex;
				CommitChanges(final: true);
			}
		}
		else
		{
			SelectedIndex = _oldSelectedIndex;
			ImageIndex = _oldSelectedIndex;
			Text = _ownerPropertyEnum.Property.Value.DisplayString;
			CommitChanges(final: true);
		}
		if (e2.WndTakingFocus == this)
		{
			ContainerControl topLevelContainer = Utilities.GetTopLevelContainer(this);
			if (topLevelContainer != null)
			{
				topLevelContainer.ActiveControl = base.Parent;
			}
		}
		HideDropDownContent();
		_ownerPropertyEnum.Property.ParentGrid.EndUpdate();
	}

	protected void HideDropDownContent()
	{
		if (_oldSelectedIndex == -1 && _ownerPropertyEnum.Property.Value.ImageIndex >= 0)
		{
			RepositionChildren();
		}
		_editorContainer.MustBeClosed -= OnDropDownContentMustBeClosed;
		_ownerPropertyEnum.Property.InPlaceCtrlInAction = null;
		_editorContainer.Visible = false;
		_editorContainer.SetContent(null, null);
		_ownerPropertyEnum.Property.ParentGrid.SetDropDownTopWindow(null);
		_ownerPropertyEnum.Property.ParentGrid.OnInPlaceCtrlFinishedEdition();
		_editorContainer.Dispose();
		_editorContainer = null;
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
		if (_edit != null && !_edit.ReadOnly && _edit.OnParentKeyDown(e))
		{
			return true;
		}
		if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
		{
			string[] displayedValues = OwnerPropertyEnumerator.Property.Value.GetDisplayedValues();
			string[] array = displayedValues;
			foreach (string text in array)
			{
				if (text.Length == 0)
				{
					if (_edit != null)
					{
						_edit.Clear();
						_edit.Focus();
					}
					else
					{
						Text = "";
						Focus();
					}
					if (RealtimeChange)
					{
						CommitChanges(final: false);
					}
					return true;
				}
			}
		}
		return false;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override bool ProcessKeyPreview(ref Message m)
	{
		if (m.Msg == 256)
		{
			Keys keys = (Keys)(int)m.WParam;
			if (keys == Keys.Down || keys == Keys.Up)
			{
				ProcessArrowKey(keys, ScrollValueOnDoubleClickSource.FromInPlaceCtrl, rotate: false);
				return true;
			}
		}
		return base.ProcessKeyPreview(ref m);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		if (e.Delta > 0)
		{
			ProcessArrowKey(Keys.Up, ScrollValueOnDoubleClickSource.FromInPlaceCtrl, rotate: false);
		}
		else
		{
			ProcessArrowKey(Keys.Down, ScrollValueOnDoubleClickSource.FromInPlaceCtrl, rotate: false);
		}
		Invalidate();
	}

	public virtual bool GoToNextPropertyOnTab(bool forward)
	{
		if (forward)
		{
			if (_edit == null || ReadOnly)
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

	public virtual PropertyValue.ValueValidationResult CommitChanges(bool final)
	{
		PropertyValue.ValueValidationResult valueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
		object obj = null;
		try
		{
			_ownerPropertyEnum.Property.ParentGrid.NotifyBeginCommitChanges(new CommitChangesEventArgs(_ownerPropertyEnum, Text, valueIsText: true));
			PropertyTypeDescriptorContext typeDescriptorContext = _ownerPropertyEnum.Property.Value.GetTypeDescriptorContext(_ownerPropertyEnum);
			bool flag = _ownerPropertyEnum.Property.Value.TypeConverter.CanConvertFrom(typeDescriptorContext, typeof(string));
			bool flag2 = false;
			if (flag)
			{
				flag2 = !_ownerPropertyEnum.Property.Value.DisplayString.Equals(Text);
			}
			object obj2 = null;
			bool hasMultipleValues = _ownerPropertyEnum.Property.Value.HasMultipleValues;
			if (!flag || flag2)
			{
				if (!hasMultipleValues)
				{
					obj2 = _ownerPropertyEnum.Property.Value.GetValue();
				}
				try
				{
					if (!hasMultipleValues || Text.Length != 0)
					{
						obj = _ownerPropertyEnum.Property.Value.ConvertDisplayedStringToValue(Text);
					}
				}
				catch (Exception e)
				{
					_currentInvalidPropertyEnum = _ownerPropertyEnum;
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
			}
			if ((hasMultipleValues && flag2) || (obj != null && (obj2 == null || !obj.Equals(obj2))) || (obj == null && obj2 != null))
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
					PropertyChangedEventArgs e2 = new PropertyChangedEventArgs(_ownerPropertyEnum);
					_ownerPropertyEnum.Property.ParentGrid.NotifyPropertyChanged(e2);
				}
				if (final)
				{
					_oldSelectedIndex = _ownerPropertyEnum.Property.Value.ImageIndex;
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
				if (final)
				{
					_oldSelectedIndex = _ownerPropertyEnum.Property.Value.ImageIndex;
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
