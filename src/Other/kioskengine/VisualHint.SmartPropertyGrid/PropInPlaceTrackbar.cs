using System;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public class PropInPlaceTrackbar : Control, IInPlaceControl
{
	private PropertyGrid _parentGrid;

	private PropInPlaceTextbox _edit;

	private SPGTrackBar _trackbar;

	private PropertyValue.ValueValidationResult _currentValueValidationResult;

	private PropertyEnumerator _currentInvalidPropertyEnum;

	private PropertyEnumerator _ownerPropertyEnum;

	private bool _insideOwnerPropertyEnumerator;

	private Control _wmFocusMissingArg;

	private bool _realtimeChange;

	public PropertyEnumerator OwnerPropertyEnumerator
	{
		get
		{
			return _ownerPropertyEnum;
		}
		set
		{
			_insideOwnerPropertyEnumerator = true;
			try
			{
				_ownerPropertyEnum = (PropertyEnumerator)value.Clone();
				if (_edit != null)
				{
					_edit.OwnerPropertyEnumerator = _ownerPropertyEnum;
				}
				if (_trackbar != null)
				{
					_trackbar.BeginInit();
					_trackbar.OwnerPropertyEnumerator = _ownerPropertyEnum;
				}
				if (_ownerPropertyEnum.Property.ParentGrid.IsInsideRefreshProperties)
				{
					ResetTrackBarValue();
					return;
				}
				if (!base.Visible)
				{
					_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
				}
				_parentGrid = _ownerPropertyEnum.Property.ParentGrid;
				if (!ReadOnly)
				{
					if (_trackbar == null)
					{
						_trackbar = new SPGTrackBar(_ownerPropertyEnum.Property.Value.UnderlyingType);
						if (_edit != null)
						{
							_trackbar.TabStop = false;
						}
						_trackbar.BeginInit();
						_trackbar.Parent = this;
						_trackbar.Scroll += OnScroll;
						_trackbar.ValueChanged += OnValueChanged;
						_trackbar.OwnerPropertyEnumerator = _ownerPropertyEnum;
					}
					ResetTrackBarValue();
					Text = OwnerPropertyEnumerator.Property.Value.DisplayString;
					if (_realtimeChange)
					{
						CommitChanges(final: false);
					}
				}
				else
				{
					if (_trackbar != null)
					{
						_trackbar = null;
					}
					Text = _ownerPropertyEnum.Property.Value.DisplayString;
					if (_realtimeChange)
					{
						CommitChanges(final: false);
					}
				}
			}
			finally
			{
				if (_trackbar != null)
				{
					_trackbar.EndInit();
				}
				_insideOwnerPropertyEnumerator = false;
			}
		}
	}

	public object Resolution
	{
		get
		{
			return _trackbar.Resolution;
		}
		set
		{
			_trackbar.Resolution = value;
			RepositionChildren();
		}
	}

	public object SmallChange
	{
		get
		{
			return _trackbar.SmallChange;
		}
		set
		{
			_trackbar.SmallChange = value;
		}
	}

	public object LargeChange
	{
		get
		{
			return _trackbar.LargeChange;
		}
		set
		{
			_trackbar.LargeChange = value;
		}
	}

	public object TickFrequency
	{
		get
		{
			return _trackbar.TickFrequency;
		}
		set
		{
			_trackbar.TickFrequency = value;
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

	public TrackBar TrackBar => _trackbar;

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

	public bool ReadOnly => _ownerPropertyEnum.Property.Value.IsReadOnly(_ownerPropertyEnum);

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

	public TextBox TextBox => _edit;

	internal void ResetTrackBarValue()
	{
		object obj = ((!OwnerPropertyEnumerator.Property.Value.HasMultipleValues) ? _ownerPropertyEnum.Property.Value.GetValue() : _trackbar.Minimum);
		try
		{
			_trackbar.Value = obj;
		}
		catch (ArgumentOutOfRangeException)
		{
			if ((obj as IComparable).CompareTo(_trackbar.Minimum) == -1)
			{
				_trackbar.Value = _trackbar.Minimum;
			}
			else
			{
				_trackbar.Value = _trackbar.Maximum;
			}
		}
	}

	public PropInPlaceTrackbar(bool editable)
	{
		base.TabStop = false;
		if (editable)
		{
			_edit = new PropInPlaceTextbox(isPassword: false);
			_edit.Parent = this;
		}
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | Utilities.DoubleBufferStyle | ControlStyles.Opaque | ControlStyles.ResizeRedraw, value: true);
		Cursor = Cursors.Arrow;
	}

	protected override void OnPaint(PaintEventArgs pevent)
	{
		PropertyValue value = _ownerPropertyEnum.Property.Value;
		_parentGrid.DrawManager.DrawPropertyValueBackground(pevent.Graphics, base.ClientRectangle, _ownerPropertyEnum, PropertyValue.DrawValueIn.InPlaceCtrl);
		new Rectangle(0, 0, GetEditWidth(), base.ClientRectangle.Height);
		Rectangle clientRectangle = base.ClientRectangle;
		if (value.HasMultipleTexts() && Text.Length == 0)
		{
			value.DrawValue(pevent.Graphics, clientRectangle, value.ForeColor, _ownerPropertyEnum, "", multipleTexts: true, PropertyValue.DrawValueIn.InPlaceCtrl);
		}
		else
		{
			value.DrawValue(pevent.Graphics, clientRectangle, value.ForeColor, _ownerPropertyEnum, Text, multipleTexts: false, PropertyValue.DrawValueIn.InPlaceCtrl);
		}
	}

	private int GetEditWidth()
	{
		if (_trackbar != null)
		{
			using (Graphics graphics = CreateGraphics())
			{
				int maxWidth = _trackbar.GetMaxWidth(graphics, Font);
				return maxWidth + 2 * _ownerPropertyEnum.Property.ParentGrid.GlobalTextMargin;
			}
		}
		return base.ClientRectangle.Width;
	}

	public virtual void RepositionChildren()
	{
		Rectangle clientRectangle = base.ClientRectangle;
		if (clientRectangle.Width <= 0)
		{
			return;
		}
		Rectangle clientRectangle2 = base.ClientRectangle;
		clientRectangle2.Height++;
		using Graphics graphics = CreateGraphics();
		clientRectangle2 = _ownerPropertyEnum.Property.Value.GetStringValueRect(graphics, clientRectangle2, Point.Empty);
		clientRectangle2.Height--;
		int globalTextMargin = _ownerPropertyEnum.Property.ParentGrid.GlobalTextMargin;
		int num = clientRectangle2.Left - globalTextMargin + ((_ownerPropertyEnum.Property.Value.EditboxLeftMargin != -1) ? (globalTextMargin - _ownerPropertyEnum.Property.Value.EditboxLeftMargin) : 0);
		if (_edit != null)
		{
			Rectangle rect = new Rectangle(num, 0, GetEditWidth(), clientRectangle.Height);
			rect.Y++;
			rect.Height--;
			int doubleword = Win32Calls.LoWord(Win32Calls.SendMessage(_edit.Handle, 212, IntPtr.Zero, IntPtr.Zero));
			int num2 = Win32Calls.LoWord(doubleword) + Win32Calls.HiWord(doubleword);
			if (rect.Width <= num2)
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
				_edit.Invalidate();
				_edit.Bounds = OwnerPropertyEnumerator.Property.ParentGrid.TransformRectIfRTL(rect, base.ClientSize.Width);
			}
			if (_trackbar != null)
			{
				Rectangle rect2 = new Rectangle(rect.Right, 2, clientRectangle.Right - rect.Right, clientRectangle.Height - 2);
				rect2 = OwnerPropertyEnumerator.Property.ParentGrid.TransformRectIfRTL(rect2, base.ClientSize.Width);
				_trackbar.Bounds = rect2;
			}
		}
		else if (_trackbar != null)
		{
			num += GetEditWidth();
			Rectangle rect3 = new Rectangle(num, 2, clientRectangle.Width - num, clientRectangle.Height - 2);
			rect3 = OwnerPropertyEnumerator.Property.ParentGrid.TransformRectIfRTL(rect3, base.ClientSize.Width);
			_trackbar.Bounds = rect3;
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		RepositionChildren();
	}

	private void OnValueChanged(object sender, EventArgs e)
	{
		if (!_insideOwnerPropertyEnumerator)
		{
			Text = _trackbar.Value.ToString();
			if (_realtimeChange)
			{
				OwnerPropertyEnumerator.Property.ParentGrid.BeginUpdate();
				CommitChanges(final: false);
				OwnerPropertyEnumerator.Property.ParentGrid.SelectAndFocusProperty(OwnerPropertyEnumerator, selectAllText: false);
				OwnerPropertyEnumerator.Property.ParentGrid.EndUpdate();
			}
			else
			{
				OwnerPropertyEnumerator.Property.ParentGrid.BeginUpdate();
				OwnerPropertyEnumerator.Property.ParentGrid.EndUpdate();
			}
		}
	}

	private void OnScroll(object sender, EventArgs e)
	{
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

	protected override void OnGotFocus(EventArgs e)
	{
		if (_edit != null)
		{
			if (_wmFocusMissingArg != _edit)
			{
				_edit.Focus();
			}
			else
			{
				base.Parent.Focus();
			}
		}
		else if (_trackbar != null && _wmFocusMissingArg != _trackbar)
		{
			_trackbar.Focus();
		}
		else
		{
			base.Parent.Focus();
		}
		base.OnGotFocus(e);
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		if (_edit != null && !Focused)
		{
			Win32Calls.PostMessage(_edit.Handle, 258, (IntPtr)e.KeyChar, IntPtr.Zero);
		}
		else
		{
			base.OnKeyPress(e);
		}
	}

	[UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
	protected override bool ProcessDialogKey(Keys keyData)
	{
		switch (keyData & Keys.KeyCode)
		{
		case Keys.Return:
		{
			if (_trackbar != null && _trackbar.Focused)
			{
				CommitChanges(final: true);
				break;
			}
			PropertyValue.ValueValidationResult result2 = CommitChanges(final: true);
			if (PropertyValue.IsErrorCode(result2) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus && _edit != null)
			{
				_edit.SelectAll();
				return true;
			}
			break;
		}
		case Keys.Tab:
		{
			PropertyValue.ValueValidationResult result = CommitChanges(_trackbar.Focused);
			if (PropertyValue.IsErrorCode(result) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
			{
				if (_edit != null)
				{
					_edit.SelectAll();
				}
				return true;
			}
			if (_edit == null)
			{
				break;
			}
			bool flag = (keyData & Keys.Shift) == 0;
			if (flag && _edit.Focused)
			{
				if (_parentGrid.NavigationKeyMode == PropertyGrid.NavigationKeyModes.ArrowKeys || (_parentGrid.TabKeyNavigationMode & PropertyGrid.TabKeyNavigationModes.TabKeyInSubControls) != PropertyGrid.TabKeyNavigationModes.NoSubMode)
				{
					OwnerPropertyEnumerator.Property.ParentGrid.SelectAndFocusProperty(OwnerPropertyEnumerator, selectAllText: false);
					_trackbar.Focus();
					return true;
				}
			}
			else if (flag && _trackbar.Focused)
			{
				if (!_realtimeChange)
				{
					CommitChanges(final: true);
				}
			}
			else if (!flag && _trackbar.Focused)
			{
				_edit.Focus();
				_edit.SelectAll();
				Invalidate(invalidateChildren: false);
				return true;
			}
			break;
		}
		case Keys.Escape:
			ResetTrackBarValue();
			Text = _trackbar.Value.ToString();
			CommitChanges(final: true);
			break;
		}
		if (base.ContainsFocus)
		{
			return base.ProcessDialogKey(keyData);
		}
		return true;
	}

	protected override bool ProcessKeyPreview(ref Message m)
	{
		if (m.Msg == 256)
		{
			Keys keys = (Keys)(int)m.WParam;
			if ((keys == Keys.Down || keys == Keys.Up || keys == Keys.Next || keys == Keys.Prior) && _edit != null && _edit.Focused)
			{
				object obj = null;
				try
				{
					obj = _ownerPropertyEnum.Property.Value.ConvertDisplayedStringToValue(Text);
				}
				catch (Exception e)
				{
					_edit.SelectAll();
					_currentValueValidationResult = PropertyValue.ValueValidationResult.TypeConverterFailed;
					_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, OwnerPropertyEnumerator, Text, PropertyValue.ValueValidationResult.TypeConverterFailed, e));
					return true;
				}
				object obj2 = obj;
				switch (keys)
				{
				case Keys.Down:
					obj2 = _trackbar.GetValueMinusSmallChange();
					break;
				case Keys.Up:
					obj2 = _trackbar.GetValuePlusSmallChange();
					break;
				case Keys.Next:
					obj2 = _trackbar.GetValueMinusLargeChange();
					break;
				case Keys.Prior:
					obj2 = _trackbar.GetValuePlusLargeChange();
					break;
				}
				PropertyValidatorBase validator = _ownerPropertyEnum.Property.Value.Validator;
				if (validator != null)
				{
					bool flag = validator.Check(obj2, modify: false);
					if (!validator.Check(obj, modify: false) && !flag)
					{
						_currentInvalidPropertyEnum = OwnerPropertyEnumerator;
						_edit.SelectAll();
						_currentValueValidationResult = PropertyValue.ValueValidationResult.ValidatorFailed;
						_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, obj, _currentValueValidationResult));
						return true;
					}
				}
				if (!obj2.Equals(_trackbar.Value))
				{
					if ((obj2 as IComparable).CompareTo(_trackbar.Maximum) == 1)
					{
						obj2 = _trackbar.Maximum;
					}
					else if ((obj2 as IComparable).CompareTo(_trackbar.Minimum) == -1)
					{
						obj2 = _trackbar.Minimum;
					}
					_trackbar.Value = obj2;
					Text = obj2.ToString();
					_edit.SelectAll();
					if (_realtimeChange)
					{
						CommitChanges(final: false);
						OwnerPropertyEnumerator.Property.ParentGrid.SelectAndFocusProperty(OwnerPropertyEnumerator, selectAllText: true);
					}
					return true;
				}
			}
		}
		return base.ProcessKeyPreview(ref m);
	}

	public void OnF4()
	{
		Focus();
	}

	public bool OnParentKeyDown(KeyEventArgs e)
	{
		if (_edit != null && _edit.OnParentKeyDown(e))
		{
			return true;
		}
		return false;
	}

	public bool OnForbidClickThrough(Point pt)
	{
		if (_trackbar == null)
		{
			return false;
		}
		return _trackbar.Bounds.Contains(pt);
	}

	public void DoubleClickFromParent(Point pt)
	{
	}

	public virtual bool GoToNextPropertyOnTab(bool forward)
	{
		if (forward)
		{
			return _trackbar.Focused;
		}
		if (_edit == null)
		{
			return _trackbar.Focused;
		}
		return _edit.Focused;
	}

	public virtual bool ScrollValueOnDoubleClick(bool forward, ScrollValueOnDoubleClickSource source)
	{
		return false;
	}

	public virtual PropertyValue.ValueValidationResult CommitChanges(bool final)
	{
		PropertyValue.ValueValidationResult valueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
		if (_edit != null && _edit.ReadOnly)
		{
			return valueValidationResult;
		}
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
					_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
					_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, _ownerPropertyEnum.Property.Value.GetValue(), _currentValueValidationResult));
				}
				if (valueValidationResult == PropertyValue.ValueValidationResult.Validated)
				{
					if (PropertyValue.IsErrorCode(_currentValueValidationResult))
					{
						_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, _ownerPropertyEnum.Property.Value.GetValue(), valueValidationResult));
					}
					_trackbar.Value = obj;
					_parentGrid.NotifyPropertyChanged(new PropertyChangedEventArgs(_ownerPropertyEnum));
				}
				_currentValueValidationResult = valueValidationResult;
				base.Parent.Refresh();
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
		return null;
	}
}
