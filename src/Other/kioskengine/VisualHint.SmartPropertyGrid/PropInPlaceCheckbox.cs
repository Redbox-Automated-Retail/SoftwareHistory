using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public class PropInPlaceCheckbox : Control, IInPlaceControl
{
	private class NullableBoolean : ICloneable
	{
		private bool _value;

		private bool _isNull;

		public bool Value
		{
			get
			{
				if (_isNull)
				{
					throw new NullReferenceException("The nullable boolean is null");
				}
				return _value;
			}
			set
			{
				_isNull = false;
				_value = value;
			}
		}

		public bool IsNull
		{
			get
			{
				return _isNull;
			}
			set
			{
				_isNull = value;
			}
		}

		public override string ToString()
		{
			if (_isNull)
			{
				throw new NullReferenceException("The nullable boolean is null");
			}
			return _value.ToString();
		}

		public object Clone()
		{
			NullableBoolean nullableBoolean = new NullableBoolean();
			nullableBoolean._isNull = IsNull;
			if (!nullableBoolean._isNull)
			{
				nullableBoolean._value = Value;
			}
			return nullableBoolean;
		}
	}

	private Control _wmFocusMissingArg;

	private int _focusIndex;

	private Enum _enumValue;

	private NullableBoolean _booleanValue;

	private ArrayList _collectionValue;

	private bool _multipleValues;

	private PropertyEnumerator _ownerPropertyEnum;

	private bool _realtimeChange = true;

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
				ResetCachedValue();
			}
		}
	}

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

	private bool IsBooleanValue
	{
		get
		{
			if (!(_ownerPropertyEnum.Property.Value.TypeConverter is NullableConverter nullableConverter) || nullableConverter.UnderlyingType != typeof(bool))
			{
				return _ownerPropertyEnum.Property.Value.UnderlyingType == typeof(bool);
			}
			return true;
		}
	}

	private bool IsEnumValue => _ownerPropertyEnum.Property.Value.UnderlyingType.IsEnum;

	public TextBox TextBox => null;

	private void ResetCachedValue()
	{
		if (IsBooleanValue)
		{
			if (_booleanValue == null)
			{
				_booleanValue = new NullableBoolean();
			}
			object value = _ownerPropertyEnum.Property.Value.GetValue();
			if (_ownerPropertyEnum.Property.Value.HasMultipleValues || value == null)
			{
				_booleanValue.IsNull = true;
			}
			else
			{
				_booleanValue.Value = (bool)value;
			}
		}
		else if (IsEnumValue)
		{
			if (Attribute.GetCustomAttribute(_ownerPropertyEnum.Property.Value.UnderlyingType, typeof(FlagsAttribute)) == null)
			{
				throw new Exception("Checkboxes for Enums must be linked to a \"flagged\" enumeration.");
			}
			if (!(_multipleValues = _ownerPropertyEnum.Property.Value.HasMultipleValues))
			{
				_enumValue = (Enum)_ownerPropertyEnum.Property.Value.GetValue();
			}
			else
			{
				_enumValue = (Enum)_ownerPropertyEnum.Property.Value.TypeConverter.ConvertFromString(_ownerPropertyEnum.Property.Value.GetTypeDescriptorContext(_ownerPropertyEnum), _ownerPropertyEnum.Property.Value.CultureInfo, "0");
			}
		}
		else if (_ownerPropertyEnum.Property.Value.UnderlyingType.Equals(typeof(ArrayList)))
		{
			if (_ownerPropertyEnum.Property.Value.HasMultipleValues)
			{
				_collectionValue = new ArrayList();
				_multipleValues = true;
			}
			else
			{
				_collectionValue = (ArrayList)((ArrayList)_ownerPropertyEnum.Property.Value.UnderlyingValue).Clone();
			}
		}
	}

	public PropInPlaceCheckbox()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | Utilities.DoubleBufferStyle | ControlStyles.Opaque | ControlStyles.ResizeRedraw, value: true);
		Cursor = Cursors.Arrow;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		PropertyGrid parentGrid = _ownerPropertyEnum.Property.ParentGrid;
		Rectangle clientRectangle = base.ClientRectangle;
		clientRectangle.Height++;
		parentGrid.DrawManager.DrawPropertyValueBackground(e.Graphics, clientRectangle, _ownerPropertyEnum, PropertyValue.DrawValueIn.InPlaceCtrl);
		Rectangle rectangle = _ownerPropertyEnum.Property.Value.DrawIcon(e.Graphics, clientRectangle, PropertyValue.DrawValueIn.InPlaceCtrl);
		int globalTextMargin = parentGrid.GlobalTextMargin;
		clientRectangle.X += globalTextMargin - 1;
		clientRectangle.Width -= globalTextMargin - 1;
		if (rectangle.Width > 0)
		{
			rectangle.Width += globalTextMargin;
			clientRectangle.X += rectangle.Width;
			clientRectangle.Width -= rectangle.Width;
		}
		Color color = (_ownerPropertyEnum.Property.Enabled ? _ownerPropertyEnum.Property.Value.ForeColor : SystemColors.GrayText);
		_ = _ownerPropertyEnum.Property.Value.HasMultipleValues;
		if (IsBooleanValue)
		{
			Rectangle rect = clientRectangle;
			rect.Height -= 4;
			rect.Y += 2;
			rect.Width = rect.Height;
			string text;
			if (_booleanValue.IsNull)
			{
				parentGrid.DrawManager.DrawCheckBoxIndeterminate(e.Graphics, parentGrid.Grid, parentGrid.TransformRectIfRTL(rect, base.ClientSize.Width), _ownerPropertyEnum.Property.Enabled, PropertyValue.DrawValueIn.InPlaceCtrl);
				text = _ownerPropertyEnum.Property.Value.ConvertValueToDisplayedString(null);
			}
			else
			{
				parentGrid.DrawManager.DrawCheckBox(e.Graphics, this, parentGrid.TransformRectIfRTL(rect, base.ClientSize.Width), _booleanValue.Value, _ownerPropertyEnum.Property.Enabled, PropertyValue.DrawValueIn.InPlaceCtrl);
				text = _ownerPropertyEnum.Property.Value.ConvertValueToDisplayedString(_booleanValue.Value);
			}
			Rectangle rect2 = clientRectangle;
			rect2.X = rect.Right + globalTextMargin;
			rect2.Width = clientRectangle.Right - rect2.Left;
			rect2 = parentGrid.TransformRectIfRTL(rect2, base.ClientSize.Width);
			Win32Calls.DrawText(e.Graphics, text, ref rect2, Font, color, 0x8824 | ((parentGrid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
			if (Focused && text.Length > 0)
			{
				Rectangle rect3 = parentGrid.TransformRectIfRTL(rect2, base.ClientSize.Width);
				rect3.X -= 2;
				rect3.Width += 2;
				rect3.Y++;
				rect3.Height -= 2;
				ControlPaint.DrawFocusRectangle(e.Graphics, parentGrid.TransformRectIfRTL(rect3, base.ClientSize.Width));
			}
		}
		else if (IsEnumValue)
		{
			string[] displayedValues = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
			if (displayedValues.Length <= 0)
			{
				return;
			}
			Rectangle rectangle2 = clientRectangle;
			rectangle2.Height = clientRectangle.Height / displayedValues.Length;
			int num = 0;
			string[] array = displayedValues;
			foreach (string displayedStr in array)
			{
				Rectangle rect4 = rectangle2;
				rect4.Height -= 4;
				rect4.Y += 2;
				rect4.Width = rect4.Height;
				if (_multipleValues)
				{
					parentGrid.DrawManager.DrawCheckBoxIndeterminate(e.Graphics, this, parentGrid.TransformRectIfRTL(rect4, base.ClientSize.Width), _ownerPropertyEnum.Property.Enabled, PropertyValue.DrawValueIn.InPlaceCtrl);
				}
				else
				{
					int hashCode = _ownerPropertyEnum.Property.Value.ConvertDisplayedStringToValue(displayedStr).GetHashCode();
					bool drawChecked = IsEnumCheckedAt(hashCode);
					parentGrid.DrawManager.DrawCheckBox(e.Graphics, this, parentGrid.TransformRectIfRTL(rect4, base.ClientSize.Width), drawChecked, _ownerPropertyEnum.Property.Enabled, PropertyValue.DrawValueIn.InPlaceCtrl);
				}
				Rectangle rect5 = rectangle2;
				rect5.X = rect4.Right + globalTextMargin;
				rect5.Width = rectangle2.Right - rect5.Left;
				rect5 = parentGrid.TransformRectIfRTL(rect5, base.ClientSize.Width);
				Win32Calls.DrawText(e.Graphics, displayedStr, ref rect5, Font, color, 0x8824 | ((parentGrid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
				if (Focused && _focusIndex == num)
				{
					Rectangle rect6 = parentGrid.TransformRectIfRTL(rect5, base.ClientSize.Width);
					rect6.X -= 2;
					rect6.Width += 2;
					rect6.Y++;
					rect6.Height -= 2;
					ControlPaint.DrawFocusRectangle(e.Graphics, parentGrid.TransformRectIfRTL(rect6, base.ClientSize.Width));
				}
				rectangle2.Offset(0, rectangle2.Height);
				num++;
			}
		}
		else
		{
			if (!_ownerPropertyEnum.Property.Value.UnderlyingType.Equals(typeof(ArrayList)))
			{
				return;
			}
			string[] displayedValues2 = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
			if (displayedValues2.Length <= 0)
			{
				return;
			}
			Rectangle rectangle3 = clientRectangle;
			rectangle3.Height = clientRectangle.Height / displayedValues2.Length;
			for (int j = 0; j < displayedValues2.Length; j++)
			{
				Rectangle rect7 = rectangle3;
				rect7.Height -= 4;
				rect7.Y += 2;
				rect7.Width = rect7.Height;
				if (_multipleValues)
				{
					parentGrid.DrawManager.DrawCheckBoxIndeterminate(e.Graphics, this, parentGrid.TransformRectIfRTL(rect7, base.ClientSize.Width), _ownerPropertyEnum.Property.Enabled, PropertyValue.DrawValueIn.InPlaceCtrl);
				}
				else
				{
					bool drawChecked2 = false;
					foreach (object item in _collectionValue)
					{
						if (item.ToString() == displayedValues2[j])
						{
							drawChecked2 = true;
						}
					}
					parentGrid.DrawManager.DrawCheckBox(e.Graphics, this, parentGrid.TransformRectIfRTL(rect7, base.ClientSize.Width), drawChecked2, _ownerPropertyEnum.Property.Enabled, PropertyValue.DrawValueIn.InPlaceCtrl);
				}
				Rectangle rect8 = rectangle3;
				rect8.X = rect7.Right + globalTextMargin;
				rect8.Width = rectangle3.Right - rect8.Left;
				rect8 = parentGrid.TransformRectIfRTL(rect8, base.ClientSize.Width);
				Win32Calls.DrawText(e.Graphics, displayedValues2[j], ref rect8, Font, color, 0x8824 | ((parentGrid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
				if (Focused && _focusIndex == j)
				{
					Rectangle rect9 = parentGrid.TransformRectIfRTL(rect8, base.ClientSize.Width);
					rect9.X -= 2;
					rect9.Width += 2;
					rect9.Y++;
					rect9.Height -= 2;
					ControlPaint.DrawFocusRectangle(e.Graphics, parentGrid.TransformRectIfRTL(rect9, base.ClientSize.Width));
				}
				rectangle3.Offset(0, rectangle3.Height);
			}
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			if (!Focused)
			{
				Focus();
			}
			Rectangle clientRectangle = base.ClientRectangle;
			Rectangle rectangle = _ownerPropertyEnum.Property.Value.DrawIcon(null, clientRectangle, PropertyValue.DrawValueIn.InPlaceCtrl);
			clientRectangle.Height++;
			int globalTextMargin = _ownerPropertyEnum.Property.ParentGrid.GlobalTextMargin;
			clientRectangle.X += globalTextMargin;
			clientRectangle.Width -= globalTextMargin;
			if (rectangle.Width > 0)
			{
				rectangle.Width += globalTextMargin;
				clientRectangle.X += rectangle.Width;
				clientRectangle.Width -= rectangle.Width;
			}
			string[] displayedValues = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
			if (displayedValues.Length > 0)
			{
				Rectangle rectangle2 = clientRectangle;
				rectangle2.Height = clientRectangle.Height / displayedValues.Length;
				for (int i = 0; i < displayedValues.Length; i++)
				{
					if (rectangle2.Contains(new Point(e.X, e.Y)))
					{
						_focusIndex = i;
						Invalidate();
						SwitchCheckboxAtIndex(i, forceRealtime: false);
						break;
					}
					rectangle2.Offset(0, rectangle2.Height);
				}
			}
		}
		base.OnMouseDown(e);
	}

	protected override bool IsInputKey(Keys keyData)
	{
		if (keyData == Keys.Up || keyData == Keys.Down)
		{
			return true;
		}
		return base.IsInputKey(keyData);
	}

	protected void ProcessArrowKey(Keys key)
	{
		int num = ((!IsEnumValue) ? _ownerPropertyEnum.Property.Value.GetDisplayedValues().Length : _ownerPropertyEnum.Property.Value.GetStandardValues().Length);
		if (num > 0)
		{
			switch (key)
			{
			case Keys.Down:
				_focusIndex = (_focusIndex + 1) % num;
				break;
			case Keys.Up:
				_focusIndex = ((--_focusIndex == -1) ? (num - 1) : _focusIndex);
				break;
			}
			Invalidate();
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if ((e.KeyCode == Keys.Down || e.KeyCode == Keys.Up) && !IsBooleanValue)
		{
			ProcessArrowKey(e.KeyCode);
		}
		else if (e.KeyCode == Keys.Space)
		{
			SwitchCheckboxAtIndex(_focusIndex, forceRealtime: false);
		}
		base.OnKeyDown(e);
	}

	[UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
	protected override bool ProcessDialogKey(Keys keyData)
	{
		Keys keys = keyData & Keys.KeyCode;
		if (keys == Keys.Return)
		{
			if (!_realtimeChange)
			{
				CommitChanges(final: true);
			}
		}
		else if (keyData == Keys.Escape)
		{
			ResetCachedValue();
		}
		if (Focused)
		{
			return base.ProcessDialogKey(keyData);
		}
		return true;
	}

	protected override void OnGotFocus(EventArgs e)
	{
		Invalidate();
		base.OnGotFocus(e);
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

	public void OnF4()
	{
		Focus();
	}

	public bool OnParentKeyDown(KeyEventArgs e)
	{
		return false;
	}

	private bool IsEnumCheckedAt(int flagValue)
	{
		if (_multipleValues && _enumValue.Equals((Enum)Enum.ToObject(_ownerPropertyEnum.Property.Value.UnderlyingType, 0)))
		{
			return false;
		}
		int hashCode = _enumValue.GetHashCode();
		if ((hashCode & flagValue) != flagValue || flagValue == 0)
		{
			if (hashCode == 0)
			{
				return flagValue == 0;
			}
			return false;
		}
		return true;
	}

	private void SwitchCheckboxAtIndex(int index, bool forceRealtime)
	{
		if (IsEnumValue)
		{
			string[] displayedValues = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
			int hashCode = _ownerPropertyEnum.Property.Value.ConvertDisplayedStringToValue(displayedValues[index]).GetHashCode();
			if (IsEnumCheckedAt(hashCode))
			{
				if (hashCode == 0)
				{
					string[] array = displayedValues;
					foreach (string displayedStr in array)
					{
						int hashCode2 = _ownerPropertyEnum.Property.Value.ConvertDisplayedStringToValue(displayedStr).GetHashCode();
						if (hashCode2 != 0)
						{
							_enumValue = (Enum)Enum.ToObject(_ownerPropertyEnum.Property.Value.UnderlyingType, Convert.ToInt32(_enumValue) | hashCode2);
						}
					}
				}
				else
				{
					_enumValue = (Enum)Enum.ToObject(_ownerPropertyEnum.Property.Value.UnderlyingType, _enumValue.GetHashCode() & ~hashCode);
				}
			}
			else if (hashCode == 0)
			{
				_enumValue = (Enum)Enum.ToObject(_ownerPropertyEnum.Property.Value.UnderlyingType, 0);
			}
			else
			{
				_enumValue = (Enum)Enum.ToObject(_ownerPropertyEnum.Property.Value.UnderlyingType, _enumValue.GetHashCode() | hashCode);
			}
			_multipleValues = false;
			Invalidate();
			if (_realtimeChange)
			{
				CommitChanges(final: false);
			}
			return;
		}
		if (IsBooleanValue)
		{
			if (_ownerPropertyEnum.Property.Value.HasMultipleValues)
			{
				_booleanValue.Value = true;
			}
			else if (_booleanValue.IsNull)
			{
				_booleanValue.Value = true;
			}
			else if (_booleanValue.Value)
			{
				_booleanValue.Value = false;
			}
			else if (_ownerPropertyEnum.Property.Value.TypeConverter is NullableConverter)
			{
				if (_ownerPropertyEnum.Property.Value.ConvertValueToDisplayedString(null) != null)
				{
					_booleanValue.IsNull = true;
				}
				else
				{
					_booleanValue.Value = true;
				}
			}
			else
			{
				_booleanValue.Value = true;
			}
			Invalidate();
			if (_realtimeChange || forceRealtime)
			{
				CommitChanges(final: false);
			}
			return;
		}
		string[] displayedValues2 = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
		if (index >= displayedValues2.Length)
		{
			return;
		}
		int num = _collectionValue.IndexOf(displayedValues2[index]);
		if (num != -1)
		{
			_collectionValue.RemoveAt(num);
		}
		else
		{
			int num2 = -1;
			for (int num3 = index - 1; num3 >= 0; num3--)
			{
				num2 = _collectionValue.IndexOf(displayedValues2[num3]);
				if (num2 != -1)
				{
					break;
				}
			}
			_collectionValue.Insert(num2 + 1, displayedValues2[index]);
		}
		_multipleValues = false;
		Invalidate();
		if (_realtimeChange)
		{
			CommitChanges(final: false);
		}
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		if (!IsBooleanValue)
		{
			if (e.Delta > 0)
			{
				ProcessArrowKey(Keys.Up);
			}
			else
			{
				ProcessArrowKey(Keys.Down);
			}
			Invalidate();
		}
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
		if (IsBooleanValue)
		{
			SwitchCheckboxAtIndex(0, forceRealtime: true);
			return true;
		}
		return false;
	}

	public virtual PropertyValue.ValueValidationResult CommitChanges(bool final)
	{
		bool flag = false;
		object value = _ownerPropertyEnum.Property.Value.GetValue();
		if (IsBooleanValue)
		{
			if (_ownerPropertyEnum.Property.Value.HasMultipleValues || (value == null && !_booleanValue.IsNull) || (value != null && (_booleanValue.IsNull || (bool)value != _booleanValue.Value)))
			{
				flag = true;
			}
		}
		else if (IsEnumValue)
		{
			if (_ownerPropertyEnum.Property.Value.HasMultipleValues)
			{
				if (!_multipleValues)
				{
					flag = true;
				}
			}
			else if (!_enumValue.Equals(value))
			{
				flag = true;
			}
		}
		else if (_ownerPropertyEnum.Property.Value.HasMultipleValues)
		{
			if (!_multipleValues)
			{
				flag = true;
			}
		}
		else
		{
			flag = false;
			ArrayList arrayList = value as ArrayList;
			if (arrayList == null && _collectionValue == null)
			{
				flag = false;
			}
			else if (arrayList == null || _collectionValue == null)
			{
				flag = true;
			}
			else if (arrayList.Count != _collectionValue.Count)
			{
				flag = true;
			}
			else
			{
				for (int i = 0; i < _collectionValue.Count; i++)
				{
					if (!arrayList[i].Equals(_collectionValue[i]))
					{
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			object obj = null;
			_ownerPropertyEnum.Property.Value.PreviousValue = value;
			obj = (IsBooleanValue ? ((!_booleanValue.IsNull) ? ((object)_booleanValue.Value) : null) : ((!IsEnumValue) ? _collectionValue.Clone() : _enumValue));
			try
			{
				_ownerPropertyEnum.Property.ParentGrid.NotifyBeginCommitChanges(new CommitChangesEventArgs(_ownerPropertyEnum, obj, valueIsText: false));
				_ownerPropertyEnum.Property.Value.SetValueFromInPlaceCtrl(obj, null);
				_ownerPropertyEnum.Property.ParentGrid.NotifyPropertyChanged(new PropertyChangedEventArgs(_ownerPropertyEnum));
				base.Parent.Invalidate();
			}
			finally
			{
				_ownerPropertyEnum.Property.ParentGrid.NotifyEndCommitChanges(new CommitChangesEventArgs(_ownerPropertyEnum, obj, valueIsText: false));
			}
		}
		Invalidate();
		if (!flag)
		{
			return PropertyValue.ValueValidationResult.PreviousValueRestored;
		}
		return PropertyValue.ValueValidationResult.Validated;
	}

	public Control GetDropDownContainer()
	{
		return null;
	}
}
