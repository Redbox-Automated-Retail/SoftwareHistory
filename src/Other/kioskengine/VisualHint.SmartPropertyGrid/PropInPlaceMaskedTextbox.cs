using System;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public class PropInPlaceMaskedTextbox : MaskedTextBox, IInPlaceControl
{
	private PropertyValue.ValueValidationResult _currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;

	private PropertyEnumerator _currentInvalidPropertyEnum;

	private PropertyEnumerator _ownerPropertyEnum;

	private Control _wmFocusMissingArg;

	public PropertyEnumerator OwnerPropertyEnumerator
	{
		get
		{
			return _ownerPropertyEnum;
		}
		set
		{
			_ownerPropertyEnum = (PropertyEnumerator)value.Clone();
			if (!_ownerPropertyEnum.Property.ParentGrid.IsInsideRefreshProperties)
			{
				RightToLeft = _ownerPropertyEnum.Property.ParentGrid.RightToLeft;
				base.FormatProvider = _ownerPropertyEnum.Property.Value.CultureInfo;
				if (!base.Visible)
				{
					_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
				}
				if (Text != _ownerPropertyEnum.Property.Value.DisplayString)
				{
					Text = _ownerPropertyEnum.Property.Value.DisplayString;
				}
				if (_ownerPropertyEnum.Property.Value.IsReadOnly(_ownerPropertyEnum) && _ownerPropertyEnum.Property.ParentGrid.ReadOnlyVisual == PropertyGrid.ReadOnlyVisuals.ReadOnlyFeel)
				{
					base.ReadOnly = true;
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
			Win32Calls.SendMessage(base.Handle, 211, (IntPtr)3, (IntPtr)Win32Calls.MakeLong(num, num));
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

	public TextBox TextBox => null;

	public PropInPlaceMaskedTextbox()
	{
		AutoSize = false;
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
		switch (keyData & Keys.KeyCode)
		{
		case Keys.Return:
		{
			PropertyValue.ValueValidationResult result2 = CommitChanges(final: true);
			if (PropertyValue.IsErrorCode(result2) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
			{
				SelectAll();
				return true;
			}
			break;
		}
		case Keys.Tab:
		{
			PropertyValue.ValueValidationResult result = CommitChanges(final: true);
			if (PropertyValue.IsErrorCode(result) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
			{
				SelectAll();
				return true;
			}
			break;
		}
		case Keys.Escape:
		{
			PropertyValue value = _ownerPropertyEnum.Property.Value;
			if (value != null)
			{
				Text = value.DisplayString;
				if (PropertyValue.IsErrorCode(_currentValueValidationResult))
				{
					_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
					_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, value.GetValue(), _currentValueValidationResult));
				}
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

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 7)
		{
			_wmFocusMissingArg = Control.FromHandle(m.WParam);
			DefWndProc(ref m);
			return;
		}
		if (m.Msg == 8)
		{
			_wmFocusMissingArg = Control.FromHandle(m.WParam);
		}
		else if (m.Msg == 20)
		{
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
			if (_ownerPropertyEnum.Property.Value != null)
			{
				string mask = base.Mask;
				base.Mask = "";
				string text = Text;
				base.Mask = mask;
				object obj2 = null;
				if (!_ownerPropertyEnum.Property.Value.HasMultipleValues)
				{
					obj2 = _ownerPropertyEnum.Property.Value.GetValue();
				}
				try
				{
					if (!_ownerPropertyEnum.Property.Value.HasMultipleValues || text.Length != 0)
					{
						obj = _ownerPropertyEnum.Property.Value.ConvertDisplayedStringToValue(text);
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
				if ((obj != null && (obj2 == null || !obj.Equals(obj2))) || (obj == null && obj2 != null))
				{
					if (base.MaskCompleted)
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
					}
					else if (_ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.IgnoreAndRestore)
					{
						OwnerPropertyEnumerator.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, OwnerPropertyEnumerator, obj, PropertyValue.ValueValidationResult.ValidatorFailed));
						Text = _ownerPropertyEnum.Property.Value.DisplayString;
						_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, OwnerPropertyEnumerator, _ownerPropertyEnum.Property.Value.GetValue(), valueValidationResult));
					}
					else
					{
						valueValidationResult = PropertyValue.ValueValidationResult.ValidatorFailed;
						_currentInvalidPropertyEnum = OwnerPropertyEnumerator;
						PropertyValidatorBase.Message = "Incomplete mask";
						OwnerPropertyEnumerator.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, obj, valueValidationResult));
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
