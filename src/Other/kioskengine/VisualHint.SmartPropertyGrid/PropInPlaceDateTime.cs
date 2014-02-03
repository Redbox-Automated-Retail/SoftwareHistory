using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public class PropInPlaceDateTime : DateTimePicker, IInPlaceControl, IMouseHookClient, IKeyboardHookClient
{
	private PropertyValue.ValueValidationResult _currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;

	private MouseHook _mouseHook;

	private KeyboardHook _keyboardHook;

	private IntPtr _closeByClickOnWindow;

	private bool _calendarClosedByKey;

	private bool _justVisible;

	private DateTime _oldValue;

	private PropertyEnumerator _currentInvalidPropertyEnum;

	private PropertyEnumerator _ownerPropertyEnum;

	private IntPtr _hMonthCalendar;

	private IntPtr _wmFocusMissingArg;

	private bool _realtimeChange;

	private Keys _monthCalCloseKey;

	public PropertyEnumerator OwnerPropertyEnumerator
	{
		get
		{
			return _ownerPropertyEnum;
		}
		set
		{
			_ownerPropertyEnum = (PropertyEnumerator)value.Clone();
			RightToLeft = _ownerPropertyEnum.Property.ParentGrid.RightToLeft;
			if (!_ownerPropertyEnum.Property.ParentGrid.IsInsideRefreshProperties)
			{
				if (!base.Visible)
				{
					_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
				}
				base.Value = (DateTime)_ownerPropertyEnum.Property.Value.GetValue();
				base.Enabled = !ReadOnly;
				_oldValue = (DateTime)_ownerPropertyEnum.Property.Value.GetValue();
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

	public TextBox TextBox => null;

	public PropInPlaceDateTime()
	{
		_mouseHook = new MouseHook(this, this);
		_keyboardHook = new KeyboardHook(this, this);
		base.ShowUpDown = true;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		Win32Calls.ModifyStyleEx(base.Handle, 512, 0, 32);
		Cursor = Cursors.Arrow;
		base.OnHandleCreated(e);
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 131)
		{
			Win32Calls.NCCALCSIZE_PARAMS nCCALCSIZE_PARAMS = (Win32Calls.NCCALCSIZE_PARAMS)Marshal.PtrToStructure(m.LParam, typeof(Win32Calls.NCCALCSIZE_PARAMS));
			if (_ownerPropertyEnum.Property.ParentGrid.RightToLeft == RightToLeft.Yes)
			{
				nCCALCSIZE_PARAMS.rgrc0.Right -= _ownerPropertyEnum.Property.ParentGrid.GlobalTextMargin - 2;
			}
			else
			{
				nCCALCSIZE_PARAMS.rgrc0.Left += ((_ownerPropertyEnum.Property.Value.EditboxLeftMargin == -1) ? _ownerPropertyEnum.Property.ParentGrid.GlobalTextMargin : _ownerPropertyEnum.Property.Value.EditboxLeftMargin) - 2;
			}
			Marshal.StructureToPtr(nCCALCSIZE_PARAMS, m.LParam, fDeleteOld: false);
		}
		else
		{
			if (m.Msg == 133)
			{
				IntPtr windowDC = Win32Calls.GetWindowDC(m.HWnd);
				Graphics graphics = Graphics.FromHdc(windowDC);
				Rectangle clientRectangle = base.ClientRectangle;
				clientRectangle.Width = _ownerPropertyEnum.Property.ParentGrid.GlobalTextMargin - 2;
				_ownerPropertyEnum.Property.ParentGrid.DrawManager.DrawPropertyValueBackground(graphics, clientRectangle, _ownerPropertyEnum, PropertyValue.DrawValueIn.InPlaceCtrl);
				Win32Calls.ReleaseDC(m.HWnd, windowDC);
				graphics.Dispose();
				return;
			}
			if (m.Msg == 20)
			{
				if (!_justVisible)
				{
					using Graphics graphics2 = Graphics.FromHdc(m.WParam);
					_ownerPropertyEnum.Property.ParentGrid.DrawManager.DrawPropertyValueBackground(graphics2, base.ClientRectangle, _ownerPropertyEnum, PropertyValue.DrawValueIn.InPlaceCtrl);
				}
				m.Result = (IntPtr)1;
				return;
			}
			if (m.Msg == 15 && _justVisible)
			{
				using (Graphics graphics3 = CreateGraphics())
				{
					_ownerPropertyEnum.Property.ParentGrid.DrawManager.DrawPropertyValueBackground(graphics3, base.ClientRectangle, _ownerPropertyEnum, PropertyValue.DrawValueIn.InPlaceCtrl);
				}
				_justVisible = false;
			}
		}
		if (m.Msg == 7)
		{
			_wmFocusMissingArg = m.WParam;
		}
		else if (m.Msg == 8)
		{
			_wmFocusMissingArg = m.WParam;
		}
		base.WndProc(ref m);
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		if (base.Visible)
		{
			if (_ownerPropertyEnum.Property.Value.Look is PropertyDateTimeLook propertyDateTimeLook)
			{
				base.CustomFormat = propertyDateTimeLook.CustomFormat;
				base.Format = propertyDateTimeLook.Format;
			}
		}
		else
		{
			_justVisible = true;
		}
		base.OnVisibleChanged(e);
	}

	protected override void OnTextChanged(EventArgs e)
	{
		if (_realtimeChange || (_closeByClickOnWindow == _hMonthCalendar && _closeByClickOnWindow != IntPtr.Zero))
		{
			CommitChanges(final: false);
		}
	}

	[UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
	protected override bool ProcessDialogKey(Keys keyData)
	{
		switch (keyData & Keys.KeyCode)
		{
		case Keys.Return:
		{
			PropertyValue.ValueValidationResult result2 = CommitChanges(final: true);
			if (PropertyValue.IsErrorCode(result2) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
			{
				return true;
			}
			break;
		}
		case Keys.Tab:
		{
			PropertyValue.ValueValidationResult result = CommitChanges(final: true);
			if (PropertyValue.IsErrorCode(result) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
			{
				return true;
			}
			break;
		}
		case Keys.Escape:
			base.Value = _oldValue;
			CommitChanges(final: true);
			break;
		}
		if (Focused)
		{
			return base.ProcessDialogKey(keyData);
		}
		return true;
	}

	public void OnF4()
	{
		Focus();
		bool flag = (Win32Calls.GetKeyState(18) & 0x8000) == 1;
		if (!flag)
		{
			Win32Calls.keybd_event(18, 0, 0u, UIntPtr.Zero);
		}
		Win32Calls.keybd_event(40, 0, 0u, UIntPtr.Zero);
		Win32Calls.keybd_event(40, 0, 2u, UIntPtr.Zero);
		if (!flag)
		{
			Win32Calls.keybd_event(18, 0, 2u, UIntPtr.Zero);
		}
	}

	public bool OnParentKeyDown(KeyEventArgs e)
	{
		return false;
	}

	public virtual void RepositionChildren()
	{
	}

	public bool OnForbidClickThrough(Point pt)
	{
		Rectangle clientRectangle = base.ClientRectangle;
		clientRectangle.X = clientRectangle.Right - SystemInformation.VerticalScrollBarWidth;
		clientRectangle.Width = SystemInformation.VerticalScrollBarWidth;
		return clientRectangle.Contains(pt);
	}

	protected override void OnDropDown(EventArgs eventargs)
	{
		_mouseHook.HookMessages = true;
		_keyboardHook.HookMessages = true;
		_ownerPropertyEnum.Property.InPlaceCtrlInAction = this;
		_hMonthCalendar = Win32Calls.SendMessage(base.Handle, 4104, IntPtr.Zero, IntPtr.Zero);
		Win32Calls.SetWindowLongPtr(_hMonthCalendar, -8, base.Handle);
		base.OnDropDown(eventargs);
	}

	protected override void OnCloseUp(EventArgs eventargs)
	{
		_mouseHook.HookMessages = false;
		_keyboardHook.HookMessages = false;
		_hMonthCalendar = IntPtr.Zero;
		Control control = Control.FromHandle(_closeByClickOnWindow);
		if (control != null)
		{
			Point mousePosition = Control.MousePosition;
			mousePosition = control.PointToClient(mousePosition);
			Win32Calls.PostMessage(_closeByClickOnWindow, 513, IntPtr.Zero, (IntPtr)Win32Calls.MakeLong(mousePosition.X, mousePosition.Y));
		}
		else if (_monthCalCloseKey != Keys.None)
		{
			if (_monthCalCloseKey == Keys.Escape)
			{
				base.Value = _oldValue;
			}
			CommitChanges(final: true);
			base.Parent.Focus();
		}
		_monthCalCloseKey = Keys.None;
		_ownerPropertyEnum.Property.InPlaceCtrlInAction = null;
		_ownerPropertyEnum.Property.ParentGrid.OnInPlaceCtrlFinishedEdition();
		_calendarClosedByKey = false;
		RecreateHandle();
		base.OnCloseUp(eventargs);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		base.OnLostFocus(e);
	}

	[CLSCompliant(false)]
	public bool OnMouseHooked(IntPtr hWnd, uint message, Point pt, uint hitTest)
	{
		if (message == 513)
		{
			_closeByClickOnWindow = hWnd;
		}
		return false;
	}

	public bool OnKeyboardHooked(int message, KeyEventArgs args)
	{
		if (message == 256)
		{
			if (args.KeyCode == Keys.F4 || (args.KeyCode == Keys.Down && args.Modifiers == Keys.Alt))
			{
				_monthCalCloseKey = Keys.Return;
				Win32Calls.keybd_event(164, 0, 18u, UIntPtr.Zero);
				Win32Calls.keybd_event(13, 0, 0u, UIntPtr.Zero);
				Win32Calls.keybd_event(13, 0, 2u, UIntPtr.Zero);
			}
			else if (args.KeyCode == Keys.Escape)
			{
				_monthCalCloseKey = Keys.Escape;
			}
			else if (args.KeyCode == Keys.Return)
			{
				_monthCalCloseKey = Keys.Return;
			}
		}
		return false;
	}

	public virtual bool GoToNextPropertyOnTab(bool forward)
	{
		return true;
	}

	public virtual bool ScrollValueOnDoubleClick(bool forward, ScrollValueOnDoubleClickSource source)
	{
		return false;
	}

	public virtual PropertyValue.ValueValidationResult CommitChanges(bool final)
	{
		PropertyValue.ValueValidationResult valueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
		if (ReadOnly)
		{
			return valueValidationResult;
		}
		DateTime dateTime = (DateTime)_ownerPropertyEnum.Property.Value.GetValue();
		if (base.Value != dateTime)
		{
			try
			{
				_ownerPropertyEnum.Property.ParentGrid.NotifyBeginCommitChanges(new CommitChangesEventArgs(_ownerPropertyEnum, base.Value, valueIsText: false));
				_ownerPropertyEnum.Property.ParentGrid.BeginUpdate();
				_ownerPropertyEnum.Property.Value.PreviousValue = dateTime;
				valueValidationResult = _ownerPropertyEnum.Property.Value.SetValueFromInPlaceCtrl(base.Value, out var invalidPropEnum, null);
				if (PropertyValue.IsErrorCode(valueValidationResult))
				{
					_currentInvalidPropertyEnum = invalidPropEnum;
					if (valueValidationResult == PropertyValue.ValueValidationResult.ValidatorFailed)
					{
						OwnerPropertyEnumerator.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, base.Value, PropertyValue.ValueValidationResult.ValidatorFailed));
					}
					else
					{
						OwnerPropertyEnumerator.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, base.Value, valueValidationResult, _ownerPropertyEnum.Property.Value.LastExceptionByClient));
					}
				}
				if (_ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.IgnoreAndRestore || valueValidationResult == PropertyValue.ValueValidationResult.Validated)
				{
					base.Value = (DateTime)_ownerPropertyEnum.Property.Value.GetValue();
					if (final)
					{
						_oldValue = base.Value;
					}
				}
				if (_ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.IgnoreAndRestore && valueValidationResult != PropertyValue.ValueValidationResult.Validated)
				{
					valueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
					_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, base.Value, valueValidationResult));
				}
				if (valueValidationResult == PropertyValue.ValueValidationResult.Validated)
				{
					if (PropertyValue.IsErrorCode(_currentValueValidationResult))
					{
						_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, base.Value, valueValidationResult));
					}
					_ownerPropertyEnum.Property.ParentGrid.NotifyPropertyChanged(new PropertyChangedEventArgs(_ownerPropertyEnum));
				}
				_currentValueValidationResult = valueValidationResult;
				_ownerPropertyEnum.Property.ParentGrid.EndUpdate();
			}
			finally
			{
				_ownerPropertyEnum.Property.ParentGrid.NotifyEndCommitChanges(new CommitChangesEventArgs(_ownerPropertyEnum, base.Value, valueIsText: false));
			}
		}
		else if (PropertyValue.IsErrorCode(_currentValueValidationResult))
		{
			_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
			_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, base.Value, _currentValueValidationResult));
		}
		return valueValidationResult;
	}

	public Control GetDropDownContainer()
	{
		return null;
	}
}
