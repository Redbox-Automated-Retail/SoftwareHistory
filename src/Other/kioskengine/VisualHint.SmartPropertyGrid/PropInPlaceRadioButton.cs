using System;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public class PropInPlaceRadioButton : Control, IInPlaceControl
{
	private Control _wmFocusMissingArg;

	private int _focusIndex = -1;

	private bool _realtimeChange = true;

	private PropertyEnumerator _ownerPropertyEnum;

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
			Text = _ownerPropertyEnum.Property.Value.DisplayString;
			string[] displayedValues = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
			for (int i = 0; i < displayedValues.Length; i++)
			{
				if (displayedValues[i].Equals(Text))
				{
					_focusIndex = i;
					break;
				}
			}
		}
	}

	public TextBox TextBox => null;

	public PropInPlaceRadioButton()
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
		string[] displayedValues = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
		if (displayedValues.Length <= 0)
		{
			return;
		}
		Rectangle rectangle2 = clientRectangle;
		rectangle2.Height = clientRectangle.Height / displayedValues.Length;
		for (int i = 0; i < displayedValues.Length; i++)
		{
			Rectangle rect = rectangle2;
			rect.Height -= 4;
			rect.Y += 2;
			rect.Width = rect.Height;
			parentGrid.DrawManager.DrawRadioButton(e.Graphics, parentGrid.Grid, parentGrid.TransformRectIfRTL(rect, base.ClientSize.Width), Text == displayedValues[i], _ownerPropertyEnum.Property.Enabled, PropertyValue.DrawValueIn.InPlaceCtrl);
			Rectangle rect2 = rectangle2;
			rect2.X = rect.Right + parentGrid.GlobalTextMargin;
			rect2.Width = rectangle2.Right - rect2.Left;
			rect2 = parentGrid.TransformRectIfRTL(rect2, base.ClientSize.Width);
			Win32Calls.DrawText(color: _ownerPropertyEnum.Property.Enabled ? _ownerPropertyEnum.Property.Value.ForeColor : SystemColors.GrayText, graphics: e.Graphics, text: displayedValues[i], rect: ref rect2, font: Font, format: 0x8824 | ((parentGrid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
			if (Focused && _focusIndex == i)
			{
				Rectangle rect3 = parentGrid.TransformRectIfRTL(rect2, base.ClientSize.Width);
				rect3.X -= 2;
				rect3.Width += 2;
				rect3.Y++;
				rect3.Height -= 2;
				ControlPaint.DrawFocusRectangle(e.Graphics, parentGrid.TransformRectIfRTL(rect3, base.ClientSize.Width));
			}
			rectangle2.Offset(0, rectangle2.Height);
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
			PropertyValue value = _ownerPropertyEnum.Property.Value;
			string[] displayedValues = value.GetDisplayedValues();
			if (displayedValues.Length > 0)
			{
				Rectangle rectangle2 = clientRectangle;
				rectangle2.Height = clientRectangle.Height / displayedValues.Length;
				_ = value.HasMultipleValues;
				for (int i = 0; i < displayedValues.Length; i++)
				{
					if (rectangle2.Contains(new Point(e.X, e.Y)) && !Text.Equals(displayedValues[i]))
					{
						_focusIndex = i;
						Text = displayedValues[i];
						if (_realtimeChange)
						{
							_ownerPropertyEnum.Property.Value.PreviousValue = _ownerPropertyEnum.Property.Value.GetValue();
							object value2 = _ownerPropertyEnum.Property.Value.ConvertDisplayedStringToValue(Text);
							_ownerPropertyEnum.Property.Value.SetValueFromInPlaceCtrl(value2, null);
							PropertyChangedEventArgs e2 = new PropertyChangedEventArgs(_ownerPropertyEnum);
							_ownerPropertyEnum.Property.ParentGrid.NotifyPropertyChanged(e2);
						}
						Invalidate();
						base.Parent.Invalidate();
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
		string[] displayedValues = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
		switch (key)
		{
		case Keys.Down:
			if (_focusIndex == -1)
			{
				_focusIndex = 0;
			}
			else
			{
				_focusIndex = (_focusIndex + 1) % displayedValues.Length;
			}
			break;
		case Keys.Up:
			if (_focusIndex == -1)
			{
				_focusIndex = displayedValues.Length - 1;
			}
			else
			{
				_focusIndex = ((--_focusIndex == -1) ? (displayedValues.Length - 1) : _focusIndex);
			}
			break;
		}
		Text = displayedValues[_focusIndex];
		if (_realtimeChange)
		{
			_ownerPropertyEnum.Property.Value.PreviousValue = _ownerPropertyEnum.Property.Value.GetValue();
			object value = _ownerPropertyEnum.Property.Value.ConvertDisplayedStringToValue(Text);
			_ownerPropertyEnum.Property.Value.SetValueFromInPlaceCtrl(value, null);
			PropertyChangedEventArgs e = new PropertyChangedEventArgs(_ownerPropertyEnum);
			_ownerPropertyEnum.Property.ParentGrid.NotifyPropertyChanged(e);
		}
		Invalidate();
		base.Parent.Invalidate();
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)
		{
			ProcessArrowKey(e.KeyCode);
		}
		else if (e.KeyCode == Keys.Space)
		{
			PropertyValue value = _ownerPropertyEnum.Property.Value;
			string[] displayedValues = value.GetDisplayedValues();
			if (_focusIndex != -1 && !Text.Equals(displayedValues[_focusIndex]))
			{
				Text = displayedValues[_focusIndex];
				if (_realtimeChange)
				{
					value.PreviousValue = value.GetValue();
					object value2 = value.ConvertDisplayedStringToValue(Text);
					value.SetValueFromInPlaceCtrl(value2, null);
					PropertyChangedEventArgs e2 = new PropertyChangedEventArgs(_ownerPropertyEnum);
					_ownerPropertyEnum.Property.ParentGrid.NotifyPropertyChanged(e2);
				}
				Invalidate();
				base.Parent.Invalidate();
			}
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
			if (!RealtimeChange)
			{
				Text = _ownerPropertyEnum.Property.Value.DisplayString;
			}
			_focusIndex = -1;
			string[] displayedValues = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
			for (int i = 0; i < displayedValues.Length; i++)
			{
				if (displayedValues[i].Equals(Text))
				{
					_focusIndex = i;
					break;
				}
			}
		}
		if (Focused)
		{
			return base.ProcessDialogKey(keyData);
		}
		return true;
	}

	protected override void OnGotFocus(EventArgs e)
	{
		Text = _ownerPropertyEnum.Property.Value.DisplayString;
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

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		string[] displayedValues = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
		if (e.Delta < 0)
		{
			if (_focusIndex == -1)
			{
				_focusIndex = 0;
			}
			else
			{
				_focusIndex = (_focusIndex + 1) % displayedValues.Length;
			}
		}
		else if (_focusIndex == -1)
		{
			_focusIndex = displayedValues.Length - 1;
		}
		else
		{
			_focusIndex = ((--_focusIndex == -1) ? (displayedValues.Length - 1) : _focusIndex);
		}
		Invalidate();
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
		if (forward)
		{
			ProcessArrowKey(Keys.Down);
		}
		else
		{
			ProcessArrowKey(Keys.Up);
		}
		if (!RealtimeChange)
		{
			CommitChanges(final: true);
		}
		return true;
	}

	public virtual PropertyValue.ValueValidationResult CommitChanges(bool final)
	{
		PropertyValue.ValueValidationResult result = PropertyValue.ValueValidationResult.PreviousValueRestored;
		if (Text != _ownerPropertyEnum.Property.Value.DisplayString)
		{
			object value = null;
			try
			{
				_ownerPropertyEnum.Property.ParentGrid.NotifyBeginCommitChanges(new CommitChangesEventArgs(_ownerPropertyEnum, Text, valueIsText: true));
				_ownerPropertyEnum.Property.Value.PreviousValue = _ownerPropertyEnum.Property.Value.GetValue();
				value = _ownerPropertyEnum.Property.Value.ConvertDisplayedStringToValue(Text);
				result = _ownerPropertyEnum.Property.Value.SetValueFromInPlaceCtrl(value, null);
				PropertyChangedEventArgs e = new PropertyChangedEventArgs(_ownerPropertyEnum);
				_ownerPropertyEnum.Property.ParentGrid.NotifyPropertyChanged(e);
			}
			finally
			{
				_ownerPropertyEnum.Property.ParentGrid.NotifyEndCommitChanges(new CommitChangesEventArgs(_ownerPropertyEnum, value, valueIsText: false));
			}
		}
		Invalidate();
		base.Parent.Invalidate();
		return result;
	}

	public Control GetDropDownContainer()
	{
		return null;
	}
}
