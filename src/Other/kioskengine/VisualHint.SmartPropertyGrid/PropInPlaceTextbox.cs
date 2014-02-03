using System;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public class PropInPlaceTextbox : TextBox, IInPlaceControl
{
	private bool _isPassword;

	private PropertyValue.ValueValidationResult _currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;

	private PropertyEnumerator _currentInvalidPropertyEnum;

	private PropertyEnumerator _ownerPropertyEnum;

	private Control _wmFocusMissingArg;

	public PropertyEnumerator CurrentInvalidPropertyEnum => _currentInvalidPropertyEnum;

	public PropertyEnumerator OwnerPropertyEnumerator
	{
		get
		{
			return _ownerPropertyEnum;
		}
		set
		{
			_ownerPropertyEnum = (PropertyEnumerator)value.Clone();
			if (_ownerPropertyEnum.Property.ParentGrid.IsInsideRefreshProperties)
			{
				return;
			}
			RightToLeft = _ownerPropertyEnum.Property.ParentGrid.RightToLeft;
			if (!base.Visible)
			{
				_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
			}
			if (Text != _ownerPropertyEnum.Property.Value.DisplayString)
			{
				Text = _ownerPropertyEnum.Property.Value.DisplayString;
			}
			if ((_ownerPropertyEnum.Property.Value.IsReadOnly(_ownerPropertyEnum) && _ownerPropertyEnum.Property.ParentGrid.ReadOnlyVisual == PropertyGrid.ReadOnlyVisuals.ReadOnlyFeel) || _ownerPropertyEnum.Property.Value.ReadOnlyByTypeConverter)
			{
				base.ReadOnly = true;
			}
			else
			{
				base.ReadOnly = false;
			}
			base.AutoCompleteMode = AutoCompleteMode.None;
			Font = Font;
			if (_ownerPropertyEnum.Property.Value.UseAutoCompletion && !base.ReadOnly)
			{
				string[] displayedValues = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
				if (displayedValues.Length > 0)
				{
					AutoCompleteStringCollection autoCompleteStringCollection = new AutoCompleteStringCollection();
					autoCompleteStringCollection.AddRange(displayedValues);
					base.AutoCompleteCustomSource = autoCompleteStringCollection;
					base.AutoCompleteSource = AutoCompleteSource.CustomSource;
					base.AutoCompleteMode = AutoCompleteMode.Append;
					Font = Font;
				}
			}
		}
	}

	protected override CreateParams CreateParams
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.ExStyle &= -513;
			if (_isPassword)
			{
				createParams.Style |= 32;
			}
			return createParams;
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
			int num = ((_ownerPropertyEnum.Property.Value.EditboxLeftMargin != -1) ? _ownerPropertyEnum.Property.Value.EditboxLeftMargin : _ownerPropertyEnum.Property.ParentGrid.GlobalTextMargin);
			Win32Calls.SendMessage(base.Handle, 211, (IntPtr)3, (RightToLeft == RightToLeft.Yes) ? ((IntPtr)Win32Calls.MakeLong(0, num)) : ((IntPtr)Win32Calls.MakeLong(num, 0)));
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

	public TextBox TextBox => this;

	protected override void OnTextChanged(EventArgs e)
	{
		if (base.AutoCompleteCustomSource != null && base.AutoCompleteCustomSource.Count > 0)
		{
			foreach (string item in base.AutoCompleteCustomSource)
			{
				if (string.Compare(item, Text, ignoreCase: true, OwnerPropertyEnumerator.Property.Value.CultureInfo) == 0)
				{
					Text = item;
					break;
				}
			}
		}
		base.OnTextChanged(e);
	}

	public PropInPlaceTextbox(bool isPassword)
	{
		AutoSize = false;
		_isPassword = isPassword;
	}

	protected override void OnGotFocus(EventArgs e)
	{
		Cursor = Cursors.IBeam;
		if (_wmFocusMissingArg == null || !_ownerPropertyEnum.Property.ParentGrid.Contains(_wmFocusMissingArg))
		{
			_ownerPropertyEnum.Property.ParentGrid.Invalidate(invalidateChildren: true);
		}
		base.OnGotFocus(e);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		Select(0, 0);
		ScrollToCaret();
		base.OnLostFocus(e);
	}

	[UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
	protected override bool ProcessDialogKey(Keys keyData)
	{
		Keys keys = keyData & Keys.KeyCode;
		if (Multiline && keys == Keys.Return && (keyData & Keys.Modifiers) == Keys.Control)
		{
			return false;
		}
		if ((keys == Keys.Return && !Multiline) || (Multiline && keyData == Keys.Return))
		{
			if (!(base.Parent is IInPlaceControl))
			{
				PropertyValue.ValueValidationResult result = CommitChanges(final: true);
				if (PropertyValue.IsErrorCode(result) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
				{
					SelectAll();
					return true;
				}
			}
		}
		else
		{
			switch (keys)
			{
			case Keys.Tab:
				if (!(base.Parent is IInPlaceControl))
				{
					PropertyValue.ValueValidationResult result2 = CommitChanges(final: true);
					if (PropertyValue.IsErrorCode(result2) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
					{
						SelectAll();
						return true;
					}
				}
				break;
			case Keys.Escape:
				if (_isPassword)
				{
					Text = _ownerPropertyEnum.Property.Value.DisplayStringWithoutLook;
				}
				else
				{
					Text = _ownerPropertyEnum.Property.Value.DisplayString;
				}
				if (!(base.Parent is IInPlaceControl))
				{
					CommitChanges(final: true);
				}
				break;
			}
		}
		if (Focused)
		{
			return base.ProcessDialogKey(keyData);
		}
		return true;
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		switch (keyData & Keys.KeyCode)
		{
		case Keys.A:
			if ((keyData & Keys.Control) != Keys.None && (keyData & Keys.Shift) == 0 && (keyData & Keys.Alt) == 0)
			{
				SelectAll();
				return true;
			}
			break;
		case Keys.C:
		case Keys.V:
		case Keys.X:
			if ((keyData & Keys.Control) != Keys.None && (keyData & Keys.Shift) == 0 && (keyData & Keys.Alt) == 0)
			{
				return false;
			}
			break;
		}
		return base.ProcessCmdKey(ref msg, keyData);
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
		else if (m.Msg == 20)
		{
			if (Multiline)
			{
				return;
			}
		}
		else if (m.Msg == 15)
		{
			if (Multiline)
			{
				using Graphics graphics = CreateGraphics();
				Win32Calls.RECT rect = default(Win32Calls.RECT);
				Win32Calls.GetUpdateRect(base.Handle, out rect, bErase: false);
				using Brush brush = new SolidBrush(BackColor);
				graphics.FillRectangle(brush, Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom));
			}
		}
		else if (m.Msg == 33)
		{
			if (!Focused && Form.ActiveForm is PropInPlaceUITypeEditor.DropDownForm)
			{
				Focus();
			}
		}
		else if (m.Msg == 515)
		{
			base.WndProc(ref m);
			if (base.Parent is IInPlaceControl)
			{
				(base.Parent as IInPlaceControl).ScrollValueOnDoubleClick(Control.ModifierKeys != Keys.Shift, ScrollValueOnDoubleClickSource.FromTextBox);
			}
			return;
		}
		base.WndProc(ref m);
	}

	public void OnF4()
	{
		Focus();
		SelectAll();
	}

	public bool OnParentKeyDown(KeyEventArgs e)
	{
		if ((e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back) && !base.ReadOnly)
		{
			Clear();
			Focus();
			return true;
		}
		return false;
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		if (!OwnerPropertyEnumerator.Property.Value.ReadOnly)
		{
			int currentIndex = -1;
			string[] displayedValues = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
			for (int i = 0; i < displayedValues.Length; i++)
			{
				if (_ownerPropertyEnum.Property.Value.DisplayString.Equals(displayedValues[i]))
				{
					currentIndex = i;
					break;
				}
			}
			if (base.ReadOnly && OwnerPropertyEnumerator.Property.Value.UseAutoCompletion)
			{
				string text = OwnerPropertyEnumerator.Property.Value.IncrementalFind(e.KeyChar, displayedValues, ref currentIndex);
				if (currentIndex != -1 && Text != text)
				{
					Text = text;
					SelectAll();
				}
			}
		}
		base.OnKeyPress(e);
	}

	public virtual void RepositionChildren()
	{
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
		if (base.ReadOnly)
		{
			return valueValidationResult;
		}
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
				_ = Text;
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
		return null;
	}
}
